using Nikse.SubtitleEdit.Core.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WhisperSampleExport;

/// <summary>
/// Exports audio clips as 16 kHz mono WAV files for Whisper fine-tuning.
///
/// Two modes:
///   IWaveformContextMenuPlugin – exports the waveform drag-selection as a
///                                single WAV (right-click on waveform).
///   ISubtitleEditPlugin        – exports each selected subtitle line as a
///                                separate WAV (Plugins menu).
/// </summary>
public class WhisperSampleExportPlugin : IWaveformContextMenuPlugin, ISubtitleEditPlugin
{
    // ── IWaveformContextMenuPlugin ────────────────────────────────────────

    string IWaveformContextMenuPlugin.Name => "WhisperSampleExport";
    string IWaveformContextMenuPlugin.MenuItemText => "Export as Whisper sample...";

    bool IWaveformContextMenuPlugin.CanExecute(string videoFileName, double startSeconds, double endSeconds)
        => !string.IsNullOrEmpty(videoFileName) && endSeconds > startSeconds;

    async Task IWaveformContextMenuPlugin.ExecuteAsync(
        string videoFileName, double startSeconds, double endSeconds, string outputFolder)
    {
        var startMs = (long)(startSeconds * 1000);
        var endMs = (long)(endSeconds * 1000);
        var outputWav = Path.Combine(outputFolder, $"sample_{startMs:D8}_{endMs:D8}.wav");
        await RunFfmpeg(videoFileName, startSeconds, endSeconds - startSeconds, outputWav);
    }

    // ── ISubtitleEditPlugin ───────────────────────────────────────────────

    string ISubtitleEditPlugin.Name => "WhisperSampleExport";
    string ISubtitleEditPlugin.MenuItemText => "Export selected lines as Whisper samples...";
    bool ISubtitleEditPlugin.NeedsOutputFolder => true;

    async Task ISubtitleEditPlugin.ExecuteAsync(SubtitleEditPluginContext context)
    {
        if (string.IsNullOrEmpty(context.VideoFileName) ||
            context.SelectedParagraphs.Count == 0 ||
            string.IsNullOrEmpty(context.OutputFolder))
        {
            return;
        }

        foreach (var p in context.SelectedParagraphs)
        {
            var startMs = (long)p.StartTime.TotalMilliseconds;
            var outputWav = Path.Combine(
                context.OutputFolder,
                $"sample_{p.Number:D5}_{startMs:D8}.wav");

            await RunFfmpeg(
                context.VideoFileName,
                p.StartTime.TotalSeconds,
                p.DurationTotalSeconds,
                outputWav);
        }
    }

    // ── shared helpers ────────────────────────────────────────────────────

    private static async Task RunFfmpeg(
        string videoFileName, double startSeconds, double durationSeconds, string outputWav)
    {
        var ffmpeg = Configuration.Settings.General.FFmpegLocation;
        if (string.IsNullOrEmpty(ffmpeg) ||
            (!Configuration.IsRunningOnWindows && !File.Exists(ffmpeg)))
        {
            ffmpeg = "ffmpeg";
        }

        var start = $"{startSeconds:0.000}".Replace(",", ".");
        var duration = $"{durationSeconds:0.000}".Replace(",", ".");
        var args = $"-y -ss {start} -t {duration} -i \"{videoFileName}\" -vn -ar 16000 -ac 1 \"{outputWav}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await Task.Run(() => process.WaitForExit());
    }
}
