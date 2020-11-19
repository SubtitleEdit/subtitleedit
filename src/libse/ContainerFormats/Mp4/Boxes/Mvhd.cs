using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Movie Header Box
    /// </summary>
    public class Mvhd : Box
    {
        public readonly ulong CreationTime;
        public readonly ulong ModificationTime;
        public readonly ulong Duration;
        public readonly ulong TimeScale;

        public Mvhd(Stream fs)
        {
            Buffer = new byte[20];
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }
            int version = Buffer[0];
            if (version == 0)
            {
                CreationTime = GetUInt(4);
                ModificationTime = GetUInt(8);
                TimeScale = GetUInt(12);
                Duration = GetUInt(16);
            }
            else
            {
                CreationTime = GetUInt64(4); // 64-bit
                ModificationTime = GetUInt64(12); // 64-bit
                TimeScale = GetUInt(20); // 32-bit
                Duration = GetUInt64(24); // 64-bit
            }
        }
    }
}
