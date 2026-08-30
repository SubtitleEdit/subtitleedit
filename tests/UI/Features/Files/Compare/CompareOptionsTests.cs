using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;

namespace UITests.Features.Files.Compare;

/// <summary>
/// The two "ignore" options and the "Show" choice, from #14299: "Ignore formatting" did nothing
/// at all on an .ass/.ssa file (the ASSA tags were not stripped), and none of the three settings
/// survived closing the window.
///
/// [AvaloniaFact] rather than [Fact]: the view model builds Avalonia controls (its TableViews, and
/// a StackPanel per CompareItem), and creating those off the Avalonia thread leaves the headless
/// session unable to set itself up - which surfaces as unrelated test classes failing their cleanup.
/// </summary>
public class CompareOptionsTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;

    public CompareOptionsTests()
    {
        Se.Settings.File.Compare = new SeCompare();
    }

    public void Dispose()
    {
        Se.Settings.File.Compare = _savedSettings;
    }

    private static CompareViewModel MakeViewModel() => new(new FileHelper(), new FolderHelper());

    [AvaloniaTheory]
    [InlineData(@"{\b1}Test bold{\b0}", "Test bold")]
    [InlineData(@"{\i1}Test italic{\i0}", "Test italic")]
    [InlineData(@"{\an8}Alignment test", "Alignment test")]
    [InlineData("<i>Test italic</i>", "Test italic")]
    public void IgnoreFormatting_StripsAssaTagsToo(string withTags, string withoutTags)
    {
        var vm = MakeViewModel();
        vm.IgnoreFormatting = true;

        Assert.Equal(vm.NormalizeForCompare(withoutTags), vm.NormalizeForCompare(withTags));
    }

    [AvaloniaFact]
    public void IgnoreFormattingOff_KeepsAssaTagsAsADifference()
    {
        var vm = MakeViewModel();

        Assert.NotEqual(vm.NormalizeForCompare("Test bold"), vm.NormalizeForCompare(@"{\b1}Test bold{\b0}"));
    }

    [AvaloniaFact]
    public void IgnoreWhiteSpace_IgnoresDoubleSpacesAndLineBreaks()
    {
        var vm = MakeViewModel();
        vm.IgnoreWhiteSpace = true;

        Assert.Equal(
            vm.NormalizeForCompare("Same text not everywhere."),
            vm.NormalizeForCompare("Same text  not" + Environment.NewLine + "everywhere."));
    }

    [AvaloniaFact]
    public void BothOptions_IgnoreTagsAndWhitespaceTogether()
    {
        var vm = MakeViewModel();
        vm.IgnoreFormatting = true;
        vm.IgnoreWhiteSpace = true;

        Assert.Equal(vm.NormalizeForCompare("Test bold"), vm.NormalizeForCompare(@" {\b1}Test  bold{\b0} "));
    }

    [AvaloniaFact]
    public void SaveSettings_RemembersShowAndBothOptions()
    {
        var vm = MakeViewModel();
        vm.SelectedCompareVisual = vm.CompareVisuals.First(p => p.Type == CompareVisualType.ShowOnlyDifferencesInText);
        vm.IgnoreWhiteSpace = true;
        vm.IgnoreFormatting = true;

        vm.SaveSettings();

        Assert.Equal(nameof(CompareVisualType.ShowOnlyDifferencesInText), Se.Settings.File.Compare.Show);
        Assert.True(Se.Settings.File.Compare.IgnoreWhitespace);
        Assert.True(Se.Settings.File.Compare.IgnoreFormatting);

        // A new window picks up where the last one left off (#14299).
        var reopened = MakeViewModel();
        Assert.Equal(CompareVisualType.ShowOnlyDifferencesInText, reopened.SelectedCompareVisual.Type);
        Assert.True(reopened.IgnoreWhiteSpace);
        Assert.True(reopened.IgnoreFormatting);
    }

    [AvaloniaFact]
    public void LoadSettings_UnknownShowValue_FallsBackToAll()
    {
        Se.Settings.File.Compare.Show = "SomethingRemoved";

        Assert.Equal(CompareVisualType.All, MakeViewModel().SelectedCompareVisual.Type);
    }

    [AvaloniaFact]
    public void IgnoreFormatting_HidesTheAssaOnlyRowsFromTheDifferenceCount()
    {
        // The screenshot in #14299: three of the six lines differ only by an ASSA tag.
        var left = MakeLines(
            "Test 1, test 2, test 3.",
            "Same text everywhere.",
            @"{\b1}Test bold{\b0}",
            @"Test {\i1}italic{\i0}",
            @"{\an8}Alignment test");
        var right = MakeLines(
            "Test 1, test 2, test 3.",
            "Same text everywhere.",
            "Test bold",
            "Test italic",
            "Alignment test");

        var vm = MakeViewModel();
        vm.IgnoreFormatting = true;
        vm.Initialize(left, "left.ass", right, "right.ass", false);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.All(vm.LeftSubtitles, item => Assert.False(item.HasDifference));
        Assert.All(vm.RightSubtitles, item => Assert.False(item.HasDifference));
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeLines(params string[] texts)
    {
        var lines = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            lines.Add(new SubtitleLineViewModel(new Paragraph(texts[i], i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        return lines;
    }
}

