using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt (sweep 14): values that were normalized on one code
/// path but not the other, and a preamble guard that the reasoning-tag branch bypassed.
/// </summary>
public class BugHunt14Tests
{
    [Fact]
    public void GetFiveLetterLanguageName_HyphenatedDictionary_ResolvesInsteadOfFallingBackToEnglish()
    {
        // The shipped dictionary list contains hyphenated names. Only the instance overload
        // replaced '-' with '_', so SpellChecker's call returned null and silently loaded the
        // en_US word lists - while added words were written to fa_IR_*.xml and never read back.
        Assert.Equal("fa_IR", SpellCheckDictionaryDisplay.GetFiveLetterLanguageName("fa-IR"));
        Assert.Equal("fa_IR", SpellCheckDictionaryDisplay.GetFiveLetterLanguageName("fa_IR"));
    }

    [Fact]
    public void LaraTranslate_LanguageList_ExposesATwoLetterIsoName()
    {
        // The full locale ("ja-JP") used to be stored as TwoLetterIsoLanguageName, so the exact
        // two-letter comparisons downstream (IsCjkLanguage, merge/split gating) matched nothing.
        var japanese = LaraTranslate.ListLanguages().First(p => p.Code == "ja-JP");

        Assert.Equal("ja", japanese.TwoLetterIsoLanguageName);
        Assert.Equal("ja-JP", japanese.Code);
    }

    [Fact]
    public void LaraTranslate_EveryLanguage_HasALanguageTagNotACountryTag()
    {
        Assert.All(LaraTranslate.ListLanguages(), p =>
            Assert.Equal(p.Code.Split('-')[0].ToLowerInvariant(), p.TwoLetterIsoLanguageName));
    }

    [Fact]
    public void RemovePreamble_ColonInSource_IsKeptEvenWithAThinkBlock()
    {
        // The colon guard was "&&"-ed with "no <think> present", so a reasoning model's answer
        // skipped it and the preamble regex ate the translation up to its first colon.
        const string original = "Anmerkung: Das ist wichtig.";
        const string input = "<think>reasoning here</think>Here is the note: this is important.";

        var result = ChatGptTranslate.RemovePreamble(original, input);

        Assert.Equal("Here is the note: this is important.", result);
    }

    [Fact]
    public void RemovePreamble_NoColonInSource_StillStripsThePreamble()
    {
        var result = ChatGptTranslate.RemovePreamble("Das ist wichtig.", "Here is the translation: this is important.");

        Assert.Equal("this is important.", result);
    }
}
