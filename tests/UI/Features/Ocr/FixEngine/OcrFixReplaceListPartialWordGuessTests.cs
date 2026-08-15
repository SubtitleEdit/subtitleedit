using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace UITests.Features.Ocr.FixEngine;

// Regression tests for https://github.com/SubtitleEdit/subtitleedit/issues/13660
// Italic subtitles make binary image compare read 'l' as 'i' ("viel" -> "viei",
// "Colin" -> "Coiin"). The letter guesser substitutes <PartialWords> pairs into unknown
// words, so the German list needs an i->l pair - but "i" already maps to "t", and the
// old Dictionary-backed storage silently dropped every duplicate "from" key (the shipped
// deu list's ii->ü was dead for the same reason). PartialWords is now a list of pairs.
public class OcrFixReplaceListPartialWordGuessTests
{
    private static OcrFixReplaceList2 MakeReplaceList(string partialWordsXml)
    {
        var path = Path.Combine(Path.GetTempPath(), $"guesstest_{Guid.NewGuid():N}_OCRFixReplaceList.xml");
        File.WriteAllText(path, $"<ReplaceList><PartialWords>{partialWordsXml}</PartialWords></ReplaceList>");
        try
        {
            return new OcrFixReplaceList2(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DuplicateFromKeys_AllPairsGenerateGuesses()
    {
        var replaceList = MakeReplaceList(
            "<WordPart from=\"i\" to=\"t\" />" +
            "<WordPart from=\"i\" to=\"l\" />");

        var guesses = replaceList.CreateGuessesFromLetters("viei", "deu").ToList();

        Assert.Contains("viel", guesses); // italic OCR error, needs i->l
        Assert.Contains("viet", guesses); // the first mapping must keep working too
    }

    [Fact]
    public void ItalicLReadAsI_NameIsGuessed()
    {
        var replaceList = MakeReplaceList("<WordPart from=\"i\" to=\"l\" />");

        var guesses = replaceList.CreateGuessesFromLetters("Coiin", "deu").ToList();

        Assert.Contains("Colin", guesses);
    }

    [Fact]
    public void ItalicDoubleLReadAsIi_CumulativeGuessCoversBothLetters()
    {
        // The guesser's cumulative chain replaces occurrences left to right, so a single
        // i->l pair also repairs double-l words - no ii->ll entry needed (which would
        // conflict with the shipped ii->tt / ii->ü pairs).
        var replaceList = MakeReplaceList("<WordPart from=\"i\" to=\"l\" />");

        var guesses = replaceList.CreateGuessesFromLetters("woiien", "deu").ToList();

        Assert.Contains("wollen", guesses);
    }

    [Fact]
    public void EmptyPlaceholderSection_DoesNotShadowTheRealSection()
    {
        // fin/fra/hrb/hun/por/spa ship an empty <PartialWords /> placeholder BEFORE the real
        // section; reading only the first section silently dropped every entry (same bug class
        // as #13658, which fixed it for the regex list only).
        var path = Path.Combine(Path.GetTempPath(), $"guesstest_{Guid.NewGuid():N}_OCRFixReplaceList.xml");
        File.WriteAllText(path,
            "<ReplaceList><PartialWords /><OtherStuff /><PartialWords><WordPart from=\"i\" to=\"l\" /></PartialWords></ReplaceList>");
        OcrFixReplaceList2 replaceList;
        try
        {
            replaceList = new OcrFixReplaceList2(path);
        }
        finally
        {
            File.Delete(path);
        }

        var guesses = replaceList.CreateGuessesFromLetters("viei", "fra").ToList();

        Assert.Contains("viel", guesses);
    }

    [Fact]
    public void ExactDuplicatePairs_AreLoadedOnce()
    {
        var replaceList = MakeReplaceList(
            "<WordPart from=\"i\" to=\"l\" />" +
            "<WordPart from=\"i\" to=\"l\" />");

        var guesses = replaceList.CreateGuessesFromLetters("viei", "deu").ToList();

        Assert.Equal(guesses.Count, guesses.Distinct().Count());
    }
}
