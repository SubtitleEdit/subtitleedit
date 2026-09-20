using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;

namespace UITests.Features.Ocr.FixEngine;

// Straightening typographic apostrophes is one of the hardcoded OCR rules: the per-word rule in
// OcrFixReplaceList2 has always honored "use hardcoded rules", and the line-level rule must too,
// or turning the setting off still rewrites a subtitle's typography.
public class OcrFixApostropheHardcodedRulesTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixApostropheHardcodedRulesTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixApostropheHardcodedRules_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"), "<ReplaceList></ReplaceList>");
    }

    [Fact]
    public void FixOcrErrors_StraightensApostrophes_OnlyWithHardcodedRules()
    {
        var previous = Configuration.Settings.Tools.OcrFixUseHardcodedRules;
        try
        {
            var engine = MakeEngine();

            Configuration.Settings.Tools.OcrFixUseHardcodedRules = false;
            Assert.Equal("‘cause I promised", engine.FixOcrErrors(0, "‘cause I promised", doTryToGuessUnknownWords: false).GetText());

            Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
            Assert.Equal("'cause I promised", engine.FixOcrErrors(0, "‘cause I promised", doTryToGuessUnknownWords: false).GetText());
        }
        finally
        {
            Configuration.Settings.Tools.OcrFixUseHardcodedRules = previous;
        }
    }

    private static IOcrFixEngine MakeEngine()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(string.Empty, 0, 1000));
        IOcrFixEngine engine = new OcrFixEngine(new AcceptAllSpellChecker());
        engine.Initialize(subtitle, "eng", new SpellCheckDictionaryDisplay { DictionaryFileName = "en_US" });
        return engine;
    }

    private sealed class AcceptAllSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => true;
        public List<string> GetSuggestions(string word) => new();
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try { Directory.Delete(_tempDictionariesFolder, true); } catch { /* best effort */ }
    }
}
