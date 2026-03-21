using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class GridLinesVisibilityDisplay
{
    public DataGridGridLinesVisibility Type { get; }
    public string DisplayName { get; }
    public GridLinesVisibilityDisplay(DataGridGridLinesVisibility type, string displayName)
    {
        Type = type;
        DisplayName = displayName;
    }
    public override string ToString() => DisplayName;

    public static GridLinesVisibilityDisplay[] GetAll()
    {
        return
        [
            new GridLinesVisibilityDisplay(DataGridGridLinesVisibility.None, Se.Language.General.None),
            new GridLinesVisibilityDisplay(DataGridGridLinesVisibility.Horizontal, Se.Language.General.Horizontal),
            new GridLinesVisibilityDisplay(DataGridGridLinesVisibility.Vertical, Se.Language.General.Vertical),
            new GridLinesVisibilityDisplay(DataGridGridLinesVisibility.All, Se.Language.General.All),
        ];
    }
}