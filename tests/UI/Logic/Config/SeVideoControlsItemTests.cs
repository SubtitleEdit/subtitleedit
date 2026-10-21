using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class SeVideoControlsItemTests
{
    [Fact]
    public void MakeDefaults_SubtitleButtonsHidden()
    {
        var items = SeVideoControlsItem.MakeDefaults();

        Assert.False(items.Single(p => p.Type == SeVideoControlsItemType.PreviousSubtitle).IsVisible);
        Assert.False(items.Single(p => p.Type == SeVideoControlsItemType.NextSubtitle).IsVisible);
        Assert.False(items.Single(p => p.Type == SeVideoControlsItemType.ToggleSubtitles).IsVisible);
        Assert.False(items.Single(p => p.Type == SeVideoControlsItemType.PlaybackSpeed).IsVisible);
    }

    [Fact]
    public void Normalize_OlderSettings_InsertsMissingItemsAfterTheirDefaultPredecessor()
    {
        // A settings file from before #16047, with the user's own order (full screen first).
        var items = new List<SeVideoControlsItem>
        {
            new() { Type = SeVideoControlsItemType.FullScreen, SortOrder = 10 },
            new() { Type = SeVideoControlsItemType.Play, SortOrder = 20 },
            new() { Type = SeVideoControlsItemType.Stop, SortOrder = 30 },
            new() { Type = SeVideoControlsItemType.PositionSlider, SortOrder = 40 },
            new() { Type = SeVideoControlsItemType.Volume, SortOrder = 50 },
            new() { Type = SeVideoControlsItemType.PositionText, SortOrder = 60 },
            new() { Type = SeVideoControlsItemType.VideoFileName, SortOrder = 70 },
            new() { Type = SeVideoControlsItemType.PlayerName, SortOrder = 80 },
        };

        var result = SeVideoControlsItem.Normalize(items);

        Assert.Equal(
        [
            SeVideoControlsItemType.FullScreen,
            SeVideoControlsItemType.Play,
            SeVideoControlsItemType.Stop,
            SeVideoControlsItemType.PreviousSubtitle,
            SeVideoControlsItemType.NextSubtitle,
            SeVideoControlsItemType.ToggleSubtitles,
            SeVideoControlsItemType.PositionSlider,
            SeVideoControlsItemType.Volume,
            SeVideoControlsItemType.PlaybackSpeed,
            SeVideoControlsItemType.PositionText,
            SeVideoControlsItemType.VideoFileName,
            SeVideoControlsItemType.PlayerName,
        ], result.Select(p => p.Type));
        Assert.False(result[3].IsVisible);
        Assert.False(result[4].IsVisible);
        Assert.False(result[5].IsVisible);
        Assert.False(result[8].IsVisible);
        Assert.Equal(Enumerable.Range(1, result.Count).Select(i => i * 10), result.Select(p => p.SortOrder));
    }

    [Fact]
    public void Normalize_Null_ReturnsDefaults()
    {
        var result = SeVideoControlsItem.Normalize(null);

        Assert.Equal(SeVideoControlsItem.MakeDefaults().Select(p => p.Type), result.Select(p => p.Type));
    }
}
