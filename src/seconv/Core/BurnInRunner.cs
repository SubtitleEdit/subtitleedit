using System.Diagnostics;
using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// <c>--burn-in-video</c>: burns the ASSA file seconv just wrote into a video with ffmpeg's
/// <c>ass</c> filter - the same filter the UI's Video &gt; Burn in uses. Styling comes from the
/// .ass itself (<c>--assa-style-file</c>, <c>--resolution</c>).
/// </summary>
internal static class BurnInRunner
{
    /// <summary>The UI's burn-in defaults (libx264, CRF 23, audio copied).</summary>
    public const string DefaultFfmpegArgs = "-c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -c:a copy";

    /// <summary>"out/movie.ass" + "in/movie.mp4" -> "out/movie_burn-in.mp4".</summary>
    public static string GetOutputFileName(string assaFileName, string videoFileName)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(assaFileName)) ?? string.Empty;
        return Path.Combine(folder, Path.GetFileNameWithoutExtension(assaFileName) + "_burn-in" + Path.GetExtension(videoFileName));
    }

    public static string BuildArguments(string videoFileName, string outputFileName, string? ffmpegArgs)
    {
        // The subtitle is referenced by a fixed relative name from the working folder: an
        // absolute path inside the filter graph would need ffmpeg's filter escaping for ':',
        // '\', ',' and quotes (Windows drive letters alone break it).
        var args = string.IsNullOrWhiteSpace(ffmpegArgs) ? DefaultFfmpegArgs : ffmpegArgs.Trim();
        return $"-hide_banner -nostdin -y -i \"{videoFileName}\" -vf ass=subtitle.ass {args} -movflags +faststart \"{outputFileName}\"";
    }

    public static string Run(string assaFileName, string videoFileName, string? ffmpegArgs, bool overwrite)
    {
        var outputFileName = GetOutputFileName(assaFileName, videoFileName);
        if (string.Equals(Path.GetFullPath(outputFileName), Path.GetFullPath(videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Burn-in output would replace the input video: {outputFileName}");
        }

        if (File.Exists(outputFileName) && !overwrite)
        {
            throw new InvalidOperationException($"Output file already exists: {outputFileName}. Pass --overwrite to replace it.");
        }

        var workFolder = Path.Combine(Path.GetTempPath(), "seconv-burn-in-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workFolder);
        try
        {
            File.Copy(assaFileName, Path.Combine(workFolder, "subtitle.ass"));

            var ffmpeg = Configuration.Settings.General.FFmpegLocation;
            if (string.IsNullOrEmpty(ffmpeg) || !File.Exists(ffmpeg))
            {
                ffmpeg = "ffmpeg"; // from PATH
            }

            using var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpeg,
                    Arguments = BuildArguments(Path.GetFullPath(videoFileName), Path.GetFullPath(outputFileName), ffmpegArgs),
                    WorkingDirectory = workFolder,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            var log = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                TryDelete(outputFileName);
                var tail = string.Join(Environment.NewLine, log.Split('\n', StringSplitOptions.RemoveEmptyEntries).TakeLast(5));
                throw new InvalidOperationException($"ffmpeg burn-in failed (exit code {process.ExitCode}):{Environment.NewLine}{tail}");
            }

            return outputFileName;
        }
        finally
        {
            try
            {
                Directory.Delete(workFolder, true);
            }
            catch
            {
                // temp folder; not worth failing the conversion over
            }
        }
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }
}
