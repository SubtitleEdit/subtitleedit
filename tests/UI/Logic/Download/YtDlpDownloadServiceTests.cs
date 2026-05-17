using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

public class YtDlpDownloadServiceTests
{
    [Theory]
    [InlineData("2025.07.21", true)]
    [InlineData(YtDlpDownloadService.CurrentVersion, false)]
    [InlineData("", true)]
    [InlineData("not-a-version", true)]
    public void IsVersionOutdated_DetectsStaleOrInvalidVersions(string installedVersion, bool expected)
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
}
