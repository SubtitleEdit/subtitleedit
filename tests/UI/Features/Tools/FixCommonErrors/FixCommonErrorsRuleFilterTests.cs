using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.FixCommonErrors;

// Step 1 lists ~38 rules flat. The "Type" combo narrows the grid to one FixType, ANDed with
// the search text. Both filter from the profile's full list (AllFixRules) and never from the
// grid collection, so hidden rules keep their selection and reappear intact.
public class FixCommonErrorsRuleFilterTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which the
    // construction test never does.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static List<FixRuleDisplayItem> MakeMixedRules()
    {
        return new List<FixRuleDisplayItem>
        {
            new("Fix commas", string.Empty, 1, true, nameof(FixCommas)), // Punctuation
            new("Fix short gaps", string.Empty, 1, true, nameof(FixShortGaps)), // Time
            new("Remove empty lines", string.Empty, 1, true, nameof(FixEmptyLines)), // Formatting
        };
    }

    private static ProfileDisplayItem MakeProfile(string name)
    {
        var rules = MakeMixedRules();
        return new ProfileDisplayItem
        {
            Name = name,
            FixRules = new System.Collections.ObjectModel.ObservableCollection<FixRuleDisplayItem>(rules),
            AllFixRules = rules,
        };
    }

    private static FixCommonErrorsViewModel BuildViewModel(out ProfileDisplayItem profile)
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);
        profile = MakeProfile("Default");
        vm.Profiles.Add(profile);
        vm.SelectedProfile = profile;
        return vm;
    }

    private static FixTypeDisplayItem TypeItem(FixCommonErrorsViewModel vm, FixType fixType)
    {
        return vm.FixTypes.First(p => p.FixType == fixType);
    }

    [AvaloniaFact]
    public void FixTypes_StartsWithAll_ThenEveryEnumMember()
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);

        Assert.Null(vm.FixTypes[0].FixType);
        Assert.Equal(Enum.GetValues<FixType>().Length + 1, vm.FixTypes.Count);
        Assert.Same(vm.FixTypes[0], vm.SelectedFixType);
    }

    [AvaloniaFact]
    public void SelectingType_ShowsOnlyRulesOfThatType()
    {
        var vm = BuildViewModel(out var profile);

        vm.SelectedFixType = TypeItem(vm, FixType.Time);

        var rule = Assert.Single(profile.FixRules);
        Assert.Equal(nameof(FixShortGaps), rule.FixCommonErrorFunctionName);
        Assert.Equal(3, profile.AllFixRules.Count);
    }

    [AvaloniaFact]
    public void SelectingAll_RestoresEveryRule()
    {
        var vm = BuildViewModel(out var profile);
        vm.SelectedFixType = TypeItem(vm, FixType.Time);

        vm.SelectedFixType = vm.FixTypes[0];

        Assert.Equal(profile.AllFixRules, profile.FixRules);
    }

    [AvaloniaFact]
    public void TypeAndSearch_AreCombined()
    {
        var vm = BuildViewModel(out var profile);

        vm.SelectedFixType = TypeItem(vm, FixType.Punctuation);
        vm.SearchText = "gap";
        Assert.Empty(profile.FixRules);

        vm.SelectedFixType = TypeItem(vm, FixType.Time);
        Assert.Single(profile.FixRules);

        vm.SearchText = string.Empty;
        Assert.Single(profile.FixRules); // the type filter alone still applies
    }

    [AvaloniaFact]
    public void SwitchingProfile_KeepsActiveFilter()
    {
        var vm = BuildViewModel(out _);
        var other = MakeProfile("Other");
        vm.Profiles.Add(other);
        vm.SelectedFixType = TypeItem(vm, FixType.Time);

        vm.SelectedProfile = other;

        var rule = Assert.Single(other.FixRules);
        Assert.Equal(nameof(FixShortGaps), rule.FixCommonErrorFunctionName);
    }

    [AvaloniaFact]
    public void HiddenRules_KeepTheirSelection()
    {
        var vm = BuildViewModel(out var profile);
        var commas = profile.AllFixRules.First(p => p.FixCommonErrorFunctionName == nameof(FixCommas));
        vm.SelectedFixType = TypeItem(vm, FixType.Time);

        vm.RulesInverseSelected(); // acts on the visible (filtered) rows only
        vm.SelectedFixType = vm.FixTypes[0];

        Assert.True(commas.IsSelected);
        Assert.False(profile.AllFixRules.First(p => p.FixCommonErrorFunctionName == nameof(FixShortGaps)).IsSelected);
    }

    [Fact]
    public void MakeDefaultRules_EveryRuleResolvesItsFixType()
    {
        var rules = FixCommonErrorsViewModel.MakeDefaultRules();

        Assert.NotEmpty(rules);
        Assert.All(rules, rule => Assert.NotNull(rule.FixType));
    }

    [Theory]
    [InlineData(nameof(FixEllipsesStart), FixType.Punctuation)]
    [InlineData(nameof(FixAloneLowercaseIToUppercaseI), FixType.Casing)]
    [InlineData(nameof(FixTurkishAnsiToUnicode), FixType.Characters)]
    [InlineData(nameof(FixDanishLetterI), FixType.Casing)]
    [InlineData(nameof(FixSpanishInvertedQuestionAndExclamationMarks), FixType.Punctuation)]
    [InlineData(nameof(FixCommonOcrErrors), FixType.Ocr)]
    public void LanguageSpecificRules_ResolveTheirFixType(string functionName, FixType expected)
    {
        Assert.True(FixRuleDisplayItem.TryResolveFixType(functionName, out var fixType));
        Assert.Equal(expected, fixType);
    }

    [Fact]
    public void UnknownRuleName_HasNoFixType()
    {
        var rule = new FixRuleDisplayItem("Unknown", string.Empty, 1, true, "NoSuchFix");

        Assert.Null(rule.FixType);
    }

    [Fact]
    public void CopyConstructor_CopiesFixType()
    {
        var original = new FixRuleDisplayItem("Fix commas", string.Empty, 1, true, nameof(FixCommas));

        var copy = new FixRuleDisplayItem(original);

        Assert.Equal(FixType.Punctuation, copy.FixType);
    }

    // The search box used to be created but never added to the window, so it was invisible.
    // Both it and the type combo live in the step 1 toolbar and hide with it.
    [AvaloniaFact]
    public void Window_ShowsSearchBoxAndTypeCombo_OnlyInStep1()
    {
        var vm = new FixCommonErrorsViewModel(null!, new WindowService(new NullServiceProvider()), null!);
        var window = new FixCommonErrorsWindow(vm);
        _windows.Add(window);

        var search = window.GetLogicalDescendants().OfType<TextBox>()
            .FirstOrDefault(p => p.PlaceholderText == Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot);
        var combo = window.GetLogicalDescendants().OfType<ComboBox>()
            .FirstOrDefault(p => ReferenceEquals(p.ItemsSource, vm.FixTypes));
        Assert.NotNull(search);
        Assert.NotNull(combo);
        Assert.True(search.IsVisible);
        Assert.True(combo.IsVisible);
        Assert.Same(vm.FixTypes[0], combo.SelectedItem);

        vm.Step1IsVisible = false;

        Assert.False(search.IsVisible);
        Assert.False(combo.IsVisible);
    }
}
