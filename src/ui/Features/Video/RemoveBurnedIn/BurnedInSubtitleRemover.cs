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

public sealed class BurnedInRemovalOptions
{
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

        // Pass 2: decode -> mask -> crispembed -> encode.
        return await InpaintAsync(o, builder, totalFrames, progress, cancellationToken);
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

    private Process StartProcess(string fileName, string arguments, bool redirectInput)
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
                WorkingDirectory = Path.GetDirectoryName(fileName) is { Length: > 0 } dir && Directory.Exists(dir) ? dir : string.Empty,
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
