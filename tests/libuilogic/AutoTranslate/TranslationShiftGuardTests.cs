using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// Issue #16555: the model merged two rows into one and padded the end with a copy of an earlier
/// translation, so every row in between held its neighbour's translation.
/// </summary>
public class TranslationShiftGuardTests
{
    private static readonly string[] Sources =
    {
        "You know that I would do everything for you. If you are",
        "always that nice to me I will give you everything I have.",
        "Come on. You don't know how beautiful this is.",
        "Do you like it?",
    };

    [Fact]
    public void AlignedReply_HasNoDuplicate()
    {
        var translations = new[]
        {
            "Du weißt, dass ich alles für dich tun würde. Wenn du",
            "immer so nett zu mir bist, gebe ich dir alles, was ich habe.",
            "Komm schon. Du weißt nicht, wie schön das ist.",
            "Gefällt es dir?",
        };

        Assert.Equal(-1, TranslationShiftGuard.FindDuplicatedTranslation(Sources, translations));
    }

    [Fact]
    public void ShiftedReplyPaddedWithEarlierLine_ReportsThePadding()
    {
        var translations = new[]
        {
            "Du weißt, dass ich alles für dich tun würde. Wenn du immer so nett zu mir bist, gebe ich dir alles.",
            "Komm schon. Du weißt nicht, wie schön das ist.",
            "Gefällt es dir?",
            "Du weißt, dass ich alles für dich tun würde. Wenn du immer so nett zu mir bist, gebe ich dir alles.",
        };

        Assert.Equal(3, TranslationShiftGuard.FindDuplicatedTranslation(Sources, translations));
    }

    [Fact]
    public void DuplicateIgnoresCaseWhitespaceAndTags()
    {
        var translations = new[]
        {
            "Komm schon. Du weißt nicht, wie schön das ist.",
            "x",
            "y",
            "<i>komm schon.</i>" + Environment.NewLine + "Du weißt nicht,  wie schön das ist.",
        };

        Assert.Equal(3, TranslationShiftGuard.FindDuplicatedTranslation(Sources, translations));
    }

    [Fact]
    public void ShortLinesMayRepeat()
    {
        var translations = new[] { "Ja.", "Danke.", "Ja.", "Danke." };

        Assert.Equal(-1, TranslationShiftGuard.FindDuplicatedTranslation(Sources, translations));
    }

    [Fact]
    public void RepeatedSourceMayRepeatItsTranslation()
    {
        var sources = new[] { "I don't know what you mean.", "Why?", "I don't know what you mean." };
        var translations = new[] { "Ich weiß nicht, was du meinst.", "Warum?", "Ich weiß nicht, was du meinst." };

        Assert.Equal(-1, TranslationShiftGuard.FindDuplicatedTranslation(sources, translations));
    }

    [Fact]
    public void DuplicateOfContextRow_IsReportedButContextIsNot()
    {
        var sources = new[] { "Come on. You don't know how beautiful this is.", "Come on. You don't know how pretty this is.", "Do you like it?" };
        var translations = new[] { "Komm schon. Du weißt nicht, wie schön das ist.", "Komm schon. Du weißt nicht, wie schön das ist.", "Gefällt es dir?" };

        Assert.Equal(1, TranslationShiftGuard.FindDuplicatedTranslation(sources, translations, firstCheckedIndex: 1));
        Assert.Equal(-1, TranslationShiftGuard.FindDuplicatedTranslation(sources, translations, firstCheckedIndex: 2));
    }

    // Issue #16585: TranslateGemma 12B (en -> nl) joined "are the ocean's" + "most spectacular
    // animals." into one row, moved the next rows up by one and padded the freed row with "...".
    [Fact]
    public void ShiftedReplyPaddedWithPlaceholder_ReportsThePlaceholder()
    {
        var sources = new[]
        {
            "...but hidden here\nare the ocean's",
            "most spectacular animals.",
            "A blue whale.",
            "At 25 metres long,",
            "it's the largest creature\nthat has ever existed.",
        };
        var translations = new[]
        {
            "...maar hier verborgen\nzich de meest spectaculaire\ndieren van de oceaan.",
            "Een blauwe vinvis.",
            "Met een lengte van 25 meter,",
            "...",
            "Het is het grootste wezen\nter wereld.",
        };

        Assert.Equal(3, TranslationShiftGuard.FindPlaceholderTranslation(sources, translations));
    }

    [Theory]
    [InlineData("\u2026")]
    [InlineData("-")]
    [InlineData("<i>...</i>")]
    public void PlaceholderVariants_AreReported(string placeholder)
    {
        Assert.Equal(1, TranslationShiftGuard.FindPlaceholderTranslation(new[] { "Close to the water...", "...the weed moves" }, new[] { "Dichtbij het water...", placeholder }));
    }

    [Fact]
    public void PunctuationOnlySource_MayStayPunctuation()
    {
        Assert.Equal(-1, TranslationShiftGuard.FindPlaceholderTranslation(new[] { "Yes.", "...", "25" }, new[] { "Ja.", "...", "25" }));
    }
}
