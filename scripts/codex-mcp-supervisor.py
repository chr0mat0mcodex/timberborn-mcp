"""Persistent stdio frontend. Python 3.8+, standard library only; no build step."""
import argparse
import ctypes
from ctypes import wintypes
import json
import os
from pathlib import Path
import queue
import subprocess
import sys
import threading
import time
import uuid

ROOT = Path(__file__).resolve().parent.parent
CONTROL = "timberborn_server_control"
TOOL = {"name": CONTROL, "description": "Control the project MCP backend while retaining the Codex connection. stop before the human build gate; start only after the developer reports live bereit. Interrupted calls are never replayed. status does not start anything.",
        "inputSchema": {"type": "object", "properties": {"action": {"type": "string", "enum": ["status", "stop", "start"]}}, "required": ["action"], "additionalProperties": False}}


def alive(pid, observed_at=None):
    if os.name != "nt":
        try:
            os.kill(pid, 0)
            return True
        except ProcessLookupError:
            return False
    kernel = ctypes.WinDLL("kernel32", use_last_error=True)
    kernel.OpenProcess.argtypes = [ctypes.c_ulong, ctypes.c_int, ctypes.c_ulong]
    kernel.OpenProcess.restype = ctypes.c_void_p
    kernel.WaitForSingleObject.argtypes = [ctypes.c_void_p, ctypes.c_ulong]
    kernel.CloseHandle.argtypes = [ctypes.c_void_p]
    kernel.GetProcessTimes.argtypes = [ctypes.c_void_p] + [ctypes.POINTER(wintypes.FILETIME)] * 4
    handle = kernel.OpenProcess(0x00100000 | 0x1000, False, pid)
    if not handle:
        if ctypes.get_last_error() == 87:  # invalid PID
            return False
        raise RuntimeError("Cannot verify supervisor process; refusing build")
    try:
        if kernel.WaitForSingleObject(handle, 0) == 0:
            return False
        if observed_at is not None:
            creation, end, kernel_time, user_time = [wintypes.FILETIME() for _ in range(4)]
            if not kernel.GetProcessTimes(handle, ctypes.byref(creation), ctypes.byref(end), ctypes.byref(kernel_time), ctypes.byref(user_time)):
                raise RuntimeError("Cannot verify supervisor process identity; refusing build")
            born = ((creation.dwHighDateTime << 32) | creation.dwLowDateTime) / 10000000 - 11644473600
            if born > observed_at:
                return False  # Windows reused a stale registry's PID.
        return True
    finally:
        kernel.CloseHandle(handle)


def atomic_json(path, value):
    temp = path.with_name(path.name + "." + uuid.uuid4().hex + ".tmp")
    try:
        temp.write_text(json.dumps(value), encoding="utf-8")
        os.replace(str(temp), str(path))
    finally:
        if temp.exists():
            temp.unlink()


def request_stop(runtime, timeout=10):
    runtime.mkdir(parents=True, exist_ok=True)
    ticket = uuid.uuid4().hex
    atomic_json(runtime / "gate.json", {"ticket": ticket})
    # Registries are heartbeats, never authority to kill a PID.
    deadline = time.monotonic() + timeout
    while True:
        pending = []
        for path in runtime.glob("instance-*.json"):
            try:
                state = json.loads(path.read_text(encoding="utf-8"))
                if alive(state["pid"], state.get("heartbeat")) and (
                        state.get("ticket") != ticket or state.get("running")):
                    pending.append(path.name)
            except (OSError, ValueError, KeyError):
                pending.append(path.name)
        if not pending:
            return
        if time.monotonic() >= deadline:
            raise RuntimeError("Supervisor stop was not acknowledged; build must not start")
        time.sleep(0.1)


