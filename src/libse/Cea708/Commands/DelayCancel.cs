namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class DelayCancel : ICea708Command
    {
        public static readonly int Id = 0x8E;

        public int LineIndex { get; set; }

        public DelayCancel(int lineIndex)
        {
            LineIndex = lineIndex;
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id };
        }
    }
}
