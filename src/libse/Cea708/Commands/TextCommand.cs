namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class TextCommand : ICommand
    {
        public int LineIndex { get; set; }

        public string Content { get; set; }

        public TextCommand(int lineIndex, string content)
        {
            LineIndex = lineIndex;
            Content = content;
        }

        public byte[] GetBytes()
        {
            return Cea708.EncodeText(Content);
        }
    }
}
