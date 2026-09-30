using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// Reads HD-DVD subtitle streams (.sup, e.g. demuxed with EVODemux). Each packet is
    /// "SP" + 8 byte little-endian PTS (90 kHz) + an HD-DVD sub picture unit, which starts with
    /// 2 zero bytes and a 32-bit big-endian unit size - that zero word is what tells it apart
    /// from the DVD "SP" sup (<see cref="SpHeader"/>), which has a 16-bit size there.
    /// </summary>
    public static class HdDvdSupParser
    {
        private const int HeaderLength = 10;
        private const int MinUnitLength = 10;

        public static bool IsHdDvdSup(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var header = new byte[HeaderLength + MinUnitLength];
                    if (fs.Read(header, 0, header.Length) != header.Length || !IsPacketHeader(header, 0, fs.Length, out var unitLength))
                    {
                        return false;
                    }

                    // A single-packet file must end exactly there; otherwise the next packet must follow.
                    var next = (long)HeaderLength + unitLength;
                    if (next == fs.Length)
                    {
                        return true;
                    }

                    fs.Position = next;
                    return fs.Read(header, 0, header.Length) == header.Length && IsPacketHeader(header, 0, fs.Length - next, out _);
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static List<HdDvdSubPicture> Parse(string fileName)
        {
            return Parse(FileUtil.ReadAllBytesShared(fileName));
        }

        public static List<HdDvdSubPicture> Parse(byte[] buffer)
        {
            var list = new List<HdDvdSubPicture>();
            long position = 0;
            while (position + HeaderLength + MinUnitLength <= buffer.Length)
            {
                if (!IsPacketHeader(buffer, (int)position, buffer.Length, out var unitLength))
                {
                    position = FindNextPacket(buffer, position + 1);
                    continue;
                }

                var pts = BitConverter.ToInt64(buffer, (int)position + 2);
                var unit = new byte[unitLength];
                Buffer.BlockCopy(buffer, (int)position + HeaderLength, unit, 0, unitLength);
                var picture = new HdDvdSubPicture(unit, pts);
                if (picture.HasImage)
                {
                    list.Add(picture);
                }

                position += HeaderLength + unitLength;
            }

            FillMissingEndTimes(list);
            return list;
        }

        /// <summary>
        /// A picture without a "stop display" command stays up until the next one replaces it,
        /// capped at the maximum display time so a stray one doesn't cover a long silence.
        /// </summary>
        private static void FillMissingEndTimes(List<HdDvdSubPicture> list)
        {
            var maxDuration = TimeSpan.FromMilliseconds(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
            for (var i = 0; i < list.Count; i++)
            {
                var picture = list[i];
                if (picture.HasStopDisplay)
                {
                    continue;
                }

                var end = picture.StartTime + maxDuration;
                if (i + 1 < list.Count && list[i + 1].StartTime > picture.StartTime && list[i + 1].StartTime < end)
                {
                    end = list[i + 1].StartTime;
                }

                picture.EndTime = end;
            }
        }

        private static long FindNextPacket(byte[] buffer, long position)
        {
            for (var i = position; i + HeaderLength + MinUnitLength <= buffer.Length; i++)
            {
                if (IsPacketHeader(buffer, (int)i, buffer.Length, out _))
                {
                    return i;
                }
            }

            return buffer.Length;
        }

        private static bool IsPacketHeader(byte[] buffer, int index, long streamLength, out int unitLength)
        {
            unitLength = 0;
            if (index + HeaderLength + MinUnitLength > buffer.Length ||
                buffer[index] != 'S' || buffer[index + 1] != 'P' ||
                buffer[index + HeaderLength] != 0 || buffer[index + HeaderLength + 1] != 0)
            {
                return false;
            }

            var size = ReadUInt32(buffer, index + HeaderLength + 2);
            var controlOffset = ReadUInt32(buffer, index + HeaderLength + 6);
            if (size < MinUnitLength || size > int.MaxValue || controlOffset < MinUnitLength || controlOffset >= size ||
                (streamLength - index - HeaderLength) < size)
            {
                return false;
            }

            unitLength = (int)size;
            return true;
        }

        private static uint ReadUInt32(byte[] buffer, int index)
        {
            return (uint)((buffer[index] << 24) | (buffer[index + 1] << 16) | (buffer[index + 2] << 8) | buffer[index + 3]);
        }
    }
}
