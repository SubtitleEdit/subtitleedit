using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class Phase7OutputTest : IDisposable
{
    private readonly string _tempRoot;

    public Phase7OutputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Phase7_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        <i>Hello</i>, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    private async Task<string> WriteInput()
    {
        var path = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(path, SrtContent);
        return path;
    }

    [Fact]
    public async Task ConvertAsync_PlainTextOutput_StripsHtmlTags()
    {
        var input = await WriteInput();
        var outFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "plaintext",
            OutputFolder = outFolder,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var outFile = Path.Combine(outFolder, "in.txt");
        Assert.True(File.Exists(outFile));
        var text = await File.ReadAllTextAsync(outFile);
        Assert.Contains("Hello, World!", text);
        Assert.Contains("This is a test subtitle.", text);
        Assert.DoesNotContain("<i>", text);
        Assert.DoesNotContain("</i>", text);
    }

    [Fact]
    public async Task ConvertAsync_MultipleReplace_AppliesAllActiveRules()
    {
        var input = await WriteInput();
        var outFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outFolder);

        var rulesXml = Path.Combine(_tempRoot, "rules.xml");
        await File.WriteAllTextAsync(rulesXml, """
            <?xml version="1.0" encoding="utf-8"?>
            <MultipleSearchAndReplaceGroups>
              <Group>
                <Name>Demo</Name>
                <IsActive>true</IsActive>
                <Rules>
                  <Rule>
                    <Active>true</Active>
                    <FindWhat>Hello</FindWhat>
                    <ReplaceWith>HOLA</ReplaceWith>
                    <SearchType>CaseSensitive</SearchType>
                  </Rule>
                  <Rule>
                    <Active>true</Active>
                    <FindWhat>test</FindWhat>
                    <ReplaceWith>TEST</ReplaceWith>
                    <SearchType>Normal</SearchType>
                  </Rule>
                </Rules>
              </Group>
            </MultipleSearchAndReplaceGroups>
            """);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "subrip",
            OutputFolder = outFolder,
            Overwrite = true,
            MultipleReplaceFile = rulesXml,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var output = await File.ReadAllTextAsync(Path.Combine(outFolder, "in.srt"));
        Assert.Contains("HOLA", output);            // case-sensitive replacement (Hello → HOLA, surrounded by <i>...</i>)
        Assert.DoesNotContain("Hello", output);     // original text gone
        Assert.Contains("This is a TEST subtitle.", output); // case-insensitive replacement
    }

    [Fact]
    public async Task ConvertAsync_CustomTextFormat_RendersTemplate()
    {
        var input = await WriteInput();
        var outFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outFolder);

        var templateXml = Path.Combine(_tempRoot, "template.xml");
        await File.WriteAllTextAsync(templateXml, """
            <?xml version="1.0" encoding="utf-8"?>
            <CustomFormatItem>
              <Name>Demo</Name>
              <Extension>.txt</Extension>
              <FormatHeader>=== {#lines} entries ===
            </FormatHeader>
              <FormatParagraph>{number}: {start} -> {end} :: {text}
            </FormatParagraph>
              <FormatFooter>=== End ===</FormatFooter>
              <FormatTimeCode>hh:mm:ss</FormatTimeCode>
            </CustomFormatItem>
            """);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "customtext",
            OutputFolder = outFolder,
            Overwrite = true,
            CustomFormatFile = templateXml,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var rendered = await File.ReadAllTextAsync(Path.Combine(outFolder, "in.txt"));
        Assert.Contains("=== 2 entries ===", rendered);
        Assert.Contains("=== End ===", rendered);
        Assert.Contains("1: 00:00:01 -> 00:00:04", rendered);
        Assert.Contains("2: 00:00:05 -> 00:00:08", rendered);
    }

    [Fact]
    public async Task ConvertAsync_CustomTextFormat_WithoutTemplate_Errors()
    {
        var input = await WriteInput();
        var outFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "customtext",
            OutputFolder = outFolder,
            Overwrite = true,
            // No CustomFormatFile
        });

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Contains("--customformat", result.Errors[0]);
    }
}
