using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.OpenOriginalMismatch;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// The prompt shown when the original subtitle does not line up 1:1 with the current one. Both of
/// its choices are remembered, and out of the box it offers the plain side-by-side view with the
/// original read-only (issue #13449).
/// </summary>
public class OpenOriginalMismatchViewModelTests
{
    [AvaloniaFact]
    public void Initialize_WithoutRememberedChoices_DefaultsToMatchingLinesOnlyAndReadOnly()
    {
        WithSettings(showAllLines: false, allowEdit: false, () =>
        {
            var vm = new OpenOriginalMismatchViewModel();
            vm.Initialize(originalLineCount: 5, currentLineCount: 3, matchingCount: 3, nonMatchingCount: 2);

            Assert.True(vm.ShowMatchingLinesOnly);
            Assert.False(vm.ShowAllOriginalLines);
            Assert.False(vm.AllowEditOfOriginal);
        });
    }

    [AvaloniaFact]
    public void Initialize_RestoresBothRememberedChoices()
    {
        WithSettings(showAllLines: true, allowEdit: true, () =>
        {
            var vm = new OpenOriginalMismatchViewModel();
            vm.Initialize(originalLineCount: 5, currentLineCount: 3, matchingCount: 3, nonMatchingCount: 2);

            Assert.True(vm.ShowAllOriginalLines);
            Assert.False(vm.ShowMatchingLinesOnly);
            Assert.True(vm.AllowEditOfOriginal);
        });
    }

    [AvaloniaFact]
    public void ShowMatchingLinesOnly_TracksTheOtherOption()
    {
        WithSettings(showAllLines: false, allowEdit: false, () =>
        {
            var vm = new OpenOriginalMismatchViewModel();
            vm.Initialize(originalLineCount: 5, currentLineCount: 3, matchingCount: 3, nonMatchingCount: 2);

            vm.ShowAllOriginalLines = true;
            Assert.False(vm.ShowMatchingLinesOnly);

            vm.ShowMatchingLinesOnly = true;
            Assert.False(vm.ShowAllOriginalLines);
        });
    }

    /// <summary>
    /// "Allow edit" means something different per mode - lossless with every original line on screen,
    /// lossy with only the matching ones - so the hint under the check box has to follow the mode.
    /// </summary>
    [AvaloniaFact]
    public void AllowEditHint_FollowsTheSelectedMode()
    {
        WithSettings(showAllLines: false, allowEdit: false, () =>
        {
            var vm = new OpenOriginalMismatchViewModel();
            vm.Initialize(originalLineCount: 5, currentLineCount: 3, matchingCount: 3, nonMatchingCount: 2);

            Assert.Equal(Se.Language.Main.AllowEditHintReadOnly, vm.AllowEditHint);

            vm.AllowEditOfOriginal = true;
            Assert.Equal(string.Format(Se.Language.Main.AllowEditHintMatchingOnlyX, 2), vm.AllowEditHint);

            vm.ShowAllOriginalLines = true;
            Assert.Equal(Se.Language.Main.AllowEditHintAllLines, vm.AllowEditHint);
        });
    }

    private static void WithSettings(bool showAllLines, bool allowEdit, Action action)
    {
        var oldShowAllLines = Se.Settings.General.ShowOriginalNonMatchingLines;
        var oldAllowEdit = Se.Settings.General.AllowEditOfOriginalSubtitle;
        try
        {
            Se.Settings.General.ShowOriginalNonMatchingLines = showAllLines;
            Se.Settings.General.AllowEditOfOriginalSubtitle = allowEdit;
            action();
        }
        finally
        {
            Se.Settings.General.ShowOriginalNonMatchingLines = oldShowAllLines;
            Se.Settings.General.AllowEditOfOriginalSubtitle = oldAllowEdit;
        }
    }
}
