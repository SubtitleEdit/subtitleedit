using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Edit Box - container for the track's edit list.
    /// </summary>
    public class Edts : Box
    {
        public Elst Elst { get; private set; }

        public Edts(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "elst")
                {
                    Elst = new Elst(fs, Size);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
