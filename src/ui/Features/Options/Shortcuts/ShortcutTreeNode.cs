using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _activeIn;
    [ObservableProperty] private string _groupName;
    [ObservableProperty] private string _groupIconName;
    [ObservableProperty] private IBrush _groupBrush;
    [ObservableProperty] private IBrush _groupSoftBrush;
    [ObservableProperty] private IBrush _groupTextBrush;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _displayShortcut;
    [ObservableProperty] private List<string> _keyParts;
    [ObservableProperty] private bool _isAssigned;
    [ObservableProperty] private bool _isUnassigned;
    [ObservableProperty] private double _activeInOpacity;

    public ShortCut? ShortCut { get; set; }

    // The grid's shortcut column renders KeyParts/IsAssigned/IsUnassigned, so they
    // must follow every DisplayShortcut update (e.g. from the edit panel), not just
    // the values computed in the constructor.
    partial void OnDisplayShortcutChanged(string value)
    {
        KeyParts = string.IsNullOrEmpty(value)
            ? new List<string>()
            : new List<string>(value.Split(" + ", StringSplitOptions.None));
        IsAssigned = KeyParts.Count > 0;
        IsUnassigned = !IsAssigned;
    }

    public ShortcutTreeNode(string activeIn, string title, string displayShortCut, ShortCut shortcut)
    {
        ActiveIn = activeIn;
        Title = title;
        ShortCut = shortcut;
        DisplayShortcut = displayShortCut;
        KeyParts = string.IsNullOrEmpty(displayShortCut)
            ? new List<string>()
            : new List<string>(displayShortCut.Split(" + ", StringSplitOptions.None));
        IsAssigned = KeyParts.Count > 0;
        IsUnassigned = !IsAssigned;
        GroupName = ShortcutGroupUi.GetName(shortcut.Group);
        GroupIconName = ShortcutGroupUi.GetIconName(shortcut.Group);
        GroupBrush = ShortcutGroupUi.GetBrush(shortcut.Group);
        GroupSoftBrush = ShortcutGroupUi.GetSoftBrush(shortcut.Group);
        GroupTextBrush = ShortcutGroupUi.GetTextBrush(shortcut.Group);
        // "Everywhere" pills are dimmed so context-scoped shortcuts stand out while scrolling.
        ActiveInOpacity = shortcut.Category == ShortcutCategory.General ? 0.55 : 1.0;
    }
}
