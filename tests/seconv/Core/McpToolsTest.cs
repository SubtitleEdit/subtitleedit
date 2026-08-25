using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using SeConv.Mcp;
using Xunit;

namespace SeConvTests.Core;

public class McpToolsTest : IDisposable
{
    private readonly string _tempDir;

    public McpToolsTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "seconv-mcp-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private static JsonElement Payload(CallToolResult result)
    {
        Assert.NotEqual(true, result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        return JsonDocument.Parse(text).RootElement;
    }

    private static string ErrorText(CallToolResult result)
    {
        Assert.True(result.IsError);
        return Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
    }

    [Fact]
    public void ListFormats_FilterMatchesIdNameAndExtension()
    {
        var all = Payload(SubtitleTools.ListFormats());
        Assert.True(all.GetProperty("total").GetInt32() > 300);

        var filtered = Payload(SubtitleTools.ListFormats("subrip"));
        var ids = filtered.GetProperty("formats").EnumerateArray().Select(f => f.GetProperty("id").GetString()).ToList();
        Assert.Contains("SubRip", ids);
        Assert.Equal(ids.Count, filtered.GetProperty("total").GetInt32());
        Assert.True(ids.Count < all.GetProperty("total").GetInt32());
    }

    [Fact]
    public void SubtitleInfo_DetectsFixtureFormat()
    {
        var info = Payload(SubtitleTools.SubtitleInfo(Fixtures.Path("test.srt")));
        Assert.Equal("SubRip", info.GetProperty("format").GetString());
        Assert.True(info.GetProperty("paragraphCount").GetInt32() > 0);
    }

    [Fact]
    public void SubtitleInfo_MissingFile_IsToolError()
    {
        var message = ErrorText(SubtitleTools.SubtitleInfo(Path.Combine(_tempDir, "nope.srt")));
        Assert.Contains("nope.srt", message);
    }

    [Fact]
    public void ReadSubtitle_PagesParagraphs()
    {
        var path = Fixtures.Path("test.srt");
        var all = Payload(SubtitleTools.ReadSubtitle(path));
        var total = all.GetProperty("total").GetInt32();
        Assert.True(total > 1);
        Assert.False(all.GetProperty("hasMore").GetBoolean());

        var page = Payload(SubtitleTools.ReadSubtitle(path, start: 2, count: 1));
        Assert.Equal(1, page.GetProperty("returned").GetInt32());
        Assert.Equal(total > 2, page.GetProperty("hasMore").GetBoolean());
        var p = Assert.Single(page.GetProperty("paragraphs").EnumerateArray());
        Assert.Equal(2, p.GetProperty("number").GetInt32());
        Assert.True(p.GetProperty("endMs").GetInt64() >= p.GetProperty("startMs").GetInt64());
        Assert.False(string.IsNullOrEmpty(p.GetProperty("text").GetString()));
    }

    [Fact]
    public void LintSubtitle_ReportsOverlap()
    {
        var path = Path.Combine(_tempDir, "overlap.srt");
        File.WriteAllText(path,
            "1\n00:00:01,000 --> 00:00:05,000\nFirst.\n\n" +
            "2\n00:00:04,000 --> 00:00:06,000\nSecond.\n");

        var report = Payload(SubtitleTools.LintSubtitle(path));
        Assert.False(report.GetProperty("isClean").GetBoolean());
        Assert.Contains(report.GetProperty("issues").EnumerateArray(),
            i => i.GetProperty("type").GetString() == "overlap");
    }

    [Fact]
    public void ListRules_ExposeIds()
    {
        var fce = Payload(SubtitleTools.ListFixCommonErrorsRules());
        Assert.Contains(fce.GetProperty("rules").EnumerateArray(), r => r.GetProperty("id").GetString() == "FixCommas");

        var rf = Payload(SubtitleTools.ListRemoveFormattingRules());
        Assert.True(rf.GetProperty("total").GetInt32() > 0);
    }

    [Fact]
    public async Task ConvertSubtitle_WritesTargetFormatWithOffset()
    {
        var result = Payload(await SubtitleTools.ConvertSubtitle(
            inputs: [Fixtures.Path("test.srt")],
            format: "webvtt",
            outputFolder: _tempDir,
            offset: "00:00:01.000"));

        Assert.True(result.GetProperty("success").GetBoolean());
        var file = Assert.Single(result.GetProperty("files").EnumerateArray());
        var output = file.GetProperty("output").GetString()!;
        Assert.EndsWith(".vtt", output);
        Assert.True(File.Exists(output));

        var source = Payload(SubtitleTools.ReadSubtitle(Fixtures.Path("test.srt"), count: 1));
        var converted = Payload(SubtitleTools.ReadSubtitle(output, count: 1));
        var sourceStart = source.GetProperty("paragraphs")[0].GetProperty("startMs").GetInt64();
        var convertedStart = converted.GetProperty("paragraphs")[0].GetProperty("startMs").GetInt64();
        Assert.Equal(sourceStart + 1000, convertedStart);
    }

    [Fact]
    public async Task ConvertSubtitle_UnknownOperation_IsToolError()
    {
        var message = ErrorText(await SubtitleTools.ConvertSubtitle(
            inputs: [Fixtures.Path("test.srt")],
            format: "srt",
            outputFolder: _tempDir,
            operations: ["Bogus"]));

        Assert.Contains("Unknown operation 'Bogus'", message);
        Assert.Empty(Directory.GetFiles(_tempDir));
    }

    [Fact]
    public async Task ConvertSubtitle_RuleSelectionImpliesOperation()
    {
        var path = Path.Combine(_tempDir, "dots.srt");
        File.WriteAllText(path, "1\n00:00:01,000 --> 00:00:03,000\n<i>Hello</i> world..\n");

        var result = Payload(await SubtitleTools.ConvertSubtitle(
            inputs: [path],
            format: "srt",
            outputFilename: Path.Combine(_tempDir, "out.srt"),
            removeFormattingRules: "all"));

        Assert.True(result.GetProperty("success").GetBoolean());
        var converted = Payload(SubtitleTools.ReadSubtitle(Path.Combine(_tempDir, "out.srt")));
        var text = converted.GetProperty("paragraphs")[0].GetProperty("text").GetString();
        Assert.DoesNotContain("<i>", text);
    }

    [Fact]
    public async Task StdioServer_ListsToolsAndAnswersCalls()
    {
        var dll = Path.Combine(AppContext.BaseDirectory, "seconv.dll");
        Assert.True(File.Exists(dll), $"seconv.dll not found next to the tests: {dll}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "seconv",
            Command = "dotnet",
            Arguments = [dll, "mcp"],
        });

        var ct = TestContext.Current.CancellationToken;
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: ct);
        Assert.Equal("seconv", client.ServerInfo.Name);

        var tools = await client.ListToolsAsync(cancellationToken: ct);
        var names = tools.Select(t => t.Name).ToHashSet();
        foreach (var expected in new[]
                 {
                     "list_formats", "subtitle_info", "read_subtitle", "lint_subtitle",
                     "convert_subtitle", "list_fix_common_errors_rules", "list_remove_formatting_rules",
                 })
        {
            Assert.Contains(expected, names);
        }

        var info = await client.CallToolAsync("subtitle_info",
            new Dictionary<string, object?> { ["path"] = Fixtures.Path("test.vtt") },
            cancellationToken: ct);
        var payload = Payload(info);
        Assert.Equal("WebVTT", payload.GetProperty("format").GetString());

        var failure = await client.CallToolAsync("subtitle_info",
            new Dictionary<string, object?> { ["path"] = Path.Combine(_tempDir, "missing.srt") },
            cancellationToken: ct);
        Assert.Contains("missing.srt", ErrorText(failure));
    }
}
