using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Logic.Mp4.Boxes;

namespace Nikse.SubtitleEdit.Logic.Mp4
{
    /// <summary>
    /// http://wiki.multimedia.cx/index.php?title=QuickTime_container
    /// </summary>
    public class Mp4Parser : Box
    {
        public string FileName { get; private set; }
        public Moov Moov { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov != null && Moov.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && (trak.Mdia.IsTextSubtitle || trak.Mdia.IsVobSubSubtitle) && trak.Mdia.Minf != null && trak.Mdia.Minf.Stbl != null)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public TimeSpan Duration
        {
            get
            {
                if (Moov != null && Moov.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                    return TimeSpan.FromSeconds((double)Moov.Mvhd.Duration / Moov.Mvhd.TimeScale);
                return new TimeSpan();
            }
        }

        /// <summary>
        /// Resolution of first video track. If not present returns 0.0
        /// </summary>
        public System.Drawing.Point VideoResolution
        {
            get
            {
                if (Moov != null && Moov.Tracks != null)
                {
                    foreach (var trak in Moov.Tracks)
                    {
                        if (trak != null && trak.Mdia != null && trak.Tkhd != null)
                        {
                            if (trak.Mdia.IsVideo)
                                return new System.Drawing.Point((int)trak.Tkhd.Width, (int)trak.Tkhd.Height);
                        }
                    }
                }
                return new System.Drawing.Point(0, 0);
            }
        }

        public Mp4Parser(string fileName)
        {
            FileName = fileName;
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ParseMp4(fs);
            fs.Close();
        }

        public Mp4Parser(FileStream fs)
        {
            FileName = null;
            ParseMp4(fs);
        }

        private void ParseMp4(FileStream fs)
        {
            int count = 0;
            pos = 0;
            fs.Seek(0, SeekOrigin.Begin);
            bool moreBytes = true;
            while (moreBytes)
            {
                moreBytes = InitializeSizeAndName(fs);
                if (size < 8)
                    return;

                if (name == "moov")
                    Moov = new Moov(fs, pos);

                count++;
                if (count > 100)
                    break;

                if (pos > (ulong)fs.Length)
                    break;
                fs.Seek((long)pos, SeekOrigin.Begin);
            }
            fs.Close();
        }

    }
}
