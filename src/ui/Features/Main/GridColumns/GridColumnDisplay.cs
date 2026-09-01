using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Main.GridColumns;

/// <summary>
/// One row in the "Columns..." dialog. An entry can cover more than one grid column
/// key when two columns are format-gated variants of the same logical column and
/// share a show/hide toggle (Style/WebVttStyle, Actor/WebVttVoice) - they move as one.
/// </summary>
public partial class GridColumnDisplay : ObservableObject
{
    [ObservableProperty] private bool _isVisible;

    public string Name { get; }

    /// <summary>
    /// False for columns the user cannot show/hide: Number and Text are always shown,
    /// and the teletext/original-text columns come and go with the loaded content.
    /// </summary>
    public bool CanToggle { get; }

    /// <summary>Grid column keys (SubtitleGridColumnKeys) this entry covers, in default order.</summary>
    public string[] Keys { get; }

    public GridColumnDisplay(string name, bool canToggle, bool isVisible, params string[] keys)
    {
        Name = name;
        CanToggle = canToggle;
        IsVisible = isVisible;
        Keys = keys;
    }
}
