using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Web VTT Configuration Box
    /// </summary>
    public class Vttc : Box
    {

        public List<string> Payload { get; set; }

        public Vttc(Stream fs, ulong maximumLength)
        {
            Payload = new List<string>();
            long max = (long)maximumLength;
            int count = 0;
            while (fs.Position < max)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "payl")
                {
                    var length = (int)(max - fs.Position);
                    if (length > 0 && length < 5000)
                    {
                        var buffer = new byte[length];
                        fs.Read(buffer, 0, length);
                        var s = Encoding.UTF8.GetString(buffer);
                        s = string.Join(Environment.NewLine, s.SplitToLines());
                        Payload.Add(s.Trim());
                        count++;
                    }
                    else
                    {
                        Payload.Add(string.Empty);
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
            if (count == 0)
            {
                Payload.Add(null);
            }
        }
    }
}
