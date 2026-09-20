using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Reads the time stamp and key frame flag of every video packet in a file and turns them into a
/// <see cref="FfmpegFrameIndex"/>. Nothing is decoded and every other stream is discarded at the
/// demuxer, so this runs at disk speed - a few seconds for a feature film on a local drive.
/// <para>
/// The file is opened a second time, separate from the player's own format context, so indexing
/// never moves the read position playback depends on.
/// </para>
/// </summary>
internal static unsafe class FfmpegFrameIndexer
{
    // Kept in a static so the native function pointer handed to libavformat stays valid.
    private static readonly AVIOInterruptCB_callback InterruptCallbackDelegate = InterruptCallback;

    private sealed class CancelState
    {
        public CancellationToken Token;
    }

    private static int InterruptCallback(void* opaque)
    {
        if (opaque == null)
        {
            return 0;
        }

        return GCHandle.FromIntPtr((IntPtr)opaque).Target is CancelState { Token.IsCancellationRequested: true } ? 1 : 0;
    }

    /// <summary>
    /// Null when cancelled, when the file cannot be read, or when the stream carries no usable
    /// time stamps.
    /// </summary>
    /// <param name="nativePath">The path as handed to avformat_open_input by the player.</param>
    /// <param name="videoStreamIndex">The stream the player shows.</param>
    /// <param name="startTimeSeconds">The player's zero point, subtracted from every time stamp.</param>
    public static FfmpegFrameIndex? Build(string nativePath, int videoStreamIndex, double startTimeSeconds, CancellationToken token)
    {
        var handle = GCHandle.Alloc(new CancelState { Token = token });
        AVFormatContext* format = null;
        AVPacket* packet = null;
        try
        {
            format = ffmpeg.avformat_alloc_context();
            if (format == null)
            {
                return null;
            }

            format->interrupt_callback.callback = InterruptCallbackDelegate;
            format->interrupt_callback.opaque = (void*)GCHandle.ToIntPtr(handle);

            // On failure avformat_open_input frees the context and nulls the pointer itself.
            if (ffmpeg.avformat_open_input(&format, nativePath, null, null) < 0 ||
                ffmpeg.avformat_find_stream_info(format, null) < 0 ||
                videoStreamIndex < 0 || videoStreamIndex >= (int)format->nb_streams)
            {
                return null;
            }

            for (var i = 0; i < (int)format->nb_streams; i++)
            {
                if (i != videoStreamIndex)
                {
                    format->streams[i]->discard = AVDiscard.AVDISCARD_ALL;
                }
            }

            var presentationTicks = new List<long>();
            var presentationKeys = new List<bool>();
            var decodeTicks = new List<long>();
            var decodeKeys = new List<bool>();
            var packets = 0;

            packet = ffmpeg.av_packet_alloc();
            while (!token.IsCancellationRequested)
            {
                var result = ffmpeg.av_read_frame(format, packet);
                if (result == -ffmpeg.EAGAIN)
                {
                    Thread.Sleep(5);
                    continue;
                }

                if (result < 0)
                {
                    break; // end of file, or a read error - index what was read
                }

                // AV_PKT_FLAG_DISCARD: decoded but never shown (outside an MP4 edit list).
                if (packet->stream_index == videoStreamIndex && (packet->flags & ffmpeg.AV_PKT_FLAG_DISCARD) == 0)
                {
                    packets++;
                    var key = (packet->flags & ffmpeg.AV_PKT_FLAG_KEY) != 0;
                    if (packet->pts != ffmpeg.AV_NOPTS_VALUE)
                    {
                        presentationTicks.Add(packet->pts);
                        presentationKeys.Add(key);
                    }

                    if (packet->dts != ffmpeg.AV_NOPTS_VALUE)
                    {
                        decodeTicks.Add(packet->dts);
                        decodeKeys.Add(key);
                    }
                }

                ffmpeg.av_packet_unref(packet);
            }

            if (token.IsCancellationRequested || packets == 0)
            {
                return null;
            }

            // Presentation times are the truth. Containers without them (AVI, raw streams) only
            // give decode times, which is what the decoder then stamps its pictures with too. A
            // stream with presentation times on just some packets (the second field of field-coded
            // pictures has none) is indexed by those alone.
            var usePresentation = presentationTicks.Count * 2 >= packets;
            var ticks = usePresentation ? presentationTicks : decodeTicks;
            var keys = usePresentation ? presentationKeys : decodeKeys;
            if (ticks.Count == 0)
            {
                return null;
            }

            var secondsPerTick = ffmpeg.av_q2d(format->streams[videoStreamIndex]->time_base);
            return FfmpegFrameIndex.Create(ticks, keys, secondsPerTick, startTimeSeconds);
        }
        finally
        {
            if (packet != null)
            {
                ffmpeg.av_packet_free(&packet);
            }

            if (format != null)
            {
                ffmpeg.avformat_close_input(&format);
            }

            handle.Free();
        }
    }
}
