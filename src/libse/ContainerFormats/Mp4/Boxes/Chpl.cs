using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Nero chapter list box, found in moov/udta. This is what ffmpeg writes for MP4 unless
    /// "-movflags disable_chpl" is used, so it covers most files that have chapters at all.
    /// </summary>
    public class Chpl : Box
    {
        /// <summary>
        /// Chapter start times are stored in 100-nanosecond units.
        /// </summary>
        private const double TicksPerMillisecond = 10000.0;

        /// <summary>
        /// 100 hours - a sanity ceiling used to detect that the box layout was misread.
        /// </summary>
        private const double MaxPlausibleMilliseconds = 100 * 60 * 60 * 1000.0;

        public List<Chapter> Chapters { get; } = new List<Chapter>();

        public Chpl(Stream fs, ulong size)
        {
            if (size < 9 || size > 1024 * 1024)
            {
                return;
            }

            Buffer = new byte[size - 8];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            // version + 3 flag bytes, then a four byte reserved field in the version 1 layout that
            // Nero and ffmpeg both write, then a single byte chapter count.
            var version = Buffer[0];
            var index = version == 1 ? 9 : 5;
            if (index >= Buffer.Length)
            {
                return;
            }

            int count = Buffer[index];
            index++;

            for (var i = 0; i < count; i++)
            {
                // 8 byte start time, 1 byte title length, then the UTF-8 title.
                if (index + 9 > Buffer.Length)
                {
                    return;
                }

                var startTicks = BinaryPrimitives.ReadUInt64BigEndian(Buffer.AsSpan(index));
                index += 8;

                int titleLength = Buffer[index];
                index++;

                if (index + titleLength > Buffer.Length)
                {
                    return;
                }

                var title = Encoding.UTF8.GetString(Buffer, index, titleLength);
                index += titleLength;

                var startMs = startTicks / TicksPerMillisecond;

                // A start time past any plausible running length means the layout guessed above did
                // not match the file, so stop rather than hand back nonsense.
                if (startMs > MaxPlausibleMilliseconds)
                {
                    return;
                }

                Chapters.Add(new Chapter(startMs, title.Trim()));
            }
        }
    }
}
