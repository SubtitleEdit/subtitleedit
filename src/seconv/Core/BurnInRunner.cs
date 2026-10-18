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

    /// <summary>Names accepted by <c>--burn-in-effects</c>; one per effect in the UI's Burn in &gt; Effects.</summary>
    public static readonly string[] EffectNames =
    [
        "fade", "font-kerning", "font-size-change", "scroll-up", "scroll-down",
        "rotate-in", "tilt-bounce", "font-size-bounce-in", "fix-rtl",
    ];

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
    /// Applies the effects to every line, the way the UI's burn-in does (BurnInEffectItem.ApplyEffect
    /// in the UI project): each effect in turn, tags prepended. Call after the ASSA styles and
    /// PlayResX/Y are final - the size effects scale from the line's style font size and the
    /// scroll effects position against the script resolution.
    /// </summary>
    public static void ApplyEffects(Subtitle subtitle, IReadOnlyList<string> effects)
    {
        var header = subtitle.Header ?? string.Empty;
        var width = ParsePlayRes("PlayResX", header, AdvancedSubStationAlpha.DefaultWidth);
        var height = ParsePlayRes("PlayResY", header, AdvancedSubStationAlpha.DefaultHeight);

        foreach (var p in subtitle.Paragraphs)
        {
            var style = AdvancedSubStationAlpha.GetSsaStyle(string.IsNullOrEmpty(p.Extra) ? "Default" : p.Extra, header);
            var fontSize = (int)style.FontSize;
            var durationMs = (int)p.Duration.TotalMilliseconds;
            foreach (var effect in effects)
            {
                p.Text = ApplyEffect(effect, p.Text, width, height, fontSize, durationMs);
            }
        }
    }

    private static int ParsePlayRes(string tag, string header, int fallback)
    {
        var value = AdvancedSubStationAlpha.GetTagValueFromHeader(tag, "[Script Info]", header);
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var v) && v > 0 ? v : fallback;
    }

    internal static string ApplyEffect(string effect, string text, int width, int height, int fontSize, int durationMs)
    {
        const int step = 250;
        if (effect.StartsWith("fade", StringComparison.Ordinal))
        {
            var ms = effect.Contains(':') ? effect[5..] : "250";
            return $"{{\\fad({ms},{ms})}}{text}";
        }

        return effect switch
        {
            "font-kerning" => $"{{\\t(\\fsp4)}}{text}",
            "font-size-change" => $"{{\\t(\\fs{(int)(fontSize * 1.2)})}}{text}",
            "scroll-up" => $"{{\\move({width / 2},{height - 60},{width / 2},{height - 80})}}{text}",
            "scroll-down" => $"{{\\move({width / 2},{height - 80},{width / 2},{height - 60})}}{text}",
            "rotate-in" => $"{{\\frx-30\\t(0,{durationMs / 2},\\frx0)}}{text}",
            "tilt-bounce" => $"{{\\frz-10\\t(0,{step},\\frz10)\\t({step},{step * 2},\\frz-6)\\t({step * 2},{step * 3},\\frz4)" +
                             $"\\t({step * 3},{step * 4},\\frz-2)\\t({step * 4},{step * 5},\\frz1)\\t({step * 5},{durationMs},\\frz0)}}{text}",
            "font-size-bounce-in" => $"{{\\fs{(int)(fontSize * 0.5)}\\t(0,{step},\\fs{(int)(fontSize * 1.2)})" +
                                     $"\\t({step},{step * 2},\\fs{(int)(fontSize * 0.9)})\\t({step * 2},{step * 3},\\fs{(int)(fontSize * 1.05)})" +
                                     $"\\t({step * 3},{durationMs},\\fs{fontSize})}}{text}",
            "fix-rtl" => Utilities.FixRtlViaUnicodeChars(text),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "unknown burn-in effect"),
        };
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
