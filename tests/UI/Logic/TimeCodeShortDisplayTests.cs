using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The video offset in the status bar / menu and the assisted split and move candidates show
/// times in the current time code mode, like the grid's gap and duration cells.
/// </summary>
public class TimeCodeShortDisplayTests : IDisposable
{
    private readonly SettingsScope _settings = new("General.UseFrameMode", "General.UseFrameNumbersPersisted");
    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public TimeCodeShortDisplayTests()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
    }

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
        _settings.Dispose();
    }

    private static void SetMode(bool frameMode, bool frameNumbers)
    {
        Se.Settings.General.UseFrameMode = frameMode;
        Se.Settings.General.UseFrameNumbersPersisted = frameNumbers;
    }

    [Fact]
    public void TimeMode_ShowsMilliseconds()
    {
        SetMode(frameMode: false, frameNumbers: true); // frame numbers only apply in frame mode

        Assert.Equal("1,500", TimeCodeShortDisplay.Format(1500));
        Assert.Equal("-1,500", TimeCodeShortDisplay.Format(-1500));
    }

    [Fact]
    public void FrameMode_ShowsSecondsAndFrames()
    {
        SetMode(frameMode: true, frameNumbers: false);

        Assert.Equal("01:12", TimeCodeShortDisplay.Format(1480));
    }

    [Fact]
    public void FrameNumbersMode_ShowsAFrameNumber()
    {
        SetMode(frameMode: true, frameNumbers: true);

        Assert.Equal("250", TimeCodeShortDisplay.Format(10_000));
        Assert.Equal("-25", TimeCodeShortDisplay.Format(-1000));
    }
}
