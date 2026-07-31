namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Which grid lines the subtitle grids draw. The member names double as the
/// persisted setting values (Appearance.GridLinesAppearance) and match the old
/// DataGridGridLinesVisibility names, so existing settings keep working.
/// </summary>
public enum SeGridLinesVisibility
{
    None,
    Horizontal,
    Vertical,
    All,
}
