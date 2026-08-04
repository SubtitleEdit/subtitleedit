using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Stsd : Box
    {
        public uint NumberOfEntries { get; set; }
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

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
