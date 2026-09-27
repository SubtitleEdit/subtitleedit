using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// SE4's "Fix common OCR errors" always split run-together words via the word split list
// ("soproudofyou" -> "so proud of you"). SE5 put all guessing behind "try to guess unknown
// words" (off by default, #12441), so it fixed a fraction of what SE4 did. The split list now
// runs regardless; the heuristic splitter and letter-substitution guesses still follow the
// setting. Also: an exact whole-word entry wins over the always-applied "IVl" -> "M" word part,
// which turned the medical "IVline" into "Mine".
public class OcrFixWordSplitWithoutGuessTests : IDisposable
{
    private readonly Func<string> _originalDictionariesFolder;
    private readonly Func<bool> _originalUseWordSplitList;
    private readonly string _tempDictionariesFolder;

    public OcrFixWordSplitWithoutGuessTests()
    {
        _originalDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalUseWordSplitList = SpellCheckConfig.UseWordSplitList;

        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixWordSplit_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        File.WriteAllLines(Path.Combine(_tempDictionariesFolder, "eng_WordSplitList.txt"),
            new[] { "so", "proud", "of", "you", "need", "Daniel", "ex", "per", "ha", "If", "a", "what", "I" });
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList>" +
            "<WholeWords><Word from=\"IVline\" to=\"IV line\" /></WholeWords>" +
            "<PartialWordsAlways><WordPart from=\"IVl\" to=\"M\" /></PartialWordsAlways>" +
            "<PartialWords><WordPart from=\"c\" to=\"o\" /></PartialWords>" +
            "</ReplaceList>");

        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        SpellCheckConfig.UseWordSplitList = () => true;
    }

    private static string Fix(string line, bool doTryToGuessUnknownWords, params string[] correctWords)
        => Fix(line, doTryToGuessUnknownWords, false, correctWords);

    private static string Fix(string line, bool doTryToGuessUnknownWords, bool fillUnknownCharacters, params string[] correctWords)
    {
        var engine = new OcrFixEngine(new FakeSpellChecker(correctWords)) { FillUnknownCharacters = fillUnknownCharacters };
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(line, 0, 3000));
        ((IOcrFixEngine)engine).Initialize(subtitle, "eng", new SpellCheckDictionaryDisplay { DictionaryFileName = string.Empty });
        return engine.FixOcrErrors(0, line, doTryToGuessUnknownWords).GetText();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RunTogetherWords_AreSplitViaWordSplitList_WithOrWithoutGuessing(bool guess)
    {
        var result = Fix("Daniel, I'm soproudofyou.", guess, "Daniel", "I'm", "so", "proud", "of", "you");

        Assert.Equal("Daniel, I'm so proud of you.", result);
    }

    [Fact]
    public void SplitWithAnUnknownPart_IsNotUsed()
    {
        // "proud" is in the split list but not in the dictionary, so the split is not confirmed.
        var result = Fix("I'm soproudofyou.", false, "I'm", "so", "of", "you");

        Assert.Equal("I'm soproudofyou.", result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WordGluedToUnknownGlyph_IsNotSplit(bool guess)
    {
        // nOCR writes "*" for a glyph it could not match; "exper" is a fragment of "experiment".
        var result = Fix("It was an exper*ment.", guess, "It", "was", "an", "ex", "per");

        Assert.Equal("It was an exper*ment.", result);
    }

    [Fact]
    public void SplitWithCapitalizedNonNamePart_IsNotUsed()
    {
        // OCR read the l in "half" as I - "of ha If a" is not a fix.
        var result = Fix("Just shy ofhaIfa mile.", false, "Just", "shy", "of", "ha", "If", "a", "mile");

        Assert.Equal("Just shy ofhaIfa mile.", result);
    }

    [Fact]
    public void SplitWithPronounI_IsUsed()
    {
        var result = Fix("You know whatI mean.", false, "You", "know", "what", "I", "mean");

        Assert.Equal("You know what I mean.", result);
    }

    [Fact]
    public void LetterSubstitutionGuesses_StillFollowTheSetting()
    {
        Assert.Equal("It is hcuse.", Fix("It is hcuse.", false, "It", "is", "house"));
        Assert.Equal("It is house.", Fix("It is hcuse.", true, "It", "is", "house"));
    }

    [Fact]
    public void WholeWordEntry_WinsOverAlwaysAppliedWordPart()
    {
        var result = Fix("Just to calm it, the IVline, now.", false, "Just", "to", "calm", "it", "the", "now", "Mine", "IV", "line");

        Assert.Equal("Just to calm it, the IV line, now.", result);
    }

    [Fact]
    public void AlwaysAppliedWordPart_StillFixesOtherWords()
    {
        var result = Fix("You IVlust go.", false, "You", "Must", "go");

        Assert.Equal("You Must go.", result);
    }

    [Theory]
    [InlineData("It was an exper*ment.", "It was an experiment.")]
    [InlineData("Hey, assho*e, put it down.", "Hey, asshole, put it down.")]
    [InlineData("I think they*ve gone.", "I think they've gone.")]
    [InlineData("A vast *andscape.", "A vast landscape.")]
    public void UnknownCharacterInWord_IsFilled_WhenExactlyOneWordFits(string line, string expected)
    {
        var result = Fix(line, false, true, "It", "was", "an", "experiment", "Hey", "asshole", "put", "it", "down", "I", "think", "they've", "gone", "A", "vast", "landscape", "Landscape");

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("A b*nd.")]        // band / bond - ambiguous
    [InlineData("Is it true*")]    // trailing: often a missed full stop
    [InlineData("A c0m*ng day.")]  // the 0 is misread too
    [InlineData("A k*n.")]         // too few known letters
    public void UnknownCharacterInWord_IsLeftAlone_WhenNotCertain(string line)
    {
        var result = Fix(line, false, true, "A", "band", "bond", "Is", "it", "true", "trued", "coming", "day", "kin");

        Assert.Equal(line, result);
    }

    [Fact]
    public void UnknownCharacterFill_IsOffByDefault()
    {
        // Outside nOCR/binary image compare output a "*" in a word is deliberate censoring.
        var result = Fix("Oh, assho*e.", false, "Oh", "asshole");

        Assert.Equal("Oh, assho*e.", result);
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalDictionariesFolder;
        SpellCheckConfig.UseWordSplitList = _originalUseWordSplitList;
        try
        {
            Directory.Delete(_tempDictionariesFolder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        private readonly HashSet<string> _correct;

        public FakeSpellChecker(params string[] correctWords)
            => _correct = new HashSet<string>(correctWords, StringComparer.Ordinal);

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => _correct.Contains(word);
        public List<string> GetSuggestions(string word) => new();
    }
}
