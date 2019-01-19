using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Fragment Box
    /// </summary>
    public class Traf : Box
    {

        public Trun Trun { get; set; }
        public Tfdt Tfdt { get; set; }

        public Traf(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

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
            if (Trun?.Samples != null && Tfdt != null)
            {
                foreach (var timeSegment in Trun.Samples)
                {
                    timeSegment.BaseMediaDecodeTime = Tfdt.BaseMediaDecodeTime;
                }
            }
        }

    }
}
