using System.Text;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

/// <summary>
/// ffmpeg, libmpv and libVLC are pinned URLs: every one of them must have a SHA-256 on record,
/// otherwise <see cref="DownloadHashManager.VerifyDownloadAsync(Stream, string?, string, CancellationToken)"/>
/// silently skips the check after a URL bump - and that hash must have been recorded for the
/// current URL, otherwise a stale hash fails every download.
/// </summary>
public class PinnedDownloadHashTests
{
    public static IEnumerable<object[]> PinnedUrls()
    {
        foreach (var url in FfmpegDownloadService.AllUrls)
        {
            yield return [url, FfmpegDownloadService.GetHashKey(url)!];
        }

        foreach (var url in LibMpvDownloadService.AllUrls)
        {
            yield return [url, LibMpvDownloadService.GetHashKey(url)!];
        }

        foreach (var url in LibVlcDownloadService.AllUrls)
        {
            yield return [url, LibVlcDownloadService.GetHashKey(url)!];
        }
    }

    [Theory]
    [MemberData(nameof(PinnedUrls))]
    public void EveryPinnedUrl_HasAKnownSha256(string url, string? key)
    {
        Assert.False(string.IsNullOrEmpty(key), $"No hash key for {url}");
        var hash = DownloadHashManager.GetLatestKnownHash(key!);
        Assert.NotNull(hash);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Theory]
    [MemberData(nameof(PinnedUrls))]
    public void EveryPinnedUrl_MatchesTheUrlItsHashWasRecordedFor(string url, string? key)
    {
        // A URL bumped in the service without bumping the hash still has *a* hash on record -
        // the stale one - so every download would then fail verification.
        Assert.True(DownloadHashManager.PinnedHashUrls.TryGetValue(key!, out var hashedUrl), $"No recorded URL for {key}");
        Assert.Equal(hashedUrl, url);
    }

    [Fact]
    public async Task VerifyDownloadAsync_Stream_ThrowsOnMismatch()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not ffmpeg"));

        await Assert.ThrowsAsync<IOException>(() =>
            DownloadHashManager.VerifyDownloadAsync(stream, DownloadHashManager.Ffmpeg.Windows, "ffmpeg", CancellationToken.None));
    }

    [Fact]
    public async Task VerifyDownloadAsync_File_ThrowsOnMismatch()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".7z");
        await File.WriteAllTextAsync(fileName, "not vlc");
        try
        {
            await Assert.ThrowsAsync<IOException>(() =>
                DownloadHashManager.VerifyDownloadAsync(fileName, DownloadHashManager.LibVlc.WindowsX64, "libVLC", CancellationToken.None));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public async Task VerifyDownloadAsync_EmptyStream_Throws()
    {
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<IOException>(() =>
            DownloadHashManager.VerifyDownloadAsync(stream, DownloadHashManager.Ffmpeg.Windows, "ffmpeg", CancellationToken.None));
    }

    [Fact]
    public async Task VerifyDownloadAsync_MissingOrEmptyFile_Throws()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".7z");
        await Assert.ThrowsAsync<IOException>(() =>
            DownloadHashManager.VerifyDownloadAsync(fileName, DownloadHashManager.LibVlc.WindowsX64, "libVLC", CancellationToken.None));

        await File.WriteAllBytesAsync(fileName, []);
        try
        {
            await Assert.ThrowsAsync<IOException>(() =>
                DownloadHashManager.VerifyDownloadAsync(fileName, DownloadHashManager.LibVlc.WindowsX64, "libVLC", CancellationToken.None));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public async Task VerifyDownloadAsync_UnknownKey_IsNoOp()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("anything"));

        await DownloadHashManager.VerifyDownloadAsync(stream, "No.Such.Key", "x", CancellationToken.None);
        await DownloadHashManager.VerifyDownloadAsync(stream, null, "x", CancellationToken.None);
    }
}
