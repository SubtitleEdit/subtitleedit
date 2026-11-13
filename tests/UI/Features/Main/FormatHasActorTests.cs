using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// The Actor column follows the format (#16596): shown for formats that keep an actor, hidden
/// for formats like SubRip that drop it on save.
/// </summary>
public class FormatHasActorTests
{
    [Fact]
    public void FormatsWithActor()
    {
        Assert.True(MainViewModel.FormatHasActor(new AdvancedSubStationAlpha()));
        Assert.True(MainViewModel.FormatHasActor(new SubStationAlpha()));
        Assert.True(MainViewModel.FormatHasActor(new WebVTT()));
        Assert.True(MainViewModel.FormatHasActor(new CsvExcel()));
    }

    [Fact]
    public void FormatsWithoutActor()
    {
        Assert.False(MainViewModel.FormatHasActor(new SubRip()));
        Assert.False(MainViewModel.FormatHasActor(new TimedText10()));
        Assert.False(MainViewModel.FormatHasActor(new MicroDvd()));
        Assert.False(MainViewModel.FormatHasActor(null));
    }
}
