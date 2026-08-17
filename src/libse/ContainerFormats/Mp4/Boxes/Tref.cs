using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Reference Box. Only the "chap" reference is read: it points from the video track to the
    /// QuickTime text track that holds the chapter titles.
    /// </summary>
    public class Tref : Box
    {
        public List<uint> ChapterTrackIds { get; } = new List<uint>();

        public Tref(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "chap" && Size >= 12 && Size < 1024)
                {
                    var payload = new byte[Size - 8];
                    if (fs.Read(payload, 0, payload.Length) == payload.Length)
                    {
                        // The payload is simply a list of four byte track ids.
                        for (var i = 0; i + 4 <= payload.Length; i += 4)
                        {
                            var trackId = BinaryPrimitives.ReadUInt32BigEndian(new System.ReadOnlySpan<byte>(payload, i, 4));
                            if (trackId > 0)
                            {
                                ChapterTrackIds.Add(trackId);
                            }
                        }
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
