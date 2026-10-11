using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// "Set start and offset the rest" has its V4 default F9 again (#15800), except on macOS where
/// F9 is "set start" (F11 shows the desktop there).
/// </summary>
public class ShortcutsSetStartAndOffsetTheRestTests
{
    private const string SetStartAndOffsetTheRest = nameof(MainViewModel.WaveformSetStartAndOffsetTheRestCommand);

    [Fact]
    public void WindowsAndLinuxDefaultIsF9()
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, isMacOS: false);

        Assert.Equal(new[] { "F9" }, defaults.Single(s => s.ActionName == SetStartAndOffsetTheRest).Keys);
        Assert.Single(defaults, s => s.Keys.Count == 1 && s.Keys[0] == "F9");
    }

    [Fact]
    public void MacOsKeepsF9OnSetStart()
    {
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, isMacOS: true);

        Assert.DoesNotContain(defaults, s => s.ActionName == SetStartAndOffsetTheRest);
        Assert.Equal(new[] { "F9" }, defaults.Single(s => s.ActionName == nameof(MainViewModel.WaveformSetStartCommand)).Keys);
    }
}
