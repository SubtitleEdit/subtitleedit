using Nikse.SubtitleEdit.Logic.LlamaCpp;

namespace SeConv.Core;

/// <summary>
/// Locates a local llama-server binary for the llama.cpp translate/OCR engines without
/// downloading anything: the Subtitle Edit data folders are probed first (so an install done
/// via the SE GUI is picked up automatically), then the system PATH.
/// </summary>
internal static class LlamaCppLocal
{
    /// <summary>
    /// Resolves llama-server and points <see cref="LlamaCppServerManager"/> at it (folder or
    /// executable override). Throws with download instructions when nothing is found;
    /// <paramref name="guiDownloadHint"/> names the SE window that installs llama.cpp
    /// (e.g. "Auto-translate &gt; llama.cpp") and <paramref name="urlOption"/> the CLI option
    /// that skips the local auto-start (e.g. "--translate-url").
    /// </summary>
    public static void EnsureServerBinary(string guiDownloadHint, string urlOption)
    {
        if (!TryEnsureServerBinary())
        {
            throw new InvalidOperationException(
                $"llama-server not found. Either: download llama.cpp in Subtitle Edit ({guiDownloadHint}), " +
                "install it on PATH (e.g. `brew install llama.cpp` or `winget install ggml.llamacpp`), " +
                $"or point {urlOption} at an already-running llama-server.");
        }
    }

    /// <summary>Same as <see cref="EnsureServerBinary"/> but returns false instead of throwing.</summary>
    public static bool TryEnsureServerBinary()
    {
        // Folder candidates in priority order: portable SE (seconv sits next to SubtitleEdit,
        // data folder = exe folder), then the installed GUI's data folder.
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "llama.cpp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "llama.cpp"),
        };

        var exeName = OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server";
        var folder = candidates.FirstOrDefault(c => File.Exists(Path.Combine(c, exeName)));
        if (folder != null)
        {
            LlamaCppServerManager.FolderOverride = folder;
            return true;
        }

        // No SE-managed install; fall back to llama.cpp from the system PATH
        // (brew install llama.cpp / winget install ggml.llamacpp / apt).
        var pathExe = FindOnPath(exeName);
        if (pathExe == null)
        {
            return false;
        }

        LlamaCppServerManager.ExecutableOverride = pathExe;
        // Models still live in an SE data folder; prefer one that exists.
        LlamaCppServerManager.FolderOverride = candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
        return true;
    }

    private static string? FindOnPath(string exeName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        foreach (var dir in pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), exeName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
