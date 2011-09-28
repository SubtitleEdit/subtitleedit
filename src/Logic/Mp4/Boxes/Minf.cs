using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Minf
    {

        public Stbl Stbl { get; private set; }

        public Minf(FileStream fs, long maximumLength, UInt32 timeScale)
        {
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
                if (name == "stbl")
                {
                    Stbl = new Stbl(fs, pos, timeScale);
                }
            }
        }

    }
}
