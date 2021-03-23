namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetWindowAttributes : ICea708Command
    {
        public static readonly int Id = 0x97;

        public int LineIndex { get; set; }

        public const int JustifyLeft = 0;
        public const int JustifyRight = 1;
        public const int JustifyCenter = 2;
        public const int JustifyFull = 3;

        public const int PrintDirectionTop = 0;
        public const int PrintDirectionBottom = 1;
        public const int PrintDirectionCenter = 2;
        public const int PrintDirectionFull = 3;

        public int FillColorRed { get; set; }
        public int FillColorGreen { get; set; }
        public int FillColorBlue { get; set; }
        public int FillOpacity { get; set; }
        public int BorderType { get; set; }
        public int BorderColorRed { get; set; }
        public int BorderColorGreen { get; set; }
        public int BorderColorBlue { get; set; }
        public int Justify { get; set; }
        public int PrintDirection { get; set; }
        public int ScrollDirection { get; set; }
        public int Wordwrap { get; set; }
        public int DisplayEffect { get; set; }
        public int EffectDirection { get; set; }
        public int EffectSpeed { get; set; }

        public SetWindowAttributes(int justify)
        {
            Justify = justify;
            PrintDirection = PrintDirectionTop;
            ScrollDirection = 3;
            Wordwrap = 0;
            DisplayEffect = 0;
            EffectDirection = 0;
            EffectSpeed = 2;
            FillColorRed = 1;
            FillColorGreen = 1;
            FillColorBlue = 1;
            FillOpacity = 3;
            BorderType = 0;
            BorderColorRed = 1;
            BorderColorGreen = 1;
            BorderColorBlue = 1;
        }

        public SetWindowAttributes(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;

            FillColorBlue = bytes[index] & 0b00000011;
            FillColorGreen = (bytes[index] & 0b00001100) >> 2;
            FillColorRed = (bytes[index] & 0b00110000) >> 4;
            FillOpacity = (bytes[index] & 0b11000000) >> 6;

            BorderColorBlue = bytes[index + 1] & 0b00000011;
            BorderColorGreen = (bytes[index + 1] & 0b00001100) >> 2;
            BorderColorRed = (bytes[index + 1] & 0b00110000) >> 4;
            BorderType = (bytes[index + 1] & 0b11000000) >> 6;

            Justify = bytes[index + 2] & 0b00000011;
            ScrollDirection = (bytes[index + 2] & 0b00001100) >> 2;
            PrintDirection = (bytes[index + 2] & 0b00110000) >> 4;
            Wordwrap = (bytes[index + 2] & 0b11000000) >> 6;

            DisplayEffect = bytes[index + 3] & 0b00000011;
            EffectDirection = (bytes[index + 3] & 0b00001100) >> 2;
            EffectSpeed = (bytes[index + 3] & 0b11110000) >> 4;
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)Id,
                (byte) ((FillColorBlue & 0b00000011) |
                       ((FillColorGreen & 0b00000011) << 2) |
                       ((FillColorRed & 0b00000011) << 4) |
                       ((FillOpacity & 0b00000011) << 6)),
                (byte)((BorderColorBlue & 0b00000011) |
                       ((BorderColorGreen & 0b00000011) << 2) |
                       ((BorderColorRed & 0b00000011) << 4) |
                       ((BorderType & 0b00000011) << 6)),
                (byte)((Justify & 0b00000011) |
                       ((ScrollDirection & 0b00000011) << 2) |
                       ((PrintDirection & 0b00000011) << 4) |
                       ((Wordwrap & 0b00000011) << 6)),
                (byte)((DisplayEffect & 0b00000011) |
                       ((EffectDirection & 0b00000011) << 2) |
                       ((EffectSpeed & 0b00001111) << 4)),
            };
        }

    }
}
