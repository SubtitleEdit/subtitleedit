using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class ModifySelectionRule
{
    public RuleType RuleType { get; set; }
    public string Name { get; set; }

    public string Text { get; set; }
    public bool HasText { get; set; }
    public bool HasMatchCase { get; set; }
    public bool MatchCase { get; set; }
    public bool HasMultiSelect { get; set; }

    public double Number { get; set; }
    public bool HasNumber { get; set; }
    public bool NumberDecimals { get; set; }
    public double NumberMinValue { get; set; }
    public double NumberMaxValue { get; set; }
    public double DefaultValue { get; set; }
    public List<MultiSelectItem> MultiSelectItems { get; set; }

    public ModifySelectionRule()
    {
        Name = string.Empty;
        Text = string.Empty;
        HasText = false;
        MatchCase = false;
        HasMatchCase = false;
        Number = 0;
        HasNumber = false;
        NumberDecimals = false;
        NumberMinValue = 0;
        NumberMaxValue = 0;
        DefaultValue = 0;
        MultiSelectItems = new List<MultiSelectItem>();
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<ModifySelectionRule> List(List<SubtitleLineViewModel> lines)
    {
        var l = Se.Language.Edit.ModifySelection;
        var g = Se.Language.General;

        var list = new List<ModifySelectionRule>
        {
            new()
            {
                RuleType = RuleType.Contains,
                Name = l.Contains,
                HasText = true,
                HasMatchCase = true,
            },
            new()
            {
                RuleType = RuleType.StartsWith,
                Name = l.StartsWith,
                HasText = true,
                HasMatchCase = true,
            },
            new()
            {
                RuleType = RuleType.EndsWith,
                Name = l.EndsWith,
                HasText = true,
                HasMatchCase = true,
            },
            new()
            {
                RuleType = RuleType.NotContains,
                Name = l.NotContains,
                HasText = true,
                HasMatchCase = true,
            },
            new()
            {
                RuleType = RuleType.AllUppercase,
                Name = l.AllUppercase,
            },
            new()
            {
                RuleType = RuleType.RegEx,
                Name = g.RegularExpression,
                HasText = true,
            },
            new()
            {
                RuleType = RuleType.Odd,
                Name = l.Odd,
            },
            new()
            {
                RuleType = RuleType.Even,
                Name = l.Even,
            },
            new()
            {
                RuleType = RuleType.DurationLessThan,
                Name = l.DurationLessThan,
                HasNumber = true,
                NumberDecimals = true,
                NumberMinValue = 0,
                NumberMaxValue = 10000,
                DefaultValue = 2.0,
            },
            new()
            {
                RuleType = RuleType.DurationGreaterThan,
                Name = l.DurationGreaterThan,
                HasNumber = true,
                NumberDecimals = true,
                NumberMinValue = 0,
                NumberMaxValue = 10000,
                DefaultValue = 2.0,
            },
            new()
            {
                RuleType = RuleType.CpsLessThan,
                Name = l.CpsLessThan,
                HasNumber = true,
                NumberDecimals = true,
                NumberMinValue = 0,
                NumberMaxValue = 99,
                DefaultValue = 15,
            },
            new()
            {
                RuleType = RuleType.CpsGreaterThan,
                Name = l.CpsGreaterThan,
                HasNumber = true,
                NumberDecimals = true,
                NumberMinValue = 0,
                NumberMaxValue = 99,
                DefaultValue = 20,
            },
            new()
            {
                RuleType = RuleType.LengthLessThan,
                Name = l.LengthLessThan,
                HasNumber = true,
                NumberMinValue = 0,
                NumberMaxValue = 200,
                DefaultValue = 42,
            },
            new()
            {
                RuleType = RuleType.LengthGreaterThan,
                Name = l.LengthGreaterThan,
                HasNumber = true,
                NumberMinValue = 0,
                NumberMaxValue = 200,
                DefaultValue = 42,
            },
            new()
            {
                RuleType = RuleType.ExactlyOneLine,
                Name = l.ExactlyOneLine,
            },
            new()
            {
                RuleType = RuleType.ExactlyTwoLines,
                Name = l.ExactlyTwoLines,
            },
            new()
            {
                RuleType = RuleType.MoreThanTwoLines,
                Name = l.MoreThanTwoLines,
            },
            new()
            {
                RuleType = RuleType.Bookmarked,
                Name = l.Bookmarked,
            },
            new()
            {
                RuleType = RuleType.BookmarkContains,
                Name = l.BookmarkContains,
                HasText = true,
                HasMatchCase = true,
            },
            new()
            {
                RuleType = RuleType.BlankLines,
                Name = l.BlankLines,
            },
            new()
            {
                RuleType = RuleType.Style,
                Name = g.Style,
                HasMultiSelect = true,
                MultiSelectItems = lines
                    .Where(p=>!string.IsNullOrEmpty(p.Style))
                    .DistinctBy(p => p.Style)
                    .Select(p=> new MultiSelectItem(p.Style))
                    .OrderBy(s => s.Name)
                    .ToList(),
            },
            new()
            {
                RuleType = RuleType.Actor,
                Name = g.Actor,
                HasMultiSelect = true,
                MultiSelectItems = lines
                    .Where(p=>!string.IsNullOrEmpty(p.Actor))
                    .DistinctBy(p => p.Style)
                    .Select(p=> new MultiSelectItem(p.Actor))
                    .OrderBy(s => s.Name)
                    .ToList(),
            }
        };

        return list;
    }

    internal bool IsMatch(SubtitleLineViewModel item)
    {
        // Evaluate based on RuleType
        var text = item.Text ?? string.Empty;

        switch (RuleType)
        {
            case RuleType.Contains:
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                return HasMatchCase && MatchCase ? text.Contains(Text) : text.IndexOf(Text, StringComparison.OrdinalIgnoreCase) >= 0;

            case RuleType.StartsWith:
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                return HasMatchCase && MatchCase ? text.StartsWith(Text) : text.StartsWith(Text, StringComparison.OrdinalIgnoreCase);

            case RuleType.EndsWith:
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                return HasMatchCase && MatchCase ? text.EndsWith(Text) : text.EndsWith(Text, StringComparison.OrdinalIgnoreCase);

            case RuleType.NotContains:
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                return HasMatchCase && MatchCase ? !text.Contains(Text) : text.IndexOf(Text, StringComparison.OrdinalIgnoreCase) < 0;

            case RuleType.AllUppercase:
                var hasLetter = false;
                foreach (var c in text)
                {
                    if (char.IsLetter(c))
                    {
                        hasLetter = true;
                        if (char.IsLower(c))
                        {
                            return false;
                        }
                    }
                }
                return hasLetter;

            case RuleType.RegEx:
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                try
                {
                    var options = HasMatchCase && MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                    return Regex.IsMatch(text, Text, options);
                }
                catch
                {
                    return false;
                }

            case RuleType.Odd:
                return (item.Number % 2) == 1;

            case RuleType.Even:
                return (item.Number % 2) == 0;

            case RuleType.DurationLessThan:
                return item.Duration.TotalMilliseconds < Number;

            case RuleType.DurationGreaterThan:
                return item.Duration.TotalMilliseconds > Number;

            case RuleType.CpsLessThan:
                return item.CharactersPerSecond < Number;

            case RuleType.CpsGreaterThan:
                return item.CharactersPerSecond > Number;

            case RuleType.LengthLessThan:
                return (text?.Length ?? 0) < (int)Number;

            case RuleType.LengthGreaterThan:
                return (text?.Length ?? 0) > (int)Number;

            case RuleType.ExactlyOneLine:
                return (text?.Split('\n').Length ?? 0) == 1;

            case RuleType.ExactlyTwoLines:
                return (text?.Split('\n').Length ?? 0) == 2;

            case RuleType.MoreThanTwoLines:
                return (text?.Split('\n').Length ?? 0) > 2;

            case RuleType.Bookmarked:
                return !string.IsNullOrEmpty(item.Bookmark);

            case RuleType.BookmarkContains:
                if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(item.Bookmark))
                {
                    return false;
                }

                return HasMatchCase && MatchCase ? item.Bookmark.Contains(Text) : item.Bookmark.IndexOf(Text, StringComparison.OrdinalIgnoreCase) >= 0;

            case RuleType.BlankLines:
                return string.IsNullOrWhiteSpace(text);

            case RuleType.Style:
                if (string.IsNullOrEmpty(item.Style))
                {
                    return false;
                }

                return MultiSelectItems.Any(p => p.Apply && p.Name == item.Style);

            case RuleType.Actor:
                if (string.IsNullOrEmpty(item.Actor))
                {
                    return false;
                }

                return MultiSelectItems.Any(p => p.Apply && p.Name == item.Actor);

            default:
                return false;
        }
    }
}
