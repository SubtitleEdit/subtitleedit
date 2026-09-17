using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;
using System.Linq;

namespace UITests.Features.Ocr.Engines;

public class OllamaOcrTests
{
    private const string Prompt =
        "You are an OCR engine. The language is English. Output only the exact text visible in the image, nothing else. Separate two lines with a single newline.";

    [Fact]
    public void CleanOcrText_SingleCorrectLine_Unchanged()
    {
        var text = "- Henry, we're here. - We're here.";

        Assert.Equal(text, OllamaOcr.CleanOcrText(text, Prompt));
    }

    [Fact]
    public void CleanOcrText_TwoCorrectLines_Unchanged()
    {
        var text = "Henry, are you there?" + Environment.NewLine + "We're coming for you.";

        Assert.Equal(text, OllamaOcr.CleanOcrText(text, Prompt));
    }

    [Fact]
    public void CleanOcrText_RepeatedLine_KeepsFirst()
    {
        var line = "- Henry, we're here. - We're here.";
        var looped = string.Join(Environment.NewLine, System.Linq.Enumerable.Repeat(line, 100));

        Assert.Equal(line, OllamaOcr.CleanOcrText(looped, Prompt));
    }

    [Fact]
    public void CleanOcrText_TwoLineCycleRepeated_KeepsOneCycle()
    {
        var a = "Henry, are you there?";
        var b = "We're coming for you.";
        var looped = string.Join(Environment.NewLine, a, b, a, b, a, b, a, b);

        Assert.Equal(a + Environment.NewLine + b, OllamaOcr.CleanOcrText(looped, Prompt));
    }

    [Fact]
    public void CleanOcrText_MarkdownFenceAfterText_Stripped()
    {
        var text = "Henry, are you there?" + Environment.NewLine +
                   "We're coming for you." + Environment.NewLine +
                   "```markdown" + Environment.NewLine +
                   "Henry, are you there?";

        Assert.Equal("Henry, are you there?" + Environment.NewLine + "We're coming for you.",
            OllamaOcr.CleanOcrText(text, Prompt));
    }

    [Fact]
    public void CleanOcrText_PromptEchoAfterText_Stripped()
    {
        var text = "Henry, are you there?" + Environment.NewLine +
                   "We're coming for you." + Environment.NewLine +
                   Prompt;

        Assert.Equal("Henry, are you there?" + Environment.NewLine + "We're coming for you.",
            OllamaOcr.CleanOcrText(text, Prompt));
    }

    [Fact]
    public void CleanOcrText_LongNarrationFrame_KeepsAllLines()
    {
        // #14920: vertical Japanese narration holds a whole paragraph per frame - lines 5..N
        // used to be silently dropped.
        var text = string.Join(Environment.NewLine,
            "『第一章』", "あらすじ", "主人公は古い地図を手に", "山奥の小さな村を訪れた。",
            "村人たちは彼を温かく迎え", "祭りの準備を手伝うよう", "頼んだのだった。");

        Assert.Equal(text, OllamaOcr.CleanOcrText(text, Prompt));
    }

    [Fact]
    public void CleanOcrText_RunawayOutput_IsCapped()
    {
        var text = string.Join(Environment.NewLine, System.Linq.Enumerable.Range(1, 100).Select(i => "Line " + i));

        var result = OllamaOcr.CleanOcrText(text, Prompt);

        Assert.Equal(OllamaOcr.MaxLines, result.Split(Environment.NewLine).Length);
    }
}
