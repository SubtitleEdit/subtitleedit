namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckSuggestionItem
{
    public string Text { get; }

    public SpellCheckSuggestionItem(string text) => Text = text;

    public override string ToString() => Text;
}
