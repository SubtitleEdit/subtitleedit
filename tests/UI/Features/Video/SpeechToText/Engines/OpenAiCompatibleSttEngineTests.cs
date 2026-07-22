using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

/// <summary>
/// <see cref="OpenAiCompatibleSttEngine"/> derives its help asset name from
/// <c>StaticName</c> - "OpenAI Compatible Server" becomes OpenAICompatibleServer.txt. Nothing
/// checks that name at compile time, and the load falls back to a hard-coded blurb on any
/// failure, so a missing or renamed asset shows placeholder text rather than failing. That is
/// exactly what happened: the asset was absent and the fallback served in its place.
/// </summary>
public class OpenAiCompatibleSttEngineTests
{
    [AvaloniaFact]
    public async Task GetHelpText_ReturnsTheEmbeddedAsset_NotTheFallback()
    {
        var engine = new OpenAiCompatibleSttEngine();

        var helpText = await engine.GetHelpText();

        // Wording from the asset; absent from the fallback blurb in the catch.
        Assert.Contains("/v1/audio/transcriptions", helpText);
        Assert.Contains("STT Endpoint URL", helpText);
        Assert.DoesNotContain("Supports any OpenAI-compatible STT API endpoint.", helpText);
    }
}
