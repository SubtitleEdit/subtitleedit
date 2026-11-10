using SeConv.Core;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

public class SubtitleInfoGathererTest : IDisposable
{
    private readonly string _tempDir;

    public SubtitleInfoGathererTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "seconv-info-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string WriteSrt(string name, string content, Encoding? encoding = null)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, encoding ?? new UTF8Encoding(true));
        return path;
    }

    [Fact]
    public void Gather_ReadsBasicSrtFields()
    {
        var path = WriteSrt("a.srt",
            "1\n00:00:01,000 --> 00:00:03,000\nHello world.\n\n" +
            "2\n00:00:05,500 --> 00:00:07,250\nGoodbye.\n");

        var info = SubtitleInfoGatherer.Gather(path);

        Assert.Equal(path, info.Path);
        Assert.Equal("SubRip", info.Format);
        Assert.Equal(".srt", info.Extension);
        Assert.Equal(2, info.ParagraphCount);
        Assert.Equal(1000, info.FirstStartMs);
        Assert.Equal(7250, info.LastEndMs);
        Assert.Equal(6250, info.DurationMs);
        Assert.True(info.FileSizeBytes > 0);
    }

    [Fact]
    public void Gather_DetectsUtf8Bom()
    {
        var path = WriteSrt("bom.srt",
            "1\n00:00:01,000 --> 00:00:03,000\nHello.\n",
            new UTF8Encoding(true));

        var info = SubtitleInfoGatherer.Gather(path);

        Assert.Contains("BOM", info.Encoding);
    }

    [Fact]
    public void Gather_MissingFile_Throws()
    {
        Assert.Throws<FileNotFoundException>(
            () => SubtitleInfoGatherer.Gather(Path.Combine(_tempDir, "nope.srt")));
    }

    [Theory]
    [InlineData("我们这个国家的时间很好。", "请来学习中文，说明一下为什么。", "zh-Hans")]
    [InlineData("我們這個國家的時間很好。", "請來學習中文，說明一下為什麼。", "zh-Hant")]
    public void Gather_ReportsChineseScript(string line1, string line2, string expected)
    {
        var path = WriteSrt("zh.srt",
            "1\n00:00:01,000 --> 00:00:03,000\n" + line1 + "\n\n" +
            "2\n00:00:04,000 --> 00:00:06,000\n" + line2 + "\n");

        var info = SubtitleInfoGatherer.Gather(path);

        Assert.Equal(expected, info.Language);
    }

    [Theory]
    [InlineData(936, "我们这个国家的时间很好。", "请来学习中文，说明一下为什么。", "zh-Hans")]
    [InlineData(950, "我們這個國家的時間很好。", "請來學習中文，說明一下為什麼。", "zh-Hant")]
    public void Gather_ReportsChineseScriptForAnsiFile(int codePage, string line1, string line2, string expected)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = WriteSrt("zh-ansi.srt",
            "1\n00:00:01,000 --> 00:00:03,000\n" + line1 + "\n\n" +
            "2\n00:00:04,000 --> 00:00:06,000\n" + line2 + "\n",
            Encoding.GetEncoding(codePage));

        var info = SubtitleInfoGatherer.Gather(path);

        Assert.Equal(expected, info.Language);
    }

    [Fact]
    public void Gather_ChineseWithoutScriptMarkersStaysZh()
    {
        var path = WriteSrt("zh-neutral.srt",
            "1\n00:00:01,000 --> 00:00:03,000\n你好。\n\n" +
            "2\n00:00:04,000 --> 00:00:06,000\n我很好。\n");

        var info = SubtitleInfoGatherer.Gather(path);

        Assert.Equal("zh", info.Language);
    }
}
