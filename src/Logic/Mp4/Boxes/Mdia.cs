using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mdia : Box
    {
        public readonly Mdhd Mdhd;
        public readonly Minf Minf;
        public readonly string HandlerType = null;
        public readonly string HandlerName = string.Empty;

        public bool IsTextSubtitle
        {
            get { return HandlerType == "sbtl" || HandlerType == "text"; }
        }

        public bool IsVobSubSubtitle
        {
            get { return HandlerType == "subp"; }
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
                if (!InitializeSizeAndName(fs))
                    return;

                if (name == "minf" && IsTextSubtitle || IsVobSubSubtitle)
                {
                    UInt32 timeScale = 90000;
                    if (Mdhd != null)
                        timeScale = Mdhd.TimeScale;
                    Minf = new Minf(fs, pos, timeScale, HandlerType);
                }
                else if (name == "hdlr")
                {
                    buffer = new byte[size - 4];
                    fs.Read(buffer, 0, buffer.Length);
                    HandlerType = GetString(8, 4);
                    if (size > 25)
                        HandlerName = GetString(24, buffer.Length - (24 + 5)); // TODO: how to find this?
                }
                else if (name == "mdhd")
                {
                    Mdhd = new Mdhd(fs, size);
                }
                fs.Seek((long)pos, SeekOrigin.Begin);
            }
        }

    }
}
