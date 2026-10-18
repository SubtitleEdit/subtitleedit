using System.Diagnostics;
using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

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

    /// <summary>Names accepted by <c>--burn-in-effects</c>.</summary>
    public static readonly string[] EffectNames = ["fade", "font-kerning", "font-size-change"];

    /// <summary>
    /// "fade:500, font-kerning" -> ["fade:500", "font-kerning"]. A bare "fade" is 250 ms in and
    /// out, like the UI's "Fade in/out".
    /// </summary>
    public static List<string> ParseEffects(string? value)
    {
        var effects = new List<string>();
        if (string.IsNullOrWhiteSpace(value))
        {
            return effects;
        }

        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var effect = item.ToLowerInvariant();
            var name = effect.Split(':')[0];
            if (!EffectNames.Contains(name))
            {
                throw new FormatException($"Unknown --burn-in-effects value '{item}'. Valid: {string.Join(", ", EffectNames)} (fade:<ms> sets the fade length).");
            }

            if (name == "fade" && effect.Contains(':') &&
                !(int.TryParse(effect[5..], NumberStyles.None, CultureInfo.InvariantCulture, out var ms) && ms > 0))
            {
                throw new FormatException($"--burn-in-effects: fade length must be a positive number of milliseconds (got '{item}').");
            }

            effects.Add(effect);
        }

        return effects;
    }

    /// <summary>
    /// Prepends the effect override tags to every line, the way the UI's burn-in does
    /// (BurnInEffectItem.ApplyEffect in the UI project). Call after the ASSA styles are final:
    /// "font-size-change" grows each line to 120% of its style's font size.
    /// </summary>
    public static void ApplyEffects(Subtitle subtitle, IReadOnlyList<string> effects)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            foreach (var effect in effects)
            {
                p.Text = EffectTag(effect, subtitle.Header, p.Extra) + p.Text;
            }
        }
    }

    private static string EffectTag(string effect, string? header, string? styleName)
    {
        if (effect.StartsWith("fade", StringComparison.Ordinal))
        {
            var ms = effect.Contains(':') ? effect[5..] : "250";
            return $"{{\\fad({ms},{ms})}}";
        }

        if (effect == "font-kerning")
        {
            return "{\\t(\\fsp4)}";
        }

        var style = AdvancedSubStationAlpha.GetSsaStyle(string.IsNullOrEmpty(styleName) ? "Default" : styleName, header ?? string.Empty);
        return $"{{\\t(\\fs{(int)(style.FontSize * 1.2m)})}}";
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
