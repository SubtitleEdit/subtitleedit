namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class ClearWindows : CommandBase
    {
        public bool[] Flags { get; set; }

        public ClearWindows(int lineIndex, byte[] bytes, int index)
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
    }
}
