using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Edit List Box - maps the track's media timeline onto the presentation timeline.
    /// A leading "empty" entry (media time -1) delays the track, and a non-zero media
    /// time on the first real entry skips into the media, i.e. moves it earlier.
    /// </summary>
    public class Elst : Box
    {
        public class Entry
        {
            /// <summary>Duration of the edit, in movie (mvhd) timescale units.</summary>
            public ulong SegmentDuration { get; set; }

            /// <summary>Start time in the media (mdhd) timescale, or -1 for an empty edit.</summary>
            public long MediaTime { get; set; }
        }

        // An edit list this long is either malformed or a frame-accurate splice list no
        // subtitle track would carry - a plain offset cannot describe it anyway.
        private const uint MaxEntries = 1000;

        public List<Entry> Entries { get; } = new List<Entry>();

        public Elst(Stream fs, ulong size)
        {
            if (size < 16)
            {
                return;
            }

            Buffer = new byte[size - 8];
            fs.ReadFully(Buffer, 0, Buffer.Length);

            var version = Buffer[0];
            var entryCount = GetUInt(4);
            if (entryCount > MaxEntries)
            {
                return;
            }

            var entrySize = version == 1 ? 20 : 12;
            var index = 8;
            for (var i = 0; i < entryCount; i++)
            {
                if (index + entrySize > Buffer.Length)
                {
                    return;
                }

                if (version == 1)
                {
                    Entries.Add(new Entry
                    {
                        SegmentDuration = GetUInt64(index),
                        MediaTime = (long)GetUInt64(index + 8),
                    });
                }
                else
                {
                    Entries.Add(new Entry
                    {
                        SegmentDuration = GetUInt(index),
                        MediaTime = GetInt(index + 4),
                    });
                }

                index += entrySize;
            }
        }
    }
}
