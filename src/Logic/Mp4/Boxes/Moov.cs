using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Moov
    {
        public Mvhd Mvhd { get; private set; }
        public List<Trak> Tracks { get; private set; }

        public Moov(FileStream fs, ulong maximumLength)
        {
            Tracks = new List<Trak>();

            var buffer = new byte[8];
            ulong pos = (ulong) fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                int bytesRead;
                fs.Seek((long)pos, SeekOrigin.Begin);
                ulong size = Helper.ReadSize(buffer, out bytesRead, fs);
                if (bytesRead < 8)
                    return;
                string name = Helper.GetString(buffer, 4, 4);
                pos = ((ulong)(fs.Position)) + size - 8;
                if (name == "trak")
                {
                    Tracks.Add(new Trak(fs, pos));
                }
                else if (name == "mvhd")
                {
                    Mvhd = new Mvhd(fs, pos);
                }
            }
        }
    }
}
