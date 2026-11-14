using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The spell-checked regex rules ("RegularExpressionsIfSpelledCorrectly") can be listed, turned
/// off and on, and the user's own added, changed and deleted from the Regex rules window - all
/// through the "_User" file (#16377).
/// </summary>
public class OcrFixSpellCheckRegexRulesTests : IDisposable
{
    private const string ShippedXml =
        "<ReplaceList>" +
        "<RegularExpressions><RegEx find=\"\\s\\x22\" replaceWith=\" «\" /></RegularExpressions>" +
        "<RegularExpressionsIfSpelledCorrectly>" +
        "<RegEx find=\"\\bde([A-Z][a-z]+)\\b\" spellCheck=\"$1\" replaceWith=\"de $1\" />" +
        "</RegularExpressionsIfSpelledCorrectly>" +
        "</ReplaceList>";

    private const string Exclamation = "\\b[iI]([A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+)";

    private readonly string _folder;
    private readonly string _fileName;

    public OcrFixSpellCheckRegexRulesTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-ocrregex-sc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _fileName = Path.Combine(_folder, "spa_OCRFixReplaceList.xml");
        File.WriteAllText(_fileName, ShippedXml);
    }

    [Fact]
    public void GetRegexRules_ListsShippedSpellCheckedRules()
    {
        var rules = new OcrFixReplaceList2(_fileName).GetRegexRules();

        Assert.Equal(2, rules.Count);
        var gated = rules.Single(r => r.IsSpellChecked);
        Assert.Equal("$1", gated.SpellCheck);
        Assert.Equal("de $1", gated.ReplaceWith);
        Assert.False(gated.IsUserRule);
        Assert.True(gated.IsEnabled);
    }

    [Fact]
    public void AddSpellCheckedRule_IsSavedAndApplied()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.AddRegexRule(Exclamation, "¡$1", "$1"));
        Assert.False(list.AddRegexRule(Exclamation, "¡$1", "$1")); // already listed

        // Live without reloading, and from the file.
        Assert.Equal("- ¡Fuera! iTheo!", Fix(list, "- iFuera! iTheo!"));
        var reloaded = new OcrFixReplaceList2(_fileName);
        Assert.Equal("- ¡Fuera! iTheo!", Fix(reloaded, "- iFuera! iTheo!"));
        var rule = reloaded.GetRegexRules().Single(r => r.Find == Exclamation);
        Assert.True(rule.IsUserRule);
        Assert.Equal("$1", rule.SpellCheck);
    }

    [Fact]
    public void SamePatternMayBeBothPlainAndSpellChecked()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.AddRegexRule(Exclamation, "¡$1", "$1"));
        Assert.True(list.AddRegexRule(Exclamation, "¡$1"));

        Assert.Equal(2, new OcrFixReplaceList2(_fileName).GetRegexRules().Count(r => r.Find == Exclamation));
    }

    [Fact]
    public void DisableShippedSpellCheckedRule_StopsItAndCanBeUndone()
    {
        var list = new OcrFixReplaceList2(_fileName);
        var gated = list.GetRegexRules().Single(r => r.IsSpellChecked);

        Assert.True(list.SetRegexRulesEnabled(new[] { gated }, false));
        Assert.Equal("Vengo deMadrid", Fix(list, "Vengo deMadrid"));
        var reloaded = new OcrFixReplaceList2(_fileName);
        Assert.Equal("Vengo deMadrid", Fix(reloaded, "Vengo deMadrid"));
        Assert.False(reloaded.GetRegexRules().Single(r => r.IsSpellChecked).IsEnabled);
        Assert.True(reloaded.GetRegexRules().Single(r => !r.IsSpellChecked).IsEnabled); // plain twin untouched

        Assert.True(list.SetRegexRulesEnabled(new[] { gated }, true));
        Assert.Equal("Vengo de Madrid", Fix(new OcrFixReplaceList2(_fileName), "Vengo deMadrid"));
    }

    [Fact]
    public void UpdateRule_CanChangeSpellCheckWordAndKind()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.AddRegexRule(Exclamation, "¡$1", "$1"));

        // Plain now: "Theo" is changed too.
        Assert.True(list.UpdateUserRegexRule(Exclamation, true, Exclamation, "¡$1", string.Empty));
        Assert.Equal("¡Theo!", Fix(list, "iTheo!"));
        var rule = new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.Find == Exclamation);
        Assert.False(rule.IsSpellChecked);

        // And back to spell-checked.
        Assert.True(list.UpdateUserRegexRule(Exclamation, false, Exclamation, "¡$1", "$1"));
        Assert.Equal("iTheo!", Fix(list, "iTheo!"));
        Assert.Equal("iTheo!", Fix(new OcrFixReplaceList2(_fileName), "iTheo!"));
    }

    [Fact]
    public void UpdateDisabledRule_KeepsItDisabledWhenKindChanges()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.AddRegexRule(Exclamation, "¡$1", "$1"));
        var rule = list.GetRegexRules().Single(r => r.Find == Exclamation);
        Assert.True(list.SetRegexRulesEnabled(new[] { rule }, false));

        Assert.True(list.UpdateUserRegexRule(Exclamation, true, Exclamation, "¡$1", string.Empty));

        var updated = new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.Find == Exclamation);
        Assert.False(updated.IsEnabled);
        Assert.Equal("iFuera!", Fix(list, "iFuera!"));
    }

    [Fact]
    public void RemoveSpellCheckedRule_OnlyUserRules()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.AddRegexRule(Exclamation, "¡$1", "$1"));

        Assert.False(list.RemoveUserRegexRule("\\bde([A-Z][a-z]+)\\b", isSpellChecked: true)); // shipped
        Assert.False(list.RemoveUserRegexRule(Exclamation)); // not a plain rule
        Assert.True(list.RemoveUserRegexRule(Exclamation, isSpellChecked: true));

        Assert.Equal("iFuera!", Fix(list, "iFuera!"));
        Assert.DoesNotContain(new OcrFixReplaceList2(_fileName).GetRegexRules(), r => r.IsUserRule);
    }

    private static string Fix(OcrFixReplaceList2 list, string text)
    {
        var checker = new FakeSpanishSpellChecker();
        return list.FixOcrErrorViaLineReplaceList(text, new Subtitle(), 0, checker, new List<string>(), spelledOK: false, checker.IsWordCorrect);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private sealed class FakeSpanishSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.OrdinalIgnoreCase) { "fuera", "vengo", "de", "madrid" };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => string.IsNullOrWhiteSpace(word) || Words.Contains(word);

        public List<string> GetSuggestions(string word) => new();
    }
}
