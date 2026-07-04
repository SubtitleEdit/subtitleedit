using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.DashScope;

namespace UITests.Features.Video.SpeechToText.DashScope;

public class DashScopeSttServiceTests
{
    private static DashScopeSttSettings MakeSettings()
        => new()
        {
            ApiKey = "test-key",
            Model = "qwen3-asr-flash-filetrans",
            Language = "en",
            Region = "international",
            EnableWords = false,
            TimeoutSeconds = 3600,
        };

    [Theory]
    [InlineData("international", "https://dashscope-intl.aliyuncs.com")]
    [InlineData("china", "https://dashscope.aliyuncs.com")]
    [InlineData("", "https://dashscope-intl.aliyuncs.com")]
    [InlineData(null, "https://dashscope-intl.aliyuncs.com")]
    public void GetBaseUrl_PicksRegion(string? region, string expected)
    {
        Assert.Equal(expected, DashScopeSttService.GetBaseUrl(region));
    }

    [Fact]
    public void BuildSubmitBody_SetsModelFileUrlAndParameters()
    {
        var settings = MakeSettings();
        settings.EnableWords = true;
        var body = DashScopeSttService.BuildSubmitBody(settings, "oss://bucket/dir/audio.mp3", "ja");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("qwen3-asr-flash-filetrans", root.GetProperty("model").GetString());
        Assert.Equal("oss://bucket/dir/audio.mp3", root.GetProperty("input").GetProperty("file_url").GetString());

        var parameters = root.GetProperty("parameters");
        Assert.True(parameters.GetProperty("enable_words").GetBoolean());
        Assert.False(parameters.GetProperty("enable_itn").GetBoolean());
        Assert.Equal("ja", parameters.GetProperty("language").GetString());
    }

    [Fact]
    public void BuildSubmitBody_OmitsLanguageWhenAutoOrEmpty()
    {
        var settings = MakeSettings();
        settings.Language = string.Empty;
        var body = DashScopeSttService.BuildSubmitBody(settings, "oss://x/y.mp3", null);

        using var doc = JsonDocument.Parse(body);
        Assert.False(doc.RootElement.GetProperty("parameters").TryGetProperty("language", out _));
    }

    [Fact]
    public void ParseTranscriptionResult_MapsSentencesToSecondsSegments()
    {
        const string json = """
            {
              "transcripts": [
                {
                  "text": "Hello there. How are you?",
                  "sentences": [
                    { "begin_time": 760, "end_time": 3240, "text": "Hello there." },
                    { "begin_time": 3500, "end_time": 5000, "text": "How are you?" }
                  ]
                }
              ]
            }
            """;

        var result = DashScopeSttService.ParseTranscriptionResult(json);

        Assert.NotNull(result.Segments);
        Assert.Equal(2, result.Segments!.Count);
        // Milliseconds converted to seconds.
        Assert.Equal(0.76, result.Segments[0].Start, 3);
        Assert.Equal(3.24, result.Segments[0].End, 3);
        Assert.Equal("Hello there.", result.Segments[0].Text);
        Assert.Equal(5.0, result.Segments[1].End, 3);
        Assert.Contains("How are you?", result.Text);
    }

    [Fact]
    public void ParseTranscriptionResult_EmptyOrMalformed_ReturnsEmpty()
    {
        var result = DashScopeSttService.ParseTranscriptionResult("not json");
        Assert.NotNull(result.Segments);
        Assert.Empty(result.Segments!);
        Assert.Equal(string.Empty, result.Text);
    }
}
