namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class HideWindows : ICea708Command
    {
        public static readonly int Id = 0x8A;

        public int LineIndex { get; set; }

        public bool[] Flags { get; set; }

        public HideWindows(bool hideAll)
        {
            Flags = WindowFlags.All(hideAll);
        }

        public HideWindows(int lineIndex, byte[] bytes, int index)
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
