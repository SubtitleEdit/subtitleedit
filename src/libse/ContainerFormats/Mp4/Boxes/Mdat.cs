using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Data Box
    /// </summary>
    public class Mdat : Box
    {
        public List<Vttc.VttData> Vtts { get; set; }

        public Mdat(Stream fs, ulong maximumLength)
        {
            Vtts = new List<Vttc.VttData>();
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "vttc")
                {
                    var vttc = new Vttc(fs, Position);
                    Vtts.Add(vttc.Data);
                }
                else if (Name == "vtte")
                {
                    var data = new Vttc.VttData();
                    Vtts.Add(data);
                    data.Payload = null;
                    data.PayloadSize = (int)Size;
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }
    }
}
