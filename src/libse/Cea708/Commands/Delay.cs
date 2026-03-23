namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class Delay : ICea708Command
    {
        public static readonly int Id = 0x8D;

        public int LineIndex { get; set; }

        public int Milliseconds { get; set; }

        public Delay(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;
            Milliseconds = bytes[index] * 100;
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id, (byte)(Milliseconds / 100) };
        }
    }
}
