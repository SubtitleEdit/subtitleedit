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

    [Fact]
    public void EnumerateDownloadedSubtitles_FindsSidecarFiles_FiltersByExtensionAndSorts()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EnumerateSubsTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "video.mkv"), ""); // not a sub — filtered out by ext
            File.WriteAllText(Path.Combine(tempDir, "video.es.vtt"), "");
            File.WriteAllText(Path.Combine(tempDir, "video.en.vtt"), "");
            File.WriteAllText(Path.Combine(tempDir, "video.fr.srt"), "");
            File.WriteAllText(Path.Combine(tempDir, "video.txt"), ""); // no language segment
            File.WriteAllText(Path.Combine(tempDir, "other.en.vtt"), ""); // different stem

            var result = YtDlpDownloadService.EnumerateDownloadedSubtitles(tempDir, "video");

            Assert.Equal(3, result.Count);
            Assert.Equal("en", result[0].LanguageCode);
            Assert.Equal("vtt", result[0].Format);
            Assert.Equal("es", result[1].LanguageCode);
            Assert.Equal("fr", result[2].LanguageCode);
            Assert.Equal("srt", result[2].Format);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void EnumerateDownloadedSubtitles_MissingDirOrEmptyStem_ReturnsEmpty()
    {
        Assert.Empty(YtDlpDownloadService.EnumerateDownloadedSubtitles("/nonexistent/path", "video"));
        Assert.Empty(YtDlpDownloadService.EnumerateDownloadedSubtitles(Path.GetTempPath(), ""));
    }

    [Fact]
    public void KnownSha256_ContainsCurrentVersion_WithAllAssetsAsHex()
    {
        Assert.True(YtDlpDownloadService.KnownSha256.ContainsKey(YtDlpDownloadService.CurrentVersion),
            "Checksums for CurrentVersion must be on record so downloads can be verified.");

        foreach (var (version, byAsset) in YtDlpDownloadService.KnownSha256)
        {
            foreach (var expectedAsset in new[] { "yt-dlp.exe", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_macos" })
            {
                Assert.True(byAsset.ContainsKey(expectedAsset), $"{version} is missing checksum for {expectedAsset}");
            }

            foreach (var hash in byAsset.Values)
            {
                Assert.Equal(64, hash.Length); // SHA-256 = 32 bytes = 64 hex chars
                Assert.Matches("^[0-9a-f]{64}$", hash);
            }
        }
    }

    [Fact]
    public async Task ComputeSha256Async_ProducesKnownDigest()
    {
        // SHA-256 of the ASCII bytes "abc" — the canonical NIST test vector.
        var path = Path.Combine(Path.GetTempPath(), "Sha256Test_" + Guid.NewGuid().ToString("N"));
        await File.WriteAllTextAsync(path, "abc", TestContext.Current.CancellationToken);
        try
        {
            var actual = await YtDlpDownloadService.ComputeSha256Async(path, TestContext.Current.CancellationToken);

            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task VerifyChecksumAsync_Mismatch_ThrowsAndDeletesFile()
    {
        // A file named like a real asset but with content that can't match the
        // published hash → must be rejected and removed.
        var path = Path.Combine(Path.GetTempPath(), "VerifyMismatch_" + Guid.NewGuid().ToString("N"), "yt-dlp.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "this is not really yt-dlp", TestContext.Current.CancellationToken);
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                YtDlpDownloadService.VerifyChecksumAsync(path, YtDlpDownloadService.CurrentVersion, TestContext.Current.CancellationToken));

            Assert.False(File.Exists(path), "A binary that fails verification must be deleted.");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyChecksumAsync_UnknownAsset_IsNoOp_AndKeepsFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "VerifyUnknownAsset_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "not-a-yt-dlp-asset");
        await File.WriteAllTextAsync(path, "content", TestContext.Current.CancellationToken);
        try
        {
            // No checksum on record for this asset name → nothing to verify, no throw.
            await YtDlpDownloadService.VerifyChecksumAsync(path, YtDlpDownloadService.CurrentVersion, TestContext.Current.CancellationToken);

            Assert.True(File.Exists(path));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyChecksumAsync_UnknownVersion_IsNoOp_AndKeepsFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "VerifyUnknownVersion_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "yt-dlp.exe");
        await File.WriteAllTextAsync(path, "content", TestContext.Current.CancellationToken);
        try
        {
            // No checksums on record for this version → nothing to verify, no throw.
            await YtDlpDownloadService.VerifyChecksumAsync(path, "1999.01.01", TestContext.Current.CancellationToken);

            Assert.True(File.Exists(path));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
