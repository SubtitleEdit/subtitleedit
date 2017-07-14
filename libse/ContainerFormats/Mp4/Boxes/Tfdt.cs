using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Tfdt : Box
    {
        public ulong BaseTime { get; set; }

        public Tfdt(Stream fs, ulong size)
        {
            Buffer = new byte[size - 8];
            fs.Read(Buffer, 0, Buffer.Length);
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
                return;

            var versionAndFlags = GetUInt(0);
            var version = versionAndFlags >> 24;
            var flags = versionAndFlags & 0xFFFFFF;
            if (version == 1)
            {
                BaseTime = GetUInt(0);
                BaseTime = GetUInt(1);
                BaseTime = GetUInt(2);
                BaseTime = GetUInt(3);
                BaseTime = GetUInt(4);
                BaseTime = GetUInt(5);
                BaseTime = GetUInt(6);
            }
            else
            {
                BaseTime = GetUInt64(5);
            }
        }
    }
}
