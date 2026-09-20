using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// Small video frames for the filmstrip row of <see cref="TimelineTracks"/>. Frames are pulled
/// out one at a time by ffmpeg on a background task, newest request first, so scrolling past a
/// stretch of the video costs nothing once the view has moved on.
/// </summary>
internal sealed class TimelineThumbnailCache
{
    public const int ThumbnailHeight = 54;

    private const int MaxCachedThumbnails = 600;
    private const int MaxPendingRequests = 48;
    private const int MaxConsecutiveFailures = 5;
    private const int FfmpegTimeoutMs = 15_000;

    private readonly Lock _lock = new();
    private readonly Dictionary<long, Bitmap?> _thumbnails = new();
    private readonly List<long> _pending = new();
    private string _videoFileName = string.Empty;
    private int _generation;
    private int _consecutiveFailures;
    private bool _workerRunning;

    /// <summary>Raised on a background thread when a requested frame has arrived.</summary>
    public event Action? ThumbnailReady;

    /// <summary>
    /// The frame at <paramref name="milliseconds"/>, or null while it is not there yet (it is
    /// then queued). Null as well for a file ffmpeg cannot read - after a few failures in a row
    /// the file is left alone.
    /// </summary>
    public Bitmap? Get(string? videoFileName, long milliseconds)
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

            _pending.Remove(milliseconds);
            _pending.Add(milliseconds);
            if (_pending.Count > MaxPendingRequests)
            {
                _pending.RemoveAt(0);
            }

            if (!_workerRunning)
            {
                _workerRunning = true;
                _ = Task.Run(Work);
            }
        }

        return null;
    }

    private void Work()
    {
        while (true)
        {
            string videoFileName;
            long milliseconds;
            int generation;
            lock (_lock)
            {
                if (_pending.Count == 0)
                {
                    _workerRunning = false;
                    return;
                }

                milliseconds = _pending[^1];
                _pending.RemoveAt(_pending.Count - 1);
                videoFileName = _videoFileName;
                generation = _generation;
            }

            var bitmap = ExtractFrame(videoFileName, milliseconds);

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

                _thumbnails[milliseconds] = bitmap;
                _consecutiveFailures = bitmap == null ? _consecutiveFailures + 1 : 0;
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

    private static Bitmap? ExtractFrame(string videoFileName, long milliseconds)
    {
        try
        {
            if (!File.Exists(videoFileName))
            {
                return null;
            }

            var seconds = (milliseconds / 1000.0).ToString("0.###", CultureInfo.InvariantCulture);
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = FfmpegHelper.GetFfmpegLocation(),
                    // Input seeking ("-ss" before "-i") lands on a nearby keyframe, which is
                    // both fast and plenty for a thumbnail. stderr is silenced rather than
                    // redirected, so only one pipe has to be drained.
                    Arguments = $"-v quiet -ss {seconds} -i \"{videoFileName}\" -frames:v 1 -an -sn " +
                                $"-vf scale=-2:{ThumbnailHeight} -f image2pipe -c:v png -",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };

#pragma warning disable CA1416 // Validate platform compatibility
            process.Start();
#pragma warning restore CA1416

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
            if (stream.Length == 0)
            {
                return null;
            }

            stream.Position = 0;
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}
