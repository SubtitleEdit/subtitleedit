using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Resolves the folder per-line generated audio files are written to.
/// </summary>
/// <remarks>
/// Every engine's <c>Speak</c> used to ignore its <c>outputFolder</c> parameter and dumped
/// GUID-named wavs into its own data folder (<c>.../TextToSpeech/&lt;Engine&gt;/</c>) — files
/// that were never cleaned up, so the folders filled with hundreds of files (#13332). The
/// caller now passes the session's generation folder (system temp by default, or the
/// user-configured folder from TTS advanced settings); <see cref="Resolve"/> falls back to the
/// engine's own folder only as a defensive default for direct callers that pass an empty
/// folder. The resolved folder is created (best-effort) so engines can write to it directly.
/// </remarks>
public static class TtsOutputFolder
{
    public static string Resolve(string requestedFolder, string engineDefaultFolder)
    {
        if (string.IsNullOrWhiteSpace(requestedFolder))
        {
            return engineDefaultFolder;
        }

        try
        {
            Directory.CreateDirectory(requestedFolder);
        }
        catch
        {
            // Best-effort: if the folder can't be created, the engine's write below fails with
            // a descriptive error anyway. Never throw from resolution.
        }

        return requestedFolder;
    }
}
