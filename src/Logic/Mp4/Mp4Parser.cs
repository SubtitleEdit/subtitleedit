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
                    if (trak.Mdia != null && trak.Mdia.IsSubtitle && trak.Mdia.Minf != null && trak.Mdia.Minf.Stbl != null)
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
            buffer = new byte[8];
            pos = 0;
            bool moreBytes = true;
            while (moreBytes)
            {
                fs.Seek((long)pos, SeekOrigin.Begin);
                moreBytes = InitializeSizeAndName(fs);
                pos = ((ulong) (fs.Position)) + size - 8;
                if (name == "moov")
                {
                    Moov = new Moov(fs, pos);                 
                }
                count++;
                if (count > 100)
                    break;
            }
            fs.Close();
        }

    }
}
