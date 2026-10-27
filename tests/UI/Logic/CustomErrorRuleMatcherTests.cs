using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.CustomErrorRules;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Custom error rules (#10136): a line whose text matches an enabled rule is an error -
/// colored in the grid, listed by List errors, found by Go to next error.
/// </summary>
public class CustomErrorRuleMatcherTests
{
    [Fact]
    public void MatchingLine_IsErrorWithRuleName()
    {
        RunWithRules(general =>
        {
            general.CustomErrorRules.Add(new SeCustomErrorRule { Name = "Review", Pattern = @"\[(CHECK|TODO)\]" });

            var marked = MakeLine("[CHECK] Hello there");
            var clean = MakeLine("Hello there");

            Assert.True(marked.HasErrors(null, null));
            Assert.False(clean.HasErrors(null, null));

            var error = Assert.Single(marked.GetErrorList(null, null));
            Assert.Equal(LineErrorType.CustomRule, error.Type);
            Assert.Equal("Review", error.Detail);
            Assert.Contains("Review", marked.AccessibleErrorText);
        });
    }

    [Fact]
    public void DisabledRuleOrSwitchOff_DoesNotMatch()
    {
        RunWithRules(general =>
        {
            general.CustomErrorRules.Add(new SeCustomErrorRule { Pattern = "TODO", Enabled = false });
            Assert.False(MakeLine("TODO fix").HasErrors(null, null));

            general.CustomErrorRules[0].Enabled = true;
            CustomErrorRuleMatcher.Invalidate();
            Assert.True(MakeLine("TODO fix").HasErrors(null, null));

            general.ColorCustomErrorRules = false;
            CustomErrorRuleMatcher.Invalidate();
            Assert.False(MakeLine("TODO fix").HasErrors(null, null));
        });
    }

    [Fact]
    public void MatchCase_IsHonored_AndInvalidPatternIsIgnored()
    {
        RunWithRules(general =>
        {
            general.CustomErrorRules.Add(new SeCustomErrorRule { Pattern = "[unclosed" });
            general.CustomErrorRules.Add(new SeCustomErrorRule { Pattern = "Verify", MatchCase = true });

            Assert.True(MakeLine("Verify this").HasErrors(null, null));
            Assert.False(MakeLine("verify this").HasErrors(null, null));
        });
    }

    [Fact]
    public void FirstMatchingRule_DecidesTheColor()
    {
        RunWithRules(general =>
        {
            general.CustomErrorRules.Add(new SeCustomErrorRule { Name = "A", Pattern = "x", Color = "#ff00ff00" });
            general.CustomErrorRules.Add(new SeCustomErrorRule { Name = "B", Pattern = "x", Color = "#ffff0000" });

            Assert.Equal("A", MakeLine("x").GetMatchingCustomErrorRule()!.DisplayName);
        });
    }

    [Fact]
    public void ViewModel_DropsRulesWithoutPattern_AndFlagsInvalidPattern()
    {
        var vm = new CustomErrorRulesViewModel();
        vm.Initialize(true, [new SeCustomErrorRule { Name = "Keep", Pattern = "abc" }]);
        vm.AddCommand.Execute(null);

        Assert.Equal(2, vm.Rules.Count);
        Assert.Same(vm.Rules[1], vm.SelectedRule);

        vm.SelectedRule!.Pattern = "(";
        Assert.False(vm.SelectedRule.IsPatternValid);
        vm.SelectedRule.Pattern = string.Empty;

        var rules = vm.GetRules();
        Assert.Equal("Keep", Assert.Single(rules).Name);
    }

    [Fact]
    public void ViewModel_TestText_ReportsMatch()
    {
        var vm = new CustomErrorRulesViewModel();
        vm.Initialize(true, [new SeCustomErrorRule { Pattern = @"\[CHECK\]" }]);

        vm.TestText = "[CHECK] hi";
        Assert.Equal(Se.Language.Options.Settings.CustomErrorRuleTestMatch, vm.TestResult);

        vm.TestText = "hi";
        Assert.Equal(Se.Language.Options.Settings.CustomErrorRuleTestNoMatch, vm.TestResult);
    }

    private static SubtitleLineViewModel MakeLine(string text)
    {
        return new SubtitleLineViewModel(new Paragraph(text, 0, 2000), null!);
    }

    // Only the custom rules may color a line here, so every built-in rule is switched off.
    private static void RunWithRules(Action<SeGeneral> test)
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorDurationTooShort = false;
            general.ColorDurationTooLong = false;
            general.ColorTextTooLong = false;
            general.ColorTextTooWide = false;
            general.ColorTextTooManyLines = false;
            general.ColorCharactersPerSecond = false;
            general.ColorWordsPerMinute = false;
            general.ColorTimeCodeOverlap = false;
            general.ColorGapTooShort = false;
            general.ColorCustomErrorRules = true;
            CustomErrorRuleMatcher.Invalidate();

            test(general);
        }
        finally
        {
            Se.Settings = originalSettings;
            CustomErrorRuleMatcher.Invalidate();
        }
    }
}
