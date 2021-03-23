namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class DisplayWindows : ICea708Command
    {
        public static readonly int Id = 0x89;

        public int LineIndex { get; set; }

        public bool[] Flags { get; set; }

        public DisplayWindows(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;
            var b = bytes[index];
            Flags = new[]
            {
                (b & 0b00000001) > 0,
                (b & 0b00000010) > 0,
                (b & 0b00000100) > 0,
                (b & 0b00001000) > 0,
                (b & 0b00010000) > 0,
                (b & 0b00100000) > 0,
                (b & 0b01000000) > 0,
                (b & 0b10000000) > 0,
            };
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)Id,
                (byte)((Flags[0] ? 0b00000001 : 0) |
                       (Flags[1] ? 0b00000010 : 0) |
                       (Flags[2] ? 0b00000100 : 0) |
                       (Flags[3] ? 0b00001000 : 0) |
                       (Flags[4] ? 0b00010000 : 0) |
                       (Flags[5] ? 0b00100000 : 0) |
                       (Flags[6] ? 0b01000000 : 0) |
                       (Flags[7] ? 0b10000000 : 0)),
            };
        }
    }
}
