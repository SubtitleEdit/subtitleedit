using Nikse.SubtitleEdit.Logic.Download;
using System.IO;
using System.Threading;
using Nikse.SubtitleEdit.UiLogic;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Logic.LlamaCpp;

/// <summary>
/// Update check for the installed llama-server binary. Lives in the UI project (not with
/// <see cref="LlamaCppServerManager"/> in libuilogic) because it depends on
/// <see cref="DownloadHashManager"/>, which only the UI's download services use.
/// </summary>
public static class LlamaCppUpdateStatus
{
    // Hashing a few hundred MB of llama-server takes long enough to stutter a combo box that asks
    // for the status every time a row is realised, so remember the answer for a given file. The
    // identity is path + size + write time: a re-download changes at least one of them.
    private static readonly Lock CacheLock = new Lock();
    private static string? _cachedFileIdentity;
    private static DownloadHashManager.UpdateStatus _cachedStatus;

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
            var info = new FileInfo(exe);
            var identity = $"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}|{key}";
            lock (CacheLock)
            {
                if (_cachedFileIdentity == identity)
                {
                    return _cachedStatus;
                }
            }

            var hash = Sha256Util.ComputeSha256(exe);
            var status = DownloadHashManager.GetStatus(key, hash);
            lock (CacheLock)
            {
                _cachedFileIdentity = identity;
                _cachedStatus = status;
            }

            return status;
        }
        catch
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }
    }
}
