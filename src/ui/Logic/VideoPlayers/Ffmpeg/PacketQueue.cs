using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Demuxed packets for one stream, handed from the demux thread to that stream's decoder thread
/// (the ffplay design). Every entry carries the seek serial it was read under: a decoder that pops
/// an entry from a newer serial knows a seek happened, flushes its codec and drops frames before
/// the seek target. An entry with a null packet marks end of stream.
/// </summary>
public sealed unsafe class PacketQueue
{
    public readonly struct Entry
    {
        public Entry(AVPacket* packet, int serial, double seekTarget)
        {
            Packet = packet;
            Serial = serial;
            SeekTarget = seekTarget;
        }

        public AVPacket* Packet { get; }
        public int Serial { get; }

        /// <summary>Position (seconds) the seek that started this serial was heading for, or -1 for the initial serial.</summary>
        public double SeekTarget { get; }

        public bool IsEndOfStream => Packet == null;
    }

    private readonly Queue<Entry> _entries = new();
    // A plain object: Monitor.Wait/PulseAll are used for the hand-off, and those do not work with System.Threading.Lock.
    private readonly object _lock = new();
    private bool _closed;

    public int Serial { get; private set; }
    public double SeekTarget { get; private set; } = -1;
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _entries.Count;
            }
        }
    }

    public long Bytes { get; private set; }

    public void Push(AVPacket* packet)
    {
        lock (_lock)
        {
            if (_closed)
            {
                if (packet != null)
                {
                    ffmpeg.av_packet_free(&packet);
                }

                return;
            }

            _entries.Enqueue(new Entry(packet, Serial, SeekTarget));
            if (packet != null)
            {
                Bytes += packet->size;
            }

            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>Waits up to <paramref name="timeoutMs"/> for an entry; false on timeout or when closed.</summary>
    public bool TryPop(out Entry entry, int timeoutMs)
    {
        lock (_lock)
        {
            if (_entries.Count == 0 && !_closed)
            {
                Monitor.Wait(_lock, timeoutMs);
            }

            if (_entries.Count == 0)
            {
                entry = default;
                return false;
            }

            entry = _entries.Dequeue();
            if (entry.Packet != null)
            {
                Bytes -= entry.Packet->size;
            }

            Monitor.PulseAll(_lock);
            return true;
        }
    }

    /// <summary>Drops everything queued and starts a new serial for the packets that follow.</summary>
    public void Flush(int newSerial, double seekTarget)
    {
        lock (_lock)
        {
            DropAll();
            Serial = newSerial;
            SeekTarget = seekTarget;
            Monitor.PulseAll(_lock);
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            _closed = true;
            DropAll();
            Monitor.PulseAll(_lock);
        }
    }

    private void DropAll()
    {
        while (_entries.Count > 0)
        {
            var entry = _entries.Dequeue();
            var packet = entry.Packet;
            if (packet != null)
            {
                ffmpeg.av_packet_free(&packet);
            }
        }

        Bytes = 0;
    }
}
