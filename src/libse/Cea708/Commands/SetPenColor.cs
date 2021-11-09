namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetPenColor : ICea708Command
    {
        public static readonly int Id = 0x91;

        public int LineIndex { get; set; }

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
        public int EdgeOpacity => ForegroundOpacity;

        public SetPenColor()
        {
            ForegroundColorRed = 2;
            ForegroundColorGreen = 2;
            ForegroundColorBlue = 2;
            ForegroundOpacity = 0;

            BackgroundColorRed = 0;
            BackgroundColorGreen = 0;
            BackgroundColorBlue = 0;
            BackgroundOpacity = 0;

            EdgeColorRed = 1;
            EdgeColorGreen = 1;
            EdgeColorBlue = 1;
        }

        public SetPenColor(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;

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
            EdgeColorRed = (bytes[index + 2] & 0b11110000) >> 4;
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)Id,
                (byte)((ForegroundColorBlue & 0b00000011) |
                       ((ForegroundColorGreen & 0b00000011) << 2) |
                       ((ForegroundColorRed & 0b00000011) << 4) |
                       ((ForegroundOpacity & 0b00000011) << 6)),
                (byte) ((BackgroundColorBlue & 0b00000011) |
                       ((BackgroundColorGreen & 0b00000011) << 2) |
                       ((BackgroundColorRed & 0b00000011) << 4) |
                       ((BackgroundOpacity & 0b00000011) << 6)),
                (byte)  ((EdgeColorBlue & 0b00000011) |
                        ((EdgeColorGreen & 0b00000011) << 2) |
                        ((EdgeColorRed & 0b00000011) << 4)),
            };
        }
    }
}
