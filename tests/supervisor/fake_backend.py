"""Synthetic stdio peer; never loads the game or native configuration."""
import json
import os
import sys

initialized = False
for line in sys.stdin:
    request = json.loads(line)
    method = request["method"]
    if method == "notifications/initialized":
        initialized = True
        continue
    if "id" not in request:
        continue
    if method == "initialize":
        result = {"protocolVersion": request["params"]["protocolVersion"], "capabilities": {"tools": {}}, "serverInfo": {"name": "synthetic", "version": "1"}}
    elif not initialized:
        raise RuntimeError("Handshake not replayed")
    elif method == "tools/list":
        result = {"tools": [{"name": "synthetic_read", "inputSchema": {"type": "object"}}]}
    elif method == "tools/call":
        name = request["params"]["name"]
        if name == "pending":
            continue
        if name == "crash":
            os._exit(3)
        result = {"content": [{"type": "text", "text": "synthetic"}]}
    else:
        result = {}
    print(json.dumps({"jsonrpc": "2.0", "id": request["id"], "result": result}), flush=True)
