using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// Small video frames for the filmstrip row of <see cref="TimelineTracks"/>. Frames are pulled
/// out one at a time by ffmpeg on a background task, newest request first, so scrolling past a
/// stretch of the video costs nothing once the view has moved on.
/// <para>
/// A frame is first taken from the keyframe before it, decoding nothing else. An exact seek
/// decodes every frame from that keyframe on, with all cores, which starved video playback
/// (#15956). Only when the keyframe is too far away for the tile is the exact frame decoded,
/// on one thread. ffmpeg runs at below normal priority either way.
/// </para>
/// <para>
/// No ffmpeg is started while <see cref="Paused"/> is set (the video playing): on macOS .NET
/// starts a process with a full fork(), and malloc is locked for every thread meanwhile - mpv's
/// included - which still gave jitter and crackling audio with an idle CPU (#15956). Frames
/// asked for meanwhile wait in the queue.
/// </para>
/// </summary>
internal sealed class TimelineThumbnailCache
{
    public const int ThumbnailHeight = 54;

    private const int MaxCachedThumbnails = 600;
    private const int MaxPendingRequests = 48;
    private const int MaxConsecutiveFailures = 5;
    private const int FfmpegTimeoutMs = 15_000;

    private static readonly Regex PtsTimeRegex = new(@"pts_time:\s*(-?[0-9.]+)", RegexOptions.CultureInvariant);

    private readonly Lock _lock = new();
    private readonly Dictionary<long, Bitmap?> _thumbnails = new();
    private readonly List<(long Milliseconds, long ToleranceMilliseconds)> _pending = new();
    private string _videoFileName = string.Empty;
    private int _generation;
    private int _consecutiveFailures;
    private bool _workerRunning;
    private bool _paused;

    /// <summary>Raised on a background thread when a requested frame has arrived.</summary>
    public event Action? ThumbnailReady;

    /// <summary>
    /// While set, no new frames are extracted (a run already going finishes). Clearing it works
    /// off the frames asked for meanwhile, newest first.
    /// </summary>
    public bool Paused
    {
        get
        {
            lock (_lock)
            {
                return _paused;
            }
        }
        set
        {
            lock (_lock)
            {
                _paused = value;
                StartWorkerIfNeeded();
            }
        }
    }

