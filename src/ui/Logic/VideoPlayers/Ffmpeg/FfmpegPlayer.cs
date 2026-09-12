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
    private double _volume = 100;
    private double _speed = 1.0;

    // Frame handed to the UI. Guarded by _currentFrameLock; the presenter swaps it, the control copies it.
    private readonly Lock _currentFrameLock = new();
    private VideoFrame? _currentFrame;
    private long _frameVersion;

    /// <summary>Raised (on a worker thread) whenever a new picture is ready for <see cref="CopyCurrentFrame"/>.</summary>
    public event Action? FrameReady;

    public string Name => string.IsNullOrEmpty(PlayerSubName) ? "ffmpeg" : $"ffmpeg-{PlayerSubName}";
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

        return Task.Run(() =>
        {
            if (_disposed)
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
                _fileName = string.Empty;
                return;
            }

            if (_disposed)
            {
                session.Dispose();
                return;
            }

            _session = session;
            session.Volume = _volume;
            session.Speed = _speed;
            session.Start();

            // Always seek once: this is what decodes and shows the first picture (at the wanted
            // position) while the player stays paused.
            session.Seek(Math.Max(0, startPositionSeconds));
        });
    }

    public void CloseFile()
    {
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
            if (session == null || value < 0 || double.IsNaN(value))
            {
                return;
            }

            session.Seek(Math.Min(value, Math.Max(0, session.Duration)));
        }
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

    private void Present(VideoFrame frame, VideoFrameQueue pool)
    {
        VideoFrame? previous;
        lock (_currentFrameLock)
        {
            previous = _currentFrame;
            _currentFrame = frame;
        }

        pool.Return(previous);
        Interlocked.Increment(ref _frameVersion);
        FrameReady?.Invoke();
    }

    private static IAudioSink CreateAudioSink()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WaveOutAudioSink();
        }

        // No sink for Linux/macOS yet - playback stays in sync, just silent.
        return new SilentAudioSink();
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

        public double Duration { get; }
        public int VideoWidth { get; }
        public int VideoHeight { get; }
        public double DisplayAspectRatio { get; }

        public Session(FfmpegPlayer owner, string fileName)
        {
            _owner = owner;
            _fileName = fileName;

            AVFormatContext* format = null;
            var result = ffmpeg.avformat_open_input(&format, NativeMediaPath.ForMpv(fileName), null, null);
            if (result < 0)
            {
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

                // The queued audio was resampled for the old speed and the clocks were anchored
                // under it; a seek to where we are re-anchors everything at the new speed.
                Seek(Position);
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
        }

        public void Play()
        {
            if (_playing)
            {
                return;
            }

            if (_endReached || (Duration > 0 && Position >= Duration - 0.01))
            {
                Seek(0);
            }

            _endReached = false;
            _playing = true;
            _wallClockBase = _pausedPosition;
            _wallClock.Restart();
            _audioSink.Resume();
            _presentWake.Set();
        }

        public void Pause()
        {
            if (!_playing)
            {
                return;
            }

            _pausedPosition = Clock();
            _playing = false;
            _wallClock.Stop();
            _audioSink.Pause();
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
            lock (_seekLock)
            {
                _requestedSerial++;
                _requestedTarget = seconds;
                _seekPending = true;
                _pausedPosition = seconds;
            }

            _endReached = false;
            _demuxWake.Set();
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
            Seek(Position); // flushes the queues; the audio thread reopens on the first packet of the new stream

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

        // ---------------------------------------------------------------- demux

        private void DemuxLoop()
        {
            var eof = false;
            try
            {
                while (!_closing)
                {
                    if (TryTakeSeek(out var target, out var serial))
                    {
                        PerformSeek(target, serial);
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

        private bool TryTakeSeek(out double target, out int serial)
        {
            lock (_seekLock)
            {
                if (!_seekPending)
                {
                    target = 0;
                    serial = 0;
                    return false;
                }

                _seekPending = false;
                target = _requestedTarget;
                serial = _requestedSerial;
                return true;
            }
        }

        private void PerformSeek(double target, int serial)
        {
            var timestamp = (long)((target + _startTimeSeconds) * ffmpeg.AV_TIME_BASE);
            var result = ffmpeg.av_seek_frame(_format, -1, timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD);
            if (result < 0)
            {
                System.Diagnostics.Debug.WriteLine($"ffmpeg seek failed: {FfmpegLibraries.ErrorText(result)}");
            }

            lock (_seekLock)
            {
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
            _audioPackets.Flush(serial, target);
            _videoFrames.Flush();
            _audioSink.Reset();
            _presentWake.Set();
        }

        // ---------------------------------------------------------------- video decode

        private void VideoLoop()
        {
            AVCodecContext* codec = null;
            AVFrame* frame = null;
            SwsContext* sws = null;
            var swsSourceWidth = 0;
            var swsSourceHeight = 0;
            var swsSourceFormat = AVPixelFormat.AV_PIX_FMT_NONE;
            var outputWidth = 0;
            var outputHeight = 0;

            try
            {
                var stream = _format->streams[_videoStreamIndex];
                codec = OpenDecoder(stream);
                frame = ffmpeg.av_frame_alloc();
                var timeBase = stream->time_base;
                var frameDuration = stream->avg_frame_rate.num > 0 && stream->avg_frame_rate.den > 0
                    ? 1.0 / ffmpeg.av_q2d(stream->avg_frame_rate)
                    : 1.0 / 25.0;

                var serial = -1;
                var dropUntil = -1.0;
                var presentedForSerial = false;
                VideoFrame? lastDropped = null; // kept so a target past the last picture still shows something

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
                        dropUntil = entry.SeekTarget;
                        presentedForSerial = false;
                        _videoFrames.Return(lastDropped);
                        lastDropped = null;
                    }

                    var packet = entry.Packet;
                    var sendResult = ffmpeg.avcodec_send_packet(codec, packet); // null = drain at end of stream
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

                        var (targetWidth, targetHeight) = OutputSize(frame->width, frame->height);
                        var format = (AVPixelFormat)frame->format;
                        if (sws == null || swsSourceWidth != frame->width || swsSourceHeight != frame->height || swsSourceFormat != format ||
                            outputWidth != targetWidth || outputHeight != targetHeight)
                        {
                            if (sws != null)
                            {
                                ffmpeg.sws_freeContext(sws);
                            }

                            const int swsBilinear = 2;
                            sws = ffmpeg.sws_getContext(frame->width, frame->height, format, targetWidth, targetHeight,
                                AVPixelFormat.AV_PIX_FMT_BGRA, swsBilinear, null, null, null);
                            swsSourceWidth = frame->width;
                            swsSourceHeight = frame->height;
                            swsSourceFormat = format;
                            outputWidth = targetWidth;
                            outputHeight = targetHeight;
                        }

                        if (sws == null)
                        {
                            ffmpeg.av_frame_unref(frame);
                            continue;
                        }

                        var beforeTarget = dropUntil >= 0 && pts < dropUntil - frameDuration * 0.5;
                        if (beforeTarget && !presentedForSerial)
                        {
                            // Skip pictures between the key frame the seek landed on and the
                            // target, but remember the last one in case the stream ends first.
                            var dropped = ConvertFrame(frame, sws, targetWidth, targetHeight, serial, pts, reuse: lastDropped);
                            if (dropped != null)
                            {
                                lastDropped = dropped;
                            }

                            ffmpeg.av_frame_unref(frame);
                            continue;
                        }

                        var converted = ConvertFrame(frame, sws, targetWidth, targetHeight, serial, pts, reuse: null);
                        ffmpeg.av_frame_unref(frame);
                        if (converted == null)
                        {
                            break; // queue closed or serial changed while waiting for a buffer
                        }

                        _videoFrames.Return(lastDropped);
                        lastDropped = null;
                        presentedForSerial = true;
                        _videoFrames.Push(converted);
                        _presentWake.Set();
                    }

                    if (entry.IsEndOfStream)
                    {
                        if (!presentedForSerial && lastDropped != null)
                        {
                            _videoFrames.Push(lastDropped);
                            lastDropped = null;
                            presentedForSerial = true;
                        }

                        var marker = _videoFrames.Rent(outputWidth, outputHeight, serial, ref _currentSerial);
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
                if (sws != null)
                {
                    ffmpeg.sws_freeContext(sws);
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

        private static (int Width, int Height) OutputSize(int width, int height)
        {
            if (width <= MaxOutputWidth || width <= 0 || height <= 0)
            {
                return (width, height);
            }

            var scaledHeight = (int)Math.Round(height * (MaxOutputWidth / (double)width));
            return (MaxOutputWidth, Math.Max(2, scaledHeight & ~1));
        }

        private VideoFrame? ConvertFrame(AVFrame* frame, SwsContext* sws, int width, int height, int serial, double pts, VideoFrame? reuse)
        {
            var target = reuse != null && reuse.Width == width && reuse.Height == height
                ? reuse
                : _videoFrames.Rent(width, height, serial, ref _currentSerial);
            if (target == null)
            {
                return null;
            }

            if (reuse != null && !ReferenceEquals(reuse, target))
            {
                _videoFrames.Return(reuse);
            }

            var destination = new byte*[] { (byte*)target.Data, null, null, null };
            var destinationStride = new[] { target.Stride, 0, 0, 0 };
            ffmpeg.sws_scale(sws, frame->data, frame->linesize, 0, frame->height, destination, destinationStride);

            target.Pts = pts;
            target.Serial = serial;
            target.IsEndOfStream = false;
            return target;
        }

        private AVCodecContext* OpenDecoder(AVStream* stream)
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
                        fixed (byte* pcmPtr = pcm)
                        {
                            var output = pcmPtr;
                            var input = frame->extended_data;
                            byte** inputPtr = input;
                            var inputSamples = frame->nb_samples;
                            if (skipInputSamples > 0)
                            {
                                inputSamples -= skipInputSamples;
                                // Planar or packed, the offset in bytes per plane is samples * bytesPerSample * (channels for packed).
                                var planar = format >= AVSampleFormat.AV_SAMPLE_FMT_U8P;
                                var bytesPerSample = ffmpeg.av_get_bytes_per_sample(format);
                                var planes = planar ? frame->ch_layout.nb_channels : 1;
                                var offset = skipInputSamples * bytesPerSample * (planar ? 1 : frame->ch_layout.nb_channels);
                                var shifted = stackalloc byte*[planes];
                                for (var p = 0; p < planes; p++)
                                {
                                    shifted[p] = input[p] + offset;
                                }

                                inputPtr = shifted;
                            }

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

                        _videoFrames.Return(_videoFrames.Pop());
                        if (_playing && (!_hasAudio || Clock() >= Duration - 0.05))
                        {
                            ReachEnd();
                        }
                        else if (_playing)
                        {
                            // Video ended first; let the audio play out before stopping.
                            var remaining = Duration - Clock();
                            _presentWake.WaitOne(Math.Clamp((int)(remaining * 1000), 1, 200));
                            if (!_playing)
                            {
                                continue;
                            }

                            ReachEnd();
                        }

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
                        ShowFrame(frame);
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
                        while (true)
                        {
                            var next = PeekSecond();
                            if (next == null || next.IsEndOfStream || next.Serial != frame.Serial || next.Pts > clock)
                            {
                                break;
                            }

                            _videoFrames.Return(_videoFrames.Pop());
                            frame = next;
                        }

                        ShowFrame(frame);
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

        private void ShowFrame(VideoFrame frame)
        {
            var popped = _videoFrames.Pop();
            if (!ReferenceEquals(popped, frame))
            {
                _videoFrames.Return(popped);
                return;
            }

            lock (_seekLock)
            {
                if (frame.Serial > _restartSerial)
                {
                    _restartSerial = frame.Serial;
                    if (!_playing)
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
            _owner.Present(frame, _videoFrames);
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
            try
            {
                _audioSink.Reset();
            }
            catch
            {
                // sink may not have been opened
            }

            JoinThread(_demuxThread);
            JoinThread(_videoThread);
            JoinThread(_audioThread);
            JoinThread(_presentThread);

            _audioSink.Dispose();
            _demuxWake.Dispose();
            _presentWake.Dispose();

            if (_format != null)
            {
                var format = _format;
                ffmpeg.avformat_close_input(&format);
                _format = null;
            }
        }

        private static void JoinThread(Thread? thread)
        {
            if (thread == null || thread == Thread.CurrentThread)
            {
                return;
            }

            if (!thread.Join(TimeSpan.FromSeconds(5)))
            {
                Se.LogError($"ffmpeg player: thread '{thread.Name}' did not stop in time");
            }
        }
    }
}
