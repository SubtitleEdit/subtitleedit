using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;
using System.Net;
using System.Net.Http;

namespace Tests.Features.Help;

public class UpdateCheckServiceTests
{
    private const string Separator = "-----------------------------------------------------------------------------------------------------";

    /// <summary>Fails the first <c>failCount</c> requests, then serves the changelog.</summary>
    private sealed class FlakyHandler : HttpMessageHandler
    {
        private readonly int _failCount;
        private readonly string _content;

        public int Requests { get; private set; }

        public FlakyHandler(int failCount, string content)
        {
            _failCount = failCount;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            if (Requests <= _failCount)
            {
                throw new HttpRequestException("proxy hiccup");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_content),
            });
        }
    }

    [Fact]
    public async Task CheckForUpdates_TransientFailures_SucceedsOnSecondRound()
    {
        var changeLog = MakeChangeLog("v999.0.0 (1st of January 2030)\r\n\r\n* Future stuff");
        var handler = new FlakyHandler(failCount: 3, changeLog); // first round (all three urls) fails
        var service = new UpdateCheckService(new HttpClient(handler)) { RetryDelay = TimeSpan.Zero };

        var result = await service.CheckForUpdates();

        Assert.Equal(4, handler.Requests);
        Assert.Equal("v999.0.0", result.LatestVersion);
        Assert.True(result.IsNewVersionAvailable);
    }

    [Fact]
    public async Task CheckForUpdates_AllAttemptsFail_ThrowsLastException()
    {
        var handler = new FlakyHandler(failCount: int.MaxValue, string.Empty);
        var service = new UpdateCheckService(new HttpClient(handler)) { RetryDelay = TimeSpan.Zero };

        await Assert.ThrowsAsync<HttpRequestException>(() => service.CheckForUpdates());

        Assert.Equal(6, handler.Requests); // three urls, two rounds
    }

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
