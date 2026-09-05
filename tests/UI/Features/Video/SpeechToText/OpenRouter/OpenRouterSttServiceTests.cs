using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

    [Theory]
    [InlineData("openai/gpt-transcribe")]
    [InlineData("openai/gpt-4o-transcribe")]
    [InlineData("openai/gpt-4o-mini-transcribe")]
    [InlineData("openai/gpt-5-transcribe")] // not released at the time of writing; matched by name shape, not an exact list
    public void BuildRequestBody_GptTranscriptionModelsUseJsonWithoutTimestamps(string model)
    {
        var body = OpenRouterSttService.BuildRequestBody(
            MakeSettings(model),
            Encoding.UTF8.GetBytes("hello-bytes"),
            "mp3",
            "ar");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("json", root.GetProperty("response_format").GetString());
        Assert.False(root.TryGetProperty("timestamp_granularities", out _));
    }

    [Theory]
    [InlineData("openai/whisper-1")]
    [InlineData("openai/whisper-large-v3")]
    [InlineData("openai/gpt-4o-audio-preview")] // "gpt-" prefixed but not a transcription model
    public void BuildRequestBody_NonTranscribeModelsKeepVerboseJson(string model)
    {
        var body = OpenRouterSttService.BuildRequestBody(
            MakeSettings(model),
            Encoding.UTF8.GetBytes("hello-bytes"),
            "mp3",
            "ar");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal("verbose_json", root.GetProperty("response_format").GetString());
        Assert.True(root.TryGetProperty("timestamp_granularities", out _));
    }

    [Theory]
    [InlineData("google/chirp-3", true)]
    [InlineData("google/chirp-3-preview", true)] // matched by name shape, not an exact list
    [InlineData("GOOGLE/CHIRP-3", true)] // case-insensitive
    [InlineData("openai/whisper-1", false)]
    [InlineData("openai/gpt-4o-transcribe", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void RequiresWavAudio_MatchesOnlyChirpModels(string? model, bool expected)
    {
        Assert.Equal(expected, OpenRouterSttService.RequiresWavAudio(model));
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

    [Fact]
    public async Task TranscribeAsync_ModelRejectsVerboseJson_RetriesOnceWithPlainJson()
    {
        // Google's Chirp models (and presumably others OpenRouter hosts under non-OpenAI
        // names) reject verbose_json the same way gpt-*-transcribe models do, but
        // IsJsonOnlyModel only recognizes the latter by name - this is exactly the #14028
        // follow-up bug: "chirp-3" doesn't match "gpt-*-transcribe" so the first request
        // still asks for verbose_json and gets rejected.
        var requestBodies = new System.Collections.Generic.List<string>();
        using var handler = new StubHandler(async (req, ct) =>
        {
            var body = await req.Content!.ReadAsStringAsync(ct);
            requestBodies.Add(body);

            if (requestBodies.Count == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        """{"error":{"message":"The selected model does not support response_format \"verbose_json\". Use \"json\" instead.","code":400}}""",
                        Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"text":"hello from chirp"}""", Encoding.UTF8, "application/json"),
            };
        });
        using var client = new HttpClient(handler);
        var service = new OpenRouterSttService(client, MakeSettings("google/chirp-3"));

        var result = await service.TranscribeAsync(Encoding.UTF8.GetBytes("audio"), "wav", "ar", CancellationToken.None);

        Assert.Equal("hello from chirp", result.Text);
        Assert.Equal(2, requestBodies.Count);

        using var firstDoc = JsonDocument.Parse(requestBodies[0]);
        Assert.Equal("verbose_json", firstDoc.RootElement.GetProperty("response_format").GetString());

        using var secondDoc = JsonDocument.Parse(requestBodies[1]);
        Assert.Equal("json", secondDoc.RootElement.GetProperty("response_format").GetString());
        Assert.False(secondDoc.RootElement.TryGetProperty("timestamp_granularities", out _));
    }

    [Fact]
    public async Task TranscribeAsync_ModelRejectsVerboseJsonWithGenericError_StillRetriesWithPlainJson()
    {
        // OpenRouter doesn't always forward the provider's specific complaint - some
        // providers' rejections come through as an opaque "Provider returned 400" with no
        // mention of response_format or verbose_json at all. Pattern-matching the message
        // text would miss this, so the retry triggers on any 400 to the first (verbose_json)
        // attempt regardless of what the body says.
        var requestBodies = new System.Collections.Generic.List<string>();
        using var handler = new StubHandler(async (req, ct) =>
        {
            var body = await req.Content!.ReadAsStringAsync(ct);
            requestBodies.Add(body);

            if (requestBodies.Count == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("""{"error":{"message":"Provider returned 400","code":400}}""", Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"text":"hello from chirp"}""", Encoding.UTF8, "application/json"),
            };
        });
        using var client = new HttpClient(handler);
        var service = new OpenRouterSttService(client, MakeSettings("google/chirp-3"));

        var result = await service.TranscribeAsync(Encoding.UTF8.GetBytes("audio"), "wav", "ar", CancellationToken.None);

        Assert.Equal("hello from chirp", result.Text);
        Assert.Equal(2, requestBodies.Count);
    }

    [Fact]
    public async Task TranscribeAsync_BothFormatsRejected_ThrowsAfterOneRetry()
    {
        // If the plain-json retry also fails, the retry must not loop again - the second
        // failure (whatever it is) is the one that surfaces to the caller.
        var callCount = 0;
        using var handler = new StubHandler((req, ct) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":{"message":"Provider returned 400","code":400}}""", Encoding.UTF8, "application/json"),
            });
        });
        using var client = new HttpClient(handler);
        var service = new OpenRouterSttService(client, MakeSettings("google/chirp-3"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.TranscribeAsync(Encoding.UTF8.GetBytes("audio"), "wav", "ar", CancellationToken.None));

        Assert.Equal(2, callCount); // one retry, then give up
    }

    [Fact]
    public async Task TranscribeAsync_NonBadRequestStatus_DoesNotRetryAndThrows()
    {
        // A 401 means the request never got far enough to be a format complaint - retrying
        // with a different response_format won't fix an auth failure, so don't waste a call.
        var callCount = 0;
        using var handler = new StubHandler((req, ct) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("""{"error":{"message":"Invalid API key","code":401}}""", Encoding.UTF8, "application/json"),
            });
        });
        using var client = new HttpClient(handler);
        var service = new OpenRouterSttService(client, MakeSettings("google/chirp-3"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.TranscribeAsync(Encoding.UTF8.GetBytes("audio"), "wav", "ar", CancellationToken.None));

        Assert.Equal(1, callCount); // no retry for a non-400 status
    }

    [Fact]
    public async Task TranscribeAsync_GptTranscribeModelRejectsRequest_DoesNotRetry()
    {
        // Already asking for plain json on the first try (IsJsonOnlyModel matched by name), so
        // a rejection here is a real failure, not the response-format mismatch this fix covers -
        // must not loop.
        var callCount = 0;
        using var handler = new StubHandler((req, ct) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":{"message":"Invalid API key","code":400}}""", Encoding.UTF8, "application/json"),
            });
        });
        using var client = new HttpClient(handler);
        var service = new OpenRouterSttService(client, MakeSettings("openai/gpt-4o-transcribe"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.TranscribeAsync(Encoding.UTF8.GetBytes("audio"), "wav", "ar", CancellationToken.None));

        Assert.Equal(1, callCount);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        {
            _send = send;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _send(request, cancellationToken);
    }
}
