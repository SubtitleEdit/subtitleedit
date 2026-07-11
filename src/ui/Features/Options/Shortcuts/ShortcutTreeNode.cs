using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _activeIn;
    [ObservableProperty] private string _groupName;
    [ObservableProperty] private string _groupIconName;
    [ObservableProperty] private IBrush _groupBrush;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _displayShortcut;

    public ShortCut? ShortCut { get; set; }

    public ShortcutTreeNode(string activeIn, string title, string displayShortCut, ShortCut shortcut)
    {
        ActiveIn = activeIn;
        Title = title;
        ShortCut = shortcut;
        DisplayShortcut = displayShortCut;
        GroupName = ShortcutGroupUi.GetName(shortcut.Group);
        GroupIconName = ShortcutGroupUi.GetIconName(shortcut.Group);
        GroupBrush = ShortcutGroupUi.GetBrush(shortcut.Group);
    }
}
