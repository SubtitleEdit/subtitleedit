using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Header Box
    /// </summary>
    public class Tkhd : Box
    {
        public readonly uint TrackId;
        public readonly ulong Duration;
        public readonly uint Width;
        public readonly uint Height;

        public Tkhd(Stream fs)
        {
            Buffer = new byte[84];
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            int version = Buffer[0];
            int addToIndex64Bit = 0;
            if (version == 1)
            {
                addToIndex64Bit = 8;
            }

            TrackId = GetUInt(12 + addToIndex64Bit);
            if (version == 1)
            {
                Duration = GetUInt64(20 + addToIndex64Bit);
                addToIndex64Bit += 4;
            }
            else
            {
                Duration = GetUInt(20 + addToIndex64Bit);
            }

            Width = (uint)GetWord(76 + addToIndex64Bit); // skip decimals
            Height = (uint)GetWord(80 + addToIndex64Bit); // skip decimals
        }
    }
}
