using Nikse.SubtitleEdit.Features.Ocr.Download;

namespace UITests.Features.Ocr.Download;

/// <summary>
/// Starting a PaddleOCR download sweeps GUID temp folders left by aborted runs - but a download
/// running at the same time in another window or dialog must keep its folder.
/// </summary>
public class PaddleOcrStaleDownloadFolderTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public PaddleOcrStaleDownloadFolderTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try { Directory.Delete(_folder, true); } catch { /* best effort */ }
    }

    [Fact]
    public void FolderWithArchiveBeingWritten_IsNotStale()
    {
        var archive = Path.Combine(_folder, "PaddleOCR.7z");
        File.WriteAllText(archive, "partial");
        var old = DateTime.UtcNow.AddDays(-2);
        Directory.SetLastWriteTimeUtc(_folder, old); // folder entry untouched since the file was created
        File.SetLastWriteTimeUtc(archive, DateTime.UtcNow); // ... but the download keeps writing

        Assert.False(DownloadPaddleOcrViewModel.IsStaleDownloadFolder(_folder, DateTime.UtcNow));
    }

    [Fact]
    public void FolderUntouchedForHours_IsStale()
    {
        var archive = Path.Combine(_folder, "PaddleOCR.7z");
        File.WriteAllText(archive, "partial");
        var old = DateTime.UtcNow - DownloadPaddleOcrViewModel.StaleDownloadFolderAge - TimeSpan.FromMinutes(1);
        File.SetLastWriteTimeUtc(archive, old);
        Directory.SetLastWriteTimeUtc(_folder, old);

        Assert.True(DownloadPaddleOcrViewModel.IsStaleDownloadFolder(_folder, DateTime.UtcNow));
    }

    [Fact]
    public void NewEmptyFolder_IsNotStale()
    {
        Assert.False(DownloadPaddleOcrViewModel.IsStaleDownloadFolder(_folder, DateTime.UtcNow));
    }

    [Fact]
    public void MissingFolder_IsNotStale()
    {
        Assert.False(DownloadPaddleOcrViewModel.IsStaleDownloadFolder(Path.Combine(_folder, "missing"), DateTime.UtcNow));
    }
}
