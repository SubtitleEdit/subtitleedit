using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenRouter;

namespace UITests.Features.Video.SpeechToText.OpenRouter;

public class OpenRouterSttServiceTests
{
    private static OpenRouterSttSettings MakeSettings(string model = "openai/whisper-1")
        => new()
        {
            ApiKey = "test-key",
            Model = model,
            Language = "en",
            Temperature = 0,
            Prompt = string.Empty,
            TimeoutSeconds = 30,
        };

    [Fact]
    public void BuildRequestBody_EncodesAudioAndAsksForTimestamps()
    {
        var audio = Encoding.UTF8.GetBytes("hello-bytes");
        var body = OpenRouterSttService.BuildRequestBody(MakeSettings(), audio, "mp3", "de");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("openai/whisper-1", root.GetProperty("model").GetString());
        Assert.Equal(System.Convert.ToBase64String(audio), root.GetProperty("input_audio").GetProperty("data").GetString());
        Assert.Equal("mp3", root.GetProperty("input_audio").GetProperty("format").GetString());
        Assert.Equal("verbose_json", root.GetProperty("response_format").GetString());

        var granularities = root.GetProperty("timestamp_granularities");
        Assert.Equal(2, granularities.GetArrayLength());
        Assert.Equal("segment", granularities[0].GetString());
        Assert.Equal("word", granularities[1].GetString());

        // Explicit language argument wins over the settings default.
        Assert.Equal("de", root.GetProperty("language").GetString());
    }

    [Fact]
    public void BuildRequestBody_OmitsEmptyLanguageAndZeroTemperature()
    {
        var settings = MakeSettings();
        settings.Language = string.Empty;
        var body = OpenRouterSttService.BuildRequestBody(settings, new byte[] { 1, 2, 3 }, "wav", null);

        using var doc = JsonDocument.Parse(body);
        Assert.False(doc.RootElement.TryGetProperty("language", out _));
        Assert.False(doc.RootElement.TryGetProperty("temperature", out _));
    }

    [Fact]
    public void ParseResponse_VerboseJson_ParsesSegments()
    {
        const string json = """
            {
              "text": "hello world",
              "segments": [
                { "id": 0, "start": 0.0, "end": 1.0, "text": "hello" },
                { "id": 1, "start": 1.0, "end": 2.0, "text": " world" }
              ]
            }
            """;

        var result = OpenRouterSttService.ParseResponse(json);

        Assert.Equal("hello world", result.Text);
        Assert.NotNull(result.Segments);
        Assert.Equal(2, result.Segments!.Count);
        Assert.Equal("hello", result.Segments[0].Text);
        Assert.Equal(1.0, result.Segments[1].Start);
    }

    [Fact]
    public void ParseResponse_WordsOnly_GroupsIntoSegments()
    {
        const string json = """
            {
              "text": "one two",
              "words": [
                { "word": "one", "start": 0.0, "end": 0.4 },
                { "word": "two", "start": 0.5, "end": 0.9 }
              ]
            }
            """;

        var result = OpenRouterSttService.ParseResponse(json);

        Assert.NotNull(result.Segments);
        Assert.True(result.Segments!.Count >= 1);
        Assert.Contains("one", result.Segments[0].Text);
    }

    [Fact]
    public void ParseResponse_PlainText_FallsBackToText()
    {
        var result = OpenRouterSttService.ParseResponse("just some text");
        Assert.Equal("just some text", result.Text);
    }
}
