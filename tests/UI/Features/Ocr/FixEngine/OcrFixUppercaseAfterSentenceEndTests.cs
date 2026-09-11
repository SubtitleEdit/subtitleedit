using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;

namespace UITests.Features.Ocr.FixEngine;

// Discussion #12929: OCR dropped the capital of a line that follows a finished sentence
// ("you're out of luck." after "Yes."). Only high-confidence cases are capitalized: the previous
// line ends directly in . ! or ? (no trailing quote, no "...", no abbreviation), the previous line
// has been OCR'd, and the capitalized first word is a dictionary word.
public class OcrFixUppercaseAfterSentenceEndTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixUppercaseAfterSentenceEndTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixUppercaseAfterSentenceEnd_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"), "<ReplaceList></ReplaceList>");
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "en_abbreviations.xml"), "<Abbreviations><Item>Mr.</Item></Abbreviations>");
    }

    [Theory]
    [InlineData("Yes.", "you're out of luck.", "You're out of luck.")]
    [InlineData("Really?", "you're out of luck.", "You're out of luck.")]
    [InlineData("Go!", "<i>you're out of luck.</i>", "<i>You're out of luck.</i>")]
    [InlineData("Go!", "- you're out of luck.", "- You're out of luck.")]
    [InlineData("Yes.", "l said no.", "I said no.")]
    [InlineData("Yes.", "l'm here.", "I'm here.")]
    [InlineData("\"I will avenge you, Mother!\"", "you're out of luck.", "you're out of luck.")] // quote: may be one sentence
    [InlineData("So if you were hoping for any,", "you're out of luck.", "you're out of luck.")]
    [InlineData("Wait...", "you're out of luck.", "you're out of luck.")]
    [InlineData("I met Mr.", "you're out of luck.", "you're out of luck.")]
    [InlineData("", "you're out of luck.", "you're out of luck.")] // previous line not OCR'd yet
    [InlineData("Yes.", "lo0k out.", "lo0k out.")] // not a dictionary word once capitalized
    [InlineData("Yes.", "[music]", "[music]")]
    [InlineData("Yes.", "...you're out of luck.", "...you're out of luck.")]
    public void FixOcrErrors_AfterSentenceEnd(string previousLine, string line, string expected)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(previousLine, 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph(string.Empty, 1000, 2000));
        IOcrFixEngine engine = new OcrFixEngine(new FakeEnglishSpellChecker());
        engine.Initialize(subtitle, "eng", new SpellCheckDictionaryDisplay { DictionaryFileName = "en_US" });

        var result = engine.FixOcrErrors(1, line, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_FirstLine_IsLeftAlone()
    {
        // No previous line means no evidence that a sentence ended - an OCR run may start anywhere.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(string.Empty, 0, 1000));
        IOcrFixEngine engine = new OcrFixEngine(new FakeEnglishSpellChecker());
        engine.Initialize(subtitle, "eng", new SpellCheckDictionaryDisplay { DictionaryFileName = "en_US" });

        var result = engine.FixOcrErrors(0, "you're out of luck.", doTryToGuessUnknownWords: false);

        Assert.Equal("you're out of luck.", result.GetText());
    }

    private sealed class FakeEnglishSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "you're", "You're", "out", "of", "luck", "I", "I'm", "said", "no", "here", "music",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => Words.Contains(word);
        public List<string> GetSuggestions(string word) => new();
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try { Directory.Delete(_tempDictionariesFolder, true); } catch { /* best effort */ }
    }
}
