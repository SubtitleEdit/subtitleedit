using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public partial class RegexRuleItem : ObservableObject
{
    [ObservableProperty] private string _find;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private bool _isEnabled;

    public bool IsUserRule { get; }
    // Called when the check box is toggled; returns false to put the old value back.
    public Func<RegexRuleItem, bool, bool>? EnabledChanging { get; set; }

    private bool _isReverting;

    public RegexRuleItem(string find, string replaceWith, bool isUserRule, bool isEnabled)
    {
        _find = find;
        _replaceWith = replaceWith;
        _isEnabled = isEnabled;
        IsUserRule = isUserRule;
    }

    public void SetEnabledWithoutSaving(bool isEnabled)
    {
        _isReverting = true;
        IsEnabled = isEnabled;
        _isReverting = false;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        if (_isReverting || EnabledChanging == null || EnabledChanging(this, value))
        {
            return;
        }

        Dispatcher.UIThread.Post(() => SetEnabledWithoutSaving(!value));
    }

    public override string ToString() => $"{Find} -> {ReplaceWith}";
}
