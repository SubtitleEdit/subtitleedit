using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Minf : Box
    {

        public readonly Stbl Stbl;

        public Minf(FileStream fs, ulong maximumLength, UInt32 timeScale, string handlerType)
        {
            pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (name == "stbl")
                    Stbl = new Stbl(fs, pos, timeScale, handlerType);

                fs.Seek((long)pos, SeekOrigin.Begin);
            }
        }

    }
}
