using FFmpeg.AutoGen;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Video player built directly on the FFmpeg libraries (libavformat/libavcodec/libswscale/
/// libswresample through FFmpeg.AutoGen), in the spirit of ffme / ffmediaelement and ffplay:
/// <list type="bullet">
/// <item>a demux thread reads packets into one <see cref="PacketQueue"/> per stream,</item>
/// <item>a video thread decodes and converts pictures to BGRA into a <see cref="VideoFrameQueue"/>,</item>
/// <item>an audio thread decodes, resamples to 16-bit stereo and feeds an <see cref="IAudioSink"/>,</item>
/// <item>a presenter thread shows each picture when the clock reaches its time stamp.</item>
/// </list>
/// The clock is the audio device (<see cref="IAudioSink.PlayedSeconds"/>) when the file has audio
/// and a stopwatch otherwise. Seeks are numbered ("serials", as in ffplay): the demux thread
/// flushes the queues and bumps the serial, and every consumer discards what belongs to an older
/// serial, so a seek never has to wait for the pipeline to drain.
/// <para>
/// Pictures are software decoded and handed to an Avalonia control through
/// <see cref="CopyCurrentFrame"/>; see <see cref="FfmpegSoftwareControl"/>.
/// </para>
/// <para>
/// Owning the pipeline is what makes frame work exact here. A background scan reads the time
/// stamp of every picture into a <see cref="FfmpegFrameIndex"/>, so seeks land on real frames and
/// <see cref="StepOneFrameForward"/> / <see cref="StepOneFrameBack"/> move by real frames, also in
/// variable frame rate files. Steps do not seek: forward shows the next decoded picture, back
/// takes one from the <see cref="VideoFrameHistory"/>. Seeks that arrive in a burst (a drag) are
/// served at the nearest key frame and land exactly once the burst settles.
/// </para>
/// </summary>
public sealed unsafe class FfmpegPlayer : IVideoPlayer, IDisposable
{
    public string PlayerSubName { get; set; } = string.Empty;

    private const int OutputSampleRate = 48000;
    private const int OutputChannels = 2;
    private const int VideoQueueCapacity = 3;

    /// <summary>Pictures wider than this are scaled down during conversion - the preview never shows more, and it keeps the BGRA pool small.</summary>
    private const int MaxOutputWidth = 1920;

    private const double MaxDemuxQueueBytes = 24 * 1024 * 1024;
    private const int MaxDemuxQueuePackets = 200;

    private string _fileName = string.Empty;
    private bool _disposed;
    private Session? _session;
    private int _loadGeneration; // bumped by LoadFile/CloseFile so a stale open cannot publish its session
    private double _volume = 100;
    private double _speed = 1.0;

    // Frame handed to the UI. Guarded by _currentFrameLock; the presenter swaps it, the control copies it.
    private readonly Lock _currentFrameLock = new();
    private VideoFrame? _currentFrame;
    private long _frameVersion;

    /// <summary>Raised (on a worker thread) whenever a new picture is ready for <see cref="CopyCurrentFrame"/>.</summary>
    public event Action? FrameReady;

    /// <summary>
    /// The subtitle drawn over the video by <see cref="FfmpegSoftwareControl"/>. mpv and VLC get
    /// an ASS file pushed into the player; this player has no renderer, so the control draws
    /// this snapshot itself. Replace it whenever the subtitle changes (see MainViewModel).
    /// </summary>
    public FfmpegPreviewSubtitle PreviewSubtitle
    {
        get => _previewSubtitle;
        set => _previewSubtitle = value ?? FfmpegPreviewSubtitle.Empty;
    }

    private volatile FfmpegPreviewSubtitle _previewSubtitle = FfmpegPreviewSubtitle.Empty;

    /// <summary>The "toggle subtitles on video player" state, shared with mpv's flag by the caller.</summary>
    public volatile bool PreviewSubtitlesVisible = true;

    /// <summary>
    /// Hardware decoder in use ("videotoolbox", "d3d11va", ...), or empty while decoding in
    /// software. Written by the video thread, read by the UI badge.
    /// </summary>
    private volatile string _decoderName = string.Empty;

    public string Name => string.IsNullOrEmpty(_decoderName) ? "ffmpeg" : $"ffmpeg ({_decoderName})";
    public string FileName => _fileName;

    public bool CanLoad()
    {
        return FfmpegLibraries.IsAvailable();
    }

    public int VideoWidth => _session?.VideoWidth ?? 0;
    public int VideoHeight => _session?.VideoHeight ?? 0;

    /// <summary>Display aspect ratio of the video (sample aspect ratio applied), or 0 when unknown.</summary>
    public double DisplayAspectRatio => _session?.DisplayAspectRatio ?? 0;

    /// <summary>Incremented on every presented picture, so a control can skip redundant copies.</summary>
    public long FrameVersion => Interlocked.Read(ref _frameVersion);

    /// <summary>
    /// Copies the current picture as BGRA into <paramref name="destination"/>. Returns false (and
    /// leaves the destination alone) when there is no picture or the sizes do not match.
    /// </summary>
    public bool CopyCurrentFrame(IntPtr destination, int destinationStride, int width, int height)
    {
        lock (_currentFrameLock)
        {
            var frame = _currentFrame;
            if (frame == null || frame.Data == IntPtr.Zero || frame.Width != width || frame.Height != height)
            {
                return false;
            }

            var rowBytes = frame.Width * 4;
            var source = (byte*)frame.Data;
            var target = (byte*)destination;
            for (var y = 0; y < height; y++)
            {
                Buffer.MemoryCopy(source + (long)y * frame.Stride, target + (long)y * destinationStride, rowBytes, rowBytes);
            }

            return true;
        }
    }

    /// <summary>Presentation time of the picture on screen, NaN when there is none.</summary>
    private double CurrentFramePts
    {
        get
        {
            lock (_currentFrameLock)
            {
                return _currentFrame?.Pts ?? double.NaN;
            }
        }
    }

    /// <summary>Size of the picture currently held for the UI (may be smaller than the video, see <see cref="MaxOutputWidth"/>).</summary>
    public (int Width, int Height) CurrentFrameSize
    {
        get
        {
            lock (_currentFrameLock)
            {
                return _currentFrame == null ? (0, 0) : (_currentFrame.Width, _currentFrame.Height);
            }
        }
    }

    public Task LoadFile(string fileName, double startPositionSeconds = 0)
    {
        CloseFile();
        _fileName = fileName;

        // Opening runs on a worker; a CloseFile or another LoadFile issued meanwhile bumps the
        // generation, and the worker then throws its session away instead of resurrecting it.
        var generation = Interlocked.Increment(ref _loadGeneration);

        return Task.Run(() =>
        {
            if (_disposed || generation != Volatile.Read(ref _loadGeneration))
            {
                return;
            }

            Session session;
            try
            {
                session = new Session(this, fileName);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, $"ffmpeg player failed to open: {fileName}");
                if (generation == Volatile.Read(ref _loadGeneration))
                {
                    _fileName = string.Empty;
                }

                return;
            }

            if (_disposed || generation != Volatile.Read(ref _loadGeneration) ||
                Interlocked.CompareExchange(ref _session, session, null) != null)
            {
                session.Dispose();
                return;
            }

            if (generation != Volatile.Read(ref _loadGeneration))
            {
                // CloseFile ran between the check and the exchange; whoever still holds the
                // session disposes it.
                if (Interlocked.CompareExchange(ref _session, null, session) == session)
                {
                    session.Dispose();
                }

                return;
            }

            try
            {
                session.Volume = _volume;
                session.Speed = _speed;
                session.Start();

                // Always seek once: this is what decodes and shows the first picture (at the wanted
                // position) while the player stays paused.
                session.Seek(Math.Max(0, startPositionSeconds));
            }
            catch (ObjectDisposedException)
            {
                // CloseFile took and disposed the session while it was being started.
            }
        });
    }

    public void CloseFile()
    {
        Interlocked.Increment(ref _loadGeneration);
        var session = Interlocked.Exchange(ref _session, null);
        _fileName = string.Empty;
        session?.Dispose();

        lock (_currentFrameLock)
        {
            // The frame belonged to the session's pool, which is gone now.
            _currentFrame?.Dispose();
            _currentFrame = null;
        }

        Interlocked.Increment(ref _frameVersion);
        FrameReady?.Invoke();
    }

    public void Play()
    {
        _session?.Play();
    }

    public void PlayOrPause()
    {
        var session = _session;
        if (session == null)
        {
            return;
        }

        if (session.IsPlaying)
        {
            session.Pause();
        }
        else
        {
            session.Play();
        }
    }

    public void Pause()
    {
        _session?.Pause();
    }

    public void Stop()
    {
        var session = _session;
        if (session == null)
        {
            return;
        }

        session.Pause();
        session.Seek(0);
    }

    public AudioTrackInfo? ToggleAudioTrack()
    {
        return _session?.ToggleAudioTrack();
    }

    public bool IsPlaying => _session?.IsPlaying ?? false;
    public bool IsPaused => !IsPlaying;

    public double Position
    {
        get => _session?.Position ?? 0;
        set
        {
            var session = _session;
            if (session == null)
            {
                return;
            }

            var target = SeekTarget(value, session.Duration);
            if (target.HasValue)
            {
                session.Seek(target.Value);
            }
        }
    }

    /// <summary>
    /// Where a Position assignment seeks to: null for an invalid value, else the value clamped
    /// to the duration - but only when the duration is known. Raw elementary streams (.h264) and
    /// some transport streams report no duration (0 or NaN), and clamping to that would turn
    /// every seek into a seek to 0.
    /// </summary>
    internal static double? SeekTarget(double value, double duration)
    {
        if (value < 0 || double.IsNaN(value))
        {
            return null;
        }

        return duration > 0 ? Math.Min(value, duration) : value;
    }

    public double Duration => _session?.Duration ?? 0;

    public int VolumeMaximum => 100;

    public double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, VolumeMaximum);
            var session = _session;
            if (session != null)
            {
                session.Volume = _volume;
            }
        }
    }

    public double Speed
    {
        get => _speed;
        set
        {
            if (value <= 0 || double.IsNaN(value))
            {
                return;
            }

            _speed = value;
            var session = _session;
            if (session != null)
            {
                session.Speed = value;
            }
        }
    }

    /// <summary>
    /// The real presentation time of every frame in the file, or null while the background scan
    /// is still running (or when the file has no usable time stamps). See <see cref="FfmpegFrameIndex"/>.
    /// </summary>
    public FfmpegFrameIndex? FrameIndex => _session?.FrameIndex;

    /// <summary>Raised (on a worker thread) when <see cref="FrameIndex"/> becomes available for the loaded file.</summary>
    public event Action? FrameIndexReady;

    /// <summary>
    /// Shows the next picture of the stream and stays paused. While the decoder is ahead (it
    /// normally is) this is a buffer swap, so holding the key down steps at decode speed.
    /// </summary>
    public void StepOneFrameForward()
    {
        _session?.Step(forward: true);
    }

    /// <summary>
    /// Shows the previous picture of the stream and stays paused. The last few pictures are kept
    /// (see <see cref="VideoFrameHistory"/>), so stepping back is normally instant as well; when
    /// the history runs out one exact seek refills it.
    /// </summary>
    public void StepOneFrameBack()
    {
        _session?.Step(forward: false);
    }

    internal int SeeksPerformed => _session?.SeeksPerformed ?? 0;
    internal int FastSeeksPerformed => _session?.FastSeeksPerformed ?? 0;

    /// <summary>Test hook: false keeps the background scan from running, as while it is still under way on a big file.</summary>
    internal bool UseFrameIndex { get; set; } = true;

    public bool SupportsPlaybackRestartEvents => true;

    public bool HasPlaybackRestartedSince(long stopwatchTimestamp)
    {
        return _session?.HasPlaybackRestartedSince(stopwatchTimestamp) ?? false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CloseFile();
    }

    /// <summary>Puts <paramref name="frame"/> on screen and hands back the picture it replaced; the caller owns that one now.</summary>
    private VideoFrame? Present(VideoFrame frame)
    {
        VideoFrame? previous;
        lock (_currentFrameLock)
        {
            previous = _currentFrame;
            _currentFrame = frame;
        }

        Interlocked.Increment(ref _frameVersion);
        FrameReady?.Invoke();
        return previous;
    }

    private static IAudioSink CreateAudioSink()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WaveOutAudioSink();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new AudioQueueAudioSink();
        }

        // Linux: PulseAudio, which on current distributions is PipeWire's pipewire-pulse. When
        // no server answers, Open throws and the session carries on with the silent sink.
        if (OperatingSystem.IsLinux() && PulseAudioSink.IsLibraryAvailable())
        {
            return new PulseAudioSink();
        }

        // No sound library - playback stays in sync, just silent.
        return new SilentAudioSink();
    }

    /// <summary>
    /// Whether samples of this format are stored one plane per channel. The same table as
    /// libavutil's av_sample_fmt_is_planar, kept managed so it can be tested without the native
    /// libraries. Note that the enum is not ordered packed-then-planar: S64 (packed) comes after
    /// the first planar formats, so a ">= U8P" comparison misclassifies it.
    /// </summary>
    internal static bool IsPlanarSampleFormat(AVSampleFormat format)
    {
        return format is AVSampleFormat.AV_SAMPLE_FMT_U8P
            or AVSampleFormat.AV_SAMPLE_FMT_S16P
            or AVSampleFormat.AV_SAMPLE_FMT_S32P
            or AVSampleFormat.AV_SAMPLE_FMT_FLTP
            or AVSampleFormat.AV_SAMPLE_FMT_DBLP
            or AVSampleFormat.AV_SAMPLE_FMT_S64P;
    }

    private static double TimestampToSeconds(long timestamp, AVRational timeBase)
    {
        return timestamp == ffmpeg.AV_NOPTS_VALUE ? double.NaN : timestamp * ffmpeg.av_q2d(timeBase);
    }

    private static string? DictionaryValue(AVDictionary* dictionary, string key)
    {
        var entry = ffmpeg.av_dict_get(dictionary, key, null, 0);
        return entry == null ? null : Marshal.PtrToStringUTF8((IntPtr)entry->value);
    }

    /// <summary>
    /// Everything that belongs to one opened file: native contexts, queues, threads and clock.
    /// A new file gets a new session, so no per-file state can leak from one load to the next.
    /// </summary>
    private sealed class Session : IDisposable
    {
        private readonly FfmpegPlayer _owner;
        private readonly string _fileName;
        private AVFormatContext* _format;
        private readonly int _videoStreamIndex = -1;
        private int _audioStreamIndex = -1;
        private readonly List<int> _audioStreamIndexes = new();
        private readonly double _startTimeSeconds;

        private readonly PacketQueue _videoPackets = new();
        private readonly PacketQueue _audioPackets = new();
        private readonly VideoFrameQueue _videoFrames = new(VideoQueueCapacity);
        private readonly IAudioSink _audioSink;
        private readonly bool _hasAudio;
        private readonly bool _hasVideo;

        private Thread? _demuxThread;
        private Thread? _videoThread;
        private Thread? _audioThread;
        private Thread? _presentThread;
        private volatile bool _closing;

        private readonly AutoResetEvent _demuxWake = new(false);
        private readonly AutoResetEvent _presentWake = new(false);

        // Seek state. _requestedSerial is bumped for every seek request; the demux thread
        // performs the newest one and publishes it as the queue serial.
        private readonly Lock _seekLock = new();
        private int _requestedSerial;
        private double _requestedTarget = -1;
        private bool _seekPending;
        private int _currentSerial; // serial the pipeline currently runs under
        private bool _requestedFast; // the pending seek may land on a key frame (scrub burst)
        private bool _previousSeekIssuedInFlight; // see ScrubSeekPolicy.JoinsBurst
        private long _lastSeekRequestTimestamp;
        private int _owesExactSerial; // a fast seek that still owes an exact landing, 0 if none
        private bool _steppedSinceSeek; // frame steps moved the picture away from where the audio is queued
        private bool _resumeAudioOnSeek; // Play came with a seek: start the device after the stale audio is dropped
        private readonly Lock _audioStateLock = new(); // Pause against the deferred device start in PerformSeek
        private SeekPlan _videoPlan; // how the video thread lands _currentSerial
        private int _seeksPerformed;
        private int _fastSeeksPerformed;

        // Frame stepping. Steps are queued here and carried out by the presenter thread, which
        // owns the picture hand-over; positive = forward.
        private int _pendingSteps;
        private long _stepRequestedTimestamp;
        private readonly VideoFrameHistory _history;
        private readonly Stack<VideoFrame> _future = new(); // pictures stepped back from, nearest on top (presenter thread only)
        private readonly int _historyCapacity;

        // Frame index, filled in by a background scan of the file.
        private volatile FfmpegFrameIndex? _frameIndex;
        private Thread? _indexThread;
        private readonly CancellationTokenSource _indexCancel = new();

        /// <summary>A key frame this many pictures (or fewer) before the target is decoded through even mid-burst - cheaper than a second seek.</summary>
        private const int CheapExactFrames = 12;

        private const int MaxPendingSteps = 3;
        private const double StepStarvedSeconds = 0.3;

        /// <summary>How the video thread treats the pictures of one seek serial.</summary>
        private readonly struct SeekPlan
        {
            public SeekPlan(int serial, double landing, double tolerance, double historyFrom)
            {
                Serial = serial;
                Landing = landing;
                Tolerance = tolerance;
                HistoryFrom = historyFrom;
            }

            public int Serial { get; }

            /// <summary>Pictures before this time (less <see cref="Tolerance"/>) are skipped; negative shows the first picture decoded.</summary>
            public double Landing { get; }

            /// <summary>NaN when no frame index was there to say: half an average frame then.</summary>
            public double Tolerance { get; }

            /// <summary>Skipped pictures from this time on are kept for stepping back; NaN as for <see cref="Tolerance"/>.</summary>
            public double HistoryFrom { get; }
        }

        // Clock.
        private readonly Stopwatch _wallClock = new();
        private double _wallClockBase; // media seconds at _wallClock zero
        private volatile bool _playing;
        private double _pausedPosition;
        private volatile bool _endReached;

        private double _audioAnchorPts = double.NaN; // media time of the first sample written since the last sink reset
        private int _audioAnchorSerial = -1;
        private double _audioSpeed = 1.0; // speed the audio currently queued was resampled for

        // Restart tracking (see IVideoPlayer.HasPlaybackRestartedSince).
        private long _lastRestartTimestamp;
        private int _restartSerial = -1;

        private volatile float _gain = 1f;
        private double _speed = 1.0;

        public FfmpegFrameIndex? FrameIndex => _frameIndex;
        public int SeeksPerformed => Volatile.Read(ref _seeksPerformed);
        public int FastSeeksPerformed => Volatile.Read(ref _fastSeeksPerformed);

        public double Duration { get; }
        public int VideoWidth { get; }
        public int VideoHeight { get; }
        public double DisplayAspectRatio { get; }

        // Kept in a static so the native function pointer handed to libavformat stays valid.
        private static readonly AVIOInterruptCB_callback InterruptCallbackDelegate = InterruptCallback;

        // Handed to libavformat as the interrupt callback's opaque; freed with the format context.
        private GCHandle _selfHandle;

        /// <summary>
        /// libavformat polls this during blocking reads and seeks: answering 1 makes the call
        /// return AVERROR_EXIT, so a demux thread stuck in av_read_frame on a slow or network
        /// file lets go when the session closes instead of outliving the format context.
        /// </summary>
        private static int InterruptCallback(void* opaque)
        {
            if (opaque == null)
            {
                return 0;
            }

            return GCHandle.FromIntPtr((IntPtr)opaque).Target is Session { _closing: true } ? 1 : 0;
        }

        public Session(FfmpegPlayer owner, string fileName)
        {
            _owner = owner;
            _fileName = fileName;

            var format = ffmpeg.avformat_alloc_context();
            if (format == null)
            {
                throw new InvalidOperationException("avformat_alloc_context failed");
            }

            _selfHandle = GCHandle.Alloc(this);
            format->interrupt_callback.callback = InterruptCallbackDelegate;
            format->interrupt_callback.opaque = (void*)GCHandle.ToIntPtr(_selfHandle);

            // On failure avformat_open_input frees the context and nulls the pointer itself.
            var result = ffmpeg.avformat_open_input(&format, NativeMediaPath.ForMpv(fileName), null, null);
            if (result < 0)
            {
                Dispose();
                throw new InvalidOperationException($"avformat_open_input: {FfmpegLibraries.ErrorText(result)}");
            }

            _format = format;
            result = ffmpeg.avformat_find_stream_info(format, null);
            if (result < 0)
            {
                Dispose();
                throw new InvalidOperationException($"avformat_find_stream_info: {FfmpegLibraries.ErrorText(result)}");
            }

            _startTimeSeconds = format->start_time == ffmpeg.AV_NOPTS_VALUE ? 0 : format->start_time / (double)ffmpeg.AV_TIME_BASE;

            _videoStreamIndex = ffmpeg.av_find_best_stream(format, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);
            if (_videoStreamIndex >= 0 && (format->streams[_videoStreamIndex]->disposition & ffmpeg.AV_DISPOSITION_ATTACHED_PIC) != 0)
            {
                _videoStreamIndex = -1; // cover art in an audio file is not a video track
            }

            _audioStreamIndex = ffmpeg.av_find_best_stream(format, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
            for (var i = 0; i < (int)format->nb_streams; i++)
            {
                if (format->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    _audioStreamIndexes.Add(i);
                }
            }

            _history = new VideoFrameHistory(_videoFrames);
            _hasVideo = _videoStreamIndex >= 0;
            _hasAudio = _audioStreamIndex >= 0;
            if (!_hasVideo && !_hasAudio)
            {
                Dispose();
                throw new InvalidOperationException("No audio or video stream found");
            }

            if (_hasVideo)
            {
                var parameters = format->streams[_videoStreamIndex]->codecpar;
                VideoWidth = parameters->width;
                VideoHeight = parameters->height;
                var sar = parameters->sample_aspect_ratio;
                var sarValue = sar.num > 0 && sar.den > 0 ? ffmpeg.av_q2d(sar) : 1.0;
                DisplayAspectRatio = VideoHeight > 0 ? VideoWidth * sarValue / VideoHeight : 0;
                var (outputWidth, outputHeight) = OutputSize(VideoWidth, VideoHeight);
                _historyCapacity = VideoFrameHistory.CapacityFor(VideoFrame.StrideFor(outputWidth), outputHeight);
            }

            var duration = format->duration == ffmpeg.AV_NOPTS_VALUE ? double.NaN : format->duration / (double)ffmpeg.AV_TIME_BASE;
            if (double.IsNaN(duration) || duration <= 0)
            {
                duration = 0;
                for (var i = 0; i < (int)format->nb_streams; i++)
                {
                    var stream = format->streams[i];
                    var streamDuration = TimestampToSeconds(stream->duration, stream->time_base);
                    if (!double.IsNaN(streamDuration) && streamDuration > duration)
                    {
                        duration = streamDuration;
                    }
                }
            }

            Duration = duration;

            _audioSink = CreateAudioSink();
            if (_hasAudio)
            {
                try
                {
                    _audioSink.Open(OutputSampleRate, OutputChannels);
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, "ffmpeg player: audio device failed to open, playing without sound");
                    _audioSink.Dispose();
                    _audioSink = new SilentAudioSink();
                    _audioSink.Open(OutputSampleRate, OutputChannels);
                }

                _audioSink.Pause();
            }
        }

        public bool IsPlaying => _playing;

        public double Volume
        {
            set => _gain = (float)(value / 100.0);
        }

        public double Speed
        {
            get => _speed;
            set
            {
                if (Math.Abs(_speed - value) < 0.0001)
                {
                    return;
                }

                _speed = value;
                if (_demuxThread == null)
                {
                    return; // not started yet: nothing is queued under the old speed
                }

                // The queued audio was resampled for the old speed and the clocks were anchored
                // under it; a seek to where we are re-anchors everything at the new speed.
                RequestSeek(Position, userSeek: false);
            }
        }

        public void Start()
        {
            _demuxThread = new Thread(DemuxLoop) { IsBackground = true, Name = "ffmpeg demux" };
            _presentThread = new Thread(PresentLoop) { IsBackground = true, Name = "ffmpeg present" };
            _demuxThread.Start();
            _presentThread.Start();

            if (_hasVideo)
            {
                _videoThread = new Thread(VideoLoop) { IsBackground = true, Name = "ffmpeg video" };
                _videoThread.Start();
            }

            if (_hasAudio)
            {
                _audioThread = new Thread(AudioLoop) { IsBackground = true, Name = "ffmpeg audio" };
                _audioThread.Start();
            }

            // The scan reads the whole file once; that is fine for a file on disk and the wrong
            // thing to do to a stream.
            if (_hasVideo && _owner.UseFrameIndex && !_fileName.Contains("://", StringComparison.Ordinal))
            {
                _indexThread = new Thread(IndexLoop) { IsBackground = true, Name = "ffmpeg frame index", Priority = ThreadPriority.BelowNormal };
                _indexThread.Start();
            }
        }

        private void IndexLoop()
        {
            try
            {
                var index = FfmpegFrameIndexer.Build(NativeMediaPath.ForMpv(_fileName), _videoStreamIndex, _startTimeSeconds, _indexCancel.Token);
                if (index == null || index.Count == 0 || _closing)
                {
                    return;
                }

                _frameIndex = index;
                _owner.FrameIndexReady?.Invoke();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ffmpeg player frame index thread");
            }
        }

        public void Play()
        {
            if (_playing)
            {
                return;
            }

            Interlocked.Exchange(ref _pendingSteps, 0);
            bool stepped;
            lock (_seekLock)
            {
                stepped = _steppedSinceSeek;
            }

            var fromStart = _endReached || (Duration > 0 && Position >= Duration - 0.01);
            _endReached = false;
            _playing = true; // before any seek below, so the demux thread sees it when it starts the device
            _wallClockBase = _pausedPosition;
            _wallClock.Restart();
            if (fromStart)
            {
                RequestSeek(0, userSeek: false, resumeAudio: true);
            }
            else if (stepped)
            {
                // Frame steps moved the picture without touching the audio, which is still
                // queued from where the last seek landed; start both from the picture on screen.
                // The device is started by PerformSeek, once that stale audio is gone.
                RequestSeek(_pausedPosition, userSeek: false, resumeAudio: true);
            }
            else
            {
                _audioSink.Resume();
            }

            _presentWake.Set();
        }

        public void Pause()
        {
            if (!_playing)
            {
                return;
            }

            _pausedPosition = Clock();
            _wallClock.Stop();
            lock (_audioStateLock)
            {
                _playing = false;
                _audioSink.Pause();
            }

            _presentWake.Set();
        }

        public double Position
        {
            get
            {
                lock (_seekLock)
                {
                    // Until the seek has landed the target is the truth - the pipeline still
                    // holds pictures from before it.
                    if (_seekPending || _restartSerial < _requestedSerial)
                    {
                        return _requestedTarget >= 0 ? _requestedTarget : _pausedPosition;
                    }
                }

                if (_endReached)
                {
                    return Duration;
                }

                return _playing ? Clock() : _pausedPosition;
            }
        }

        public void Seek(double seconds)
        {
            Interlocked.Exchange(ref _pendingSteps, 0); // a seek supersedes steps still waiting
            RequestSeek(seconds, userSeek: true);
        }

        /// <summary>
        /// Queues a seek for the demux thread. Seeks from outside (<paramref name="userSeek"/>)
        /// that arrive in a burst - a slider drag, a wheel spin - are served at a key frame and
        /// owe an exact landing once the burst settles, exactly the two-tier scheme the mpv player
        /// uses (see <see cref="ScrubSeekPolicy"/>); the player's own seeks are always exact.
        /// </summary>
        private void RequestSeek(double seconds, bool userSeek, bool resumeAudio = false)
        {
            lock (_seekLock)
            {
                var now = Stopwatch.GetTimestamp();
                var inFlight = _requestedSerial > 0 &&
                               (_seekPending || _restartSerial < _requestedSerial) &&
                               Stopwatch.GetElapsedTime(_lastSeekRequestTimestamp, now).TotalSeconds < ScrubSeekPolicy.MaxSeekInFlightSeconds;
                var fast = userSeek && _hasVideo && ScrubSeekPolicy.JoinsBurst(inFlight, _previousSeekIssuedInFlight);
                _previousSeekIssuedInFlight = userSeek && inFlight;
                _lastSeekRequestTimestamp = now;

                _requestedSerial++;
                _requestedTarget = seconds;
                _requestedFast = fast;
                _owesExactSerial = fast ? _requestedSerial : 0;
                _seekPending = true;
                _pausedPosition = seconds;
                _steppedSinceSeek = false;
                _resumeAudioOnSeek |= resumeAudio; // sticky: a newer seek must not lose the Play that is waiting on it
            }

            _endReached = false;
            _demuxWake.Set();
        }

        /// <summary>Queues one frame step; the presenter thread carries it out (see <see cref="HandleStep"/>).</summary>
        public void Step(bool forward)
        {
            if (!_hasVideo)
            {
                return;
            }

            Pause();
            var pending = Volatile.Read(ref _pendingSteps);
            if ((forward && pending < 0) || (!forward && pending > 0))
            {
                Interlocked.Exchange(ref _pendingSteps, 0); // changed direction: forget the other way
                pending = 0;
            }

            if (Math.Abs(pending) >= MaxPendingSteps)
            {
                return; // key repeat outruns the decoder - do not queue up a backlog
            }

            if (pending == 0)
            {
                Interlocked.Exchange(ref _stepRequestedTimestamp, Stopwatch.GetTimestamp());
            }

            Interlocked.Add(ref _pendingSteps, forward ? 1 : -1);
            _presentWake.Set();
        }

        public bool HasPlaybackRestartedSince(long stopwatchTimestamp)
        {
            if (Interlocked.Read(ref _lastRestartTimestamp) <= stopwatchTimestamp)
            {
                return false;
            }

            lock (_seekLock)
            {
                return _restartSerial >= _requestedSerial;
            }
        }

        public AudioTrackInfo? ToggleAudioTrack()
        {
            if (_audioStreamIndexes.Count < 2)
            {
                return null;
            }

            var current = _audioStreamIndexes.IndexOf(_audioStreamIndex);
            var next = _audioStreamIndexes[(current + 1) % _audioStreamIndexes.Count];
            _audioStreamIndex = next;
            RequestSeek(Position, userSeek: false); // flushes the queues; the audio thread reopens on the first packet of the new stream

            var stream = _format->streams[next];
            return new AudioTrackInfo
            {
                Id = _audioStreamIndexes.IndexOf(next) + 1,
                FfIndex = next,
                Language = DictionaryValue(stream->metadata, "language"),
                Title = DictionaryValue(stream->metadata, "title"),
                IsSelected = true,
                IsDefault = (stream->disposition & ffmpeg.AV_DISPOSITION_DEFAULT) != 0,
                Codec = ffmpeg.avcodec_get_name(stream->codecpar->codec_id),
                Channels = stream->codecpar->ch_layout.nb_channels,
            };
        }

        /// <summary>Current media time while playing.</summary>
        private double Clock()
        {
            if (_hasAudio)
            {
                lock (_seekLock)
                {
                    if (!double.IsNaN(_audioAnchorPts) && _audioAnchorSerial == _currentSerial)
                    {
                        return _audioAnchorPts + _audioSink.PlayedSeconds * _audioSpeed;
                    }
                }
            }

            return _wallClockBase + _wallClock.Elapsed.TotalSeconds * _speed;
        }

        /// <summary>True when a seek newer than the given serial has been requested (performed or not).</summary>
        private bool SeekRequestedSince(int serial)
        {
            lock (_seekLock)
            {
                return _requestedSerial != serial;
            }
        }

        // ---------------------------------------------------------------- demux

        private void DemuxLoop()
        {
            var eof = false;
            try
            {
                while (!_closing)
                {
                    if (TryTakeSeek(out var target, out var serial, out var fast))
                    {
                        PerformSeek(target, serial, fast);
                        eof = false;
                        continue;
                    }

                    if (eof)
                    {
                        _demuxWake.WaitOne(100);
                        continue;
                    }

                    if (_videoPackets.Bytes + _audioPackets.Bytes > MaxDemuxQueueBytes ||
                        ((!_hasVideo || _videoPackets.Count > MaxDemuxQueuePackets) &&
                         (!_hasAudio || _audioPackets.Count > MaxDemuxQueuePackets)))
                    {
                        _demuxWake.WaitOne(10);
                        continue;
                    }

                    var packet = ffmpeg.av_packet_alloc();
                    var result = ffmpeg.av_read_frame(_format, packet);
                    if (result < 0)
                    {
                        ffmpeg.av_packet_free(&packet);
                        if (result == -ffmpeg.EAGAIN)
                        {
                            _demuxWake.WaitOne(10);
                            continue;
                        }

                        // End of file - or a read error, which ffplay treats the same way.
                        eof = true;
                        _videoPackets.Push(null);
                        _audioPackets.Push(null);
                        continue;
                    }

                    if (packet->stream_index == _videoStreamIndex)
                    {
                        _videoPackets.Push(packet);
                    }
                    else if (packet->stream_index == _audioStreamIndex)
                    {
                        _audioPackets.Push(packet);
                    }
                    else
                    {
                        ffmpeg.av_packet_free(&packet);
                    }
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ffmpeg player demux thread");
            }
        }

        private bool TryTakeSeek(out double target, out int serial, out bool fast)
        {
            lock (_seekLock)
            {
                if (!_seekPending)
                {
                    target = 0;
                    serial = 0;
                    fast = false;
                    return false;
                }

                _seekPending = false;
                target = _requestedTarget;
                serial = _requestedSerial;
                fast = _requestedFast;
                return true;
            }
        }

        private void PerformSeek(double target, int serial, bool fast)
        {
            // Without the frame index: ask for the target time and let libavformat pick the key
            // frame before it. With it, the landing frame and its key frame are known, so the
            // demuxer is sent to exactly that key frame, in the stream's own time base.
            var seekStream = -1;
            var timestamp = (long)((target + _startTimeSeconds) * ffmpeg.AV_TIME_BASE);
            var landing = target;
            var tolerance = double.NaN;
            var historyFrom = double.NaN;
            var audioTarget = target;

            var index = _hasVideo ? _frameIndex : null;
            var nearest = index?.NearestIndex(target) ?? -1;
            if (index != null && nearest >= 0)
            {
                var keyFrame = index.KeyFrameAtOrBefore(nearest);
                if (fast && keyFrame >= 0 && nearest - keyFrame <= CheapExactFrames)
                {
                    fast = false; // the target is a few pictures past its key frame: land on it right away
                }

                if (fast)
                {
                    keyFrame = index.NearestKeyFrame(target);
                }

                if (keyFrame >= 0)
                {
                    seekStream = _videoStreamIndex;
                    timestamp = index.TicksAt(keyFrame);
                    if (fast)
                    {
                        audioTarget = index.SecondsAt(keyFrame);
                    }
                }

                landing = index.SecondsAt(nearest);
                tolerance = index.MatchTolerance;
                historyFrom = index.SecondsAt(Math.Max(0, nearest - _historyCapacity)) - tolerance;
            }

            if (fast)
            {
                landing = -1; // show the key frame itself; the exact landing follows when the burst settles
            }

            var result = ffmpeg.av_seek_frame(_format, seekStream, timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD);
            if (result < 0)
            {
                System.Diagnostics.Debug.WriteLine($"ffmpeg seek failed: {FfmpegLibraries.ErrorText(result)}");
            }

            Interlocked.Increment(ref _seeksPerformed);
            if (fast)
            {
                Interlocked.Increment(ref _fastSeeksPerformed);
            }

            bool resumeAudio;
            lock (_seekLock)
            {
                if (!fast && _owesExactSerial == serial)
                {
                    _owesExactSerial = 0;
                }

                resumeAudio = _resumeAudioOnSeek;
                _resumeAudioOnSeek = false;
                _videoPlan = new SeekPlan(serial, landing, tolerance, historyFrom);
                _currentSerial = serial;
                _audioAnchorPts = double.NaN;
                _audioAnchorSerial = -1;
                _audioSpeed = _speed;
                _wallClockBase = target;
                if (_playing)
                {
                    _wallClock.Restart();
                }
                else
                {
                    _wallClock.Reset();
                }
            }

            _videoPackets.Flush(serial, target);
            _audioPackets.Flush(serial, audioTarget);
            _videoFrames.Flush();
            _audioSink.Reset();
            if (resumeAudio)
            {
                lock (_audioStateLock)
                {
                    if (_playing)
                    {
                        _audioSink.Resume();
                    }
                }
            }

            _presentWake.Set();
        }

        // ---------------------------------------------------------------- video decode

        private void VideoLoop()
        {
            AVCodecContext* codec = null;
            AVFrame* frame = null;
            AVFrame* transferFrame = null; // hardware pictures are copied into this one
            AVFrame* heldFrame = null; // the newest picture skipped on the way to a seek target, unconverted
            var converter = new BgraConverter();

            try
            {
                var stream = _format->streams[_videoStreamIndex];
                var hardware = HardwareDeviceTypes.Length > 0;
                codec = OpenDecoder(stream, hardware);
                hardware = codec->hw_device_ctx != null;
                _owner._decoderName = hardware ? HardwareDeviceName(codec) : string.Empty;
                frame = ffmpeg.av_frame_alloc();
                transferFrame = ffmpeg.av_frame_alloc();
                heldFrame = ffmpeg.av_frame_alloc();
                var timeBase = stream->time_base;
                var frameDuration = stream->avg_frame_rate.num > 0 && stream->avg_frame_rate.den > 0
                    ? 1.0 / ffmpeg.av_q2d(stream->avg_frame_rate)
                    : 1.0 / 25.0;

                var serial = -1;
                var dropUntil = -1.0;
                var keepFrom = double.MaxValue; // skipped pictures from here on go to the step-back history
                var presentedForSerial = false;
                var heldPts = 0.0;
                var lastIndexPosition = -1;
                var indexHits = 0;
                var indexMisses = 0;

                while (!_closing)
                {
                    if (!_videoPackets.TryPop(out var entry, 50))
                    {
                        continue;
                    }

                    if (entry.Serial != serial)
                    {
                        ffmpeg.avcodec_flush_buffers(codec);
                        serial = entry.Serial;
                        SeekPlan plan;
                        lock (_seekLock)
                        {
                            plan = _videoPlan;
                        }

                        if (plan.Serial == serial && plan.Landing >= 0)
                        {
                            var tolerance = double.IsNaN(plan.Tolerance) ? frameDuration * 0.5 : plan.Tolerance;
                            dropUntil = plan.Landing - tolerance;
                            keepFrom = double.IsNaN(plan.HistoryFrom)
                                ? plan.Landing - (_historyCapacity + 0.5) * frameDuration
                                : plan.HistoryFrom;
                        }
                        else
                        {
                            dropUntil = -1; // initial serial, or a fast seek: show the first picture
                            keepFrom = double.MaxValue;
                        }

                        presentedForSerial = false;
                        ffmpeg.av_frame_unref(heldFrame);
                        _history.Reset(serial);
                        lastIndexPosition = -1;
                    }

                    var packet = entry.Packet;
                    var sendResult = ffmpeg.avcodec_send_packet(codec, packet); // null = drain at end of stream
                    if (packet != null)
                    {
                        ffmpeg.av_packet_free(&packet);
                    }

                    if (sendResult < 0 && sendResult != -ffmpeg.EAGAIN && sendResult != ffmpeg.AVERROR_EOF)
                    {
                        if (hardware)
                        {
                            // The hardware decoder rejected the stream - retry it in software.
                            FallBackToSoftware(ref codec, stream, sendResult, ref hardware);
                            serial = -1;
                        }

                        continue;
                    }

                    var hardwareFailed = false;
                    while (!_closing)
                    {
                        var receiveResult = ffmpeg.avcodec_receive_frame(codec, frame);
                        if (receiveResult < 0)
                        {
                            hardwareFailed = hardware && receiveResult != -ffmpeg.EAGAIN && receiveResult != ffmpeg.AVERROR_EOF;
                            break;
                        }

                        var pts = TimestampToSeconds(frame->best_effort_timestamp, timeBase);
                        if (double.IsNaN(pts))
                        {
                            pts = TimestampToSeconds(frame->pts, timeBase);
                        }

                        pts = double.IsNaN(pts) ? 0 : pts - _startTimeSeconds;
                        CheckFrameIndex(pts, ref lastIndexPosition, ref indexHits, ref indexMisses);

                        var beforeTarget = dropUntil >= 0 && pts < dropUntil && !presentedForSerial;
                        if (beforeTarget && pts < keepFrom)
                        {
                            // A picture between the key frame the seek landed on and the target:
                            // decoded because the ones after it need it, never looked at - so not
                            // fetched from GPU memory or converted either. The newest one is held
                            // on to as it is, in case the stream ends before the target.
                            ffmpeg.av_frame_unref(heldFrame);
                            ffmpeg.av_frame_move_ref(heldFrame, frame);
                            heldPts = pts;
                            continue;
                        }

                        var converted = ToBgra(frame, transferFrame, converter, serial, pts, out var failure);
                        ffmpeg.av_frame_unref(frame);
                        if (failure == ConvertFailure.Transfer)
                        {
                            hardwareFailed = true;
                            break;
                        }

                        if (failure == ConvertFailure.NoConverter)
                        {
                            continue;
                        }

                        if (converted == null)
                        {
                            break; // queue closed or serial changed while waiting for a buffer
                        }

                        ffmpeg.av_frame_unref(heldFrame);
                        if (beforeTarget)
                        {
                            // One of the last few pictures before the target: kept converted, so
                            // stepping back from the target is instant.
                            _history.Add(converted);
                            continue;
                        }

                        presentedForSerial = true;
                        _videoFrames.Push(converted);
                        _presentWake.Set();
                    }

                    if (hardwareFailed)
                    {
                        // The hardware decoder could not decode or hand back this picture
                        // (unsupported profile, for example): reopen in software and replay from
                        // the key frame.
                        FallBackToSoftware(ref codec, stream, 0, ref hardware);
                        serial = -1;
                        RequestSeek(Position, userSeek: false);
                        continue;
                    }

                    if (entry.IsEndOfStream)
                    {
                        if (!presentedForSerial)
                        {
                            // The target lies past the last picture: show that one instead.
                            var last = _history.TakeNewest(serial);
                            if (last == null && heldFrame->width > 0)
                            {
                                last = ToBgra(heldFrame, transferFrame, converter, serial, heldPts, out _);
                            }

                            ffmpeg.av_frame_unref(heldFrame);
                            if (last != null)
                            {
                                _videoFrames.Push(last);
                                presentedForSerial = true;
                            }
                        }

                        var marker = _videoFrames.Rent(converter.OutputWidth, converter.OutputHeight, serial, ref _currentSerial);
                        if (marker != null)
                        {
                            marker.Serial = serial;
                            marker.IsEndOfStream = true;
                            marker.Pts = double.MaxValue;
                            _videoFrames.Push(marker);
                        }

                        _presentWake.Set();
                    }
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ffmpeg player video thread");
            }
            finally
            {
                converter.Free();
                if (frame != null)
                {
                    ffmpeg.av_frame_free(&frame);
                }

                if (transferFrame != null)
                {
                    ffmpeg.av_frame_free(&transferFrame);
                }

                if (heldFrame != null)
                {
                    ffmpeg.av_frame_free(&heldFrame);
                }

                if (codec != null)
                {
                    ffmpeg.avcodec_free_context(&codec);
                }
            }
        }

        /// <summary>
        /// Checks the frame index against what the decoder really produces. Consecutive pictures
        /// must be consecutive index entries; when they keep not being (field-coded video, where
        /// every field is a packet but every pair one picture; broken time stamps), the index
        /// describes something other than the pictures on screen and is dropped - stepping and
        /// seeking then fall back to the average frame rate, as before the scan finished.
        /// </summary>
        private void CheckFrameIndex(double pts, ref int lastPosition, ref int hits, ref int misses)
        {
            var index = _frameIndex;
            if (index == null)
            {
                return;
            }

            var position = index.NearestIndex(pts);
            var found = position >= 0 && Math.Abs(index.SecondsAt(position) - pts) <= index.MatchTolerance;
            if (lastPosition >= 0)
            {
                if (found && position == lastPosition + 1)
                {
                    hits++;
                }
                else
                {
                    misses++;
                }
            }

            lastPosition = found ? position : -1;
            if (misses >= 10 && misses * 4 > hits)
            {
                Se.LogError($"ffmpeg player: frame index of '{_fileName}' does not match the decoded pictures ({misses} misses, {hits} hits) - not using it");
                _frameIndex = null;
            }
        }

        /// <summary>
        /// Replaces the hardware decoder context with a software one. The caller's pointer is
        /// nulled before the new decoder is opened, so when OpenDecoder throws the caller's
        /// cleanup does not free the context a second time.
        /// </summary>
        private void FallBackToSoftware(ref AVCodecContext* codec, AVStream* stream, int error, ref bool hardware)
        {
            var reason = error < 0 ? FfmpegLibraries.ErrorText(error) : "picture transfer failed";
            Se.LogError($"ffmpeg player: {HardwareDeviceName(codec)} decoding failed for {ffmpeg.avcodec_get_name(stream->codecpar->codec_id)} ({reason}), falling back to software decoding");
            var old = codec;
            codec = null;
            ffmpeg.avcodec_free_context(&old);
            hardware = false;
            _owner._decoderName = string.Empty;
            codec = OpenDecoder(stream, hardware: false);
        }

        private static (int Width, int Height) OutputSize(int width, int height)
        {
            if (width <= MaxOutputWidth || width <= 0 || height <= 0)
            {
                return (width, height);
            }

            var scaledHeight = (int)Math.Round(height * (MaxOutputWidth / (double)width));
            return (MaxOutputWidth, Math.Max(2, scaledHeight & ~1));
        }

        private enum ConvertFailure
        {
            None,
            Transfer, // the hardware picture could not be copied to system memory
            NoConverter, // swscale has no path from this pixel format
            NoBuffer, // queue closed, or the serial moved on while waiting for a buffer
        }

        /// <summary>The swscale context of the video thread, rebuilt when the picture size or format changes.</summary>
        private sealed class BgraConverter
        {
            public SwsContext* Sws;
            public int SourceWidth;
            public int SourceHeight;
            public AVPixelFormat SourceFormat = AVPixelFormat.AV_PIX_FMT_NONE;
            public int OutputWidth;
            public int OutputHeight;

            public void Free()
            {
                if (Sws != null)
                {
                    ffmpeg.sws_freeContext(Sws);
                    Sws = null;
                }
            }
        }

        /// <summary>
        /// A decoded picture as a pooled BGRA frame. Only called for pictures that are going to be
        /// looked at: the ones skipped on the way to a seek target never get here, which saves the
        /// GPU read-back as well as the conversion for each of them.
        /// </summary>
        private VideoFrame? ToBgra(AVFrame* frame, AVFrame* transferFrame, BgraConverter converter, int serial, double pts, out ConvertFailure failure)
        {
            var picture = frame;
            if (frame->hw_frames_ctx != null)
            {
                // The picture lives in GPU memory (IOSurface, D3D11 texture, DXVA2 surface); pull
                // it into system memory (NV12 typically) so swscale can convert it like a
                // software picture.
                ffmpeg.av_frame_unref(transferFrame);
                if (ffmpeg.av_hwframe_transfer_data(transferFrame, frame, 0) < 0)
                {
                    failure = ConvertFailure.Transfer;
                    return null;
                }

                picture = transferFrame;
            }

            try
            {
                var (width, height) = OutputSize(picture->width, picture->height);
                var format = (AVPixelFormat)picture->format;
                if (converter.Sws == null || converter.SourceWidth != picture->width || converter.SourceHeight != picture->height ||
                    converter.SourceFormat != format || converter.OutputWidth != width || converter.OutputHeight != height)
                {
                    converter.Free();
                    const int swsBilinear = 2;
                    converter.Sws = ffmpeg.sws_getContext(picture->width, picture->height, format, width, height,
                        AVPixelFormat.AV_PIX_FMT_BGRA, swsBilinear, null, null, null);
                    converter.SourceWidth = picture->width;
                    converter.SourceHeight = picture->height;
                    converter.SourceFormat = format;
                    converter.OutputWidth = width;
                    converter.OutputHeight = height;
                }

                if (converter.Sws == null)
                {
                    failure = ConvertFailure.NoConverter;
                    return null;
                }

                var target = _videoFrames.Rent(width, height, serial, ref _currentSerial);
                if (target == null)
                {
                    failure = ConvertFailure.NoBuffer;
                    return null;
                }

                var destination = new byte*[] { (byte*)target.Data, null, null, null };
                var destinationStride = new[] { target.Stride, 0, 0, 0 };
                ffmpeg.sws_scale(converter.Sws, picture->data, picture->linesize, 0, picture->height, destination, destinationStride);

                target.Pts = pts;
                target.Serial = serial;
                target.IsEndOfStream = false;
                failure = ConvertFailure.None;
                return target;
            }
            finally
            {
                ffmpeg.av_frame_unref(transferFrame);
            }
        }

        /// <summary>
        /// Hardware decoder device types to try on this platform, in order of preference:
        /// VideoToolbox on macOS; D3D11VA on Windows with DXVA2 as the older fallback. Every one
        /// of them hands decoded pictures back as NV12 through av_hwframe_transfer_data, so the
        /// swscale stage does not care which was picked.
        /// </summary>
        private static readonly AVHWDeviceType[] HardwareDeviceTypes = OperatingSystem.IsMacOS()
            ? new[] { AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX }
            : OperatingSystem.IsWindows()
                ? new[] { AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA, AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 }
                : Array.Empty<AVHWDeviceType>();

        // Kept in a static so the native function pointer handed to libavcodec stays valid.
        private static readonly AVCodecContext_get_format GetHardwareFormatDelegate = GetHardwareFormat;

        /// <summary>The surface pixel format a hardware device type decodes into.</summary>
        private static AVPixelFormat SurfaceFormat(AVHWDeviceType deviceType)
        {
            return deviceType switch
            {
                AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
                AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_D3D11,
                AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_DXVA2_VLD,
                _ => AVPixelFormat.AV_PIX_FMT_NONE,
            };
        }

        private static AVHWDeviceType AttachedDeviceType(AVCodecContext* context)
        {
            return context->hw_device_ctx == null
                ? AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
                : ((AVHWDeviceContext*)context->hw_device_ctx->data)->type;
        }

        private static string HardwareDeviceName(AVCodecContext* context)
        {
            var deviceType = AttachedDeviceType(context);
            return deviceType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
                ? "hardware"
                : ffmpeg.av_hwdevice_get_type_name(deviceType);
        }

        /// <summary>
        /// libavcodec's pixel-format negotiation: pick the surface format of the attached hardware
        /// device when it is offered, else let the default choose a software format.
        /// </summary>
        private static AVPixelFormat GetHardwareFormat(AVCodecContext* context, AVPixelFormat* formats)
        {
            var wanted = SurfaceFormat(AttachedDeviceType(context));
            if (wanted != AVPixelFormat.AV_PIX_FMT_NONE)
            {
                for (var p = formats; *p != AVPixelFormat.AV_PIX_FMT_NONE; p++)
                {
                    if (*p == wanted)
                    {
                        return *p;
                    }
                }
            }

            return ffmpeg.avcodec_default_get_format(context, formats);
        }

        /// <summary>
        /// True when the device can transfer decoded pictures into at least one system-memory
        /// format; without one av_hwframe_transfer_data would fail on every frame.
        /// </summary>
        private static bool HasTransferFormats(AVBufferRef* device)
        {
            var constraints = ffmpeg.av_hwdevice_get_hwframe_constraints(device, null);
            if (constraints == null)
            {
                return false;
            }

            var hasFormats = constraints->valid_sw_formats != null && *constraints->valid_sw_formats != AVPixelFormat.AV_PIX_FMT_NONE;
            ffmpeg.av_hwframe_constraints_free(&constraints);
            return hasFormats;
        }

        /// <summary>True when the decoder can use a device context of the given type.</summary>
        private static bool SupportsHardwareDevice(AVCodec* decoder, AVHWDeviceType deviceType)
        {
            for (var i = 0; ; i++)
            {
                var config = ffmpeg.avcodec_get_hw_config(decoder, i);
                if (config == null)
                {
                    return false;
                }

                if (config->device_type == deviceType &&
                    (config->methods & (int)AvCodecHwConfigMethod.AV_CODEC_HW_CONFIG_METHOD_HW_DEVICE_CTX) != 0)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Opens a decoder for the stream. With <paramref name="hardware"/> the first device type
        /// from <see cref="HardwareDeviceTypes"/> that the decoder supports and that can be created
        /// is attached; the caller can tell by <c>hw_device_ctx</c> being set. Any hardware setup
        /// failure silently means software.
        /// </summary>
        private AVCodecContext* OpenDecoder(AVStream* stream, bool hardware = false)
        {
            var decoder = ffmpeg.avcodec_find_decoder(stream->codecpar->codec_id);
            if (decoder == null)
            {
                throw new InvalidOperationException($"No decoder for {ffmpeg.avcodec_get_name(stream->codecpar->codec_id)}");
            }

            var codec = ffmpeg.avcodec_alloc_context3(decoder);
            if (codec == null)
            {
                throw new InvalidOperationException("avcodec_alloc_context3 failed");
            }

            var result = ffmpeg.avcodec_parameters_to_context(codec, stream->codecpar);
            if (result < 0)
            {
                ffmpeg.avcodec_free_context(&codec);
                throw new InvalidOperationException($"avcodec_parameters_to_context: {FfmpegLibraries.ErrorText(result)}");
            }

            codec->pkt_timebase = stream->time_base;
            codec->thread_count = 0; // auto

            if (hardware && stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                foreach (var deviceType in HardwareDeviceTypes)
                {
                    if (!SupportsHardwareDevice(decoder, deviceType))
                    {
                        continue;
                    }

                    AVBufferRef* device = null;
                    if (ffmpeg.av_hwdevice_ctx_create(&device, deviceType, null, null, 0) < 0)
                    {
                        continue;
                    }

                    if (!HasTransferFormats(device))
                    {
                        // The device came up but cannot hand pictures back to system memory
                        // (seen with D3D11VA on some drivers): try the next type, else software.
                        Se.LogError($"ffmpeg player: {ffmpeg.av_hwdevice_get_type_name(deviceType)} reports no transfer formats, skipping");
                        ffmpeg.av_buffer_unref(&device);
                        continue;
                    }

                    codec->hw_device_ctx = device; // freed with the codec context
                    codec->get_format = GetHardwareFormatDelegate;
                    break;
                }
            }

            result = ffmpeg.avcodec_open2(codec, decoder, null);
            if (result < 0)
            {
                ffmpeg.avcodec_free_context(&codec);
                throw new InvalidOperationException($"avcodec_open2: {FfmpegLibraries.ErrorText(result)}");
            }

            return codec;
        }

        // ---------------------------------------------------------------- audio decode

        private void AudioLoop()
        {
            AVCodecContext* codec = null;
            AVFrame* frame = null;
            SwrContext* swr = null;
            var swrSampleRate = 0;
            var swrFormat = AVSampleFormat.AV_SAMPLE_FMT_NONE;
            var swrChannels = 0;
            var swrSpeed = 0.0;
            var codecStreamIndex = -1;
            byte[] pcm = [];

            try
            {
                frame = ffmpeg.av_frame_alloc();
                var serial = -1;
                var dropUntil = -1.0;
                var timeBase = default(AVRational);
                var anchored = false;

                while (!_closing)
                {
                    if (!_audioPackets.TryPop(out var entry, 50))
                    {
                        continue;
                    }

                    var packet = entry.Packet;
                    if (packet != null && packet->stream_index != codecStreamIndex)
                    {
                        if (codec != null)
                        {
                            ffmpeg.avcodec_free_context(&codec);
                        }

                        codecStreamIndex = packet->stream_index;
                        var stream = _format->streams[codecStreamIndex];
                        timeBase = stream->time_base;
                        codec = OpenDecoder(stream);
                    }

                    if (codec == null)
                    {
                        if (packet != null)
                        {
                            ffmpeg.av_packet_free(&packet);
                        }

                        continue;
                    }

                    if (entry.Serial != serial)
                    {
                        ffmpeg.avcodec_flush_buffers(codec);
                        serial = entry.Serial;
                        dropUntil = entry.SeekTarget;
                        anchored = false;
                        if (swr != null)
                        {
                            ffmpeg.swr_free(&swr); // forget buffered samples from before the seek
                        }
                    }

                    var sendResult = ffmpeg.avcodec_send_packet(codec, packet);
                    if (packet != null)
                    {
                        ffmpeg.av_packet_free(&packet);
                    }

                    if (sendResult < 0 && sendResult != -ffmpeg.EAGAIN && sendResult != ffmpeg.AVERROR_EOF)
                    {
                        continue;
                    }

                    while (!_closing)
                    {
                        var receiveResult = ffmpeg.avcodec_receive_frame(codec, frame);
                        if (receiveResult < 0)
                        {
                            break;
                        }

                        var pts = TimestampToSeconds(frame->best_effort_timestamp, timeBase);
                        if (double.IsNaN(pts))
                        {
                            pts = TimestampToSeconds(frame->pts, timeBase);
                        }

                        pts = double.IsNaN(pts) ? 0 : pts - _startTimeSeconds;
                        var frameSeconds = frame->sample_rate > 0 ? frame->nb_samples / (double)frame->sample_rate : 0;
                        if (dropUntil >= 0 && !anchored && pts + frameSeconds < dropUntil)
                        {
                            ffmpeg.av_frame_unref(frame);
                            continue;
                        }

                        var speed = _speed;
                        var format = (AVSampleFormat)frame->format;
                        if (swr == null || swrSampleRate != frame->sample_rate || swrFormat != format ||
                            swrChannels != frame->ch_layout.nb_channels || Math.Abs(swrSpeed - speed) > 0.0001)
                        {
                            if (swr != null)
                            {
                                ffmpeg.swr_free(&swr);
                            }

                            AVChannelLayout outLayout;
                            ffmpeg.av_channel_layout_default(&outLayout, OutputChannels);
                            var inLayout = frame->ch_layout;
                            SwrContext* newSwr = null;

                            // Speed is done by resampling: the device keeps playing at the output
                            // rate, so producing fewer/more samples per input second makes the
                            // audio faster/slower (with a pitch change, like a tape).
                            var outRate = Math.Max(8000, (int)Math.Round(OutputSampleRate / speed));
                            var setResult = ffmpeg.swr_alloc_set_opts2(&newSwr, &outLayout, AVSampleFormat.AV_SAMPLE_FMT_S16, outRate,
                                &inLayout, format, frame->sample_rate, 0, null);
                            ffmpeg.av_channel_layout_uninit(&outLayout);
                            if (setResult < 0 || newSwr == null || ffmpeg.swr_init(newSwr) < 0)
                            {
                                if (newSwr != null)
                                {
                                    ffmpeg.swr_free(&newSwr);
                                }

                                ffmpeg.av_frame_unref(frame);
                                continue;
                            }

                            swr = newSwr;
                            swrSampleRate = frame->sample_rate;
                            swrFormat = format;
                            swrChannels = frame->ch_layout.nb_channels;
                            swrSpeed = speed;
                        }

                        // Trim the part of the first frame that lies before the seek target.
                        var skipInputSamples = 0;
                        if (dropUntil >= 0 && !anchored && pts < dropUntil && frame->sample_rate > 0)
                        {
                            skipInputSamples = Math.Min(frame->nb_samples, (int)((dropUntil - pts) * frame->sample_rate));
                        }

                        var outSamplesMax = (int)(frame->nb_samples * (double)OutputSampleRate / speed / frame->sample_rate) + 256;
                        var needed = outSamplesMax * OutputChannels * 2;
                        if (pcm.Length < needed)
                        {
                            pcm = new byte[needed];
                        }

                        int written;
                        var input = frame->extended_data;
                        var inputSamples = frame->nb_samples;
                        // Only the first frame after a seek is trimmed, so this small array is
                        // allocated once per seek, not per frame (a stackalloc here would grow the
                        // stack on every trimmed frame of the decode loop).
                        byte*[]? shifted = null;
                        if (skipInputSamples > 0)
                        {
                            inputSamples -= skipInputSamples;
                            // Planar or packed, the offset in bytes per plane is samples * bytesPerSample * (channels for packed).
                            var planar = IsPlanarSampleFormat(format);
                            var bytesPerSample = ffmpeg.av_get_bytes_per_sample(format);
                            var planes = planar ? frame->ch_layout.nb_channels : 1;
                            var offset = skipInputSamples * bytesPerSample * (planar ? 1 : frame->ch_layout.nb_channels);
                            shifted = new byte*[planes];
                            for (var p = 0; p < planes; p++)
                            {
                                shifted[p] = input[p] + offset;
                            }
                        }

                        fixed (byte* pcmPtr = pcm)
                        fixed (byte** shiftedPtr = shifted)
                        {
                            var output = pcmPtr;
                            var inputPtr = shiftedPtr != null ? shiftedPtr : input;
                            written = inputSamples > 0
                                ? ffmpeg.swr_convert(swr, &output, outSamplesMax, inputPtr, inputSamples)
                                : 0;
                        }

                        var samplePts = pts + skipInputSamples / (double)Math.Max(1, frame->sample_rate);
                        ffmpeg.av_frame_unref(frame);
                        if (written <= 0)
                        {
                            continue;
                        }

                        var bytes = written * OutputChannels * 2;
                        ApplyGain(pcm, bytes, _gain);

                        if (!anchored)
                        {
                            lock (_seekLock)
                            {
                                if (serial != _currentSerial)
                                {
                                    break; // a newer seek is on its way - do not anchor to stale audio
                                }

                                _audioAnchorPts = samplePts;
                                _audioAnchorSerial = serial;
                                _audioSpeed = speed;
                                if (!_hasVideo && serial > _restartSerial)
                                {
                                    // No picture will ever land this seek - the first audio does.
                                    _restartSerial = serial;
                                    if (!_playing)
                                    {
                                        _pausedPosition = samplePts;
                                    }

                                    Interlocked.Exchange(ref _lastRestartTimestamp, Stopwatch.GetTimestamp());
                                }
                            }

                            anchored = true;
                        }

                        if (!_audioSink.Write(new ReadOnlySpan<byte>(pcm, 0, bytes)))
                        {
                            break; // reset (seek) or closed while waiting for room
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ffmpeg player audio thread");
            }
            finally
            {
                if (swr != null)
                {
                    ffmpeg.swr_free(&swr);
                }

                if (frame != null)
                {
                    ffmpeg.av_frame_free(&frame);
                }

                if (codec != null)
                {
                    ffmpeg.avcodec_free_context(&codec);
                }
            }
        }

        private static void ApplyGain(byte[] pcm, int bytes, float gain)
        {
            if (Math.Abs(gain - 1f) < 0.001f)
            {
                return;
            }

            var samples = MemoryMarshal.Cast<byte, short>(new Span<byte>(pcm, 0, bytes));
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = (short)Math.Clamp((int)(samples[i] * gain), short.MinValue, short.MaxValue);
            }
        }

        // ---------------------------------------------------------------- present

        private void PresentLoop()
        {
            try
            {
                while (!_closing)
                {
                    if (Volatile.Read(ref _pendingSteps) != 0 && HandleStep())
                    {
                        continue;
                    }

                    var frame = _videoFrames.Peek();
                    if (frame == null)
                    {
                        if (!_hasVideo)
                        {
                            PresentAudioOnlyTick();
                        }

                        _presentWake.WaitOne(20);
                        continue;
                    }

                    int currentSerial;
                    lock (_seekLock)
                    {
                        currentSerial = _currentSerial;
                    }

                    if (frame.Serial != currentSerial)
                    {
                        _videoFrames.Return(_videoFrames.Pop());
                        continue;
                    }

                    if (frame.IsEndOfStream)
                    {
                        if (!_playing)
                        {
                            // Keep the marker: it is what tells a later Play that the end was
                            // reached, so the clock does not run on past the duration.
                            _presentWake.WaitOne(50);
                            continue;
                        }

                        if (_hasAudio)
                        {
                            // Video ended first; let the audio play out before stopping. The wake
                            // event is also set by Play/Pause/Seek and by pushed pictures, so one
                            // wait is not enough - keep waiting until the clock reaches the end,
                            // unless playback was paused or a seek moved the pipeline on. The
                            // marker stays queued meanwhile so a pause here still knows the end.
                            var interrupted = false;
                            var lastClock = double.NegativeInfinity;
                            var stalledSince = Stopwatch.GetTimestamp();
                            while (_playing && !_closing)
                            {
                                var now = Clock();
                                if (now >= Duration - 0.05)
                                {
                                    break;
                                }

                                if (now > lastClock + 0.0005)
                                {
                                    lastClock = now;
                                    stalledSince = Stopwatch.GetTimestamp();
                                }
                                else if (Stopwatch.GetElapsedTime(stalledSince).TotalSeconds > 0.5)
                                {
                                    break; // the audio ran dry short of the reported duration
                                }

                                if (SeekRequestedSince(frame.Serial))
                                {
                                    interrupted = true;
                                    break;
                                }

                                _presentWake.WaitOne(Math.Clamp((int)((Duration - now) * 1000), 1, 200));
                            }

                            if (interrupted || !_playing || _closing || SeekRequestedSince(frame.Serial))
                            {
                                continue;
                            }
                        }

                        var popped = _videoFrames.Pop();
                        _videoFrames.Return(popped);
                        if (!ReferenceEquals(popped, frame))
                        {
                            continue; // flushed by a seek while waiting
                        }

                        ReachEnd();
                        continue;
                    }

                    bool firstOfSerial;
                    lock (_seekLock)
                    {
                        firstOfSerial = _restartSerial < frame.Serial;
                    }

                    if (firstOfSerial)
                    {
                        // The seek has landed: show it right away, playing or paused.
                        ShowFrame(frame, contiguous: false);
                        continue;
                    }

                    if (!_playing)
                    {
                        _presentWake.WaitOne(50);
                        continue;
                    }

                    var clock = Clock();
                    var delay = frame.Pts - clock;
                    if (delay <= 0.002)
                    {
                        // Late: drop everything but the last picture that is already due.
                        var dropped = false;
                        while (true)
                        {
                            var next = PeekSecond();
                            if (next == null || next.IsEndOfStream || next.Serial != frame.Serial || next.Pts > clock)
                            {
                                break;
                            }

                            _videoFrames.Return(_videoFrames.Pop());
                            frame = next;
                            dropped = true;
                        }

                        ShowFrame(frame, contiguous: !dropped);
                        continue;
                    }

                    _presentWake.WaitOne(Math.Clamp((int)(delay * 1000), 1, 50));
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "ffmpeg player present thread");
            }
        }

        private VideoFrame? PeekSecond()
        {
            return _videoFrames.PeekSecond();
        }

        private void PresentAudioOnlyTick()
        {
            if (_playing && Duration > 0 && Clock() >= Duration)
            {
                ReachEnd();
            }
        }

        private void ReachEnd()
        {
            _pausedPosition = Duration;
            _playing = false;
            _endReached = true;
            _wallClock.Stop();
            _audioSink.Pause();
        }

        /// <param name="frame">The head of the queue.</param>
        /// <param name="contiguous">The picture follows the one on screen with nothing skipped in between, so that one can join the step-back history.</param>
        private void ShowFrame(VideoFrame frame, bool contiguous)
        {
            var popped = _videoFrames.Pop();
            if (!ReferenceEquals(popped, frame))
            {
                _videoFrames.Return(popped);
                return;
            }

            var landed = false;
            var owedExactTarget = double.NaN;
            lock (_seekLock)
            {
                if (frame.Serial > _restartSerial)
                {
                    landed = true;
                    _restartSerial = frame.Serial;
                    if (_owesExactSerial == frame.Serial && _requestedSerial == frame.Serial)
                    {
                        // A scrub burst ended on this key frame. The position stays the spot that
                        // was asked for - the key frame can be a whole GOP away from it - and the
                        // exact landing is paid below.
                        owedExactTarget = _requestedTarget;
                        _owesExactSerial = 0;
                    }
                    else if (!_playing)
                    {
                        _pausedPosition = frame.Pts;
                    }
                }
                else if (!_playing)
                {
                    _pausedPosition = frame.Pts;
                }
            }

            // Timestamp after the serial so HasPlaybackRestartedSince never sees a new
            // timestamp with an old serial.
            Interlocked.Exchange(ref _lastRestartTimestamp, Stopwatch.GetTimestamp());
            if (landed)
            {
                ReturnFuture(); // pictures stepped back from belong to the position before the seek
            }

            var previous = _owner.Present(frame);
            if (previous != null)
            {
                if (contiguous && previous.Serial == frame.Serial)
                {
                    _history.Add(previous);
                }
                else
                {
                    if (!landed)
                    {
                        _history.Clear(); // pictures were skipped: the history no longer leads up to this one
                    }

                    _videoFrames.Return(previous);
                }
            }

            if (!double.IsNaN(owedExactTarget))
            {
                RequestSeek(owedExactTarget, userSeek: false);
            }
        }

        private void ReturnFuture()
        {
            while (_future.Count > 0)
            {
                _videoFrames.Return(_future.Pop());
            }
        }

        /// <summary>
        /// Carries out one queued frame step (presenter thread). Returns false when the step has
        /// to wait - for a seek to land, or for the decoder to deliver the next picture - and the
        /// presenter should go on with its normal work meanwhile.
        /// <para>
        /// Forward takes the next picture from the decoded queue, back takes the newest one from
        /// the history; neither touches the demuxer, so both are instant. Only when the history is
        /// empty does a step back turn into an exact seek to the previous frame - and that seek
        /// refills the history on its way, so the steps after it are instant again.
        /// </para>
        /// </summary>
        private bool HandleStep()
        {
            var pending = Volatile.Read(ref _pendingSteps);
            if (pending == 0)
            {
                return false;
            }

            if (_playing)
            {
                Interlocked.Exchange(ref _pendingSteps, 0); // Play came after the step request
                return false;
            }

            var forward = pending > 0;
            var waited = Stopwatch.GetElapsedTime(Interlocked.Read(ref _stepRequestedTimestamp)).TotalSeconds;
            bool seekInFlight;
            int currentSerial;
            lock (_seekLock)
            {
                seekInFlight = _seekPending || _restartSerial < _requestedSerial;
                currentSerial = _currentSerial;
            }

            if (seekInFlight)
            {
                // Step from where the seek lands, not from a picture that is about to be replaced.
                if (waited > ScrubSeekPolicy.MaxSeekInFlightSeconds)
                {
                    Interlocked.Exchange(ref _pendingSteps, 0);
                }

                return false;
            }

            var currentPts = _owner.CurrentFramePts;
            if (double.IsNaN(currentPts))
            {
                Interlocked.Exchange(ref _pendingSteps, 0);
                return false;
            }

            VideoFrame? next = null;
            if (forward)
            {
                if (_future.Count > 0)
                {
                    next = _future.Pop();
                }
                else
                {
                    var head = _videoFrames.Peek();
                    if (head != null && head.Serial != currentSerial)
                    {
                        _videoFrames.Return(_videoFrames.Pop());
                        return true;
                    }

                    if (head != null && head.IsEndOfStream)
                    {
                        Interlocked.Exchange(ref _pendingSteps, 0); // on the last picture
                        return false;
                    }

                    if (head == null && waited < StepStarvedSeconds)
                    {
                        return false; // the decoder is about to deliver it
                    }

                    if (head != null)
                    {
                        next = _videoFrames.Pop();
                        if (!ReferenceEquals(next, head))
                        {
                            _videoFrames.Return(next);
                            return true; // flushed by a seek in between
                        }
                    }
                }
            }
            else
            {
                next = _history.TakeNewest(currentSerial);
            }

            ConsumeStep(forward);
            if (next == null)
            {
                var target = NeighbourFrameSeconds(currentPts, forward);
                if (!double.IsNaN(target))
                {
                    RequestSeek(target, userSeek: false);
                }

                return true;
            }

            var previous = _owner.Present(next);
            if (previous != null)
            {
                if (previous.Serial != next.Serial)
                {
                    _videoFrames.Return(previous);
                }
                else if (forward)
                {
                    _history.Add(previous);
                }
                else
                {
                    _future.Push(previous);
                }
            }

            lock (_seekLock)
            {
                _pausedPosition = next.Pts;
                _steppedSinceSeek = true;
            }

            Interlocked.Exchange(ref _lastRestartTimestamp, Stopwatch.GetTimestamp());
            return true;
        }

        private void ConsumeStep(bool forward)
        {
            // Step() resets the counter when the direction changes, so only count down while the
            // sign still matches.
            int current;
            int updated;
            do
            {
                current = Volatile.Read(ref _pendingSteps);
                if (current == 0 || current > 0 != forward)
                {
                    return;
                }

                updated = current + (forward ? -1 : 1);
            }
            while (Interlocked.CompareExchange(ref _pendingSteps, updated, current) != current);

            if (updated != 0)
            {
                Interlocked.Exchange(ref _stepRequestedTimestamp, Stopwatch.GetTimestamp());
            }
        }

        /// <summary>
        /// Start of the frame next to the one at <paramref name="currentPts"/>: from the frame
        /// index when there is one, else a step of the average frame duration. NaN at either end
        /// of the file.
        /// </summary>
        private double NeighbourFrameSeconds(double currentPts, bool forward)
        {
            var index = _frameIndex;
            if (index != null)
            {
                var neighbour = index.NeighbourIndex(currentPts, forward);
                return neighbour < 0 ? double.NaN : index.SecondsAt(neighbour);
            }

            var rate = _format->streams[_videoStreamIndex]->avg_frame_rate;
            var frameDuration = rate.num > 0 && rate.den > 0 ? 1.0 / ffmpeg.av_q2d(rate) : 1.0 / 25.0;
            var target = currentPts + (forward ? frameDuration : -frameDuration);
            if (target < -frameDuration * 0.5 || (Duration > 0 && target > Duration))
            {
                return double.NaN;
            }

            return Math.Max(0, target);
        }

        // ---------------------------------------------------------------- teardown

        public void Dispose()
        {
            _closing = true;
            _playing = false;
            _videoPackets.Close();
            _audioPackets.Close();
            _videoFrames.Close();
            _demuxWake.Set();
            _presentWake.Set();

            // The constructor disposes a half-built session (stream info failed, no usable
            // stream) before the sink exists, so the sink is null on those paths.
            var audioSink = _audioSink;
            if (audioSink != null)
            {
                try
                {
                    audioSink.Reset();
                }
                catch
                {
                    // sink may not have been opened
                }
            }

            _indexCancel.Cancel();
            var stopped = JoinThread(_demuxThread);
            stopped &= JoinThread(_videoThread);
            stopped &= JoinThread(_audioThread);
            stopped &= JoinThread(_presentThread);
            if (JoinThread(_indexThread)) // has its own format context, so it cannot hold this one hostage
            {
                _indexCancel.Dispose();
            }

            // The frame queue is closed, so these go to their Dispose rather than back to a pool.
            _history?.Reset(-1);
            ReturnFuture();

            audioSink?.Dispose();
            _demuxWake.Dispose();
            _presentWake.Dispose();

            if (!stopped)
            {
                // A worker is still inside libavformat/libavcodec with this context; closing it
                // now would be a use-after-free. Leak it (and the handle its interrupt callback
                // dereferences) rather than crash.
                Se.LogError($"ffmpeg player: leaking the format context of '{_fileName}' because a thread did not stop");
                return;
            }

            if (_format != null)
            {
                var format = _format;
                ffmpeg.avformat_close_input(&format);
                _format = null;
            }

            if (_selfHandle.IsAllocated)
            {
                _selfHandle.Free();
            }
        }

        /// <summary>False when the thread is still running after the timeout.</summary>
        private static bool JoinThread(Thread? thread)
        {
            if (thread == null || thread == Thread.CurrentThread)
            {
                return true;
            }

            if (thread.Join(TimeSpan.FromSeconds(5)))
            {
                return true;
            }

            Se.LogError($"ffmpeg player: thread '{thread.Name}' did not stop in time");
            return false;
        }
    }
}
