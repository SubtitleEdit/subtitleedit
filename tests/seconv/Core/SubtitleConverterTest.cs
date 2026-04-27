using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class SubtitleConverterTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    public SubtitleConverterTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SeConvTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public async Task ConvertAsync_FixtureSrtToVtt_ProducesOutput()
    {
        // Arrange
        var inputFile = Fixtures.Path("test.srt");
        Assert.True(File.Exists(inputFile), $"Fixture missing: {inputFile}");

        var outputFolder = Path.Combine(_tempRoot, "fixture-out");
        Directory.CreateDirectory(outputFolder);

        var options = new ConversionOptions
        {
            Patterns = [inputFile],
            Format = "WebVTT",
            OutputFolder = outputFolder,
            Overwrite = true,
        };

        // Act
        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(options);

        // Assert
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(1, result.SuccessfulFiles);
        Assert.Single(Directory.GetFiles(outputFolder, "*.vtt"));
    }

    [Fact]
    public async Task ConvertAsync_TwoInputFiles_OneWithCommaInPath_ConvertsAllFiles()
    {
        // Arrange
        // First input file: plain folder
        var plainFolder = Path.Combine(_tempRoot, "normal");
        Directory.CreateDirectory(plainFolder);
        var file1 = Path.Combine(plainFolder, "input1.srt");
        await File.WriteAllTextAsync(file1, SrtContent);

        // Second input file: folder name contains a comma
        var commaFolder = Path.Combine(_tempRoot, "testing, testing");
        Directory.CreateDirectory(commaFolder);
        var file2 = Path.Combine(commaFolder, "input2.srt");
        await File.WriteAllTextAsync(file2, SrtContent);

        var outputFolder = Path.Combine(_tempRoot, "output");
        Directory.CreateDirectory(outputFolder);

        var options = new ConversionOptions
        {
            // Each path is passed as a separate element (mirrors how the CLI receives them
            // when the user writes: seconv "path1" "path2, with comma" --format ASS)
            Patterns = [file1, file2],
            Format = "Advanced Sub Station Alpha",
            OutputFolder = outputFolder,
            Overwrite = true,
        };

        // Act
        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(options);

        // Assert
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(2, result.SuccessfulFiles);

        var outputFiles = Directory.GetFiles(outputFolder, "*.ass");
        Assert.Equal(2, outputFiles.Length);
    }
}
