using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OutputFileNameTest : IDisposable
{
    private readonly string _tempRoot;

    public OutputFileNameTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "OutputFileName_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private ConversionOptions Opts(bool overwrite = false, string? outputFilename = null) =>
        new()
        {
            Patterns = ["dummy.srt"],
            Format = "WebVTT",
            OutputFolder = _tempRoot,
            Overwrite = overwrite,
            OutputFilename = outputFilename,
        };

    [Fact]
    public void Resolve_FileNotExists_ReturnsBasePath()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts());

        Assert.Equal(Path.Combine(_tempRoot, "input.vtt"), result);
    }

    [Fact]
    public void Resolve_FileExistsAndOverwrite_ReturnsBasePath()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "preexisting");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true));

        Assert.Equal(Path.Combine(_tempRoot, "input.vtt"), result);
    }

    [Fact]
    public void Resolve_FileExistsNoOverwrite_RotatesToUnderscore2()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "preexisting");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: false));

        Assert.Equal(Path.Combine(_tempRoot, "input_2.vtt"), result);
    }

    [Fact]
    public void Resolve_MultipleCollisions_PicksNextFreeNumber()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "");
        File.WriteAllText(Path.Combine(_tempRoot, "input_2.vtt"), "");
        File.WriteAllText(Path.Combine(_tempRoot, "input_3.vtt"), "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: false));

        Assert.Equal(Path.Combine(_tempRoot, "input_4.vtt"), result);
    }

    [Fact]
    public void Resolve_OutputFilenameSet_UsesItVerbatim()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        var explicitOutput = Path.Combine(_tempRoot, "renamed.vtt");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true, outputFilename: explicitOutput));

        Assert.Equal(explicitOutput, result);
    }
}
