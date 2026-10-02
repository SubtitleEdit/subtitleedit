using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// TranslateGemma 12B (llama.cpp, en -> de/ja) sometimes hands a merged block back in English,
/// and the reply was written out as the translation. The guard spots such a reply.
/// </summary>
public class TranslationEchoGuardTests
{
    [Theory]
    [InlineData("I was at the office until late.", "I was at the office until late.")]
    [InlineData("I was at the office until late.", "  i WAS at the office   until late. ")]
    [InlineData("Not when the people he owes\ncarry guns.", "Not when the people he owes carry guns.")]
    [InlineData("Not when the people he owes<br />carry guns.", "Not when the people he owes\r\ncarry guns.")]
    [InlineData("<i>Somebody is standing outside.</i>", "Somebody is standing outside.")]
    [InlineData("{\\an8}Somebody is standing outside.", "Somebody is standing outside.")]
    public void SourceReturnedAsIs_IsEcho(string source, string reply)
    {
        Assert.True(TranslationEchoGuard.IsUntranslatedEcho(source, reply, "English", "German"));
    }

    [Fact]
    public void MergedBlockReturnedAsIs_IsEcho()
    {
        var block = "Where were you last night?\nI was at the office until late.\nDon't lie to me. I called them.";

        Assert.True(TranslationEchoGuard.IsUntranslatedEcho(block, block, "English", "Japanese"));
    }

    [Fact]
    public void RealTranslation_IsNotEcho()
    {
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(
            "I was at the office until late.", "Ich war bis spät im Büro.", "English", "German"));
    }

    [Fact]
    public void PartlyTranslatedReply_IsNotEcho()
    {
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(
            "Where were you last night?\nI was at the office until late.",
            "Wo warst du letzte Nacht?\nI was at the office until late.",
            "English", "German"));
    }

    [Fact]
    public void MergedBlockMostlyEchoed_IsEcho()
    {
        // One line translated must not let the echoed rest through.
        Assert.True(TranslationEchoGuard.IsUntranslatedEcho(
            "Where were you last night?\nI was at the office until late.\nDon't lie to me, I called them.",
            "Wo warst du letzte Nacht?\nI was at the office until late.\nDon't lie to me, I called them.",
            "English", "German"));
    }

    [Fact]
    public void MergedBlockWithOneLineSameInBothLanguages_IsNotEcho()
    {
        // A line already in the target language legitimately comes back unchanged.
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(
            "Where were you last night?\nIch war bis spät im Büro.\nDon't lie to me, I called them.",
            "Wo warst du letzte Nacht?\nIch war bis spät im Büro.\nLüg mich nicht an, ich habe sie angerufen.",
            "English", "German"));
    }

    [Fact]
    public void MergedBlockOfShortLines_IsJudgedAsAWhole()
    {
        var block = "Where is he?\nIn the car.\nThe red one?";

        Assert.True(TranslationEchoGuard.IsUntranslatedEcho(block, block, "English", "German"));
    }

    [Theory]
    [InlineData("Spanish", "Spanish (Latin America)")]
    [InlineData("Chinese (Simplified)", "Chinese (Traditional)")]
    [InlineData("Portuguese (Brazil)", "portuguese")]
    public void VariantsOfOneLanguage_AreNeverEcho(string sourceLanguage, string targetLanguage)
    {
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(
            "I was at the office until late.", "I was at the office until late.", sourceLanguage, targetLanguage));
    }

    [Theory]
    [InlineData("English", "English")]
    [InlineData("english", "English ")]
    [InlineData("", "German")]
    [InlineData("English", "")]
    public void SameOrUnknownLanguage_IsNeverEcho(string sourceLanguage, string targetLanguage)
    {
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(
            "I was at the office until late.", "I was at the office until late.", sourceLanguage, targetLanguage));
    }

    [Theory]
    [InlineData("Erik.")]
    [InlineData("Okay")]
    [InlineData("Okay, okay!")]
    [InlineData("Hi, John.")]
    [InlineData("Erik Andersson.")]
    [InlineData("1984")]
    [InlineData("12:45 - 13:00")]
    [InlineData("Ha ha ha!")] // three words, but too few letters to need translating
    [InlineData("<i>Erik.</i>")]
    [InlineData("♪ ♪")]
    public void ShortLinesNumbersAndNames_AreNeverEcho(string text)
    {
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho(text, text, "English", "German"));
    }

    [Fact]
    public void EmptyReply_IsNotEcho()
    {
        // An empty reply is already a failure of its own - the guard is not what reports it.
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho("I was at the office until late.", "", "English", "German"));
    }

    [Fact]
    public void NoSpaceScriptSource_UsesLetterCount()
    {
        Assert.True(TranslationEchoGuard.IsUntranslatedEcho("昨夜はどこにいたの？", "昨夜はどこにいたの？", "Japanese", "English"));
        Assert.False(TranslationEchoGuard.IsUntranslatedEcho("東京", "東京", "Japanese", "English"));
    }
}
