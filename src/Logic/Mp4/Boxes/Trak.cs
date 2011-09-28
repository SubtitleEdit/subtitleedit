using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Trak
    {

        public Mdia Mdia { get; private set; }

        public Trak(FileStream fs, long maximumLength)
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
                if (name == "mdia")
                {
                    Mdia = new Mdia(fs, pos);
                }
            }
        }
    }
}
