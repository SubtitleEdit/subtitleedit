using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>How the burned-in lines are removed (the order of the method combo box).</summary>
public enum BurnedInMethod
{
    Inpaint,
    BlackBox,
    Blur,
    Delogo,
}

public sealed class BurnedInRemovalOptions
{
    public BurnedInMethod Method { get; init; }

    public string VideoFileName { get; init; } = string.Empty;
    public string OutputFileName { get; init; } = string.Empty;
    public int VideoWidth { get; init; }
    public int VideoHeight { get; init; }
    public double FrameRate { get; init; }
    public double DurationSeconds { get; init; }
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public int RegionWidth { get; init; }
    public int RegionHeight { get; init; }

    /// <summary>OCR line times (start, end).</summary>
    public IReadOnlyList<(TimeSpan Start, TimeSpan End)> Lines { get; init; } = Array.Empty<(TimeSpan, TimeSpan)>();

    /// <summary>Sample rate the OCR ran at - sets how much each line span is padded.</summary>
    public double OcrFramesPerSecond { get; init; } = 5;

    public int BrightnessMinimum { get; init; } = 190;
    public bool RequireOutline { get; init; } = true;
    public int Grow { get; init; } = 6;

    /// <summary>Extra mask width in px on the left and right of each line.</summary>
    public int ExtraWidth { get; init; } = 5;
    public bool Temporal { get; init; } = true;

    /// <summary>Box blur radius in px (blur method).</summary>
    public int BlurStrength { get; init; } = 20;

    public int Crf { get; init; } = 18;
    public string CrispEmbedExecutable { get; init; } = string.Empty;
    public string ModelFileName { get; init; } = string.Empty;
}

/// <summary>
/// Removes burned-in subtitles from a video with CrispEmbed's MI-GAN inpainting.
/// <list type="number">
/// <item>Pass 1 decodes only the scan area (gray) of the frames inside the OCR line spans and
/// builds one text mask per line (<see cref="BurnedInMaskBuilder"/>).</item>
/// <item>Pass 2 pipes every frame as raw RGB through <c>crispembed --inpaint-stream</c>
/// (frame + mask in, inpainted frame out, with temporal consistency) into an ffmpeg encoder
/// that copies the audio from the source. No frames touch the disk.</item>
/// </list>
/// <see cref="MakeBoxesAssaAsync"/> runs pass 1 only and returns an ASSA subtitle with one black
/// drawing per line instead - the video is not changed. The blur and delogo methods replace
/// pass 2 with one ffmpeg run that filters each line's box (<see cref="MakeFilterGraph"/>).
/// </summary>
public sealed class BurnedInSubtitleRemover
{
    public sealed record ProgressInfo(double Percent, int Pass, long Frame, long TotalFrames, double FramesPerSecond);

    private readonly StringBuilder _log = new();

    public string Log
    {
        get
        {
            lock (_log)
            {
                return _log.ToString();
            }
        }
    }

    /// <summary>Padded frame spans of the OCR lines.</summary>
    /// <summary>Frames added before and after each OCR line: OCR timings are only as exact as its sample rate.</summary>
    public static int GetPadFrames(BurnedInRemovalOptions o) => (int)Math.Ceiling(o.FrameRate / Math.Max(1, o.OcrFramesPerSecond)) + 2;

    public static List<(long StartFrame, long EndFrame)> GetSpans(BurnedInRemovalOptions o)
    {
        long pad = GetPadFrames(o);
        return o.Lines
            .Select(l => ((long)Math.Floor(l.Start.TotalSeconds * o.FrameRate) - pad, (long)Math.Ceiling(l.End.TotalSeconds * o.FrameRate) + pad))
            .ToList();
    }

    public static BurnedInMaskBuilder MakeMaskBuilder(BurnedInRemovalOptions o, IEnumerable<(long StartFrame, long EndFrame)> spans)
    {
        return new BurnedInMaskBuilder(o.VideoWidth, o.VideoHeight, o.RegionX, o.RegionY, o.RegionWidth, o.RegionHeight,
            spans, o.BrightnessMinimum, o.RequireOutline, o.Grow, GetPadFrames(o), (int)Math.Round(1.5 * o.FrameRate), o.ExtraWidth);
    }

