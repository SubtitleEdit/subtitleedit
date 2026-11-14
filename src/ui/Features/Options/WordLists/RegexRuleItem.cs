using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public partial class RegexRuleItem : ObservableObject
{
    [ObservableProperty] private string _find;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpellChecked))]
    private string _spellCheck;

    public string ReplaceAllFrom { get; }
    public string ReplaceAllTo { get; }

    /// <summary>A rule from "RegularExpressionsIfSpelledCorrectly": only applied when the dictionary knows the word.</summary>
    public bool IsSpellChecked => SpellCheck.Length > 0;

    public bool IsUserRule { get; }
    // Called when the check box is toggled; returns false to put the old value back.
    public Func<RegexRuleItem, bool, bool>? EnabledChanging { get; set; }

    private bool _isReverting;

    public RegexRuleItem(string find, string replaceWith, bool isUserRule, bool isEnabled,
        string spellCheck = "", string replaceAllFrom = "", string replaceAllTo = "")
    {
        _find = find;
        _replaceWith = replaceWith;
        _isEnabled = isEnabled;
        _spellCheck = spellCheck;
        ReplaceAllFrom = replaceAllFrom;
        ReplaceAllTo = replaceAllTo;
        IsUserRule = isUserRule;
    }

    public OcrFixRegexRule ToRule() =>
        new(Find, ReplaceWith, IsUserRule, IsEnabled, SpellCheck, ReplaceAllFrom, ReplaceAllTo);

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

    public override string ToString() => IsSpellChecked
        ? $"{Find} -> {ReplaceWith}  ({string.Format(Se.Language.Options.WordLists.SpellCheckWordsX, SpellCheck)})"
        : $"{Find} -> {ReplaceWith}";
}
