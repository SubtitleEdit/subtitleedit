using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// #12126: "Fix common OCR errors" applied no corrections on a fresh config because it required a
// matching Hunspell dictionary (.dic) - and only en_US.dic ships with SE. The OCR replace list is
// spell-check independent, so its replacements must still be applied when no dictionary is installed
// (this worked in SE 4). Drives the real FixCommonOcrErrors.Fix path with only the bundled-style
// spa_OCRFixReplaceList.xml present and no Spanish dictionary - the exact reporter scenario.
public class OcrFixNoDictionaryReplaceListTests : IDisposable
{
    private readonly string _originalSeDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly IOcrFixEngine? _originalOcrFixEngine;
    private readonly string _tempDictionariesFolder;

    public OcrFixNoDictionaryReplaceListTests()
    {
        _originalSeDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalOcrFixEngine = FixCommonOcrErrors.OcrFixEngine;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixNoDicTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // Spanish OCR replace list with the reporter's whole-word accent fixes. No spa .dic/.aff -
        // so there is no Hunspell dictionary for the language.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "spa_OCRFixReplaceList.xml"),
            "<ReplaceList><WholeWords>" +
            "<Word from=\"aca\" to=\"acá\" />" +
            "<Word from=\"dia\" to=\"día\" />" +
            "</WholeWords></ReplaceList>");
    }

    [Fact]
    public void Fix_WithoutHunspellDictionary_StillAppliesOcrReplaceList()
    {
        FixCommonOcrErrors.OcrFixEngine = new OcrFixEngine(new SpellChecker());

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hola, aca estoy. Un dia volvere.", 0, 3000));

        var callbacks = new FakeFixCallbacks("es");
        new FixCommonOcrErrors().Fix(subtitle, callbacks);

        Assert.Equal("Hola, acá estoy. Un día volvere.", subtitle.Paragraphs[0].Text);
        Assert.Equal(1, callbacks.FixCount);
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalSeDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        FixCommonOcrErrors.OcrFixEngine = _originalOcrFixEngine;
        try
        {
            Directory.Delete(_tempDictionariesFolder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private sealed class FakeFixCallbacks : IFixCallbacks
    {
        public FakeFixCallbacks(string language) => Language = language;

        public int FixCount { get; private set; }

        public bool AllowFix(Paragraph p, string action) => true;
        public void AddFixToListView(Paragraph p, string action, string before, string after) => FixCount++;
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked) => FixCount++;
        public void LogStatus(string sender, string message) { }
        public void LogStatus(string sender, string message, bool isImportant) { }
        public void UpdateFixStatus(int fixes, string message) { }
        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
        public void AddToTotalErrors(int count) { }
        public void AddToDeleteIndices(int index) { }
        public SubtitleFormat Format => new SubRip();
        public Encoding Encoding => Encoding.UTF8;
        public string Language { get; }
    }
}
