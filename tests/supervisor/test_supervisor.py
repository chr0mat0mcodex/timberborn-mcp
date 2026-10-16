import importlib.util
import json
from pathlib import Path
import queue
import sys
import subprocess
import tempfile
import threading
import time
import unittest
from unittest.mock import Mock, patch

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

    def test_restart_storage_errors_identify_phase_and_do_not_retry(self):
        self.initialize()
        self.assertIn("result", self.control("stop"))
        original = module.atomic_json
        failures = []

        def fail_catalog(path, value):
            if path == self.server.catalog:
                failures.append(path)
                raise PermissionError(13, "private_path_must_not_escape")
            return original(path, value)

        with patch.object(module, "atomic_json", side_effect=fail_catalog), \
                patch.object(module, "print") as diagnostic:
            result = self.control("start")
        self.assertIn("start_catalog_write", result["error"]["message"])
        self.assertIn("errno=13", result["error"]["message"])
        self.assertNotIn("private_path", json.dumps(result))
        diagnostic.assert_called()
        self.assertEqual(1, len(failures))
        self.assertIsNone(self.server.process)
        self.assertIsNone(self.server.reader_thread)

    def test_start_publish_failure_is_not_retried_during_cleanup(self):
        self.server.closed.set()
        self.server.watcher.join(timeout=3)
        self.assertFalse(self.server.watcher.is_alive())
        # Suppress only the watcher; start still observes an open connection.
        with patch.object(self.server.closed, "is_set", return_value=False), \
                patch.object(self.server, "publish", side_effect=[None, PermissionError(13, "private_status")]) as publish, \
                patch.object(module, "print"):
            result = self.control("start")
        self.assertIn("start_status_write", result["error"]["message"])
        self.assertNotIn("private_status", json.dumps(result))
        self.assertEqual(2, publish.call_count)  # Initial stop + final start; no cleanup write.
        self.assertIsNone(self.server.process)
        self.assertIsNone(self.server.reader_thread)

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

    @unittest.skipUnless(sys.platform == "win32", "Windows cross-process file mutex")
    def test_atomic_replace_waits_for_status_reader_in_other_process(self):
        # The fixture supervisor publishes instance-*.json concurrently. Its
        # short-lived atomic-write files are not leftovers from this test.
        atomic_runtime = self.runtime / "atomic-replace"
        atomic_runtime.mkdir()
        path = atomic_runtime / "shared-status.json"
        module.atomic_json(path, {"ticket": "before"})
        code = (
            "import importlib.util,sys; from pathlib import Path; "
            "spec=importlib.util.spec_from_file_location('supervisor',sys.argv[1]); "
            "m=importlib.util.module_from_spec(spec); spec.loader.exec_module(m); "
            "print('ready',flush=True); "
            "m.atomic_json(Path(sys.argv[2]),{'ticket':'after'})"
        )
        child = None
        try:
            with module.shared_text_reader(path) as reader:
                child = subprocess.Popen([sys.executable, "-B", "-c", code,
                                          str(ROOT / "scripts/codex-mcp-supervisor.py"), str(path)],
                                         stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True,
                                         creationflags=subprocess.CREATE_NO_WINDOW)
                self.assertEqual("ready", child.stdout.readline().strip())
                with self.assertRaises(subprocess.TimeoutExpired):
                    child.communicate(timeout=0.2)
                self.assertEqual("before", json.load(reader)["ticket"])
            _, error = child.communicate(timeout=5)
            self.assertEqual(0, child.returncode, error)
            self.assertEqual("after", module.read_json(path)["ticket"])
            self.assertEqual([], list(atomic_runtime.glob("*.tmp")))
        finally:
            if child is not None:
                if child.poll() is None:
                    child.kill()
                child.communicate(timeout=5)

    def test_protected_pid_requires_positive_reuse_evidence(self):
        with patch.object(module, "windows_process_birth", return_value=200):
            self.assertTrue(module.protected_pid_was_reused(123, 100))
            self.assertFalse(module.protected_pid_was_reused(123, 200))
            self.assertFalse(module.protected_pid_was_reused(123, 300))
        with patch.object(module, "windows_process_birth") as birth:
            self.assertFalse(module.protected_pid_was_reused(123, None))
            birth.assert_not_called()
        with patch.object(module, "windows_process_birth", side_effect=RuntimeError("unavailable")):
            with self.assertRaises(RuntimeError):
                module.protected_pid_was_reused(123, 100)

    def test_storage_failure_stops_backend_without_second_publish(self):
        self.initialize()
        child = self.server.process
        self.server.closed.set()
        self.server.watcher.join(timeout=3)
        self.assertFalse(self.server.watcher.is_alive())
        closed = Mock()
        closed.wait.side_effect = [False, True]
        with patch.object(self.server, "closed", closed), \
                patch.object(self.server, "publish", side_effect=PermissionError("synthetic")) as publish, \
                patch.object(module, "print") as diagnostic:
            self.server.watch()
            self.assertEqual(1, publish.call_count)
            diagnostic.assert_called()
        self.assertIsNone(self.server.process)
        self.assertIsNotNone(child.poll())
        self.assertTrue(child.stdin.closed)
        self.assertTrue(child.stdout.closed)

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

    def test_close_stops_child_without_republishing_registry(self):
        self.initialize()
        child = self.server.process
        self.server.closed.set()
        self.server.watcher.join(timeout=6)
        self.assertFalse(self.server.watcher.is_alive())
        with patch.object(self.server, "publish", side_effect=PermissionError("synthetic")) as publish:
            self.server.close()
            publish.assert_not_called()
        self.assertIsNone(self.server.process)
        self.assertIsNotNone(child.poll())
        self.assertTrue(child.stdin.closed)
        self.assertTrue(child.stdout.closed)
        self.assertFalse(self.server.registry.exists())
        self.assertEqual([], list(self.runtime.glob("*.tmp")))

    def test_close_retains_registry_when_backend_stop_fails(self):
        self.server.closed.set()
        self.server.watcher.join(timeout=6)
        self.assertFalse(self.server.watcher.is_alive())
        with patch.object(self.server, "stop", side_effect=RuntimeError("synthetic stop failure")) as stop:
            with self.assertRaisesRegex(RuntimeError, "synthetic stop failure"):
                self.server.close()
            stop.assert_called_once_with(publish=False)
        self.assertTrue(self.server.registry.exists())


if __name__ == "__main__":
    unittest.main()
