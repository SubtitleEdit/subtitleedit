namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class Reset : ICea708Command
    {
        public static readonly int Id = 0x8F;

        public int LineIndex { get; set; }

        public Reset(int lineIndex)
        {
            LineIndex = lineIndex;
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Id };
        }
    }
}
