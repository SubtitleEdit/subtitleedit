using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// Each entry in data.translations has more string properties than "translatedText" - reading them all
/// appended the source language code ("en", "ja") to every translated subtitle (#15085).
/// </summary>
public class GoogleTranslateV2ParseTests
{
    [Fact]
    public void DetectedSourceLanguage_IsNotPartOfTheTranslation()
    {
        var json = "{ \"data\": { \"translations\": [ { \"translatedText\": \"Bedankt.\", \"detectedSourceLanguage\": \"en\" } ] } }";

        Assert.Equal("Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
    }

    [Fact]
    public void Model_IsNotPartOfTheTranslation()
    {
        var json = "{ \"data\": { \"translations\": [ { \"detectedSourceLanguage\": \"ja\", \"model\": \"nmt\", \"translatedText\": \"Bedankt.\" } ] } }";

        Assert.Equal("Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
    }

    [Fact]
    public void MultipleTranslations_AreJoinedWithNewLine()
    {
        var json = "{ \"data\": { \"translations\": [ { \"translatedText\": \"Hallo.\" }, { \"translatedText\": \"Bedankt.\" } ] } }";

        Assert.Equal("Hallo." + Environment.NewLine + "Bedankt.", GoogleTranslateV2.ParseTranslations(json, "nl"));
    }
}
