using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Vttc : Box
    {

        public string Payload { get; set; }

        public Vttc(FileStream fs, ulong maximumLength)
        {
            long max = (long)maximumLength;
            StringBuilder sb = null;
            while (fs.Position < max)
            {
                if (!InitializeSizeAndName(fs))
                    return;

                if (Name == "payl")
                {
                    Console.WriteLine("payl");
                    var length = (int) (max - fs.Position);
                    if (length > 0 && length < 5000)
                    {
                        if (sb == null)
                            sb = new StringBuilder();

                        var buffer = new byte[length];
                        fs.Read(buffer, 0, length);
                        var s = Encoding.UTF8.GetString(buffer);
                        sb.AppendLine();
                        sb.AppendLine(s);
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
            if (sb != null)
                Payload = sb.ToString().Trim();
        }
    }
}
