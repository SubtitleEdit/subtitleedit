using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;

namespace UITests.Features.Video.TextToSpeech.DownloadTts;

public class DownloadTtsViewModelTests
{
    [Fact]
    public void PiperLinuxSymbolicLinks_UsesVersionIndependentLibraryPatterns()
    {
        var links = DownloadTtsViewModel.PiperLinuxSymbolicLinks.ToList();

        Assert.Contains(links, link =>
            link.SourceFilePattern == "libpiper_phonemize.so.1.*" &&
            link.LinkFileName == "libpiper_phonemize.so.1");
        Assert.Contains(links, link =>
            link.SourceFilePattern == "libespeak-ng.so.1.*" &&
            link.LinkFileName == "libespeak-ng.so.1");
    }

    [Fact]
    public void FindSymbolicLinkSource_FindsVersionedLibraryFile()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        try
        {
            var sourcePath = Path.Combine(folder, "libespeak-ng.so.1.53.0.0");
            File.WriteAllText(sourcePath, string.Empty);

            var result = DownloadTtsViewModel.FindSymbolicLinkSource(folder, "libespeak-ng.so.1.*");

            Assert.Equal(sourcePath, result);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void CreateSymbolicLinkProcessStartInfo_UsesForceNoDereferenceLinkCommand()
    {
        var processStartInfo = DownloadTtsViewModel.CreateSymbolicLinkProcessStartInfo(
            "/tmp/piper/libespeak-ng.so.1.53.0.0",
            "/tmp/piper/libespeak-ng.so.1");

        Assert.Equal("/bin/bash", processStartInfo.FileName);
        Assert.Equal("-c", processStartInfo.ArgumentList[0]);
        Assert.Contains("ln -sfn --", processStartInfo.ArgumentList[1]);
        Assert.Contains("'/tmp/piper/libespeak-ng.so.1.53.0.0'", processStartInfo.ArgumentList[1]);
        Assert.Contains("'/tmp/piper/libespeak-ng.so.1'", processStartInfo.ArgumentList[1]);
    }
}
