using Nikse.SubtitleEdit.Logic.SevenZipExtractor;

namespace UITests.Logic.SevenZipExtractor;

public class UnpackerTests : IDisposable
{
    private const string ArchiveBase64 = "N3q8ryccAASRme6J7wAAAAAAAAAWAAAAAAAAANtGIGABABZlbmdpbmVleGVjdXRhYmxlbGljZW5zZQDgAlUAzF0AAIEzB64Pz7XvEA/Ual595dfdm+avUngQmcP0+WCm1/B0sii/q/CHmny4m6aKmeaETC1j6oYCbmCvutV7/FCFvtanCgHlX/zjPbLzpoLXTRu5YlZGWdBR+YNjTU+jaIr+BgzCBmucvUyzM93+Zf2XK7H3EeosnQ88EhVRM096o8AbJeFkMSXMddJen437oUr4YA7ZtjnpAZxnmMuFuDTcR0hzuZ2JwNeddfoONNW9EvkwlskRI/+0AJxZRToJCGFpXlUTmVVoSecnAAAAABcGGwEJgNQABwsBAAEhIQEYDIJWAAA=";
    private const string AbsolutePathArchiveBase64 = "N3q8ryccAASLHia9CwAAAAAAAACCAAAAAAAAACgymrgBAAZvdXRzaWRlAAEEBgABCQsABwsBAAEhIQEADAcACAoBpv3qtQAABQEZDAAAAAAAAAAAAAAAABE7AC8AdABtAHAALwBzAGUAMQAzADIAOAA4AGUAdgBpAGwALwBvAHUAdABzAGkAZABlAC4AdAB4AHQAAAAZABQKAQB8salAgSXdARUGAQAggKSBAAA=";

    private readonly string _tempFolder;

    public UnpackerTests()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), "SeUnpackerTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempFolder);
    }

    [Fact]
    public void Extract7ZipSlow_StripsWrapperWithoutRepeatingOriginalEntryPath()
    {
        var archiveFileName = Path.Combine(_tempFolder, "fixture.7z");
        var outputFolder = Path.Combine(_tempFolder, "output");
        File.WriteAllBytes(archiveFileName, Convert.FromBase64String(ArchiveBase64));

        Unpacker.Extract7ZipSlow(
            archiveFileName,
            outputFolder,
            "Faster-Whisper-XXL",
            new CancellationTokenSource(),
            _ => { });

        Assert.Equal("engine", File.ReadAllText(Path.Combine(outputFolder, "_xxl_data", "numpy", "core", "engine.bin")));
        Assert.Equal("executable", File.ReadAllText(Path.Combine(outputFolder, "whisper-faster")));
        Assert.Equal("license", File.ReadAllText(Path.Combine(outputFolder, "license.txt")));
        Assert.False(Directory.Exists(Path.Combine(outputFolder, "_xxl_data", "numpy", "core", "Faster-Whisper-XXL")));
        Assert.Equal(3, Directory.GetFiles(outputFolder, "*", SearchOption.AllDirectories).Length);
    }

    [Fact]
    public void Extract7ZipSlow_RejectsAbsoluteEntryPath()
    {
        var archiveFileName = Path.Combine(_tempFolder, "absolute-path.7z");
        var outputFolder = Path.Combine(_tempFolder, "absolute-output");
        File.WriteAllBytes(archiveFileName, Convert.FromBase64String(AbsolutePathArchiveBase64));

        var exception = Assert.Throws<InvalidDataException>(() => Unpacker.Extract7ZipSlow(
            archiveFileName,
            outputFolder,
            string.Empty,
            new CancellationTokenSource(),
            _ => { }));

        Assert.Contains("outside the extraction folder", exception.Message);
        Assert.Empty(Directory.GetFiles(_tempFolder, "outside.txt", SearchOption.AllDirectories));
    }

    public void Dispose()
    {
        Directory.Delete(_tempFolder, true);
    }
}