    /// <summary>
    /// The frame at <paramref name="milliseconds"/>, or null while it is not there yet (it is
    /// then queued). Null as well for a file ffmpeg cannot read - after a few failures in a row
    /// the file is left alone. A keyframe up to <paramref name="toleranceMilliseconds"/> before
    /// the spot is good enough.
    /// </summary>
    public Bitmap? Get(string? videoFileName, long milliseconds, long toleranceMilliseconds)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return null;
        }

        lock (_lock)
        {
            if (!string.Equals(videoFileName, _videoFileName, StringComparison.Ordinal))
            {
                // Bitmaps are only dropped, never disposed: one may be part of the frame the
                // compositor is drawing right now.
                _thumbnails.Clear();
                _pending.Clear();
                _videoFileName = videoFileName;
                _consecutiveFailures = 0;
                _generation++;
            }

            if (_thumbnails.TryGetValue(milliseconds, out var bitmap))
            {
                return bitmap;
            }

            if (_consecutiveFailures >= MaxConsecutiveFailures)
            {
                return null;
            }

            _pending.RemoveAll(p => p.Milliseconds == milliseconds);
            _pending.Add((milliseconds, toleranceMilliseconds));
            if (_pending.Count > MaxPendingRequests)
            {
                _pending.RemoveAt(0);
            }

            StartWorkerIfNeeded();
        }

        return null;
    }

    // Called with _lock held
    private void StartWorkerIfNeeded()
    {
        if (!_workerRunning && !_paused && _pending.Count > 0)
        {
            _workerRunning = true;
            _ = Task.Run(Work);
        }
    }

    private void Work()
    {
        while (true)
        {
            string videoFileName;
            long milliseconds;
            long toleranceMilliseconds;
            int generation;
            lock (_lock)
            {
                if (_pending.Count == 0 || _paused)
                {
                    _workerRunning = false;
                    return;
                }

                (milliseconds, toleranceMilliseconds) = _pending[^1];
                _pending.RemoveAt(_pending.Count - 1);
                videoFileName = _videoFileName;
                generation = _generation;
            }

            var failed = false;
            var bitmap = ExtractKeyframe(videoFileName, milliseconds, toleranceMilliseconds) ??
                         ExtractFrame(videoFileName, milliseconds, out failed);

            lock (_lock)
            {
                if (generation != _generation)
                {
                    continue; // another video was opened meanwhile
                }

                if (_thumbnails.Count >= MaxCachedThumbnails)
                {
                    _thumbnails.Clear();
                }

                // No picture without a failure is a spot past the end of the video - the view
                // runs past it for a short clip, and zoomed in at the end. Those must not count:
                // they are taken first (newest request first), and five of them used to switch
                // the filmstrip off for the rest of the video.
                _thumbnails[milliseconds] = bitmap;
                if (bitmap != null)
                {
                    _consecutiveFailures = 0;
                }
                else if (failed)
                {
                    _consecutiveFailures++;
                }

                if (_consecutiveFailures >= MaxConsecutiveFailures)
                {
                    _pending.Clear();
                }
            }

            if (bitmap != null)
            {
                ThumbnailReady?.Invoke();
            }
        }
    }

    /// <summary>
    /// The keyframe at or before <paramref name="milliseconds"/>, decoding keyframes only. Null
    /// when ffmpeg gives nothing, or the keyframe is more than
    /// <paramref name="toleranceMilliseconds"/> away.
    /// </summary>
    private static Bitmap? ExtractKeyframe(string videoFileName, long milliseconds, long toleranceMilliseconds)
    {
        if (!File.Exists(videoFileName))
        {
            return null;
        }

        // "-noaccurate_seek" keeps the frames before the seek point and "-skip_frame nokey"
        // decodes keyframes only, so the first frame out is the keyframe the seek landed on.
        // showinfo logs its time relative to the seek point (negative when before it).
        var arguments = $"-hide_banner -nostats -noaccurate_seek -skip_frame nokey -threads 1 -ss {ToSeconds(milliseconds)} " +
                        $"-i \"{videoFileName}\" -frames:v 1 -an -sn -dn " +
                        $"-vf showinfo,scale=-2:{ThumbnailHeight} -f image2pipe -c:v png -";
        var bitmap = RunFfmpeg(arguments, readLog: true, out _, out var log);
        if (bitmap == null)
        {
            return null;
        }

        var match = PtsTimeRegex.Match(log);
        if (match.Success &&
            double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var offsetSeconds) &&
            Math.Abs(offsetSeconds * 1000.0) <= toleranceMilliseconds)
        {
            return bitmap;
        }

        return null;
    }

    /// <summary>The exact frame at <paramref name="milliseconds"/>, decoded on one thread.</summary>
    private static Bitmap? ExtractFrame(string videoFileName, long milliseconds, out bool failed)
    {
        failed = true;
        if (!File.Exists(videoFileName))
        {
            return null;
        }

        // stderr is silenced rather than redirected, so only one pipe has to be drained
        var arguments = $"-v quiet -threads 1 -ss {ToSeconds(milliseconds)} -i \"{videoFileName}\" -frames:v 1 -an -sn -dn " +
                        $"-vf scale=-2:{ThumbnailHeight} -f image2pipe -c:v png -";
        return RunFfmpeg(arguments, readLog: false, out failed, out _);
    }

    private static string ToSeconds(long milliseconds)
    {
        return (milliseconds / 1000.0).ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static Bitmap? RunFfmpeg(string arguments, bool readLog, out bool failed, out string log)
    {
        failed = true;
        log = string.Empty;
        try
        {
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = FfmpegHelper.GetFfmpegLocation(),
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = readLog,
                }
            };

#pragma warning disable CA1416 // Validate platform compatibility
            process.Start();
#pragma warning restore CA1416

            try
            {
                // Thumbnails are a nice-to-have - the video player comes first
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch
            {
                // not allowed, or already gone
            }

            // stderr is drained alongside stdout, or a full pipe would block ffmpeg
            var logTask = readLog ? process.StandardError.ReadToEndAsync() : Task.FromResult(string.Empty);

            using var timeout = new CancellationTokenSource(FfmpegTimeoutMs);
            using var registration = timeout.Token.Register(() =>
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    // already gone
                }
            });

            using var stream = new MemoryStream();
            process.StandardOutput.BaseStream.CopyTo(stream);
            process.WaitForExit();
            log = logTask.GetAwaiter().GetResult();
            if (stream.Length == 0)
            {
                // ffmpeg exits with 0 and writes nothing when the seek is past the last frame
                failed = process.ExitCode != 0 || timeout.IsCancellationRequested;
                return null;
            }

            stream.Position = 0;
            var bitmap = new Bitmap(stream);
            failed = false;
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
