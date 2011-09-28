using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Moov
    {
        public Mvhd Mvhd { get; private set; }
        public List<Trak> Tracks { get; private set; }

        public Moov(FileStream fs, long maximumLength)
        {
            Tracks = new List<Trak>();

            var buffer = new byte[8];
            long pos = fs.Position;
            while (fs.Position < maximumLength)
            {
                fs.Seek(pos, SeekOrigin.Begin);
                int bytesRead = fs.Read(buffer, 0, 8);
                if (bytesRead < buffer.Length)
                    return;
                var size = Helper.GetUInt(buffer, 0);
                string name = Helper.GetString(buffer, 4, 4);

                pos = fs.Position + size - 8;
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
