using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

/// <summary>
/// The settings dialog decides whether to run the heavyweight ApplySettings (a full layout
/// rebuild, video player included) by comparing snapshots of the settings. Window geometry must
/// not take part in that comparison: every window writes its own position when it closes, so it
/// changes as a side effect of merely using the dialog - and in undocked mode of the Apply
/// itself, which closes and re-creates the video and waveform windows (issue #14218).
/// </summary>
public class SettingsChangeSnapshotTests
{
    [Fact]
    public void RememberedWindowGeometryIsNotAChange()
    {
        using var scope = new SettingsScope("General.WindowPositions");

        Se.Settings.General.WindowPositions = new List<SeWindowPosition>
        {
            new("SettingsWindow", false, false, 10, 20, 800, 600),
        };
        var before = SettingsChangeSnapshot.Take();

        // The settings window moved, and an Apply re-saved the undocked windows.
        Se.Settings.General.WindowPositions = new List<SeWindowPosition>
        {
            new("SettingsWindow", false, false, 900, 40, 800, 600),
            new("VideoPlayerUndockedWindow", true, false, 1920, 0, 1920, 1080),
        };

        Assert.Equal(before, SettingsChangeSnapshot.Take());
    }

    [Fact]
    public void ASettingThatNeedsApplyingIsAChange()
    {
        using var scope = new SettingsScope("Video.MpvPreviewAlignment");

        var before = SettingsChangeSnapshot.Take();

        // The change from the issue: subtitle position in the video player.
        Se.Settings.Video.MpvPreviewAlignment = Se.Settings.Video.MpvPreviewAlignment == "7" ? "2" : "7";

        Assert.NotEqual(before, SettingsChangeSnapshot.Take());
    }
}
