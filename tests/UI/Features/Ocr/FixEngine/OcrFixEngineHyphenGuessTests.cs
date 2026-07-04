using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UITests.Features.Ocr.FixEngine;

// Regression test for https://github.com/SubtitleEdit/subtitleedit/issues/12156
// "Fix common OCR errors" turned Dutch hyphenated words into a split with a stray space next to
// the hyphen, e.g. "vaudeville-veteraan" -> "vaudeville- veteraan". A hyphenated compound whose
// parts are each correctly spelled must be left untouched and kept out of the guess/split path.
public class OcrFixEngineHyphenGuessTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixEngineHyphenGuessTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        // Empty temp folder so no nld_OCRFixReplaceList.xml / names interfere - the fake spell
        // checker is the only source of truth about which words are correct.
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixHyphenTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    [Theory]
    [InlineData("een vaudeville-veteraan", "vaudeville", "veteraan")]
    [InlineData("het Bailey-gebied", "Bailey", "gebied")]
    public void FixOcrErrors_HyphenatedWordOfCorrectParts_IsLeftUnchangedAndNotGuessed(string line, string partA, string partB)
    {
        var engine = new OcrFixEngine(new FakeSpellChecker("een", "het", partA, partB));
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(line, 0, 3000));
        ((IOcrFixEngine)engine).Initialize(subtitle, "nld", new SpellCheckDictionaryDisplay { DictionaryFileName = string.Empty });

        var result = engine.FixOcrErrors(0, line, doTryToGuessUnknownWords: true);

        Assert.Equal(line, result.GetText());
        Assert.DoesNotContain(result.Words, w => w.GuessUsed);

        // The gate must also mark the compound as correctly spelled (not a flagged unknown word),
        // so it is not underlined / offered for "fixing" in the OCR window.
        var hyphenWord = result.Words.Single(w => w.Word.Contains('-'));
        Assert.True(hyphenWord.IsSpellCheckedOk);
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
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
