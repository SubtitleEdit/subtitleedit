namespace Nikse.SubtitleEdit.Core.SpellCheck
{
    public class SpellCheckWord
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public int Length => Text.Length;
    }
}
