using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Moof : Box
    {
        public Traf Traf { get; set; }

        public Moof(FileStream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "traf")
                    Traf = new Traf(fs, Position);

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}