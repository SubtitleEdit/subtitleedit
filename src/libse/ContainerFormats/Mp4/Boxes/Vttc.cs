using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Web VTT Configuration Box
    /// </summary>
    public class Vttc : Box
    {
        public class VttData
        {
            public string Payload { get; set; }
            public int PayloadSize { get; set; }
            public string Style { get; set; }
        }

        public VttData Data { get; set; }

        public Vttc(Stream fs, ulong maximumLength)
        {
            Data = new VttData { PayloadSize = 8 };
            var max = (long)maximumLength;
            var count = 0;
            while (fs.Position < max)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "payl")
                {
                    var length = (int)Size - 8;
                    if (length > 0 && length < 5000)
                    {
                        var buffer = new byte[length];
                        fs.Read(buffer, 0, length);
                        var s = Encoding.UTF8.GetString(buffer);
                        s = string.Join(Environment.NewLine, s.SplitToLines());
                        Data.Payload = s.Trim();
                        Data.PayloadSize += (int)Size;
                        count++;
                    }
                    else
                    {
                        Data.Payload = string.Empty;
                    }
                }
                else if (Name == "sttg")
                {
                    var length = (int)Size-8;
                    if (length > 0 && length < 5000)
                    {
                        var buffer = new byte[length];
                        fs.Read(buffer, 0, length);
                        var s = Encoding.UTF8.GetString(buffer);
                        s = string.Join(Environment.NewLine, s.SplitToLines());
                        Data.Style = s.Trim();
                        Data.PayloadSize += (int)Size;
                        count++;
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            if (count == 0)
            {
                Data.Payload = null;
            }
        }
    }
}
