using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

public class YtDlpDownloadServiceTests
{
    [Theory]
    [InlineData("2025.07.21", true)]   // definitively older → nag
    [InlineData(YtDlpDownloadService.CurrentVersion, false)] // current → don't nag
    [InlineData("", false)]            // unknown → don't nag (subprocess returned nothing)
    [InlineData("not-a-version", false)] // unknown → don't nag (unparseable output)
    public void IsVersionOutdated_OnlyFlagsConfirmedOlderVersions(string installedVersion, bool expected)
    {
        var actual = YtDlpDownloadService.IsVersionOutdated(installedVersion);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsVersionOutdated_AllowsVersionsNewerThanCurrent()
    {
        var currentVersion = Version.Parse(YtDlpDownloadService.CurrentVersion);
        var newerVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);

        var actual = YtDlpDownloadService.IsVersionOutdated(newerVersion.ToString());

        Assert.False(actual);
    }

    [Fact]
    public async Task IsInstalledVersionOutdated_ThrowsWhenAlreadyCanceled()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            YtDlpDownloadService.IsInstalledVersionOutdated(cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData("2026.03.17\n", "2026.03.17")]
    [InlineData("v2026.03.17\n", "2026.03.17")]
    [InlineData("  2026.03.17  \n", "2026.03.17")]
    [InlineData("2026.03.17.1\n", "2026.03.17.1")]
    [InlineData("[debug] Bootstrapping...\nExtracting bundle\n2026.03.17\n", "2026.03.17")]
    [InlineData("", null)]
    [InlineData("   \n  \n", null)]
    [InlineData("not a version at all", null)]
    public void ExtractVersion_PullsVersionFromAnyLineInOutput(string output, string? expected)
    {
        var actual = YtDlpDownloadService.ExtractVersion(output);

        Assert.Equal(expected, actual);
    }
}
