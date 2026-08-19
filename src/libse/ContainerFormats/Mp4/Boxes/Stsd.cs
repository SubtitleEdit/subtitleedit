using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Stsd : Box
    {
        // Sample entries are small (codec configuration only) - a larger one is malformed
        private const int MaxSampleEntryPayload = 64 * 1024;

        public uint NumberOfEntries { get; set; }

        /// <summary>
        /// The sample entry that <see cref="Box.Name"/> describes, without its 8-byte box
        /// header, e.g. for reading the VobSub palette out of an "mp4s" entry.
        /// </summary>
        public byte[] SampleEntryPayload { get; private set; }

        /// <summary>
        /// A "tx3g" sample entry whose displayFlags mark the samples as forced
        /// (0x80000000 = all samples forced, 0x40000000 = some samples forced) - how
        /// QuickTime/AVFoundation labels a forced-subtitles track.
        /// </summary>
        public bool IsForcedSubtitle { get; private set; }

        public Stsd(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;

            Buffer = new byte[8];
            fs.ReadFully(Buffer, 0, Buffer.Length);
            NumberOfEntries = GetUInt(4);

            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                SampleEntryPayload = null;
                if (Size > 8 && Size <= MaxSampleEntryPayload && (ulong)fs.Position + Size - 8 <= (ulong)fs.Length)
                {
                    var payload = new byte[Size - 8];
                    fs.ReadFully(payload, 0, payload.Length);
                    SampleEntryPayload = payload;

                    // 6 reserved bytes + 2-byte data reference index, then 4-byte displayFlags
                    if (Name == "tx3g" && payload.Length >= 12)
                    {
                        var displayFlags = ((uint)payload[8] << 24) | ((uint)payload[9] << 16) | ((uint)payload[10] << 8) | payload[11];
                        if ((displayFlags & 0xC0000000) != 0)
                        {
                            IsForcedSubtitle = true;
                        }
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
