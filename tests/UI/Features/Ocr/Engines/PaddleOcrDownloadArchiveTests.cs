using System;
using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Download;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// Guards the Paddle OCR download table against a half-applied version bump: the engine and
/// the models used to be pinned in several places, and the folder stripped when unpacking was
/// a separate literal from the URL it belongs to. Both failure modes only show up at runtime
/// (a 404, or an unpack that produces an empty install), so they are pinned down here.
/// </summary>
public class PaddleOcrDownloadArchiveTests
{
    public static TheoryData<PaddleOcrDownloadType> AllDownloadTypes()
    {
        var data = new TheoryData<PaddleOcrDownloadType>();
        foreach (var downloadType in Enum.GetValues<PaddleOcrDownloadType>())
        {
            data.Add(downloadType);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllDownloadTypes))]
    public void EveryDownloadType_HasAnArchive(PaddleOcrDownloadType downloadType)
    {
        var archive = PaddleOcr.GetArchive(downloadType);

        Assert.NotEmpty(archive.Urls);
        Assert.NotEmpty(archive.RootFolderInArchive);
        Assert.All(archive.Urls, url => Assert.StartsWith("https://github.com/timminator/PaddleOCR-Standalone/releases/download/", url, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(AllDownloadTypes))]
    public void EveryDownloadType_UsesOneRelease(PaddleOcrDownloadType downloadType)
    {
        // Engine and models are versioned independently upstream but downloaded from the same
        // tag here; mixing tags within one archive would download volumes that do not match.
        var tags = PaddleOcr.GetArchive(downloadType).Urls
            .Select(url => url[..url.LastIndexOf('/')])
            .Distinct();

        Assert.Single(tags);
    }

    [Theory]
    [MemberData(nameof(AllDownloadTypes))]
    public void MultiVolumeArchives_AreOrderedByVolumeNumber(PaddleOcrDownloadType downloadType)
    {
        // The extractor is handed the first downloaded file, so ".7z.001" has to come first.
        var fileNames = PaddleOcr.GetArchive(downloadType).Urls
            .Select(url => url[(url.LastIndexOf('/') + 1)..])
            .ToList();

        if (fileNames.Count == 1)
        {
            Assert.DoesNotContain(".7z.", fileNames[0], StringComparison.Ordinal);
            return;
        }

        Assert.Equal(fileNames.OrderBy(f => f, StringComparer.Ordinal), fileNames);
        Assert.EndsWith(".7z.001", fileNames[0], StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(AllDownloadTypes))]
    public void EngineArchives_StripTheFolderNamedAfterTheArchive(PaddleOcrDownloadType downloadType)
    {
        if (downloadType == PaddleOcrDownloadType.Models)
        {
            // The models archive is the exception: its root folder drops the ".VideOCR" part.
            var models = PaddleOcr.GetArchive(PaddleOcrDownloadType.Models);
            Assert.Equal("PaddleOCR.PP-OCRv5.support.files", models.RootFolderInArchive);
            return;
        }

        var archive = PaddleOcr.GetArchive(downloadType);
        var fileName = archive.Urls[0][(archive.Urls[0].LastIndexOf('/') + 1)..];

        Assert.Equal(fileName[..fileName.IndexOf(".7z", StringComparison.Ordinal)], archive.RootFolderInArchive);
    }
}
