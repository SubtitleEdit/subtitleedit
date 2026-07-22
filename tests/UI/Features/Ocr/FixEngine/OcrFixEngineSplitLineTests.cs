using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace UITests.Features.Ocr.FixEngine;

public class OcrFixEngineSplitLineTests
{
    // Regression test for https://github.com/SubtitleEdit/subtitleedit/issues/11479
    // A stray '<' (no matching '>') used to make SplitLine spin forever, adding empty
    // words until the process ran out of memory (OutOfMemoryException).
    [Theory]
    [InlineData("a < b")]
    [InlineData("<")]
    [InlineData("3 < 5 and 7 > 2")]
    [InlineData("<unfinished tag")]
    [InlineData("end with <")]
    public void SplitLine_StrayLessThan_DoesNotHangAndKeepsAllText(string line)
    {
        var result = OcrFixEngine.SplitLine(line, 0);

        // No characters lost or duplicated: concatenated parts must equal the input.
        Assert.Equal(line, string.Concat(result.Words.Select(w => w.Word)));
        // No empty parts (the infinite loop produced zero-length words).
        Assert.DoesNotContain(result.Words, w => w.Word.Length == 0);
    }

    [Fact]
    public void SplitLine_ValidTag_IsParsedAsTag()
    {
        var result = OcrFixEngine.SplitLine("<i>Hello</i>", 0);

        var tags = result.Words.Where(w => w.LinePartType == OcrFixLinePartType.Tag).ToList();
        Assert.Equal(2, tags.Count);
        Assert.Equal("<i>", tags[0].Word);
        Assert.Equal("</i>", tags[1].Word);
        Assert.Equal("<i>Hello</i>", string.Concat(result.Words.Select(w => w.Word)));
    }
}
