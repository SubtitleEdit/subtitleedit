using Nikse.SubtitleEdit.Core.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WhisperSampleExport;

public class WhisperSampleExportPlugin : IWaveformContextMenuPlugin
{
    public string Name => "WhisperSampleExport";
    public string MenuItemText => "Export as Whisper sample...";

    public bool CanExecute(string videoFileName, double startSeconds, double endSeconds)
        => !string.IsNullOrEmpty(videoFileName) && endSeconds > startSeconds;

    public async Task ExecuteAsync(string videoFileName, double startSeconds, double endSeconds, string outputFolder)
    {
        var startMs = (long)(startSeconds * 1000);
        var endMs = (long)(endSeconds * 1000);
        var outputWav = Path.Combine(outputFolder, $"sample_{startMs:D8}_{endMs:D8}.wav");

        var ffmpeg = Configuration.Settings.General.FFmpegLocation;
        if (string.IsNullOrEmpty(ffmpeg) || (!Configuration.IsRunningOnWindows && !File.Exists(ffmpeg)))
            ffmpeg = "ffmpeg";

        var start = $"{startSeconds:0.000}".Replace(",", ".");
        var duration = $"{(endSeconds - startSeconds):0.000}".Replace(",", ".");
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
