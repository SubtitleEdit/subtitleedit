using System.Collections.ObjectModel;
using System.Reflection;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Xunit;

namespace UITests.Features.Translate;

/// <summary>
/// Picking a different auto-translate engine rebuilds both language combos from that engine and
/// re-derives a default, which discarded the languages the user had just chosen (#13943). The
/// picks are now carried across when the new engine can offer the same language - which it has to
/// recognise despite engines spelling languages differently ("en" vs "eng_Latn" vs "English").
/// </summary>
public class AutoTranslateLanguageCarryOverTests
{
    private static TranslationPair? FindSameLanguage(TranslationPair? previous, ObservableCollection<TranslationPair> languages)
    {
        var method = typeof(AutoTranslateViewModel).GetMethod(
            "FindSameLanguage", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (TranslationPair?)method.Invoke(null, new object?[] { previous, languages });
    }

    private static ObservableCollection<TranslationPair> Languages(params (string Name, string Code)[] items)
    {
        var list = new ObservableCollection<TranslationPair>();
        foreach (var (name, code) in items)
        {
            list.Add(new TranslationPair(name, code));
        }

        return list;
    }

    [Fact]
    public void TheSameCodeIsCarriedOver()
    {
        var languages = Languages(("English", "en"), ("German", "de"));

        var match = FindSameLanguage(new TranslationPair("German", "de"), languages);

        Assert.Equal("de", match?.Code);
    }

    /// <summary>NLLB spells English "eng_Latn"; the name is what bridges the two engines.</summary>
    [Fact]
    public void ADifferentCodeForTheSameLanguageIsMatchedByName()
    {
        var languages = Languages(("English", "eng_Latn"), ("German", "deu_Latn"));

        var match = FindSameLanguage(new TranslationPair("German", "de"), languages);

        Assert.Equal("deu_Latn", match?.Code);
    }

    [Fact]
    public void NameMatchingIgnoresCase()
    {
        var languages = Languages(("german", "deu_Latn"));

        var match = FindSameLanguage(new TranslationPair("German", "de"), languages);

        Assert.Equal("deu_Latn", match?.Code);
    }

    /// <summary>
    /// When the new engine genuinely cannot do that language, nothing is carried over and the
    /// re-derived default is left to stand.
    /// </summary>
    [Fact]
    public void ALanguageTheNewEngineLacksIsNotCarriedOver()
    {
        var languages = Languages(("English", "en"), ("French", "fr"));

        var match = FindSameLanguage(new TranslationPair("Klingon", "tlh"), languages);

        Assert.Null(match);
    }

    [Fact]
    public void NoPreviousSelectionCarriesNothing()
    {
        var match = FindSameLanguage(null, Languages(("English", "en")));

        Assert.Null(match);
    }

    /// <summary>An empty code must not match another entry that also has an empty code.</summary>
    [Fact]
    public void EmptyCodesDoNotMatchEachOther()
    {
        var languages = Languages(("French", string.Empty));

        var match = FindSameLanguage(new TranslationPair("German", string.Empty), languages);

        Assert.Null(match);
    }
}
