using System.IO;
using Nikse.SubtitleEdit.Core.Common;

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

                // Parens matter: without them, `IsVobSubSubtitle || IsClosedCaption
                // || IsVideo` bind independently of `Name == "minf"`, so e.g. a
                // mdhd box on a video track was being parsed as if it were minf —
                // which left Mdhd unset and forced the default 90000 timescale
                // for video tracks (incorrect MP4 timing for CEA-608/708).
                if (Name == "minf" && (IsTextSubtitle || IsVobSubSubtitle || IsClosedCaption || IsVideo))
                {
                    ulong timeScale = 90000;
                    if (Mdhd != null)
                    {
                        timeScale = Mdhd.TimeScale;
                    }

                    var tempMinf = new Minf(fs, Position, timeScale, HandlerType, this);
                    if (tempMinf.Stbl != null)
                    {
                        Minf = tempMinf;
                    }
                }
                else if (Name == "hdlr")
                {
                    Buffer = new byte[Size - 4];
                    fs.ReadFully(Buffer, 0, Buffer.Length);
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
