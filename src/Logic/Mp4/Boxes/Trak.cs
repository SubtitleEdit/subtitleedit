using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Trak : Box
    {

        public readonly Mdia Mdia;
        public readonly Tkhd Tkhd;

        public Trak(FileStream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "mdia")
                    Mdia = new Mdia(fs, Position);
                else if (Name == "tkhd")
                    Tkhd = new Tkhd(fs);

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
