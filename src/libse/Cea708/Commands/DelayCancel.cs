namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class DelayCancel : ICommand
    {
        public static readonly int Id = 0x8E;

        public int LineIndex { get; set; }

        public DelayCancel(int lineIndex)
        {
            LineIndex = lineIndex;
        }

        public byte[] GetBytes()
        {
            throw new System.NotImplementedException();
        }
    }
}