class Supervisor:
    def __init__(self, command, runtime, env=None, emit=None):
        self.command, self.runtime, self.env = command, runtime, env
        runtime.mkdir(parents=True, exist_ok=True)
        self.registry = runtime / ("instance-" + uuid.uuid4().hex + ".json")
        self.gate = runtime / "gate.json"
        self.lifecycle = threading.RLock()
        self.lock = threading.RLock()
        self.output_lock = threading.Lock()
        self.emit = emit or self.write_stdout
        self.process = None
        self.reader_thread = None
        self.pending = {}
        self.client_ids = {}
        self.counter = 0
        self.tools = []
        self.catalog = runtime / "tools-cache.json"
        try:
            cached = json.loads(self.catalog.read_text(encoding="utf-8"))
            if isinstance(cached, list) and all(isinstance(t, dict) and isinstance(t.get("name"), str) and isinstance(t.get("inputSchema"), dict) and t["name"] != CONTROL for t in cached):
                self.tools = cached
        except (OSError, ValueError):
            pass  # Missing/corrupt discovery cache must not prevent lifecycle control.
        self.params = None
        self.initialized = False
        self.closed = threading.Event()
        self.ticket = None
        self.publish()
        self.watcher = threading.Thread(target=self.watch, daemon=True)
        self.watcher.start()

    def write_stdout(self, message):
        sys.stdout.write(json.dumps(message, separators=(",", ":")) + "\n")
        sys.stdout.flush()

    def send(self, message):
        with self.output_lock:
            self.emit(message)

    def publish(self):
        with self.lock:
            atomic_json(self.registry, {"pid": os.getpid(), "heartbeat": time.time(), "running": self.process is not None,
                                        "ticket": self.ticket})

    def watch(self):
        while not self.closed.wait(0.2):
            try:
                with self.lifecycle:
                    if self.gate.exists():
                        ticket = json.loads(self.gate.read_text(encoding="utf-8"))["ticket"]
                        self.stop()
                        self.ticket = ticket
                    self.publish()
            except Exception:
                # Fail closed if lifecycle storage is unavailable.
                self.stop()
                print("Supervisor lifecycle storage unavailable", file=sys.stderr)

    def reader(self, process):
        try:
            for line in process.stdout:
                message = json.loads(line)
                with self.lock:
                    if process is not self.process:
                        return
                    if "id" in message and "method" not in message:
                        waiting = self.pending.pop(message["id"], None)
                        if waiting:
                            waiting.put(message)
                    elif "id" in message:
                        self.child_send(process, {"jsonrpc": "2.0", "id": message["id"],
                                                 "error": {"code": -32601, "message": "Client callbacks are not supported"}})
                    elif self.initialized:
                        self.send(message)
        except (OSError, ValueError):
            pass
        finally:
            # The reader owns stdout; close it here, never concurrently with read.
            process.stdout.close()
            with self.lock:
                if self.process is process:
                    self.fail_pending()
            # The next explicit start cleans up any crashed child. Never replay.

    def child_send(self, process, message):
        process.stdin.write(json.dumps(message) + "\n")
        process.stdin.flush()

    def fail_pending(self):
        for waiting in self.pending.values():
            waiting.put({"error": {"code": -32001, "message": "Backend interrupted; action outcome unknown. Inspect game state before any retry."}})
        self.pending.clear()
        self.client_ids.clear()

    def rpc(self, method, params=None, timeout=120, client_id=None):
        with self.lock:
            process = self.process
            if process is None or process.poll() is not None:
                raise RuntimeError("Backend stopped. Use timberborn_server_control start after live bereit")
            self.counter += 1
            ident = self.counter
            waiting = queue.Queue()
            self.pending[ident] = waiting
            if client_id is not None:
                self.client_ids[client_id] = ident
            try:
                self.child_send(process, {"jsonrpc": "2.0", "id": ident, "method": method, "params": params or {}})
            except OSError:
                self.pending.pop(ident, None)
                if client_id is not None:
                    self.client_ids.pop(client_id, None)
                raise RuntimeError("Backend write failed; action outcome unknown")
        try:
            return waiting.get(timeout=timeout)
        except queue.Empty:
            with self.lock:
                self.pending.pop(ident, None)
            raise RuntimeError("Backend response timed out; action outcome unknown; do not blindly retry")
        finally:
            with self.lock:
                if client_id is not None:
                    self.client_ids.pop(client_id, None)

    def stop(self):
        with self.lifecycle:
            with self.lock:
                process = self.process
                self.fail_pending()
                if process is not None:
                    if process.poll() is None:
                        process.terminate()
                    try:
                        process.wait(timeout=5)
                    except subprocess.TimeoutExpired:
                        process.kill()
                        process.wait(timeout=5)
                    self.process = None
                    process.stdin.close()
            # Reader cleanup needs self.lock, so join only after releasing it.
            if self.reader_thread is not None:
                self.reader_thread.join(timeout=5)
                if self.reader_thread.is_alive():
                    raise RuntimeError("Backend reader did not stop; lifecycle cleanup incomplete")
                self.reader_thread = None
            self.publish()

    def start(self, explicit=False):
        with self.lifecycle:
            if self.closed.is_set():
                raise RuntimeError("Supervisor connection is closing")
            if explicit and self.gate.exists():
                self.gate.unlink()
            if self.gate.exists():
                return
            if self.process is not None and self.process.poll() is None:
                return
            self.stop()
            self.process = subprocess.Popen(self.command, cwd=str(ROOT), env=self.env,
                                            stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                            stderr=None, text=True, encoding="utf-8", bufsize=1,
                                            creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
            self.reader_thread = threading.Thread(target=self.reader, args=(self.process,), daemon=True)
            self.reader_thread.start()
            try:
                init = dict(self.params or {"protocolVersion": "2024-11-05"})
                init["capabilities"] = {}
                init["clientInfo"] = {"name": "timberborn-supervisor", "version": "1"}
                result = self.rpc("initialize", init, 10)
                if "error" in result:
                    raise RuntimeError("Backend initialization failed")
                with self.lock:
                    self.child_send(self.process, {"jsonrpc": "2.0", "method": "notifications/initialized"})
                listing = self.rpc("tools/list", timeout=10)
                if "error" in listing or listing.get("result", {}).get("nextCursor"):
                    raise RuntimeError("Backend tool discovery failed or unexpected pagination")
                self.tools = listing["result"]["tools"]
                if any(t["name"] == CONTROL for t in self.tools):
                    raise RuntimeError("Lifecycle tool name collision")
                atomic_json(self.catalog, self.tools)
                if self.gate.exists():
                    self.stop()
                self.publish()
                if self.initialized:
                    self.send({"jsonrpc": "2.0", "method": "notifications/tools/list_changed"})
            except Exception:
                self.stop()
                raise

    def handle(self, request):
        ident = request.get("id")
        method = request.get("method")
        if "id" not in request:
            if method == "notifications/initialized":
                self.initialized = True
            elif method == "notifications/cancelled":
                with self.lock:
                    params = dict(request.get("params", {}))
                    target = self.client_ids.get(params.get("requestId"))
                    if target is not None and self.process is not None:
                        params["requestId"] = target
                        self.child_send(self.process, {"jsonrpc": "2.0", "method": method, "params": params})
            return
        try:
            params = request.get("params", {})
            if method == "initialize":
                self.params = params
                try:
                    self.start()
                except Exception:
                    print("Backend unavailable; lifecycle tool remains available", file=sys.stderr)
                result = {"protocolVersion": params["protocolVersion"], "capabilities": {"tools": {"listChanged": True}},
                          "serverInfo": {"name": "timberborn-supervisor", "version": "1"}}
            elif method == "ping":
                result = {}
            elif method == "tools/list":
                result = {"tools": [TOOL] + self.tools}
            elif method == "tools/call" and params.get("name") == CONTROL:
                action = params.get("arguments", {}).get("action")
                if action == "stop":
                    request_stop(self.runtime)
                elif action == "start":
                    self.start(explicit=True)
                elif action != "status":
                    raise ValueError("action must be stop, start or status")
                result = {"content": [{"type": "text", "text": json.dumps({"running": self.process is not None and self.process.poll() is None, "gate": self.gate.exists()})}]}
            else:
                response = self.rpc(method, params, client_id=ident)
                response.update(jsonrpc="2.0", id=ident)
                self.send(response)
                return
            self.send({"jsonrpc": "2.0", "id": ident, "result": result})
        except Exception as error:
            # Only controlled errors are returned; no paths/configuration contents.
            detail = str(error) if isinstance(error, (RuntimeError, ValueError)) else "Supervisor operation failed; see local diagnostics"
            self.send({"jsonrpc": "2.0", "id": ident, "error": {"code": -32000, "message": detail}})

    def close(self):
        self.closed.set()
        self.watcher.join(timeout=6)
        self.stop()
        self.registry.unlink(missing_ok=True)


def main():
    sys.stdin.reconfigure(encoding="utf-8")
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--config")
    parser.add_argument("--control", choices=["stop"])
    args = parser.parse_args()
    runtime = ROOT / ".local" / "codex-mcp-supervisor"
    if args.control:
        request_stop(runtime)
        print("[OK] Backend-Starts gesperrt; vorhandene Vorschaltprozesse haben den Stopp bestätigt.")
        return
    if not args.config or not Path(args.config).is_file():
        parser.error("--config must reference the existing native configuration")
    env = os.environ.copy()
    env.update(TIMBERBORN_BACKEND="native", TIMBERBORN_NATIVE_CONFIG=str(Path(args.config).resolve()))
    for flag in ("WRITES", "VALIDATION", "PLACEMENT", "LODGE_PLACEMENT", "SPEED_CONTROL", "STAFFING", "PRIORITIES", "AREAS", "BUILDING_SETTINGS", "BUILDING_PLACEMENT", "REMOVAL", "RESEARCH"):
        env["TIMBERBORN_ENABLE_" + flag] = "1"
    command = ["dotnet", str(ROOT / "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")]
    supervisor = Supervisor(command, runtime, env)
    try:
        for line in sys.stdin:
            request = json.loads(line)
            # Independent lifecycle calls must be able to interrupt a pending game call.
            if "id" in request:
                threading.Thread(target=supervisor.handle, args=(request,), daemon=True).start()
            else:
                supervisor.handle(request)
    finally:
        supervisor.close()


if __name__ == "__main__":
    main()
