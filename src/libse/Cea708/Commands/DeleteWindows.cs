namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class DeleteWindows : ICea708Command
    {
        public static readonly int Id = 0x8C;

        public int LineIndex { get; set; }

        public bool[] Flags { get; set; }

        public DeleteWindows(bool deleteAll)
        {
            Flags = WindowFlags.All(deleteAll);
        }

        public DeleteWindows(int lineIndex, byte[] bytes, int index)
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
