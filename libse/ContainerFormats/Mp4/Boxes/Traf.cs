using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Traf : Box
    {

        public Trun Trun { get; set; }
        public Tfdt Tfdt { get; set; }

        public Traf(FileStream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "trun")
                {
                    Trun = new Trun(fs, Position);
                }
                else if (Name == "tfdt")
                {
                    Tfdt = new Tfdt(fs, Size);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
