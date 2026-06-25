using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;

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
}
