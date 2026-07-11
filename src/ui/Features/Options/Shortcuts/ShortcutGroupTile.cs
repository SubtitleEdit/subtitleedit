using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

/// <summary>
/// One clickable tile in the group filter strip. Group == null means "all groups".
/// </summary>
public class ShortcutGroupTile
{
    public ShortcutGroup? Group { get; }
    public string Name { get; }
    public string IconName { get; }
    public IBrush Brush { get; }
    public int Count { get; }

    public ShortcutGroupTile(ShortcutGroup group, int count)
    {
        Group = group;
        Name = ShortcutGroupUi.GetName(group);
        IconName = ShortcutGroupUi.GetIconName(group);
        Brush = ShortcutGroupUi.GetBrush(group);
        Count = count;
    }

    public ShortcutGroupTile(int count) // "All" tile
    {
        Group = null;
        Name = Se.Language.General.All;
        IconName = IconNames.ViewGrid;
        Brush = new SolidColorBrush(Color.Parse("#8494a4"));
        Count = count;
    }
}
