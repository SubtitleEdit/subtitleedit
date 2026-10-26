using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The regex rules of an OCR replace list can be listed, turned off and on, and the user's own
/// added, changed and deleted - all through the "_User" file, the shipped list is never written
/// (issue #16398: the Spanish «» quote rules could not be turned off from the UI).
/// </summary>
public class OcrFixRegexRulesTests : IDisposable
{
    private const string ShippedXml =
        "<ReplaceList><RegularExpressions>" +
        "<RegEx find=\"\\s\\x22\" replaceWith=\" «\" />" +
        "<RegEx find=\"\\x22\\s\" replaceWith=\"» \" />" +
        "</RegularExpressions></ReplaceList>";

    private readonly string _folder;
    private readonly string _fileName;

    public OcrFixRegexRulesTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-ocrregex-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _fileName = Path.Combine(_folder, "spa_OCRFixReplaceList.xml");
        File.WriteAllText(_fileName, ShippedXml);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    private static string Fix(OcrFixReplaceList2 list, string text)
    {
        return list.FixOcrErrorViaLineReplaceList(text, new Subtitle(), 0, new NoSpellChecker(), new List<string>(), true);
    }

    [Fact]
    public void GetRegexRules_ListsShippedRulesAsEnabled()
    {
        var rules = new OcrFixReplaceList2(_fileName).GetRegexRules();

        Assert.Equal(2, rules.Count);
        Assert.All(rules, r => Assert.True(r.IsEnabled));
        Assert.All(rules, r => Assert.False(r.IsUserRule));
        Assert.Equal("\\s\\x22", rules[0].Find);
    }

    [Fact]
    public void DisabledRule_IsNotAppliedAndSurvivesReload()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.Equal("dijo «hola» ya", Fix(list, "dijo \"hola\" ya"));

        Assert.True(list.SetRegexRuleEnabled("\\s\\x22", " «", false));
        Assert.Equal("dijo \"hola» ya", Fix(list, "dijo \"hola\" ya"));

        var reloaded = new OcrFixReplaceList2(_fileName);
        Assert.Equal("dijo \"hola» ya", Fix(reloaded, "dijo \"hola\" ya"));
        Assert.False(reloaded.GetRegexRules().Single(r => r.Find == "\\s\\x22").IsEnabled);
        Assert.Equal(ShippedXml, File.ReadAllText(_fileName));
    }

    [Fact]
    public void ReEnabledRule_IsAppliedAgain()
    {
        var list = new OcrFixReplaceList2(_fileName);
        Assert.True(list.SetRegexRuleEnabled("\\s\\x22", " «", false));
        Assert.True(list.SetRegexRuleEnabled("\\s\\x22", " «", true));

        var reloaded = new OcrFixReplaceList2(_fileName);
        Assert.All(reloaded.GetRegexRules(), r => Assert.True(r.IsEnabled));
        Assert.Equal("dijo «hola» ya", Fix(reloaded, "dijo \"hola\" ya"));
    }

    [Fact]
    public void SetRegexRulesEnabled_DisablesSeveralAtOnce()
    {
        var list = new OcrFixReplaceList2(_fileName);
        var all = list.GetRegexRules().Select(r => new KeyValuePair<string, string>(r.Find, r.ReplaceWith)).ToList();

        Assert.True(list.SetRegexRulesEnabled(all, false));

        var reloaded = new OcrFixReplaceList2(_fileName);
        Assert.All(reloaded.GetRegexRules(), r => Assert.False(r.IsEnabled));
        Assert.Equal("dijo \"hola\" ya", Fix(reloaded, "dijo \"hola\" ya"));
    }

    [Fact]
    public void UserRule_CanBeAddedDisabledUpdatedAndDeleted()
    {
        var list = new OcrFixReplaceList2(_fileName);

        Assert.True(list.AddRegexRule("colour", "color"));
        Assert.False(list.AddRegexRule("colour", "color"));
        Assert.False(list.AddRegexRule("(", "x"));
        Assert.Equal("color", Fix(list, "colour"));

        var rule = new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.IsUserRule);
        Assert.Equal("colour", rule.Find);

        Assert.True(list.SetRegexRuleEnabled("colour", "color", false));
        Assert.Equal("colour", Fix(new OcrFixReplaceList2(_fileName), "colour"));

        Assert.True(list.UpdateUserRegexRule("colour", "flavour", "flavor"));
        var updated = new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.IsUserRule);
        Assert.Equal("flavour", updated.Find);
        Assert.False(updated.IsEnabled);

        Assert.True(list.RemoveUserRegexRule("flavour"));
        Assert.DoesNotContain(new OcrFixReplaceList2(_fileName).GetRegexRules(), r => r.IsUserRule);
    }

    [Fact]
    public void ShippedRule_CannotBeDeleted()
    {
        var list = new OcrFixReplaceList2(_fileName);

        Assert.False(list.RemoveUserRegexRule("\\s\\x22"));
        Assert.False(list.UpdateUserRegexRule("\\s\\x22", "\\s'", "«"));
    }

    private sealed class NoSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => true;

        public List<string> GetSuggestions(string word) => new();
    }
}
