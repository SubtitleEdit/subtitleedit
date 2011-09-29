using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mdia
    {

        public Mdhd Mdhd { get; private set; }
        public Minf Minf { get; private set; }
        public readonly string HandlerType = null;

        public Mdia(FileStream fs, ulong maximumLength)
        {
            var buffer = new byte[8];
            ulong pos = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                int bytesRead;
                fs.Seek((long)pos, SeekOrigin.Begin);
                ulong size = Helper.ReadSize(buffer, out bytesRead, fs);
                if (bytesRead < 8)
                    return;
                string name = Helper.GetString(buffer, 4, 4);
                pos = ((ulong)(fs.Position)) + size - 8;
                if (name == "minf")
                {
                    UInt32 timeScale = 90000;
                    if (Mdhd != null)
                        timeScale = Mdhd.TimeScale;
                    Minf = new Minf(fs, pos, timeScale);
                }
                else if (name == "hdlr")
                {

                    byte[] b = new byte[size - 4];
                    fs.Read(b, 0, b.Length);
                    HandlerType = Helper.GetString(b, 8, 4);
                }
                else if (name == "mdhd")
                {
                    Mdhd = new Mdhd(fs, size);
                }

            }
        }

    }
}
