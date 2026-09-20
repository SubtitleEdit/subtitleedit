using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// A Gemini reply without any text part used to come back as an empty translation, which the
/// translate loop silently retried - with a slow model the window looked hung (#14926). The error
/// now names the reason the reply gives.
/// </summary>
public class GeminiTranslateNoTextTests
{
    [Fact]
    public void BlockedPrompt_NamesTheBlockReason()
    {
        var message = GeminiTranslate.MakeNoTextMessage("{ \"promptFeedback\": { \"blockReason\": \"SAFETY\" } }");

        Assert.Contains("prompt blocked: SAFETY", message);
    }

    [Fact]
    public void CandidateWithoutText_NamesTheFinishReason()
    {
        var message = GeminiTranslate.MakeNoTextMessage("{ \"candidates\": [ { \"content\": { \"role\": \"model\" }, \"finishReason\": \"MAX_TOKENS\" } ] }");

        Assert.Contains("finish reason: MAX_TOKENS", message);
    }

    [Fact]
    public void NoReason_StillExplains()
    {
        var message = GeminiTranslate.MakeNoTextMessage("{}");

        Assert.StartsWith("Google Gemini returned no translated text.", message);
    }

    [Fact]
    public void FlashLiteLatestIsOffered()
    {
        Assert.Contains("gemini-flash-lite-latest", GeminiTranslate.Models);
    }
}
