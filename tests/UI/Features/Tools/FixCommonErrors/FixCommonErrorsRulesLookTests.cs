using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.FixCommonErrors;

/// <summary>
/// Step 1 shows how many rules are on and splits each example into its before and after.
/// </summary>
public class FixCommonErrorsRulesLookTests
{
    private static string ExpectedCount(int on, int of) => string.Format(Se.Language.Tools.FixCommonErrors.RulesEnabledXOfY, on, of);

    private static ProfileDisplayItem MakeProfile()
    {
        var rules = new[]
        {
            new FixRuleDisplayItem("Spaces", "a  b -> a b", 1, true, "x"),
            new FixRuleDisplayItem("Commas", string.Empty, 2, false, "y"),
            new FixRuleDisplayItem("Periods", "a. -> a", 3, false, "z"),
        };
        return new ProfileDisplayItem { Name = "Test", FixRules = new(rules), AllFixRules = rules.ToList() };
    }

    [AvaloniaFact]
    public void EnabledCount_FollowsTheCheckBoxes_AndIgnoresTheSearch()
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);
        var profile = MakeProfile();
        vm.SelectedProfile = profile;
        Assert.Equal(ExpectedCount(1, 3), vm.RulesEnabledText);

        profile.AllFixRules[1].IsSelected = true;
        Assert.Equal(ExpectedCount(2, 3), vm.RulesEnabledText);

        vm.SearchText = "Spaces";
        vm.ApplyRuleFilter();
        Assert.Single(profile.FixRules);
        Assert.Equal(ExpectedCount(2, 3), vm.RulesEnabledText);
    }

    [AvaloniaFact]
    public void EnabledCount_StopsFollowingAProfileThatIsNoLongerSelected()
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);
        var first = MakeProfile();
        vm.SelectedProfile = first;
        vm.SelectedProfile = MakeProfile();

        first.AllFixRules[1].IsSelected = true;

        Assert.Equal(ExpectedCount(1, 3), vm.RulesEnabledText);
    }

    [Theory]
    [InlineData("Hey you!. -> Hey you!", "Hey you!.", "Hey you!")]
    [InlineData(",, -> ,", ",,", ",")]
    [InlineData("No arrow in this one", "No arrow in this one", "")]
    public void Example_SplitsAtTheArrow(string example, string before, string after)
    {
        var rule = new FixRuleDisplayItem("Name", example, 1, true, "x");

        Assert.Equal(before, rule.ExampleBefore);
        Assert.Equal(after, rule.ExampleAfter);
    }
}
