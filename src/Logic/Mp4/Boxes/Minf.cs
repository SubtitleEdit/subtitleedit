using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Minf : Box
    {

        public readonly Stbl Stbl;

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
