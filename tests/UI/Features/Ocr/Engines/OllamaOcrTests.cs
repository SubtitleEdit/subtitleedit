using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;

namespace UITests.Features.Ocr.Engines;

public class OllamaOcrTests
{
    [Fact]
    public void CollapseRepetition_SingleLineRepeated_CollapsesToOne()
    {
        var line = "- Henry, we're here. - We're here.";
        var looped = string.Join(Environment.NewLine, System.Linq.Enumerable.Repeat(line, 100));

        var result = OllamaOcr.CollapseRepetition(looped);

        Assert.Equal(line, result);
    }

    [Fact]
    public void CollapseRepetition_TwoLineCycleRepeated_CollapsesToOneCycle()
    {
        var block = "- Henry, we're here." + Environment.NewLine + "- We're here.";
        var looped = string.Join(Environment.NewLine, System.Linq.Enumerable.Repeat(block, 30));

        var result = OllamaOcr.CollapseRepetition(looped);

        Assert.Equal(block, result);
    }

    [Fact]
    public void CollapseRepetition_DominantLineWithTrailingVariants_CollapsesToDominant()
    {
        // Mirrors a real bounded loop: the correct line repeated many times, then a couple of
        // OCR spacing variants and a truncated tail (from the token cap).
        var line = "- Henry, we're here. - We're here.";
        var lines = new System.Collections.Generic.List<string>();
        for (var i = 0; i < 27; i++)
        {
            lines.Add(line);
        }
        lines.Add("- Henry,we're here. - We're here.");
        lines.Add("- Henry, we're here. - We're");

        var result = OllamaOcr.CollapseRepetition(string.Join(Environment.NewLine, lines));

        Assert.Equal(line, result);
    }

    [Fact]
    public void CollapseRepetition_NormalTwoLineText_LeftUnchanged()
    {
        var text = "- Henry, we're here." + Environment.NewLine + "- We're here.";

        var result = OllamaOcr.CollapseRepetition(text);

        Assert.Equal(text, result);
    }

    [Fact]
    public void CollapseRepetition_RepeatedWordWithinDialog_NotCollapsed()
    {
        // A real (non-looping) subtitle that happens to repeat a short phrase a couple of times
        // must not be touched.
        var text = "Run, run, run!" + Environment.NewLine + "Don't stop now.";

        var result = OllamaOcr.CollapseRepetition(text);

        Assert.Equal(text, result);
    }
}
