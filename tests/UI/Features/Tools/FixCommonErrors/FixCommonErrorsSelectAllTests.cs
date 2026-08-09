using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

namespace UITests.Features.Tools.FixCommonErrors;

// Toggling "Select all" sets IsSelected on every visible fix item; each set raises
// PropertyChanged, and the per-item handler used to re-run the full summary/chip recount,
// making one click O(visible x fixes). The bulk loop must suppress the per-item recounts
// and run exactly one at the end - with the same end state as per-item toggling.
public class FixCommonErrorsSelectAllTests
{
    private static FixCommonErrorsViewModel BuildViewModelWithFixes(int count, bool isSelected)
    {
        var vm = new FixCommonErrorsViewModel(null!, null!, null!);
        for (var i = 0; i < count; i++)
        {
            // Public IFixCallbacks entry point; _previewMode starts true, so this populates
            // Fixes and (with no filter chips yet) VisibleFixes.
            vm.AddFixToListView(new Paragraph($"line {i}", i * 1000, i * 1000 + 900), "Fix commas", "before", "after", isSelected);
        }

        return vm;
    }

    [AvaloniaFact]
    public void FixesSelectAll_RunsSummaryRecountOnce_NotOncePerItem()
    {
        var vm = BuildViewModelWithFixes(5, isSelected: false);

        // ApplySelectedFixesText is rewritten with the global selected count on every summary
        // recount, so per-item recounts would step it "(1)", "(2)", ... - one event per item.
        var applyTextChanges = 0;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.ApplySelectedFixesText))
            {
                applyTextChanges++;
            }
        };

        vm.FixesSelectAll();

        Assert.Equal(1, applyTextChanges);
    }

    [AvaloniaFact]
    public void FixesSelectAll_EndStateMatchesPerItemToggling()
    {
        var vm = BuildViewModelWithFixes(5, isSelected: false);

        vm.FixesSelectAll();

        Assert.All(vm.Fixes, f => Assert.True(f.IsSelected));
        Assert.EndsWith("(5)", vm.ApplySelectedFixesText);
        Assert.Equal(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.SelectNone, vm.FixesSelectAllText);
    }

    [AvaloniaFact]
    public void FixesSelectAll_DeselectsAll_WhenEverythingIsSelected()
    {
        var vm = BuildViewModelWithFixes(3, isSelected: true);

        vm.FixesSelectAll();

        Assert.All(vm.Fixes, f => Assert.False(f.IsSelected));
        Assert.EndsWith("(0)", vm.ApplySelectedFixesText);
        Assert.Equal(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.SelectAll, vm.FixesSelectAllText);
    }

    [AvaloniaFact]
    public void SingleItemToggle_StillUpdatesSummary()
    {
        var vm = BuildViewModelWithFixes(2, isSelected: false);

        vm.Fixes[0].IsSelected = true;

        Assert.EndsWith("(1)", vm.ApplySelectedFixesText);
    }
}
