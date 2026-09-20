using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Makes sure an ffmpeg is at hand before a feature that runs it opens: the configured one, the
/// one Subtitle Edit downloaded, one on the PATH (issue #11760) or in /usr/local/bin - failing
/// those, the download is offered on Windows and macOS, and elsewhere the message says what to
/// install instead of failing silently (issue #14390).
/// </summary>
public static class FfmpegRequirement
{
    /// <param name="owner">Owner of the question and message boxes.</param>
    /// <param name="downloadFfmpeg">Shows the download dialog; returns the downloaded ffmpeg's path, or empty.</param>
    /// <param name="showStatus">Where to say that ffmpeg was downloaded and installed, if anywhere.</param>
    public static async Task<bool> EnsureAsync(Window owner, Func<Task<string>> downloadFfmpeg, Action<string>? showStatus = null)
    {
        if (FfmpegHelper.IsFfmpegInstalled())
        {
            return true;
        }

        if (File.Exists(DownloadFfmpegViewModel.GetFfmpegFileName()))
        {
            FfmpegHelper.SetFfmpegPath(DownloadFfmpegViewModel.GetFfmpegFileName());
            return true;
        }

        var systemFfmpeg = FfmpegHelper.GetSystemFfmpegPath();
        if (!string.IsNullOrEmpty(systemFfmpeg))
        {
            FfmpegHelper.SetFfmpegPath(systemFfmpeg);
            return true;
        }

        if (!OperatingSystem.IsWindows() && File.Exists("/usr/local/bin/ffmpeg"))
        {
            FfmpegHelper.SetFfmpegPath("/usr/local/bin/ffmpeg");
            return true;
        }

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            var answer = await MessageBox.Show(
                owner,
                Se.Language.Main.DownloadFfmpegTitle,
                Se.Language.Main.DownloadFfmpegQuestion,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var ffmpegFileName = await downloadFfmpeg();
            if (!string.IsNullOrEmpty(ffmpegFileName))
            {
                FfmpegHelper.SetFfmpegPath(ffmpegFileName);
                showStatus?.Invoke(string.Format(Se.Language.Main.FfmpegDownloadedAndInstalledToX, ffmpegFileName));
                return true;
            }

            return false;
        }

        await MessageBox.Show(
            owner,
            Se.Language.Title,
            Se.Language.Main.FfmpegNotFoundInstallHint,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        return false;
    }
}
