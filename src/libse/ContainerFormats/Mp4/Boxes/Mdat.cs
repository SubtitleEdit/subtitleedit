using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Data Box
    /// </summary>
    public class Mdat : Box
    {
        public List<string> Payloads { get; set; }

        public Mdat(Stream fs, ulong maximumLength)
        {
            Payloads = new List<string>();
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "vttc")
                {
                    var vttc = new Vttc(fs, Position);
                    if (vttc.Payload != null)
                    {
                        Payloads.AddRange(vttc.Payload);
                    }
                }
                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
