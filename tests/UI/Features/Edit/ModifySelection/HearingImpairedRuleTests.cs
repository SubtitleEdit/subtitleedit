using Nikse.SubtitleEdit.Features.Edit.ModifySelection;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Edit.ModifySelection;

public class HearingImpairedRuleTests
{
    [Fact]
    public void HearingImpairedRule_MatchesHearingImpairedLinesOnly()
    {
        WithDefaultSettings(() =>
        {
            var lines = MakeLines("[DOOR SLAMS]", "(sighs)", "Hello there, how are you?");
            var rule = MakeRule(lines);

            Assert.True(rule.IsMatch(lines[0]));
            Assert.True(rule.IsMatch(lines[1]));
            Assert.False(rule.IsMatch(lines[2]));
        });
    }

    [Fact]
    public void HearingImpairedRule_MatchesNarratorName()
    {
        WithDefaultSettings(() =>
        {
            var lines = MakeLines("MAN: Get out of here!");
            var rule = MakeRule(lines);

            Assert.True(rule.IsMatch(lines[0]));
        });
    }

    [Fact]
    public void HearingImpairedRule_WithNoOptionsTicked_MatchesNothing()
    {
        WithDefaultSettings(() =>
        {
            var lines = MakeLines("[DOOR SLAMS]", "(sighs)");
            var rule = MakeRule(lines);
            rule.HearingImpairedOptions = new HearingImpairedRuleOptions();

            Assert.All(lines, line => Assert.False(rule.IsMatch(line)));
        });
    }

    [Fact]
    public void HearingImpairedRule_UntickingAnOption_TakesEffectImmediately()
    {
        WithDefaultSettings(() =>
        {
            var lines = MakeLines("[DOOR SLAMS]", "(sighs)");
            var rule = MakeRule(lines);
            Assert.True(rule.IsMatch(lines[1]));

            rule.HearingImpairedOptions!.Parentheses = false;

            Assert.False(rule.IsMatch(lines[1]));
            Assert.True(rule.IsMatch(lines[0]));
        });
    }

    [Fact]
    public void HearingImpairedRule_TakesOptionsFromRemoveTextForHiSettings()
    {
        WithDefaultSettings(() =>
        {
            Se.Settings.Tools.RemoveTextForHi.IsRemoveParenthesesOn = false;
            Se.Settings.Tools.RemoveTextForHi.IsRemoveTextUppercaseLineOn = true;

            var rule = MakeRule(MakeLines("Hello there, how are you?"));

            Assert.False(rule.HearingImpairedOptions!.Parentheses);
            Assert.True(rule.HearingImpairedOptions.Brackets);
            Assert.True(rule.HearingImpairedOptions.UppercaseLine);
        });
    }

    private static ModifySelectionRule MakeRule(List<SubtitleLineViewModel> lines)
    {
        return ModifySelectionRule.List(lines).Single(r => r.RuleType == RuleType.HearingImpaired);
    }

    private static List<SubtitleLineViewModel> MakeLines(params string[] texts)
    {
        var lines = new List<SubtitleLineViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            lines.Add(new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = texts[i],
                StartTime = TimeSpan.FromSeconds(i * 2),
                EndTime = TimeSpan.FromSeconds(i * 2 + 1.5),
            });
        }

        return lines;
    }

    private static void WithDefaultSettings(Action action)
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            action();
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }
}
