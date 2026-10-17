using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.CustomErrorRules;

/// <summary>One editable row in <see cref="CustomErrorRulesWindow"/>.</summary>
public partial class CustomErrorRuleItem : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _pattern;
    [ObservableProperty] private bool _matchCase;
    [ObservableProperty] private Color _color;
    [ObservableProperty] private bool _isPatternValid;

    public CustomErrorRuleItem()
    {
        _isEnabled = true;
        _name = string.Empty;
        _pattern = string.Empty;
        _color = SeCustomErrorRule.DefaultColor.FromHexToColor();
        _isPatternValid = true;
    }

    public static CustomErrorRuleItem FromSetting(SeCustomErrorRule rule)
    {
        var item = new CustomErrorRuleItem
        {
            IsEnabled = rule.Enabled,
            Name = rule.Name ?? string.Empty,
            Pattern = rule.Pattern ?? string.Empty,
            MatchCase = rule.MatchCase,
            Color = (string.IsNullOrEmpty(rule.Color) ? SeCustomErrorRule.DefaultColor : rule.Color).FromHexToColor(),
        };
        item.Validate();
        return item;
    }

    public SeCustomErrorRule ToSetting()
    {
        return new SeCustomErrorRule
        {
            Enabled = IsEnabled,
            Name = Name.Trim(),
            Pattern = Pattern,
            MatchCase = MatchCase,
            Color = Color.FromColorToHex(),
        };
    }

    partial void OnPatternChanged(string value) => Validate();

    partial void OnMatchCaseChanged(bool value) => Validate();

    private void Validate()
    {
        IsPatternValid = string.IsNullOrEmpty(Pattern) || CustomErrorRuleMatcher.TryCreateRegex(Pattern, MatchCase) != null;
    }

    // Screen readers announce list items through ToString().
    public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Pattern : Name;
}
