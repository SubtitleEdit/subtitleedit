using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Xunit;

namespace UITests.Features.Translate;

/// <summary>
/// The auto-translate target combo always opened on Abkhaz for ChatGPT (#14903): the LLM engines
/// keep the English name in Code, the default only matched on Code, and so it fell back to the
/// alphabetically first entry. The default is now the last target, then the reverse of the
/// last run, then the UI language, then the OS language, then English - matched however the engine spells the language.
/// </summary>
public class AutoTranslateDefaultTargetLanguageTests
{
    private static readonly List<TranslationPair> ChatGptLanguages = ChatGptTranslate.ListLanguages();

    private static List<TranslationPair> GoogleLike() => new()
    {
        new TranslationPair("Abkhaz", "ab"),
        new TranslationPair("Chinese (Simplified)", "zh-CN"),
        new TranslationPair("Chinese (Traditional)", "zh-TW"),
        new TranslationPair("English", "en"),
        new TranslationPair("German", "de"),
        new TranslationPair("Japanese", "ja"),
    };

    [Fact]
    public void LastTargetSavedAsNameIsRestored()
    {
        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(ChatGptLanguages, null, "Vietnamese", "en-US");

        Assert.Equal("Vietnamese", target?.Code);
    }

    /// <summary>A translation run saves the ISO code, not the ChatGPT code ("Vietnamese").</summary>
    [Fact]
    public void LastTargetSavedAsIsoCodeIsRestoredForANameCodedEngine()
    {
        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(ChatGptLanguages, null, "vi", "en-US");

        Assert.Equal("Vietnamese", target?.Code);
    }

    [Fact]
    public void NoLastTargetUsesTheUiLanguage()
    {
        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(ChatGptLanguages, null, string.Empty, "de-DE");

        Assert.Equal("German", target?.Code);
    }

    [Fact]
    public void UiCultureMatchesARegionalCodeExactly()
    {
        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(GoogleLike(), null, string.Empty, "zh-TW");

        Assert.Equal("zh-TW", target?.Code);
    }

    [Fact]
    public void UnknownLastTargetAndUiLanguageFallBackToEnglishNotTheFirstEntry()
    {
        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(ChatGptLanguages, null, "Klingon", "tlh-QO");

        Assert.Equal("English", target?.Code);
    }

    [Fact]
    public void LastTargetEqualToTheSourceIsSkipped()
    {
        var source = ChatGptLanguages.First(p => p.Code == "Japanese");

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(ChatGptLanguages, source, "Japanese", "fr-FR");

        Assert.Equal("French", target?.Code);
    }

    [Fact]
    public void EnglishSourceWithEnglishUiPicksGerman()
    {
        var source = new TranslationPair("English", "en");

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(GoogleLike(), source, string.Empty, "en-US");

        Assert.Equal("de", target?.Code);
    }

    [Fact]
    public void EnglishSourceWithEnglishUiPicksTheOsLanguage()
    {
        var languages = GoogleLike();
        languages.Add(new TranslationPair("Danish", "da"));
        var source = new TranslationPair("English", "en");

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(languages, source, string.Empty, "en-US", null, "da-DK");

        Assert.Equal("da", target?.Code);
    }

    [Fact]
    public void UiLanguageWinsOverTheOsLanguage()
    {
        var languages = GoogleLike();
        languages.Add(new TranslationPair("Danish", "da"));

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(languages, null, string.Empty, "en-US", null, "da-DK");

        Assert.Equal("en", target?.Code);
    }

    /// <summary>Last run was Danish to English, now the subtitle is English: suggest the reverse.</summary>
    [Fact]
    public void LastTargetEqualToTheSourcePicksTheLastSource()
    {
        var languages = GoogleLike();
        languages.Add(new TranslationPair("Danish", "da"));
        var source = new TranslationPair("English", "en");

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(languages, source, "en", "en-US", "da", "en-US");

        Assert.Equal("da", target?.Code);
    }

    [Fact]
    public void LastSourceIsNotUsedWhenTheLastTargetIsStillValid()
    {
        var source = new TranslationPair("English", "en");

        var target = AutoTranslateViewModel.FindDefaultTargetLanguage(GoogleLike(), source, "ja", "en-US", "de", "en-US");

        Assert.Equal("ja", target?.Code);
    }

    [Fact]
    public void NoLanguagesGivesNull()
    {
        Assert.Null(AutoTranslateViewModel.FindDefaultTargetLanguage(new List<TranslationPair>(), null, "en", "en-US"));
    }
}