    /// <summary>BT.709 for HD, BT.601 below - used for both the RGB decode and the encode, so
    /// untouched pixels survive the round trip whatever the source is tagged as.</summary>
    public static string ColorMatrix(int videoHeight) => videoHeight >= 720 ? "bt709" : "bt601";

    /// <summary>Output color tags for the matrix (the scale filter says "bt601", the
    /// -colorspace option only knows "smpte170m"/"bt470bg").</summary>
    public static string ColorTags(string matrix) => matrix == "bt709"
        ? "-colorspace bt709 -color_primaries bt709 -color_trc bt709"
        : "-colorspace smpte170m -color_primaries smpte170m -color_trc smpte170m";

    public async Task<bool> RunAsync(BurnedInRemovalOptions o, IProgress<ProgressInfo> progress, CancellationToken cancellationToken)
    {
        var totalFrames = Math.Max(1, (long)Math.Round(o.DurationSeconds * o.FrameRate));
        var builder = MakeMaskBuilder(o, GetSpans(o));

        // Pass 1: masks.
        if (!await BuildMasksAsync(o, builder, -1, p => progress.Report(new ProgressInfo(p.Frame * 15.0 / totalFrames, 1, p.Frame, totalFrames, p.Fps)), cancellationToken))
        {
            return false;
        }

        AppendLog($"Masks: {builder.LinesWithText} of {o.Lines.Count} lines have text in the scan area");
        cancellationToken.ThrowIfCancellationRequested();

        // Pass 2: decode -> mask -> crispembed -> encode, or an ffmpeg blur/delogo of each line's box.
        return o.Method is BurnedInMethod.Blur or BurnedInMethod.Delogo
            ? await FilterAsync(o, builder, totalFrames, progress, cancellationToken)
            : await InpaintAsync(o, builder, totalFrames, progress, cancellationToken);
    }

    /// <summary>
    /// Builds the masks (pass 1, reported as 0-100 %) and returns an ASSA subtitle that covers
    /// each line with a black box, or null if it failed or no line has text in the scan area.
    /// </summary>
    public async Task<string?> MakeBoxesAssaAsync(BurnedInRemovalOptions o, IProgress<ProgressInfo> progress, CancellationToken cancellationToken)
    {
        var totalFrames = Math.Max(1, (long)Math.Round(o.DurationSeconds * o.FrameRate));
        var builder = MakeMaskBuilder(o, GetSpans(o));
        if (!await BuildMasksAsync(o, builder, -1, p => progress.Report(new ProgressInfo(p.Frame * 100.0 / totalFrames, 1, p.Frame, totalFrames, p.Fps)), cancellationToken))
        {
            return null;
        }

        AppendLog($"Masks: {builder.LinesWithText} of {o.Lines.Count} lines have text in the scan area");
        cancellationToken.ThrowIfCancellationRequested();
        return builder.LinesWithText > 0 ? MakeBoxesAssa(o, builder.GetLineBoxes()) : null;
    }

