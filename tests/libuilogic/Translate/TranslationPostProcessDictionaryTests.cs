using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

public class TranslationPostProcessDictionaryTests
{
    [Fact]
    public void EveryBundledDictionaryPassesEditorValidation()
    {
        Assert.All(TranslationPostProcessDictionary.Languages, language =>
            Assert.NotEmpty(TranslationPostProcessDictionary.Read(language)));
    }

    [Theory]
    [InlineData("Danish", "Danish", "da", "Tak.")]
    [InlineData("Norwegian", "Norwegian", "no", "Takk.")]
    [InlineData("Brazilian Portuguese", "Brazilian Portuguese", "pt-BR", "Obrigado.")]
    [InlineData("Chinese", "Chinese", "zh-TW", "謝謝。")]
    public void EmbeddedDictionariesResolveProviderLanguageMetadata(string name, string code, string iso, string expected)
    {
        var rules = TranslationPostProcessDictionary.Get(
            new TranslationPair("English", "EN-US"), new TranslationPair(name, code, iso));
        // Portuguese thanks depend on the speaker's gender, so they are suggestions; the first is applied.
        var rule = rules?.Match("Thank you.", "Thank you.");
        Assert.Equal(expected, rule?.Replacement ?? rule?.Suggestions[0]);
    }

    [Fact]
    public void DanishRequiresTheObservedSourceAndDraft()
    {
        var rules = TranslationPostProcessDictionary.Get(new TranslationPair("English", "en"), new TranslationPair("Danish", "da"));
        Assert.NotNull(rules);
        Assert.Null(rules.Match("You got it.", "Det gør jeg."));
        Assert.Null(rules.Match("I got it.", "Du har det."));
        Assert.Null(rules.Match("- You got it.", "- Du har det."));
        Assert.Null(rules.Match("I wouldn't count on it.", "Jeg ville ikke regne med det."));
        Assert.Equal("Det ville jeg ikke regne med.", rules.Match("I wouldn’t count on it.", "Jeg kan ikke regne på det.")?.Replacement);
        var review = rules.Match("You got it.", "Du har det.");
        Assert.NotNull(review);
        Assert.Null(review.Replacement);
        Assert.Contains("Det gør jeg.", review.Suggestions);
    }

    [Theory]
    [InlineData("Sorry about that.", "Undskyld det.", "Undskyld.")]
    [InlineData("Sorry to hear that.", "Undskyld at høre det.", "Det er jeg ked af at høre.")]
    [InlineData("Sorry for your loss.", "Undskyld for dit tab.", "Jeg kondolerer.")]
    [InlineData("Sorry I'm late.", "Undskyld, jeg er sen.", "Undskyld, jeg kommer for sent.")]
    [InlineData("You're welcome.", "Du er velkommen.", "Selv tak.")]
    public void DanishFixesLiteralApologiesAndReplies(string source, string draft, string expected)
    {
        var rules = TranslationPostProcessDictionary.Get(new TranslationPair("English", "en"), new TranslationPair("Danish", "da"));
        Assert.NotNull(rules);
        Assert.Equal(expected, rules.Match(source, draft)?.Replacement);
    }

    [Theory]
    [InlineData("Swedish", "sv", "Sorry about that.", "Ursäkta det.", "Förlåt.")]
    [InlineData("Swedish", "sv", "Sorry for your loss.", "Ledsen för din förlust.", "Jag beklagar sorgen.")]
    [InlineData("Swedish", "sv", "You're welcome.", "Du är välkommen.", "Varsågod.")]
    [InlineData("Norwegian", "nb", "Sorry about that.", "Unnskyld det.", "Unnskyld.")]
    [InlineData("Norwegian", "nb", "Sorry to hear that.", "Unnskyld å høre det.", "Leit å høre.")]
    [InlineData("Norwegian", "nb", "You're welcome.", "Du er velkommen.", "Bare hyggelig.")]
    public void ScandinavianFixesLiteralApologiesAndReplies(string name, string code, string source, string draft, string expected)
    {
        var rules = TranslationPostProcessDictionary.Get(new TranslationPair("English", "en"), new TranslationPair(name, code));
        Assert.NotNull(rules);
        Assert.Equal(expected, rules.Match(source, draft)?.Replacement);
    }

    [Fact]
    public void UserOverridesReloadAndCanRemoveBundledRules()
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            var source = new TranslationPair("English", "en");
            var target = new TranslationPair("Danish", "da");
            var defaults = TranslationPostProcessDictionary.Read("da");
            var edit = new TranslationPostProcessDictionary.Entry("en", "Thank you.", "Thank you.",
                new TranslationPostProcessDictionary.Rule("Mange tak!", Array.Empty<string>(), null));
            Assert.Equal("Tak.", TranslationPostProcessDictionary.Get(source, target, folder.FullName)
                ?.Match("Thank you.", "Thank you.")?.Replacement);

            TranslationPostProcessDictionary.Save("da", new[] { edit }, folder.FullName);
            Assert.Equal("Mange tak!", TranslationPostProcessDictionary.Get(source, target, folder.FullName)
                ?.Match("Thank you.", "Thank you.")?.Replacement);
            Assert.Null(TranslationPostProcessDictionary.Get(source, target, folder.FullName)?.Match("Good night.", "Good night."));

            Assert.Throws<InvalidDataException>(() =>
                TranslationPostProcessDictionary.Save("da", new[] { edit, edit }, folder.FullName));
            Assert.Single(TranslationPostProcessDictionary.Read("da", folder.FullName));
            Assert.Equal(defaults.Count, TranslationPostProcessDictionary.Read("da").Count);

            File.WriteAllText(Path.Combine(folder.FullName, "da.json"), "{}");
            Assert.Throws<InvalidDataException>(() => TranslationPostProcessDictionary.Read("da", folder.FullName));
            Assert.Equal("Tak.", TranslationPostProcessDictionary.Get(source, target, folder.FullName)
                ?.Match("Thank you.", "Thank you.")?.Replacement);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }
}
