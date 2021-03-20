namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetCurrentWindow : ICommand
    {
        public static readonly int IdStart = 0x80;
        public static readonly int IdEnd = 0x87;

        public int LineIndex { get; set; }
        public int WindowIndex { get; set; }

        public SetCurrentWindow(int lineIndex, int index)
        {
            LineIndex = lineIndex;
            WindowIndex = index;
        }

        public byte[] GetBytes()
        {
            throw new System.NotImplementedException();
        }
    }
}
