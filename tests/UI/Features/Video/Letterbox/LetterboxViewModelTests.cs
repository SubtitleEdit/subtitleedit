using Nikse.SubtitleEdit.Features.Video.Letterbox;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.Letterbox;

public class LetterboxViewModelTests
{
    [Fact]
    public void Ok_SavesEnabledAndHeightsToSettings()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = false;
        Se.Settings.Video.Letterbox.TopHeightPercent = 0;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 0;

        var vm = new LetterboxViewModel();
        vm.Initialize(null);

        vm.Enabled = true;
        vm.TopHeightPercent = 12;
        vm.BottomHeightPercent = 18;

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.True(Se.Settings.Video.Letterbox.Enabled);
        Assert.Equal(12, Se.Settings.Video.Letterbox.TopHeightPercent);
        Assert.Equal(18, Se.Settings.Video.Letterbox.BottomHeightPercent);
    }

    [Fact]
    public void Cancel_RestoresTheValuesFromBeforeTheDialogOpened()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = true;
        Se.Settings.Video.Letterbox.TopHeightPercent = 10;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 10;

        var vm = new LetterboxViewModel(); // snapshots enabled/10/10 in its constructor
        vm.Initialize(null);

        vm.TopHeightPercent = 30;
        vm.BottomHeightPercent = 5;
        vm.Enabled = false;

        vm.CancelCommand.Execute(null);

        Assert.False(vm.OkPressed);
        Assert.True(vm.Enabled);
        Assert.Equal(10, vm.TopHeightPercent);
        Assert.Equal(10, vm.BottomHeightPercent);

        // Cancel must not have persisted the values it briefly held before reverting.
        Assert.True(Se.Settings.Video.Letterbox.Enabled);
        Assert.Equal(10, Se.Settings.Video.Letterbox.TopHeightPercent);
    }

    [Fact]
    public void Apply_SavesWithoutClosingOrMarkingOkPressed()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = false;
        Se.Settings.Video.Letterbox.TopHeightPercent = 0;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 0;

        var vm = new LetterboxViewModel();
        vm.Initialize(null);

        vm.Enabled = true;
        vm.TopHeightPercent = 8;

        vm.ApplyCommand.Execute(null);

        Assert.False(vm.OkPressed);
        Assert.True(Se.Settings.Video.Letterbox.Enabled);
        Assert.Equal(8, Se.Settings.Video.Letterbox.TopHeightPercent);
    }

    [Fact]
    public void ApplyThenCancel_RevertsToTheAppliedValuesNotTheOriginalOnes()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = false;
        Se.Settings.Video.Letterbox.TopHeightPercent = 0;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 0;

        var vm = new LetterboxViewModel();
        vm.Initialize(null);

        vm.Enabled = true;
        vm.TopHeightPercent = 15;
        vm.ApplyCommand.Execute(null); // commits 15 to disk and refreshes the revert snapshot

        vm.TopHeightPercent = 30; // a further, uncommitted drag
        vm.CancelCommand.Execute(null);

        // Cancel undoes the drag made AFTER Apply, not the whole dialog session - otherwise the
        // settings file (left at 15 by Apply) and the just-reverted in-memory/live state would
        // disagree until the next unrelated save.
        Assert.Equal(15, vm.TopHeightPercent);
        Assert.Equal(15, Se.Settings.Video.Letterbox.TopHeightPercent);
    }

    [Fact]
    public void Initialize_WithNoPlayer_MarksThePlayerAsUnsupported()
    {
        var vm = new LetterboxViewModel();

        vm.Initialize(null);

        Assert.False(vm.IsPlayerSupported);
    }

    [Fact]
    public void RevertIfNotConfirmed_WhenOkWasNotPressed_RestoresTheSnapshot()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = true;
        Se.Settings.Video.Letterbox.TopHeightPercent = 10;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 10;

        var vm = new LetterboxViewModel(); // snapshots enabled/10/10
        vm.Initialize(null);

        vm.TopHeightPercent = 25; // dragged, then the window is closed via the title-bar X (no Ok)
        vm.RevertIfNotConfirmed();

        Assert.Equal(10, vm.TopHeightPercent);
    }

    [Fact]
    public void RevertIfNotConfirmed_AfterOk_DoesNothing()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = false;
        Se.Settings.Video.Letterbox.TopHeightPercent = 0;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 0;

        var vm = new LetterboxViewModel();
        vm.Initialize(null);

        vm.TopHeightPercent = 20;
        vm.OkCommand.Execute(null); // OkPressed = true, window closes, Closing fires next

        vm.RevertIfNotConfirmed(); // what the window's OnClosing calls for every close path

        Assert.Equal(20, vm.TopHeightPercent);
    }

    [Fact]
    public void ConstructorLoadDoesNotTouchSettingsBeforeInitialize()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = true;
        Se.Settings.Video.Letterbox.TopHeightPercent = 7;
        Se.Settings.Video.Letterbox.BottomHeightPercent = 9;

        // Building the view model reads the settings into its own bound properties (LoadSettings)
        // without pushing anything back out (ApplyLive is gated on Initialize having run) - so the
        // settings object is untouched by construction alone.
        var vm = new LetterboxViewModel();

        Assert.True(vm.Enabled);
        Assert.Equal(7, vm.TopHeightPercent);
        Assert.Equal(9, vm.BottomHeightPercent);
        Assert.Equal(7, Se.Settings.Video.Letterbox.TopHeightPercent);
    }
}
