using Nikse.SubtitleEdit.Logic.Download;
using System.IO;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.LlamaCpp;

/// <summary>
/// Update check for the installed llama-server binary. Lives in the UI project (not with
/// <see cref="LlamaCppServerManager"/> in libuilogic) because it depends on
/// <see cref="DownloadHashManager"/>, which only the UI's download services use.
/// </summary>
public static class LlamaCppUpdateStatus
{
    /// <summary>
    /// Returns the update status of the installed llama-server binary relative to the version
    /// pinned in <see cref="LlamaCppDownloadService"/>. The check hashes the executable on disk
    /// directly (no install-time sidecar needed) because <see cref="DownloadHashManager.LlamaCpp"/>
    /// tracks SHA-256s of the unpacked llama-server binary per OS/arch. Returns
    /// <see cref="DownloadHashManager.UpdateStatus.Unknown"/> when nothing is installed, the
    /// platform key cannot be resolved, or the on-disk hash does not match any known release —
    /// so callers do not false-positive an update prompt.
    /// </summary>
    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var exe = LlamaCppServerManager.GetExecutable();
        if (!File.Exists(exe))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var key = DownloadHashManager.ResolveLlamaCppExecutableKey();
        if (string.IsNullOrEmpty(key))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        try
        {
            var hash = Sha256Util.ComputeSha256(exe);
            return DownloadHashManager.GetStatus(key, hash);
        }
        catch
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }
    }
}
