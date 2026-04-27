namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckWord
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Length => Text.Length;
}
