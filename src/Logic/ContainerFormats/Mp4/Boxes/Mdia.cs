using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.Mp4.Boxes
{
    public class Mdia : Box
    {
        public Mdhd Mdhd;
        public Minf Minf;
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

        public bool IsClosedCaption
        {
            get { return HandlerType == "clcp"; }
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
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "minf" && IsTextSubtitle || IsVobSubSubtitle || IsClosedCaption || IsVideo)
                {
                    UInt32 timeScale = 90000;
                    if (Mdhd != null)
                        timeScale = Mdhd.TimeScale;
                    Minf = new Minf(fs, Position, timeScale, HandlerType, this);
                }
                else if (Name == "hdlr")
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    HandlerType = GetString(8, 4);
                    if (Size > 25)
                        HandlerName = GetString(24, Buffer.Length - (24 + 5)); // TODO: How to find this?
                }
                else if (Name == "mdhd")
                {
                    Mdhd = new Mdhd(fs, Size);
                }
                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
