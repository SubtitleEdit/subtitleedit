namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetPenAttributes : ICommand
    {
        public static readonly int Id = 0x90;

        public int LineIndex { get; set; }

        public int PenSize { get; set; }
        public int Offset { get; set; }
        public int TextTag { get; set; }
        public int FontTag { get; set; }
        public int EdgeType { get; set; }
        public bool Underline { get; set; }
        public bool Italics { get; set; }

        public SetPenAttributes(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;
            PenSize = bytes[index] & 0b00000011;
            Offset = (bytes[index] & 0b00001100) >> 2;
            TextTag = bytes[index] >> 4;
            FontTag = bytes[index + 1] & 0b00000111;
            EdgeType = (bytes[index + 1] & 0b00111000) >> 2;
            Underline = (bytes[index + 1] & 0b01000000) > 0;
            Italics = (bytes[index + 1] & 0b10000000) > 0;
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
