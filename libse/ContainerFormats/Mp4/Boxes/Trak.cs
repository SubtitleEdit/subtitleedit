﻿using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Trak : Box
    {

        public Mdia Mdia;
        public Tkhd Tkhd;

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
