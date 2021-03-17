namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class PenColor
    {
        public int ForegroundColorBlue { get; set; }
        public int ForegroundColorGreen { get; set; }
        public int ForegroundColorRed { get; set; }
        public int ForegroundOpacity { get; set; } // SOLID=0, FLASH=1, TRANSLUCENT=2

        public int BackgroundColorBlue { get; set; }
        public int BackgroundColorGreen { get; set; }
        public int BackgroundColorRed { get; set; }
        public int BackgroundOpacity { get; set; }

        public int EdgeColorBlue { get; set; }
        public int EdgeColorGreen { get; set; }
        public int EdgeColorRed { get; set; }
        public int EdgeOpacity { get; set; }


        public PenColor(byte[] bytes, int index)
        {
            ForegroundColorBlue = bytes[index] & 0b00000011;
            ForegroundColorGreen = (bytes[index] & 0b00001100) >> 2;
            ForegroundColorRed = (bytes[index] & 0b00110000) >> 4;
            ForegroundOpacity = bytes[index] >> 6;

            BackgroundColorBlue = bytes[index + 1] & 0b00000011;
            BackgroundColorGreen = (bytes[index + 1] & 0b00001100) >> 2;
            BackgroundColorRed = (bytes[index + 1] & 0b00110000) >> 4;
            BackgroundOpacity = bytes[index + 1] >> 6;

            EdgeColorBlue = bytes[index + 2] & 0b00000011;
            EdgeColorGreen = (bytes[index + 2] & 0b00001100) >> 2;
            EdgeColorRed = (bytes[index + 2] & 0b00110000) >> 4;
            EdgeOpacity = ForegroundOpacity;
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)0
            };
        }
    }
}
