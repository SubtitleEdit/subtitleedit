using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

namespace Tests.Features.Help;

public class UpdateCheckServiceTests
{
    private const string Separator = "-----------------------------------------------------------------------------------------------------";

    private static string MakeChangeLog(params string[] blocks)
    {
        return "Subtitle Edit Changelog\r\n\r\n" + Separator + "\r\n\r\n" +
               string.Join("\r\n\r\n" + Separator + "\r\n\r\n", blocks);
    }

    [Fact]
    public void ParseLatestChangeLog_StableOnly_SkipsBetaBlocks()
    {
        var changeLog = MakeChangeLog(
            "v5.2.0-beta7 (8th of August 2026)\r\n\r\n* Beta stuff",
            "v5.2.0-beta6 (7th of August 2026)\r\n\r\n* More beta stuff",
            "v5.1.0 (13th of July 2026)\r\n\r\n* Stable stuff");

        var result = UpdateCheckService.ParseLatestChangeLog(changeLog, includePrereleases: false);

        Assert.StartsWith("v5.1.0", result);
        Assert.Equal("v5.1.0", UpdateCheckService.ParseLatestVersion(result));
    }

    [Fact]
    public void ParseLatestChangeLog_WithPrereleases_ReturnsNewestBlock()
    {
        var changeLog = MakeChangeLog(
            "v5.2.0-beta7 (8th of August 2026)\r\n\r\n* Beta stuff",
            "v5.1.0 (13th of July 2026)\r\n\r\n* Stable stuff");

        var result = UpdateCheckService.ParseLatestChangeLog(changeLog, includePrereleases: true);

        Assert.Equal("v5.2.0-beta7", UpdateCheckService.ParseLatestVersion(result));
    }

    [Fact]
    public void ParseLatestChangeLog_SkipsUnreleasedDraftEntry()
    {
        var changeLog = MakeChangeLog(
            "v5.2.0 (xth August 2026)\r\n\r\n* Not released yet",
            "v5.1.0 (13th of July 2026)\r\n\r\n* Stable stuff");

        var result = UpdateCheckService.ParseLatestChangeLog(changeLog, includePrereleases: true);

        Assert.Equal("v5.1.0", UpdateCheckService.ParseLatestVersion(result));
    }

    [Theory]
    [InlineData("v5.2.0-beta7", true)]
    [InlineData("v5.2.0-rc1", true)]
    [InlineData("v5.2.0", false)]
    [InlineData("v5.1.0", false)]
    public void IsPrerelease(string version, bool expected)
    {
        Assert.Equal(expected, UpdateCheckService.IsPrerelease(version));
    }

    [Theory]
    [InlineData(UpdateCheckService.ChannelStable, "v5.2.0-beta7", false)]
    [InlineData(UpdateCheckService.ChannelBeta, "v5.1.0", true)]
    [InlineData("", "v5.2.0-beta7", true)] // auto: beta build follows betas
    [InlineData("", "v5.1.0", false)] // auto: stable build follows stable only
    public void ResolveIncludePrereleases(string channel, string currentVersion, bool expected)
    {
        Assert.Equal(expected, UpdateCheckService.ResolveIncludePrereleases(channel, currentVersion));
    }

    [Theory]
    [InlineData("v5.2.0", "v5.1.0", true)]
    [InlineData("v5.1.0", "v5.2.0-beta7", false)] // stable-only beta user stays put until the next final
    [InlineData("v5.2.0", "v5.2.0-beta7", true)] // the final release outranks its own betas
    [InlineData("v5.2.0-beta8", "v5.2.0-beta7", true)]
    [InlineData("v5.1.0", "v5.1.0", false)]
    public void IsNewerThan(string latest, string current, bool expected)
    {
        Assert.Equal(expected, UpdateCheckService.IsNewerThan(latest, current));
    }
}
