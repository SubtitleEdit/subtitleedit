using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mdia : Box
    {
        public readonly Mdhd Mdhd;
        public readonly Minf Minf;
        public readonly string HandlerType = null;

        public bool IsSubtitle
        {
            get { return HandlerType == "sbtl"; }

        }

        public bool IsVideo
        {
            get { return HandlerType == "vide"; }

        }

        public bool IsAudio
        {
            get { return HandlerType == "soun"; }
        }

        public Mdia(FileStream fs, ulong maximumLength)
        {
            pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                fs.Seek((long)pos, SeekOrigin.Begin);
                if (!InitializeSizeAndName(fs))
                    return;
                if (name == "minf")
                {
                    UInt32 timeScale = 90000;
                    if (Mdhd != null)
                        timeScale = Mdhd.TimeScale;
                    Minf = new Minf(fs, pos, timeScale);
                }
                else if (name == "hdlr")
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    HandlerType = GetString(8, 4);
                }
                else if (name == "mdhd")
                {
                    Mdhd = new Mdhd(fs, size);
                }
            }
        }

    }
}
