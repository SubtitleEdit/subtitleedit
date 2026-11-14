namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class ToggleWindows : ICea708Command
    {
        public static readonly int Id = 0x8B;

        public int LineIndex { get; set; }

        public bool[] Flags { get; set; }

        public ToggleWindows(bool toggleAll)
        {
            Flags = WindowFlags.All(toggleAll);
        }

        public ToggleWindows(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;

            Flags = WindowFlags.FromByte(bytes[index]);
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id, WindowFlags.ToByte(Flags) };
        }
    }
}
