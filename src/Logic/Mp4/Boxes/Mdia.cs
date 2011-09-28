using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mdia
    {

        public Mdhd Mdhd { get; private set; }
        public Minf Minf { get; private set; }
        public readonly string HandlerType = null;

        public Mdia(FileStream fs, long maximumLength)
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
