using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// "Use empty waveform" is remembered on the recent-file entry of the subtitle and video it was
/// picked for (#16710 follow-up). Saving the entry again without a decision keeps it, so the many
/// AddToRecentFiles calls (open, save, selection) do not erase it.
/// </summary>
public class RecentFileEmptyWaveformTests
{
    private static void Add(SeFile file, string video, bool? useEmptyWaveform = null) =>
        file.AddToRecentFiles("/subs/a.srt", string.Empty, video, 0, "UTF-8", 0, false, -1, string.Empty, useEmptyWaveform);

    [Fact]
    public void ChoiceIsStored()
    {
        var file = new SeFile();

        Add(file, "/video/a.mkv", true);

        Assert.True(file.RecentFiles[0].UseEmptyWaveform);
    }

    [Fact]
    public void SavingWithoutDecisionKeepsTheChoice()
    {
        var file = new SeFile();
        Add(file, "/video/a.mkv", true);

        Add(file, "/video/a.mkv");

        Assert.Single(file.RecentFiles);
        Assert.True(file.RecentFiles[0].UseEmptyWaveform);
    }

    [Fact]
    public void SavingWithoutVideoKeepsTheChoice()
    {
        var file = new SeFile();
        Add(file, "/video/a.mkv", true);

        Add(file, string.Empty);

        Assert.Equal("/video/a.mkv", file.RecentFiles[0].VideoFileName);
        Assert.True(file.RecentFiles[0].UseEmptyWaveform);
    }

    [Fact]
    public void OtherVideoDoesNotInheritTheChoice()
    {
        var file = new SeFile();
        Add(file, "/video/a.mkv", true);

        Add(file, "/video/b.mkv");

        Assert.False(file.RecentFiles[0].UseEmptyWaveform);
    }

    [Fact]
    public void ExplicitFalseClearsTheChoice()
    {
        var file = new SeFile();
        Add(file, "/video/a.mkv", true);

        Add(file, "/video/a.mkv", false);

        Assert.False(file.RecentFiles[0].UseEmptyWaveform);
    }
}
