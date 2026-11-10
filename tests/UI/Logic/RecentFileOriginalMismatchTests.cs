using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The answer to "the original does not have the same number of subtitles" is stored on the
/// recent-file entry of the pair, so reopening it does not ask again (#16752).
/// </summary>
public class RecentFileOriginalMismatchTests
{
    private static void Add(SeFile file, string original, bool? showAll = null, bool? allowEdit = null) =>
        file.AddToRecentFiles("/subs/a.da.srt", original, string.Empty, 0, "UTF-8", 0, false, -1, string.Empty, null, showAll, allowEdit);

    [Fact]
    public void ChoiceIsStored()
    {
        var file = new SeFile();

        Add(file, "/subs/a.en.srt", true, false);

        Assert.True(file.RecentFiles[0].OriginalShowAllLines);
        Assert.False(file.RecentFiles[0].OriginalAllowEdit);
    }

    [Fact]
    public void NotAskedIsNull()
    {
        var file = new SeFile();

        Add(file, "/subs/a.en.srt");

        Assert.Null(file.RecentFiles[0].OriginalShowAllLines);
        Assert.Null(file.RecentFiles[0].OriginalAllowEdit);
    }

    [Fact]
    public void NewChoiceReplacesOldOne()
    {
        var file = new SeFile();
        Add(file, "/subs/a.en.srt", true, false);

        Add(file, "/subs/a.en.srt", false, true);

        Assert.Single(file.RecentFiles);
        Assert.False(file.RecentFiles[0].OriginalShowAllLines);
        Assert.True(file.RecentFiles[0].OriginalAllowEdit);
    }

    [Fact]
    public void OtherOriginalHasItsOwnEntry()
    {
        var file = new SeFile();
        Add(file, "/subs/a.en.srt", true, true);

        Add(file, "/subs/a.sv.srt");

        Assert.Equal(2, file.RecentFiles.Count);
        Assert.Null(file.RecentFiles[0].OriginalShowAllLines);
        Assert.True(file.RecentFiles[1].OriginalShowAllLines);
    }

    [Fact]
    public void ChoiceSurvivesSettingsRoundTrip()
    {
        var settings = new Se();
        Add(settings.File, "/subs/a.en.srt", true, false);

        var json = System.Text.Json.JsonSerializer.Serialize(settings, SeJsonContext.Default.Se);
        var back = System.Text.Json.JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;

        Assert.True(back.File.RecentFiles[0].OriginalShowAllLines);
        Assert.False(back.File.RecentFiles[0].OriginalAllowEdit);
    }
}
