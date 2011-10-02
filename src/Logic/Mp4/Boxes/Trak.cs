using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Trak : Box
    {

        public readonly Mdia Mdia;
        public readonly Tkhd Tkhd;

        public Trak(FileStream fs, ulong maximumLength)
        {
            pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (name == "mdia")
                    Mdia = new Mdia(fs, pos);
                else if (name == "tkhd")
                    Tkhd = new Tkhd(fs, pos);

                fs.Seek((long)pos, SeekOrigin.Begin);
            }
        }
    }
}
