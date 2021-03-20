namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class HideWindows : ICommand
    {
        public static readonly int Id = 0x8A;

        public int LineIndex { get; set; }

        public bool[] Flags { get; set; }

        public HideWindows(int lineIndex, byte[] bytes, int index)
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
            throw new System.NotImplementedException();
        }
    }
}
