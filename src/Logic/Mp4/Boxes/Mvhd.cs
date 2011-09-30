using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mvhd : Box
    {

        public readonly uint Duration;
        public readonly uint TimeScale;

        public Mvhd(FileStream fs, ulong maximumLength)
        {
            buffer = new byte[20];
            int bytesRead = fs.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length)
                return;

            uint TimeScale = GetUInt(12);
            uint Duration = GetUInt(16);
        }

    }
}
