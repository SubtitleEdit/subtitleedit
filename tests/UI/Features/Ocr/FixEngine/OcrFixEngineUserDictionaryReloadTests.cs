using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Repro for https://github.com/SubtitleEdit/subtitleedit/issues/12824
// "Add to user dictionary" in the OCR spell check must make the word known so the prompt advances.
public class OcrFixEngineUserDictionaryReloadTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixEngineUserDictionaryReloadTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrUserDictTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    [Theory]
    [InlineData("en_US.dic")]   // standard five-letter dictionary
    [InlineData("es_ANY.dic")]  // GetFiveLetterLanguageName() maps this to es_ES
    public void AddToUserDictionary_ThenReload_MakesWordKnown(string dictionaryFileName)
    {
        const string line = "Hello Kryten";
        var engine = new OcrFixEngine(new FakeSpellChecker("Hello"));
        var dictionary = new SpellCheckDictionaryDisplay { DictionaryFileName = dictionaryFileName };
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(line, 0, 3000));
        ((IOcrFixEngine)engine).Initialize(subtitle, "eng", dictionary);

        // Before: "Kryten" is unknown.
        var before = engine.FixOcrErrors(0, line, doTryToGuessUnknownWords: false);
        Assert.False(before.Words.Single(w => w.Word == "Kryten").IsSpellCheckedOk);

        // Simulate the OCR view model's "Add to user dictionary" handler.
        UserWordsHelper.AddToUserDictionary("Kryten", dictionary.GetFiveLetterLanguageName() ?? "en_US");
        engine.ReloadNames();

        // After: "Kryten" should be recognized, so the prompt advances instead of re-opening.
        var after = engine.FixOcrErrors(0, line, doTryToGuessUnknownWords: false);
        Assert.True(after.Words.Single(w => w.Word == "Kryten").IsSpellCheckedOk);
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
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
