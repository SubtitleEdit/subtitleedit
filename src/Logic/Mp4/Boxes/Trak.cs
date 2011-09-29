using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Trak
    {

        public Mdia Mdia { get; private set; }

        public Trak(FileStream fs, ulong maximumLength)
        {
            var buffer = new byte[8];
            ulong pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                int bytesRead;
                fs.Seek((long)pos, SeekOrigin.Begin);
                ulong size = Helper.ReadSize(buffer, out bytesRead, fs);
                if (bytesRead < buffer.Length)
                    return;
                string name = Helper.GetString(buffer, 4, 4);

                pos = ((ulong)(fs.Position)) + size - 8;
                if (name == "mdia")
                {
                    Mdia = new Mdia(fs, pos);
                }
            }
        }
    }
}
