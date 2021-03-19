namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class TextCommand : CommandBase
    {
        public string Content { get; set; }

        public TextCommand(int lineIndex, string content)
        {
            LineIndex = lineIndex;
            Content = content;
        }
    }
}
