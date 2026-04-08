using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

namespace UITests.Features.Ocr;

public class UnknownWordItemTests
{
    [Fact]
    public void DisplayWord_ShouldPreferFixedWordOverRawOcrToken()
    {
        var unknownWord = MakeUnknownWordItem("recuérdaIo", "recuérdalo");

        Assert.Equal("recuérdalo", unknownWord.DisplayWord);
    }

    [Fact]
    public void ResolveSubmittedWord_ShouldPreferEditedPopupWord()
    {
        var unknownWord = MakeUnknownWordItem("recuérdaIo", "recuérdalo");

        var result = unknownWord.ResolveSubmittedWord("  recuérdame  ");

        Assert.Equal("recuérdame", result);
    }

    [Fact]
    public void ResolveSubmittedWord_ShouldFallbackToDisplayWordWhenPopupWordIsEmpty()
    {
        var unknownWord = MakeUnknownWordItem("recuérdaIo", "recuérdalo");

        var result = unknownWord.ResolveSubmittedWord("   ");

        Assert.Equal("recuérdalo", result);
    }

    private static UnknownWordItem MakeUnknownWordItem(string rawWord, string fixedWord)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(new TimeCode(0), new TimeCode(1000), fixedWord));

        var item = new OcrSubtitleDummy(subtitle).MakeOcrSubtitleItems()[0];
        var lineResult = new OcrFixLineResult(0, fixedWord);
        var word = new OcrFixLinePartResult
        {
            LinePartType = OcrFixLinePartType.Word,
            Word = rawWord,
            FixedWord = fixedWord,
            WordIndex = 0,
        };

        return new UnknownWordItem(item, lineResult, word);
    }
}
