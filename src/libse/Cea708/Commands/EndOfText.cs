namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class EndOfText : ICea708Command
    {
        public static readonly int Id = 0x03;

        public int LineIndex { get; set; }

        public int Milliseconds { get; set; }

        public EndOfText(int lineIndex)
        {
            LineIndex = lineIndex;
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id };
        }
    }
}
