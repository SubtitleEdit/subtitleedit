using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Text;

namespace UITests.Features.Tools.FixCommonErrors;

// "Fix common OCR errors" wrote its fix even when the user unticked it: it never asked AllowFix,
// which is what honors the check box during the apply pass (issue #12441).
public class FixCommonOcrErrorsTests : IDisposable
{
    public void Dispose() => FixCommonOcrErrors.OcrFixEngine = null;

    [Theory]
    [InlineData(true, "fixed text")]   // ticked -> the fix is written
    [InlineData(false, "broken text")] // unticked -> the paragraph is left alone
    public void Fix_OnlyAppliesWhenAllowFixSaysSo(bool allowFix, string expected)
    {
        FixCommonOcrErrors.OcrFixEngine = new FakeOcrFixEngine("fixed text");
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("broken text", 0, 1000));
        var callbacks = new FakeCallbacks(allowFix);

        new FixCommonOcrErrors().Fix(subtitle, callbacks);

        Assert.Equal(expected, subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void Fix_AllowFixIsAskedWithTheActionShownInTheList()
    {
        FixCommonOcrErrors.OcrFixEngine = new FakeOcrFixEngine("fixed text");
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("broken text", 0, 1000));
        var callbacks = new FakeCallbacks(allowFix: true);

        new FixCommonOcrErrors().Fix(subtitle, callbacks);

        // the action must match what AddFixToListView records, or the check box cannot be matched back
        Assert.Equal(FixCommonOcrErrors.Language.FixText, Assert.Single(callbacks.AllowFixActions));
    }

    private sealed class FakeOcrFixEngine : IOcrFixEngine
    {
        private readonly string _fixedText;

        public FakeOcrFixEngine(string fixedText) => _fixedText = fixedText;

        public OcrFixLineResult FixOcrErrors(int index, string text, bool doTryToGuessUnknownWords)
            => new(index, _fixedText);

        public void Initialize(Subtitle subtitle, string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary) { }
        public void Unload() { }
        public bool IsLoaded() => true;
        public List<string> GetSpellCheckSuggestions(string word) => new();
        public void ChangeAll(string from, string to) { }
        public void SkipAll(string word) { }
        public void AddName(string name) { }
        public List<string> ReloadNames() => new();
    }

    private sealed class FakeCallbacks : IFixCallbacks
    {
        private readonly bool _allowFix;

        public FakeCallbacks(bool allowFix) => _allowFix = allowFix;

        public List<string> AllowFixActions { get; } = new();

        public bool AllowFix(Paragraph p, string action)
        {
            AllowFixActions.Add(action);
            return _allowFix;
        }

        public void AddFixToListView(Paragraph p, string action, string before, string after) { }
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked) { }
        public void LogStatus(string sender, string message) { }
        public void LogStatus(string sender, string message, bool isImportant) { }
        public void UpdateFixStatus(int fixes, string message) { }
        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
        public void AddToTotalErrors(int count) { }
        public void AddToDeleteIndices(int index) { }
        public SubtitleFormat Format => new SubRip();
        public Encoding Encoding => Encoding.UTF8;
        public string Language => "en";
    }
}
