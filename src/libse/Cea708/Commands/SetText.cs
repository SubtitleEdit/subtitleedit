namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class SetText : ICea708Command
    {
        public int LineIndex { get; set; }

        public string Content { get; set; }

        public SetText(string content)
        {
            Content = content;
        }

        public SetText(int lineIndex, string content)
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
