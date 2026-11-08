using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// "List errors" showed time codes counted from zero with a video offset set, and dropped the
/// leading zero hours (00:01:21:13 as 01:21:13) - #16571. It should read like the main grid.
/// </summary>
public class ErrorListItemTimeCodeTests : IDisposable
{
    private readonly SettingsScope _scope;
    private readonly long _videoOffset;
    private readonly double _frameRate;

    public ErrorListItemTimeCodeTests()
    {
        _scope = new SettingsScope("General.UseFrameMode", "General.UseFrameNumbersPersisted");
        _videoOffset = Se.Settings.General.CurrentVideoOffsetInMs;
        _frameRate = Configuration.Settings.General.CurrentFrameRate;

        Se.Settings.General.UseFrameMode = true;
        Se.Settings.General.UseFrameNumbersPersisted = false;
        Se.Settings.General.CurrentVideoOffsetInMs = 0;
        Configuration.Settings.General.CurrentFrameRate = 25;
    }

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _frameRate;
        Se.Settings.General.CurrentVideoOffsetInMs = _videoOffset;
        _scope.Dispose();
    }

    private static ErrorListItem MakeItem(double startMs, double endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph("Text", startMs, endMs), null!) { Number = 1 };
        return new ErrorListItem(line, new LineError(LineErrorType.DurationTooShort, "200 < 1000"));
    }

    [AvaloniaFact]
    public void ShowAndHide_KeepLeadingZeroHours()
    {
        var item = MakeItem(0, 81_520); // 00:01:21:13 at 25 fps

        Assert.Equal("00:00:00:00", item.Show);
        Assert.Equal("00:01:21:13", item.Hide);
    }

    [AvaloniaFact]
    public void ShowAndHide_ApplyTheVideoOffset()
    {
        Se.Settings.General.CurrentVideoOffsetInMs = 10 * 60 * 60 * 1000; // 10:00:00:00

        var item = MakeItem(0, 81_520);

        Assert.Equal("10:00:00:00", item.Show);
        Assert.Equal("10:01:21:13", item.Hide);
    }
}
