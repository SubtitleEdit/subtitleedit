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
            Flags = WindowFlags.FromByte(bytes[index]);
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id, WindowFlags.ToByte(Flags) };
        }
    }
}
