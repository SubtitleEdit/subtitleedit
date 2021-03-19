namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class CurrentWindow : CommandBase
    {
        public int WindowIndex { get; set; }

        public CurrentWindow(int lineIndex, int index)
        {
            LineIndex = lineIndex;
            WindowIndex = index;
        }
    }
}
