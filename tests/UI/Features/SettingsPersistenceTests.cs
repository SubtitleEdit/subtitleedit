using Nikse.SubtitleEdit.Features.Files.ExportPlainText;
using Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;
using Nikse.SubtitleEdit.Features.Tools.MergeShortLines;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features;

/// <summary>
/// Dialogs that offer a number the user can change have to reopen on the number they last used -
/// and they have to keep that in their own settings key, not by writing back over the app-wide
/// default (#13514 established both halves for split/rebalance long lines). Three more dialogs had
/// a <c>SaveSettings</c> that stored nothing at all, so every visit reset to the general default.
///
/// Every global these tests touch - the dialog's own key and the general default it falls back to -
/// goes through <see cref="SettingsScope"/>, because Se.Settings is one instance shared by the
/// whole run and a leaked value silently changes what later tests start from.
/// </summary>
public class SettingsPersistenceTests
{
    private static void Invoke(object vm, string method) =>
        vm.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null);

    private static void Set(object vm, string property, object value) =>
        vm.GetType().GetProperty(property)!.SetValue(vm, value);

    private static object? Get(object vm, string property) =>
        vm.GetType().GetProperty(property)!.GetValue(vm);

    [Fact]
    public void ApplyMinGap_RemembersTheGap()
    {
        using var _ = new SettingsScope(
            "Tools.ApplyMinGapMilliseconds",
            "General.MinimumBetweenLines.Milliseconds",
            "General.UseFrameMode");

        Se.Settings.Tools.ApplyMinGapMilliseconds = 0;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 24;
        Se.Settings.General.UseFrameMode = false;

        var vm = new ApplyMinGapViewModel();
        Invoke(vm, "LoadSettings");
        Assert.Equal(24, Get(vm, "MinGapMsOrFrames")); // falls back to the general default

        Set(vm, "MinGapMsOrFrames", 120);
        Invoke(vm, "SaveSettings");

        var reopened = new ApplyMinGapViewModel();
        Invoke(reopened, "LoadSettings");
        Assert.Equal(120, Get(reopened, "MinGapMsOrFrames"));

        // The dialog keeps its own copy - the app-wide default is left alone.
        Assert.Equal(24, Se.Settings.General.MinimumBetweenLines.Milliseconds);
    }

    /// <summary>
    /// The gap box holds frames in frame mode and milliseconds otherwise. Keeping both in one
    /// settings key meant a gap saved as 120 ms reopened as 120 frames (five seconds at 24 fps)
    /// once the user switched the time format.
    /// </summary>
    [Fact]
    public void ApplyMinGap_KeepsMillisecondsAndFramesApart()
    {
        using var _ = new SettingsScope(
            "Tools.ApplyMinGapMilliseconds",
            "Tools.ApplyMinGapFrames",
            "General.MinimumBetweenLines.Milliseconds",
            "General.MinimumBetweenLines.Frames",
            "General.UseFrameMode");

        Se.Settings.Tools.ApplyMinGapMilliseconds = 0;
        Se.Settings.Tools.ApplyMinGapFrames = 0;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 24;
        Se.Settings.General.MinimumBetweenLines.Frames = 2;

        Se.Settings.General.UseFrameMode = false;
        var msVm = new ApplyMinGapViewModel();
        Invoke(msVm, "LoadSettings");
        Set(msVm, "MinGapMsOrFrames", 120);
        Invoke(msVm, "SaveSettings");

        // Switching to frame mode must not read the 120 back as a frame count.
        Se.Settings.General.UseFrameMode = true;
        var frameVm = new ApplyMinGapViewModel();
        Invoke(frameVm, "LoadSettings");
        Assert.Equal(2, Get(frameVm, "MinGapMsOrFrames")); // the general frame default

        Set(frameVm, "MinGapMsOrFrames", 3);
        Invoke(frameVm, "SaveSettings");

        // ...and back again: each unit remembers its own last value.
        Se.Settings.General.UseFrameMode = false;
        var backToMs = new ApplyMinGapViewModel();
        Invoke(backToMs, "LoadSettings");
        Assert.Equal(120, Get(backToMs, "MinGapMsOrFrames"));

        Se.Settings.General.UseFrameMode = true;
        var backToFrames = new ApplyMinGapViewModel();
        Invoke(backToFrames, "LoadSettings");
        Assert.Equal(3, Get(backToFrames, "MinGapMsOrFrames"));
    }

    [Fact]
    public void MergeShortLines_RemembersMaxLengthAndMaxLines()
    {
        using var _ = new SettingsScope(
            "Tools.MergeShortLinesSingleLineMaxLength",
            "Tools.MergeShortLinesMaxNumberOfLines",
            "General.SubtitleLineMaximumLength",
            "General.MaxNumberOfLines");

        Se.Settings.Tools.MergeShortLinesSingleLineMaxLength = 0;
        Se.Settings.Tools.MergeShortLinesMaxNumberOfLines = 0;
        Se.Settings.General.SubtitleLineMaximumLength = 43;
        Se.Settings.General.MaxNumberOfLines = 2;

        var vm = new MergeShortLinesViewModel();
        Invoke(vm, "LoadSettings");
        Assert.Equal(43, Get(vm, "SingleLineMaxLength"));
        Assert.Equal(2, Get(vm, "MaxNumberOfLines"));

        Set(vm, "SingleLineMaxLength", 37);
        Set(vm, "MaxNumberOfLines", 3);
        Invoke(vm, "SaveSettings");

        var reopened = new MergeShortLinesViewModel();
        Invoke(reopened, "LoadSettings");
        Assert.Equal(37, Get(reopened, "SingleLineMaxLength"));
        Assert.Equal(3, Get(reopened, "MaxNumberOfLines"));

        Assert.Equal(43, Se.Settings.General.SubtitleLineMaximumLength);
        Assert.Equal(2, Se.Settings.General.MaxNumberOfLines);
    }

    [Fact]
    public void ApplyDurationLimits_RemembersMinAndMaxDuration()
    {
        using var _ = new SettingsScope(
            "Tools.ApplyDurationLimitsMinDurationMs",
            "Tools.ApplyDurationLimitsMaxDurationMs",
            "General.SubtitleMinimumDisplayMilliseconds",
            "General.SubtitleMaximumDisplayMilliseconds");

        Se.Settings.Tools.ApplyDurationLimitsMinDurationMs = 0;
        Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs = 0;
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 8000;

        var vm = new ApplyDurationLimitsViewModel();
        Invoke(vm, "LoadSettings");
        Assert.Equal(1000, Get(vm, "MinDurationMs"));
        Assert.Equal(8000, Get(vm, "MaxDurationMs"));

        Set(vm, "MinDurationMs", 1200);
        Set(vm, "MaxDurationMs", 6000);
        Invoke(vm, "SaveSettings");

        var reopened = new ApplyDurationLimitsViewModel();
        Invoke(reopened, "LoadSettings");
        Assert.Equal(1200, Get(reopened, "MinDurationMs"));
        Assert.Equal(6000, Get(reopened, "MaxDurationMs"));

        Assert.Equal(1000, Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
        Assert.Equal(8000, Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
    }

    // Cancel has to leave the options alone - Escape in the same dialog already did, so the two
    // ways out disagreed and the button was the wrong one.
    [Fact]
    public void ExportPlainText_CancelDoesNotSaveSettings()
    {
        using var _ = new SettingsScope("File.ExportPlainText.ShowLineNumbers");

        Se.Settings.File.ExportPlainText.ShowLineNumbers = false;

        var vm = new ExportPlainTextViewModel(null!, null!);
        Set(vm, "ShowLineNumbers", true);
        vm.CancelCommand.Execute(null);

        Assert.False(Se.Settings.File.ExportPlainText.ShowLineNumbers);
    }
}
