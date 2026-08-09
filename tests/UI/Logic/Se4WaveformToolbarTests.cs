using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Se4Setup;

namespace UITests.Logic;

// ApplyWaveformToolbar works on the global Se.Settings.Waveform, so restore what was there
// afterwards - other tests in this assembly read the same instance.
public class Se4WaveformToolbarTests : IDisposable
{
    private static readonly SeWaveformToolbarItemType[] ExpectedVisibleInOrder =
    [
        SeWaveformToolbarItemType.Play,
        SeWaveformToolbarItemType.TextPrevious,
        SeWaveformToolbarItemType.TextPlay,
        SeWaveformToolbarItemType.TextPause,
        SeWaveformToolbarItemType.TextNext,
        SeWaveformToolbarItemType.New,
        SeWaveformToolbarItemType.SetStart,
        SeWaveformToolbarItemType.SetEnd,
        SeWaveformToolbarItemType.SetStartAndOffsetTheRest,
        SeWaveformToolbarItemType.HorizontalZoom,
        SeWaveformToolbarItemType.VideoPositionSlider,
        SeWaveformToolbarItemType.AudioTrackPicker,
        SeWaveformToolbarItemType.PlaybackSpeed,
        SeWaveformToolbarItemType.AutoSelectOnPlay,
        SeWaveformToolbarItemType.Center,
        SeWaveformToolbarItemType.More,
    ];

    private readonly List<SeWaveformToolbarItem> _originalItems = Se.Settings.Waveform.ToolbarItems;
    private readonly bool _originalShowToolbar = Se.Settings.Waveform.ShowToolbar;

    public void Dispose()
    {
        Se.Settings.Waveform.ToolbarItems = _originalItems;
        Se.Settings.Waveform.ShowToolbar = _originalShowToolbar;
    }

    [Fact]
    public void ApplyWaveformToolbar_ShowsSe4ButtonsInSe4Order()
    {
        var waveform = Se.Settings.Waveform;
        waveform.ToolbarItems = new SeWaveform().ToolbarItems;
        waveform.ShowToolbar = false;

        Se4SetupApplier.ApplyWaveformToolbar();

        Assert.True(waveform.ShowToolbar);

        var visible = waveform.ToolbarItems
            .Where(p => p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .Select(p => p.Type)
            .ToArray();

        Assert.Equal(ExpectedVisibleInOrder, visible);
    }

    [Fact]
    public void ApplyWaveformToolbar_HidesItemsWithNoSe4Counterpart()
    {
        var waveform = Se.Settings.Waveform;

        // Start from "everything on" so the hiding is what the assertions measure.
        waveform.ToolbarItems = new SeWaveform().ToolbarItems;
        foreach (var item in waveform.ToolbarItems)
        {
            item.IsVisible = true;
        }

        Se4SetupApplier.ApplyWaveformToolbar();

        var hidden = waveform.ToolbarItems.Where(p => !p.IsVisible).Select(p => p.Type).ToList();

        Assert.Contains(SeWaveformToolbarItemType.Repeat, hidden);
        Assert.Contains(SeWaveformToolbarItemType.PlaySelection, hidden);
        Assert.Contains(SeWaveformToolbarItemType.PlayNext, hidden);
        Assert.Contains(SeWaveformToolbarItemType.RemoveBlankLines, hidden);
        Assert.Contains(SeWaveformToolbarItemType.VerticalZoom, hidden);
        Assert.Contains(SeWaveformToolbarItemType.VideoSeek, hidden);
    }

    [Fact]
    public void ApplyWaveformToolbar_AddsItemTypesMissingFromOlderSettings()
    {
        var waveform = Se.Settings.Waveform;

        // An old Settings.json predating the SE 4 text buttons.
        waveform.ToolbarItems = new SeWaveform().ToolbarItems
            .Where(p => p.Type is not (SeWaveformToolbarItemType.TextPrevious
                or SeWaveformToolbarItemType.TextPlay
                or SeWaveformToolbarItemType.TextPause
                or SeWaveformToolbarItemType.TextNext))
            .ToList();

        Se4SetupApplier.ApplyWaveformToolbar();

        foreach (var type in ExpectedVisibleInOrder)
        {
            var item = waveform.ToolbarItems.SingleOrDefault(p => p.Type == type);
            Assert.NotNull(item);
            Assert.True(item!.IsVisible, $"{type} should be visible after the SE 4 setup");
        }
    }
}
