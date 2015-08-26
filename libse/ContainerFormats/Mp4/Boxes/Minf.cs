using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Minf : Box
    {

        public Stbl Stbl;

        public Minf(FileStream fs, ulong maximumLength, UInt32 timeScale, string handlerType, Mdia mdia)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "stbl")
                    Stbl = new Stbl(fs, Position, timeScale, handlerType, mdia);

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
