import importlib.util
import json
from pathlib import Path
import queue
import sys
import tempfile
import threading
import time
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("supervisor", ROOT / "scripts/codex-mcp-supervisor.py")
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class LifecycleTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.runtime = Path(self.temp.name)
        self.output = queue.Queue()
        self.server = module.Supervisor([sys.executable, "-u", str(Path(__file__).with_name("fake_backend.py"))], self.runtime, emit=self.output.put)
        self.addCleanup(self.temp.cleanup)
        self.addCleanup(self.server.close)
        self.next_id = 0

    def call(self, method, params=None):
        self.next_id += 1
        ident = self.next_id
        self.server.handle({"jsonrpc": "2.0", "id": ident, "method": method, "params": params or {}})
        while True:
            message = self.output.get(timeout=3)
            if message.get("id") == ident:
                return message

    def initialize(self):
        result = self.call("initialize", {"protocolVersion": "2024-11-05"})
        self.assertIn("result", result)
        self.server.handle({"method": "notifications/initialized"})

    def control(self, action):
        return self.call("tools/call", {"name": module.CONTROL, "arguments": {"action": action}})

    def test_stop_restart_preserves_frontend_and_replays_handshake(self):
        self.initialize()
        first = self.server.process
        names = [t["name"] for t in self.call("tools/list")["result"]["tools"]]
        self.assertEqual([module.CONTROL, "synthetic_read"], names)
        self.assertIn("result", self.control("stop"))
        self.assertIsNotNone(first.poll())
        self.assertTrue(first.stdin.closed)
        self.assertTrue(first.stdout.closed)
        self.assertIsNone(self.server.reader_thread)
        self.assertEqual(names, [t["name"] for t in self.call("tools/list")["result"]["tools"]])
        self.assertIn("error", self.call("tools/call", {"name": "synthetic_read"}))
        self.assertIn("result", self.control("start"))
        self.assertIsNot(first, self.server.process)
        self.assertIn("result", self.call("tools/call", {"name": "synthetic_read"}))

    def test_gate_suppresses_initial_start_until_explicit_live_start(self):
        module.request_stop(self.runtime)
        self.initialize()
        self.assertIsNone(self.server.process)
        self.assertIn("result", self.control("status"))
        self.assertIsNone(self.server.process)
        self.control("start")
        self.assertIsNotNone(self.server.process)
        self.assertFalse(self.server.gate.exists())

    def test_pending_action_is_failed_without_replay(self):
        self.initialize()
        result = queue.Queue()
        thread = threading.Thread(target=lambda: result.put(self.server.rpc("tools/call", {"name": "pending"})))
        thread.start()
        deadline = time.monotonic() + 3
        while not self.server.pending and time.monotonic() < deadline:
            time.sleep(0.01)
        self.assertTrue(self.server.pending)
        self.server.stop()
        thread.join(timeout=3)
        self.assertFalse(thread.is_alive())
        self.assertIn("outcome unknown", result.get(timeout=1)["error"]["message"])
        self.server.start(explicit=True)
        self.assertFalse(self.server.pending)

    def test_crash_reports_unknown_and_does_not_auto_restart(self):
        self.initialize()
        child = self.server.process
        result = self.call("tools/call", {"name": "crash"})
        self.assertIn("outcome unknown", result["error"]["message"])
        child.wait(timeout=3)
        self.assertIs(child, self.server.process)
        self.assertIn("result", self.control("start"))
        self.assertTrue(child.stdin.closed)
        self.assertTrue(child.stdout.closed)

    def test_gate_stops_multiple_instances(self):
        other = module.Supervisor(self.server.command, self.runtime, emit=lambda message: None)
        try:
            self.initialize()
            other.start()
            module.request_stop(self.runtime)
            self.assertIsNone(self.server.process)
            self.assertIsNone(other.process)
        finally:
            other.close()

    def test_unacknowledged_live_registry_blocks_gate(self):
        import os
        path = self.runtime / "instance-unresponsive.json"
        module.atomic_json(path, {"pid": os.getpid(), "running": True, "ticket": None})
        with self.assertRaisesRegex(RuntimeError, "not acknowledged"):
            module.request_stop(self.runtime, timeout=0.1)
        path.unlink()

    @unittest.skipUnless(sys.platform == "win32", "Windows PID identity")
    def test_reused_pid_does_not_block_gate(self):
        import os
        path = self.runtime / "instance-reused.json"
        module.atomic_json(path, {"pid": os.getpid(), "heartbeat": 0, "running": True, "ticket": None})
        module.request_stop(self.runtime)
        self.assertFalse(module.alive(os.getpid(), observed_at=0))
        self.assertTrue(module.alive(os.getpid(), observed_at=time.time()))

    def test_cached_tools_available_on_new_frontend_while_gate_closed(self):
        self.initialize()
        module.request_stop(self.runtime)
        other = module.Supervisor(self.server.command, self.runtime, emit=lambda message: None)
        try:
            other.start()
            self.assertIsNone(other.process)
            self.assertEqual(self.server.tools, other.tools)
            self.assertEqual("synthetic_read", other.tools[0]["name"])
        finally:
            other.close()


if __name__ == "__main__":
    unittest.main()
