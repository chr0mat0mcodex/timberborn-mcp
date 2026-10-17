namespace Timberborn.McpServer;

// Caller holds the journal lease. Retry file publication only, never game actions.
public static class CheckpointPublication
{
    public static void Replace(string temp, string path, Action<string, string>? replaceFile = null)
    {
        byte[] previous = File.ReadAllBytes(path), next = File.ReadAllBytes(temp);
        for (int attempt = 0; ; attempt++) {
            try {
                if (replaceFile is null) File.Replace(temp, path, null);
                else replaceFile(temp, path);
                return;
            } catch (IOException ex) when (ex.HResult is unchecked((int)0x80070020) or
                unchecked((int)0x80070021) or unchecked((int)0x80070497)) {
                // Windows sharing/lock violation or unable-to-remove-replaced (1175).
                // Read errors, missing files and changed bytes fail closed. Do not
                // guess whether an exchange happened from the exception alone.
                if (attempt < 3) Thread.Sleep(25 << attempt); // At most 175 ms total.
                if (!File.Exists(path)) throw;
                byte[] current = File.ReadAllBytes(path);
                if (current.AsSpan().SequenceEqual(next)) return;
                if (attempt >= 3 || !current.AsSpan().SequenceEqual(previous) ||
                    !File.Exists(temp) || !File.ReadAllBytes(temp).AsSpan().SequenceEqual(next)) throw;
            }
        }
    }

}
