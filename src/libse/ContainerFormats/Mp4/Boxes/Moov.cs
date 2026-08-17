using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Movie Box
    /// </summary>
    public class Moov : Box
    {
        public Mvhd Mvhd;
        public List<Trak> Tracks;
        public List<Trex> Trexs = new List<Trex>();

        /// <summary>
        /// Nero chapter list from moov/udta, when the file has one.
        /// </summary>
        public Chpl Chpl;

        public Moov(Stream fs, ulong maximumLength)
        {
            Tracks = new List<Trak>();
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "trak")
                {
                    Tracks.Add(new Trak(fs, Position));
                }
                else if (Name == "mvhd")
                {
                    Mvhd = new Mvhd(fs);
                }
                else if (Name == "mvex") // Movie Extends Box - fragment defaults
                {
                    var mvexEnd = Position;
                    while (fs.Position < (long)mvexEnd)
                    {
                        if (!InitializeSizeAndName(fs))
                        {
                            return;
                        }

                        if (Name == "trex")
                        {
                            Trexs.Add(new Trex(fs, Size));
                        }

                        fs.Seek((long)Position, SeekOrigin.Begin);
                    }

                    Position = mvexEnd;
                }
                else if (Name == "udta") // User Data Box - holds the Nero chapter list
                {
                    var udtaEnd = Position;
                    while (fs.Position < (long)udtaEnd)
                    {
                        if (!InitializeSizeAndName(fs))
                        {
                            return;
                        }

                        if (Name == "chpl")
                        {
                            Chpl = new Chpl(fs, Size);
                        }

                        fs.Seek((long)Position, SeekOrigin.Begin);
                    }

                    Position = udtaEnd;
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
