using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Fragment Box
    /// </summary>
    public class Traf : Box
    {
        /// <summary>
        /// All track fragment runs, in file order. ISO/IEC 14496-12 allows several trun
        /// boxes per traf; keeping only the last one dropped all earlier sample runs.
        /// </summary>
        public List<Trun> Truns { get; } = new List<Trun>();

        /// <summary>
        /// First track fragment run (for callers that only handle a single run).
        /// </summary>
        public Trun Trun => Truns.Count > 0 ? Truns[0] : null;

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
                    Truns.Add(new Trun(fs, Position));
                }
                else if (Name == "tfhd")
                {
                    Tfhd = new Tfhd(fs, Size);
                }
                else if (Name == "tfdt")
                {
                    Tfdt = new Tfdt(fs, Size);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            foreach (var trun in Truns)
            {
                if (trun.Samples == null)
                {
                    continue;
                }

                foreach (var timeSegment in trun.Samples)
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
}
