using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Box
    /// </summary>
    public class Mdia : Box
    {
        public Mdhd Mdhd;
        public Minf Minf;
        public readonly string HandlerType;
        public readonly string HandlerName = string.Empty;

        public bool IsTextSubtitle => HandlerType == "sbtl" || HandlerType == "text";

        public bool IsVobSubSubtitle => HandlerType == "subp";

        public bool IsClosedCaption => HandlerType == "clcp";

        public bool IsVideo => HandlerType == "vide";

        public bool IsAudio => HandlerType == "soun";

        public Mdia(Stream fs, ulong maximumLength)
        {
            HandlerType = string.Empty;
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "minf" && IsTextSubtitle || IsVobSubSubtitle || IsClosedCaption || IsVideo)
                {
                    ulong timeScale = 90000;
                    if (Mdhd != null)
                    {
                        timeScale = Mdhd.TimeScale;
                    }

                    Minf = new Minf(fs, Position, timeScale, HandlerType, this);
                }
                else if (Name == "hdlr")
                {
                    Buffer = new byte[Size - 4];
                    fs.Read(Buffer, 0, Buffer.Length);
                    HandlerType = GetString(8, 4);
                    if (Size > 25)
                    {
                        HandlerName = GetString(24, Buffer.Length - (24 + 5)); // TODO: How to find this?
                    }
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
