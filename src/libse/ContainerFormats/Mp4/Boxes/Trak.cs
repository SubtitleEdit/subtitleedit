using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Box
    /// </summary>
    public class Trak : Box
    {
        public Mdia Mdia { get; set; }
        public Tkhd Tkhd { get; set; }
        public Edts Edts { get; set; }
        public Tref Tref { get; set; }

        public Trak(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "mdia")
                {
                    Mdia = new Mdia(fs, Position);
                }
                else if (Name == "tkhd")
                {
                    Tkhd = new Tkhd(fs);
                }
                else if (Name == "edts")
                {
                    Edts = new Edts(fs, Position);
                }
                else if (Name == "tref")
                {
                    Tref = new Tref(fs, Position);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
