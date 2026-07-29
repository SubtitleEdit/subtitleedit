using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveUnicodeCharacters;

public partial class RemoveUnicodeCharacterItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked = true;
    [ObservableProperty] private string _replaceWith = string.Empty;

    /// <summary>The character (one rune - may be a surrogate pair, e.g. an emoji).</summary>
    public string Character { get; init; } = string.Empty;

    /// <summary>Code point display, e.g. "U+266A".</summary>
    public string CodeDisplay { get; init; } = string.Empty;

    public int Count { get; init; }

    /// <summary>Line numbers the character occurs in, e.g. "1, 5, 7".</summary>
    public string LinesDisplay { get; init; } = string.Empty;
}
