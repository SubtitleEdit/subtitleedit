using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Movie Fragment Box
    /// </summary>
    public class Moof : Box
    {
        /// <summary>
        /// All track fragments in this movie fragment. A muxed fMP4/DASH segment has one
        /// traf per track (video + audio + subtitles), so keeping only one loses data.
        /// </summary>
        public List<Traf> Trafs { get; } = new List<Traf>();

        /// <summary>
        /// First track fragment (for callers that only handle single-track fragments).
        /// </summary>
        public Traf Traf => Trafs.Count > 0 ? Trafs[0] : null;

        public Moof(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "traf")
                {
                    Trafs.Add(new Traf(fs, Position));
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
