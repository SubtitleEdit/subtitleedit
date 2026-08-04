using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class GridLinesVisibilityDisplay
{
    public SeGridLinesVisibility Type { get; }
    public string DisplayName { get; }
    public GridLinesVisibilityDisplay(SeGridLinesVisibility type, string displayName)
    {
        Type = type;
        DisplayName = displayName;
    }
    public override string ToString() => DisplayName;

    public static GridLinesVisibilityDisplay[] GetAll()
    {
        return
        [
            new GridLinesVisibilityDisplay(SeGridLinesVisibility.None, Se.Language.General.None),
            new GridLinesVisibilityDisplay(SeGridLinesVisibility.Horizontal, Se.Language.General.Horizontal),
            new GridLinesVisibilityDisplay(SeGridLinesVisibility.Vertical, Se.Language.General.Vertical),
            new GridLinesVisibilityDisplay(SeGridLinesVisibility.All, Se.Language.General.All),
        ];
    }
}