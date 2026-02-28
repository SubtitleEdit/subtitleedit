using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _category;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _displayShortcut;

    public ShortCut? ShortCut { get; set; }

    public ShortcutTreeNode(string category, string title, string displayShortCut, ShortCut shortcut)
    {
        Category = category;
        Title = title;
        ShortCut = shortcut;
        DisplayShortcut = displayShortCut;
    }
}