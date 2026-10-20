using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// LightOnOCR-3 ends every line but the last with a markdown hard break (two trailing spaces);
/// the llama.cpp OCR engines used to trim only the whole result, so the spaces stayed in the
/// subtitle text.
/// </summary>
public class OcrHelperTrimLinesTests
{
    [Fact]
    public void TrimsMarkdownHardBreakAtLineEnd()
    {
        var input = "I told you not to come back here.  " + Environment.NewLine + "You never listen to me!";
        var expected = "I told you not to come back here." + Environment.NewLine + "You never listen to me!";
        Assert.Equal(expected, OcrHelper.TrimAiOcrLines(input));
    }

    [Fact]
    public void NormalizesBareLineFeeds()
    {
        Assert.Equal("a" + Environment.NewLine + "b", OcrHelper.TrimAiOcrLines("  a  \n b "));
    }

    [Fact]
    public void LeavesSingleLineUntouched()
    {
        Assert.Equal("♪ And I will always love you ♪", OcrHelper.TrimAiOcrLines("♪ And I will always love you ♪"));
    }

    [Fact]
    public void EmptyAndNullReturnEmpty()
    {
        Assert.Equal(string.Empty, OcrHelper.TrimAiOcrLines(null));
        Assert.Equal(string.Empty, OcrHelper.TrimAiOcrLines(string.Empty));
    }
}