    /// <summary>
    /// ASSA with a black rectangle drawing (\p1) per box, shown for the box's frame span. Times
    /// are rounded outwards to whole centiseconds so the box never shows a frame late.
    /// </summary>
    public static string MakeBoxesAssa(BurnedInRemovalOptions o, IEnumerable<(long StartFrame, long EndFrame, int X, int Y, int Width, int Height)> boxes,
        string colour = "&H00000000")
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Script Info]");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine($"PlayResX: {o.VideoWidth}");
        sb.AppendLine($"PlayResY: {o.VideoHeight}");
        sb.AppendLine("ScaledBorderAndShadow: yes");
        sb.AppendLine("WrapStyle: 2");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
        sb.AppendLine($"Style: Box,Arial,20,{colour},{colour},{colour},{colour},0,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1");
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
        foreach (var (startFrame, endFrame, x, y, w, h) in boxes)
        {
            var start = (long)Math.Floor(Math.Max(0, startFrame) / o.FrameRate * 100);
            var end = (long)Math.Ceiling((endFrame + 1) / o.FrameRate * 100);
            if (end <= start || w <= 0 || h <= 0)
            {
                continue;
            }

            sb.AppendLine(FormattableString.Invariant(
                $"Dialogue: 0,{AssaTime(start)},{AssaTime(end)},Box,,0,0,0,,{{\\an7\\pos(0,0)\\bord0\\shad0\\p1}}m {x} {y} l {x + w} {y} {x + w} {y + h} {x} {y + h}{{\\p0}}"));
        }

        return sb.ToString();
    }

    private static string AssaTime(long centiseconds) =>
        FormattableString.Invariant($"{centiseconds / 360000}:{centiseconds / 6000 % 60:00}:{centiseconds / 100 % 60:00}.{centiseconds % 100:00}");

    /// <summary>
    /// ffmpeg filter graph ([0:v] in, [v] out) for the blur and delogo methods. Delogo is one
    /// timeline-enabled delogo per box. Blur blurs the boxes' bounding area once and overlays it
    /// through a soft matte: the boxes drawn white on black by the ass filter
    /// (<paramref name="matteAssName"/>, a file in ffmpeg's working folder).
    /// Timestamps start at 0 so box frame numbers line up with the mask pass.
    /// </summary>
    public static string MakeFilterGraph(BurnedInRemovalOptions o, IReadOnlyList<(long StartFrame, long EndFrame, int X, int Y, int Width, int Height)> boxes,
        string matteAssName)
    {
        var sb = new StringBuilder("[0:v]setpts=PTS-STARTPTS");
        if (o.Method == BurnedInMethod.Delogo)
        {
            foreach (var (startFrame, endFrame, bx, by, bw, bh) in boxes)
            {
                // delogo needs a band of real pixels on every side of the logo.
                var x = Math.Clamp(bx, 1, o.VideoWidth - 3);
                var y = Math.Clamp(by, 1, o.VideoHeight - 3);
                var w = Math.Min(bx + bw, o.VideoWidth - 1) - x;
                var h = Math.Min(by + bh, o.VideoHeight - 1) - y;
                if (w < 2 || h < 2)
                {
                    continue;
                }

                sb.Append(FormattableString.Invariant(
                    $",delogo=x={x}:y={y}:w={w}:h={h}:enable='between(t,{startFrame / o.FrameRate:0.###},{(endFrame + 1) / o.FrameRate:0.###})'"));
            }

            sb.Append(",format=yuv420p[v]");
            return sb.ToString();
        }

        // Blur: the bounding area of all boxes plus room for the soft edge, on even values (4:2:0).
        const int feather = 4;
        var x0 = boxes.Count == 0 ? 0 : boxes.Min(b => b.X) - 2 * feather;
        var y0 = boxes.Count == 0 ? 0 : boxes.Min(b => b.Y) - 2 * feather;
        var x1 = boxes.Count == 0 ? 2 : boxes.Max(b => b.X + b.Width) + 2 * feather;
        var y1 = boxes.Count == 0 ? 2 : boxes.Max(b => b.Y + b.Height) + 2 * feather;
        x0 = Math.Max(0, x0) & ~1;
        y0 = Math.Max(0, y0) & ~1;
        x1 = Math.Min(o.VideoWidth & ~1, (x1 + 1) & ~1);
        y1 = Math.Min(o.VideoHeight & ~1, (y1 + 1) & ~1);
        var cw = Math.Max(2, x1 - x0);
        var ch = Math.Max(2, y1 - y0);

        // boxblur's radius may not exceed half the (chroma) plane size.
        var radius = Math.Clamp(o.BlurStrength, 1, Math.Max(1, Math.Min(cw, ch) / 4 - 1));
        var crop = $"crop={cw}:{ch}:{x0}:{y0}";
        sb.Append($",split[base][src];[src]{crop},boxblur={radius}:2,format=yuva420p[blur];");
        sb.Append(FormattableString.Invariant(
            $"color=c=black:s={o.VideoWidth}x{o.VideoHeight}:r={o.FrameRate:0.######}:d={o.DurationSeconds + 5:0.###},format=yuv420p,"));
        sb.Append($"ass={matteAssName},{crop},format=gray,lut=c0='255*gt(val,128)',boxblur={feather}:1[matte];");
        sb.Append($"[blur][matte]alphamerge[soft];[base][soft]overlay={x0}:{y0}:eof_action=pass,format=yuv420p[v]");
        return sb.ToString();
    }

    private async Task<bool> FilterAsync(BurnedInRemovalOptions o, BurnedInMaskBuilder builder, long totalFrames,
        IProgress<ProgressInfo> progress, CancellationToken cancellationToken)
    {
        // ffmpeg's ass filter parses its argument (':' and '\' in a Windows path), so ffmpeg runs
        // in the temp folder and gets bare file names, like the burn-in feature does.
        var folder = Path.GetTempPath();
        var id = Guid.NewGuid().ToString("N");
        var assName = "se-burned-in-matte-" + id + ".ass";
        var scriptName = "se-burned-in-filter-" + id + ".txt";
        var boxes = builder.GetLineBoxes().ToList();
        await File.WriteAllTextAsync(Path.Combine(folder, assName), MakeBoxesAssa(o, boxes, "&H00FFFFFF"), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(folder, scriptName), MakeFilterGraph(o, boxes, assName), cancellationToken);
        try
        {
            var args = $"-nostdin -v error -y -progress pipe:1 -nostats -i \"{o.VideoFileName}\" -filter_complex_script {scriptName} " +
                       $"-map \"[v]\" -map 0:a? -c:a copy -c:v libx264 -crf {o.Crf} -preset medium \"{o.OutputFileName}\"";
            AppendLog("ffmpeg " + args);
            using var encoder = StartProcess(FfmpegHelper.GetFfmpegLocation(), args, redirectInput: false, workingDirectory: folder);
            var stopwatch = Stopwatch.StartNew();
            using (cancellationToken.Register(() => Kill(encoder)))
            {
                string? line;
                while ((line = await encoder.StandardOutput.ReadLineAsync(cancellationToken)) != null)
                {
                    if (line.StartsWith("frame=", StringComparison.Ordinal) && long.TryParse(line.AsSpan(6), out var frame))
                    {
                        var fps = frame / Math.Max(0.001, stopwatch.Elapsed.TotalSeconds);
                        progress.Report(new ProgressInfo(15 + 85.0 * Math.Min(1.0, (double)frame / totalFrames), 2, frame, totalFrames, fps));
                    }
                }
            }

            await encoder.WaitForExitAsync(CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();
            AppendLog($"{o.Method} done in {stopwatch.Elapsed.TotalSeconds:0.0} s, exit code {encoder.ExitCode}");
            return encoder.ExitCode == 0 && File.Exists(o.OutputFileName);
        }
        finally
        {
            foreach (var name in new[] { assName, scriptName })
            {
                try
                {
                    File.Delete(Path.Combine(folder, name));
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    /// <summary>
    /// Decodes the scan area (gray) and feeds the frames inside the line spans to the builder.
    /// <paramref name="startSeconds"/> &gt;= 0 decodes from there (frame 0 = that time).
    /// </summary>
    public async Task<bool> BuildMasksAsync(BurnedInRemovalOptions o, BurnedInMaskBuilder builder, double startSeconds,
        Action<(long Frame, double Fps)>? progress, CancellationToken cancellationToken, double durationSeconds = -1)
    {
        var seek = startSeconds >= 0 ? $"-ss {startSeconds.ToString("0.###", CultureInfo.InvariantCulture)} " : string.Empty;
        var length = durationSeconds > 0 ? $"-t {durationSeconds.ToString("0.###", CultureInfo.InvariantCulture)} " : string.Empty;
        var args = $"-nostdin -v error {seek}{length}-i \"{o.VideoFileName}\" -map 0:v:0 " +
                   $"-vf \"crop={builder.RegionWidth}:{builder.RegionHeight}:{builder.RegionX}:{builder.RegionY}," +
                   $"scale=in_color_matrix={ColorMatrix(o.VideoHeight)}:in_range=tv,format=rgb24\" -f rawvideo -pix_fmt rgb24 -";
        using var decoder = StartProcess(FfmpegHelper.GetFfmpegLocation(), args, redirectInput: false);
        // RGB with the same conversion as the inpainting pass, so the brightness test (RGB luma,
        // as in Video OCR) sees the same values there.
        var frameSize = builder.RegionWidth * builder.RegionHeight;
        var buffer = new byte[frameSize * 3];
        var luma = new byte[frameSize];
        var stdout = decoder.StandardOutput.BaseStream;
        var stopwatch = Stopwatch.StartNew();
        long frame = 0;
        using (cancellationToken.Register(() => Kill(decoder)))
        {
            while (await ReadExactlyAsync(stdout, buffer, cancellationToken))
            {
                if (builder.NeedsFrame(frame))
                {
                    for (int i = 0, j = 0; i < frameSize; i++, j += 3)
                    {
                        luma[i] = (byte)((buffer[j] * 299 + buffer[j + 1] * 587 + buffer[j + 2] * 114) / 1000);
                    }

                    builder.AddRegionFrame(frame, luma);
                }

                frame++;
                if (frame % 50 == 0)
                {
                    progress?.Invoke((frame, frame / Math.Max(0.001, stopwatch.Elapsed.TotalSeconds)));
                }
            }
        }

        await decoder.WaitForExitAsync(CancellationToken.None);
        cancellationToken.ThrowIfCancellationRequested();
        builder.Finish();
        if (frame == 0)
        {
            AppendLog("Mask pass decoded no frames");
            return false;
        }

        return true;
    }

    private async Task<bool> InpaintAsync(BurnedInRemovalOptions o, BurnedInMaskBuilder builder, long totalFrames,
        IProgress<ProgressInfo> progress, CancellationToken cancellationToken)
    {
        int w = o.VideoWidth, h = o.VideoHeight;
        var matrix = ColorMatrix(h);
        var decoderArgs = $"-nostdin -v error -i \"{o.VideoFileName}\" -map 0:v:0 " +
                          $"-vf \"scale=in_color_matrix={matrix}:in_range=tv,format=rgb24\" -f rawvideo -pix_fmt rgb24 -";
        var crispArgs = $"--inpaint-model \"{o.ModelFileName}\" --inpaint-stream {w}x{h}" + (o.Temporal ? " --inpaint-temporal" : string.Empty);
        var rate = o.FrameRate.ToString("0.######", CultureInfo.InvariantCulture);
        var encoderArgs = $"-nostdin -v error -y -f rawvideo -pix_fmt rgb24 -s {w}x{h} -framerate {rate} -i - -i \"{o.VideoFileName}\" " +
                          "-map 0:v:0 -map 1:a? -c:a copy " +
                          $"-vf \"scale=out_color_matrix={matrix}:out_range=tv,format=yuv420p\" {ColorTags(matrix)} " +
                          $"-c:v libx264 -crf {o.Crf} -preset medium \"{o.OutputFileName}\"";

        AppendLog("ffmpeg " + decoderArgs);
        AppendLog("crispembed " + crispArgs);
        AppendLog("ffmpeg " + encoderArgs);

        using var decoder = StartProcess(FfmpegHelper.GetFfmpegLocation(), decoderArgs, redirectInput: false);
        using var crisp = StartProcess(o.CrispEmbedExecutable, crispArgs, redirectInput: true);
        using var encoder = StartProcess(FfmpegHelper.GetFfmpegLocation(), encoderArgs, redirectInput: true);
        var all = new[] { decoder, crisp, encoder };
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = linked.Token;
        using var registration = token.Register(() =>
        {
            foreach (var p in all)
            {
                Kill(p);
            }
        });

        // If any process dies early (bad argument, codec error), the others block on a pipe nobody
        // reads any more - kill them all so the pipe tasks end and the error is reported.
        var failed = 0;
        void KillAll()
        {
            Interlocked.Exchange(ref failed, 1);
            foreach (var p in all)
            {
                Kill(p);
            }
        }

        foreach (var p in all)
        {
            var process = p;
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                try
                {
                    if (process.ExitCode != 0)
                    {
                        KillAll();
                    }
                }
                catch
                {
                    // ignore
                }
            };

            if (process.HasExited && process.ExitCode != 0)
            {
                KillAll();
            }
        }

        var frameBytes = w * h * 3;
        long framesOut = 0;
        var stopwatch = Stopwatch.StartNew();

        // Decoder -> masks -> crispembed.
        var feed = Task.Run(async () =>
        {
            var rgb = new byte[frameBytes];
            var mask = new byte[w * h];
            var input = crisp.StandardInput.BaseStream;
            long frame = 0;
            try
            {
                while (await ReadExactlyAsync(decoder.StandardOutput.BaseStream, rgb, token))
                {
                    Array.Clear(mask);
                    builder.GetFrameMask(frame, rgb, mask);
                    await input.WriteAsync(rgb, token);
                    await input.WriteAsync(mask, token);
                    frame++;
                }
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // the process may already be gone
                }
            }

            return frame;
        }, token);

        // crispembed -> encoder.
        var drain = Task.Run(async () =>
        {
            var frame = new byte[frameBytes];
            var output = encoder.StandardInput.BaseStream;
            try
            {
                while (await ReadExactlyAsync(crisp.StandardOutput.BaseStream, frame, token))
                {
                    await output.WriteAsync(frame, token);
                    var n = Interlocked.Increment(ref framesOut);
                    if (n % 10 == 0)
                    {
                        var fps = n / Math.Max(0.001, stopwatch.Elapsed.TotalSeconds);
                        progress.Report(new ProgressInfo(15 + 85.0 * Math.Min(1.0, (double)n / totalFrames), 2, n, totalFrames, fps));
                    }
                }
            }
            finally
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    // the process may already be gone
                }
            }
        }, token);

        _ = feed.ContinueWith(_ => KillAll(), TaskContinuationOptions.OnlyOnFaulted);
        _ = drain.ContinueWith(_ => KillAll(), TaskContinuationOptions.OnlyOnFaulted);

        long framesIn;
        try
        {
            framesIn = await feed;
            await drain;
        }
        catch (Exception exception)
        {
            foreach (var p in all)
            {
                Kill(p);
            }

            cancellationToken.ThrowIfCancellationRequested();
            AppendLog("Pipe failed: " + exception.Message);
            return false;
        }

        foreach (var p in all)
        {
            await p.WaitForExitAsync(CancellationToken.None);
        }

        cancellationToken.ThrowIfCancellationRequested();
        AppendLog($"Frames: {framesIn} decoded, {Interlocked.Read(ref framesOut)} encoded in {stopwatch.Elapsed.TotalSeconds:0.0} s");
        var ok = Interlocked.CompareExchange(ref failed, 0, 0) == 0 && decoder.ExitCode == 0 && crisp.ExitCode == 0 && encoder.ExitCode == 0 &&
                 framesIn > 0 && framesIn == Interlocked.Read(ref framesOut) && File.Exists(o.OutputFileName);
        if (!ok)
        {
            AppendLog($"Exit codes: decoder {decoder.ExitCode}, crispembed {crisp.ExitCode}, encoder {encoder.ExitCode}");
        }

        return ok;
    }

    private Process StartProcess(string fileName, string arguments, bool redirectInput, string? workingDirectory = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = redirectInput,
                WorkingDirectory = workingDirectory ??
                                   (Path.GetDirectoryName(fileName) is { Length: > 0 } dir && Directory.Exists(dir) ? dir : string.Empty),
            },
        };

        var name = Path.GetFileNameWithoutExtension(fileName);
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                AppendLog($"[{name}] {e.Data}");
            }
        };
        process.Start();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task<bool> ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken);
            if (n == 0)
            {
                return false;
            }

            read += n;
        }

        return true;
    }

    private static void Kill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void AppendLog(string line)
    {
        lock (_log)
        {
            if (_log.Length < 200_000)
            {
                _log.AppendLine(line);
            }
        }
    }
}
