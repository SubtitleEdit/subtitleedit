namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetPenAttributes : ICea708Command
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

        public SetPenAttributes(bool italic)
        {
            PenSize = 1;
            Offset = 1;
            TextTag = 0;
            FontTag = 0;
            EdgeType = 0;
            Underline = false;
            Italics = italic;
        }

        public SetPenAttributes(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;
            PenSize = bytes[index] & 0b00000011;
            Offset = (bytes[index] & 0b00001100) >> 2;
            TextTag = bytes[index] >> 4;
            FontTag = bytes[index + 1] & 0b00000111;
            EdgeType = (bytes[index + 1] & 0b00111000) >> 3;
            Underline = (bytes[index + 1] & 0b01000000) > 0;
            Italics = (bytes[index + 1] & 0b10000000) > 0;
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)Id,
                (byte)((PenSize & 0b00000011) |
                       ((Offset & 0b00000011) << 2) |
                       (TextTag << 4)),
                (byte)((FontTag & 0b00000111) |
                       ((EdgeType & 0b00000111) << 3) |
                       (Underline ? 0b01000000 : 0) |
                       (Italics ? 0b10000000 : 0)),
            };
        }
    }
}
