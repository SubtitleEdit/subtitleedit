using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.FixCommonErrors;

/// <summary>
/// The "Search rules..." box in step 1 of Fix common errors (#14893). It was created but never
/// added to the window, so the rule list could not be searched. Filtering must never lose the
/// rules it hides: they keep their tick state, and apply/save read the profile's full list.
/// </summary>
public class FixCommonErrorsRuleSearchTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static ProfileDisplayItem MakeProfile(string name)
    {
        var rules = new[]
        {
            new FixRuleDisplayItem("Fix short display times", "", 1, true, "FixShortDisplayTimes"),
            new FixRuleDisplayItem("Fix long display times", "", 1, true, "FixLongDisplayTimes"),
            new FixRuleDisplayItem("Fix short gaps", "", 1, true, "FixShortGaps"),
            new FixRuleDisplayItem("Remove unneeded spaces", "", 1, true, "FixUnneededSpaces"),
        };

        return new ProfileDisplayItem
        {
            Name = name,
            FixRules = new ObservableCollection<FixRuleDisplayItem>(rules),
            AllFixRules = rules.ToList(),
        };
    }

    private static FixCommonErrorsViewModel MakeViewModel(out ProfileDisplayItem profile)
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);
        profile = MakeProfile("Default");
        vm.Profiles.Add(profile);
        vm.SelectedProfile = profile;
        return vm;
    }

    [AvaloniaFact]
    public void SearchText_FiltersRulesByName_CaseInsensitive()
    {
        var vm = MakeViewModel(out var profile);

        vm.SearchText = "SHORT";

        Assert.Equal(new[] { "Fix short display times", "Fix short gaps" }, profile.FixRules.Select(r => r.Name));
    }

    [AvaloniaFact]
    public void ClearingSearchText_RestoresEveryRule()
    {
        var vm = MakeViewModel(out var profile);

        vm.SearchText = "gaps";
        vm.SearchText = string.Empty;

        Assert.Equal(4, profile.FixRules.Count);
        Assert.Equal(profile.AllFixRules, profile.FixRules);
    }

    [AvaloniaFact]
    public void HiddenRules_KeepTheirSelection()
    {
        var vm = MakeViewModel(out var profile);
        profile.AllFixRules.First(r => r.Name == "Fix short gaps").IsSelected = false;

        vm.SearchText = "display";
        vm.RulesInverseSelected(); // only the two visible rules
        vm.SearchText = string.Empty;

        Assert.Equal(
            new[] { false, false, false, true },
            profile.FixRules.Select(r => r.IsSelected));
    }

    [AvaloniaFact]
    public void SwitchingProfile_KeepsTheSearchApplied()
    {
        var vm = MakeViewModel(out _);
        var other = MakeProfile("Other");
        vm.Profiles.Add(other);

        vm.SearchText = "spaces";
        vm.SelectedProfile = other;

        Assert.Equal(new[] { "Remove unneeded spaces" }, other.FixRules.Select(r => r.Name));
    }

    [AvaloniaFact]
    public void Search_KeepsTheHeaderSortOrder()
    {
        var vm = MakeViewModel(out var profile);

        // What a header click on "Name" does: reorder the grid collection in place.
        var sorted = profile.FixRules.OrderBy(r => r.Name).ToList();
        profile.FixRules.Clear();
        foreach (var rule in sorted)
        {
            profile.FixRules.Add(rule);
        }

        vm.SearchText = "fix";
        Assert.Equal(new[] { "Fix long display times", "Fix short display times", "Fix short gaps" },
            profile.FixRules.Select(r => r.Name));

        vm.SearchText = string.Empty;
        Assert.Equal(sorted, profile.FixRules);
    }

    [AvaloniaFact]
    public void Window_ShowsTheSearchBox_AndTypingFilters()
    {
        var vm = new FixCommonErrorsViewModel(null!, new WindowService(new NullServiceProvider()), null!);
        var profile = MakeProfile("Default");
        vm.Profiles.Add(profile);
        vm.SelectedProfile = profile;
        var window = new FixCommonErrorsWindow(vm);
        try
        {
            var searchBox = window.GetLogicalDescendants().OfType<TextBox>()
                .FirstOrDefault(t => t.PlaceholderText == Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot);
            Assert.NotNull(searchBox);
            Assert.True(searchBox.IsVisible);

            searchBox.Text = "gaps";

            Assert.Equal("gaps", vm.SearchText);
            Assert.Equal(new[] { "Fix short gaps" }, profile.FixRules.Select(r => r.Name));

            vm.Step1IsVisible = false;
            Assert.False(searchBox.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_SearchBox_HasSearchIcon_AndClearButtonThatClearsTheSearch()
    {
        var vm = new FixCommonErrorsViewModel(null!, new WindowService(new NullServiceProvider()), null!);
        var profile = MakeProfile("Default");
        vm.Profiles.Add(profile);
        vm.SelectedProfile = profile;
        var window = new FixCommonErrorsWindow(vm);
        try
        {
            var searchBox = window.GetLogicalDescendants().OfType<TextBox>()
                .First(t => t.PlaceholderText == Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot);
            Assert.IsType<Optris.Icons.Avalonia.Icon>(searchBox.InnerLeftContent);
            var clearButton = Assert.IsType<Button>(searchBox.InnerRightContent);
            Assert.Equal(Se.Language.General.Clear, AutomationProperties.GetName(clearButton));
            Assert.False(clearButton.IsVisible);

            searchBox.Text = "gaps";
            Assert.True(clearButton.IsVisible);

            clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(string.Empty, vm.SearchText);
            Assert.Equal(string.Empty, searchBox.Text ?? string.Empty);
            Assert.Equal(4, profile.FixRules.Count);
            Assert.False(clearButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }
}
