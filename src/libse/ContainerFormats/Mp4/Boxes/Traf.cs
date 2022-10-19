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
        public Tfhd Tfhd { get; set; }

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
                else if (Name == "tfhd")
                {
                    Tfhd = new Tfhd(fs, Size);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            if (Trun?.Samples == null)
            {
                return;
            }

            foreach (var timeSegment in Trun.Samples)
            {
                if (Tfdt != null)
                {
                    timeSegment.BaseMediaDecodeTime = Tfdt.BaseMediaDecodeTime;
                }

                if (Tfhd == null)
                {
                    continue;
                }

                if (timeSegment.Duration == null && Tfhd.DefaultSampleDuration != null)
                {
                    timeSegment.Duration = Tfhd.DefaultSampleDuration;
                }

                if (timeSegment.Size == null && Tfhd.DefaultSampleSize != null)
                {
                    timeSegment.Size = Tfhd.DefaultSampleSize;
                }
            }
        }
    }
}
