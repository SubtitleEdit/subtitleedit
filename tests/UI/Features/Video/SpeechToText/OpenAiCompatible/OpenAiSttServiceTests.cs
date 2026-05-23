using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

namespace UITests.Features.Video.SpeechToText.OpenAiCompatible;

public class OpenAiSttServiceTests
{
    private static OpenAiCompatibleSettings MakeSettings(string model = "whisper-1", string extraHeaders = "")
        => new()
        {
            EndpointUrl = "http://localhost:8000/v1/audio/transcriptions",
            ApiKey = "test-key",
            Model = model,
            Language = "en",
            TimeoutSeconds = 30,
            Temperature = 0,
            Prompt = string.Empty,
            ExtraHeaders = extraHeaders,
        };

    private static string MakeTinyWav()
    {
        // Minimal valid 44-byte WAV header with 0 samples; the service only
        // streams the file content as multipart, so the bytes don't have to
        // be playable.
        var path = Path.Combine(Path.GetTempPath(), $"se-stt-test-{Guid.NewGuid()}.wav");
        var header = new byte[]
        {
            (byte)'R', (byte)'I', (byte)'F', (byte)'F', 36, 0, 0, 0,
            (byte)'W', (byte)'A', (byte)'V', (byte)'E',
            (byte)'f', (byte)'m', (byte)'t', (byte)' ', 16, 0, 0, 0,
            1, 0, 1, 0, 0x40, 0x1f, 0, 0, 0x80, 0x3e, 0, 0, 2, 0, 16, 0,
            (byte)'d', (byte)'a', (byte)'t', (byte)'a', 0, 0, 0, 0,
        };
        File.WriteAllBytes(path, header);
        return path;
    }

    [Fact]
    public async Task TranscribeAsync_NonStreamingJson_ParsesSegments()
    {
        const string json = """
            {
              "text": "hello world",
              "language": "en",
              "duration": 2.0,
              "segments": [
                { "id": 0, "start": 0.0, "end": 1.0, "text": "hello" },
                { "id": 1, "start": 1.0, "end": 2.0, "text": " world" }
              ]
            }
            """;

        using var handler = new StubHandler((req, ct) => Task.FromResult(JsonResponse(json)));
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, MakeSettings());

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            var response = await service.TranscribeAsync(wav, cancellationToken: ct);

            Assert.Equal("hello world", response.Text);
            Assert.NotNull(response.Segments);
            Assert.Equal(2, response.Segments!.Count);
            Assert.Equal("hello", response.Segments[0].Text);
            Assert.Equal(0.0, response.Segments[0].Start);
            Assert.Equal(1.0, response.Segments[0].End);
            Assert.Equal(" world", response.Segments[1].Text);
            Assert.Equal("en", response.Language);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public async Task TranscribeAsync_PlainTextBody_FallsBackToSingleSegment()
    {
        // Body that JsonSerializer cannot deserialize into OpenAiCompatibleSttResponse
        // forces the fallback branch that wraps it as a single-segment response.
        using var handler = new StubHandler((req, ct) =>
            Task.FromResult(JsonResponse("not valid json at all", contentType: "application/json")));
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, MakeSettings());

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            var response = await service.TranscribeAsync(wav, cancellationToken: ct);

            Assert.Equal("not valid json at all", response.Text);
            Assert.NotNull(response.Segments);
            Assert.Single(response.Segments!);
            Assert.Equal("not valid json at all", response.Segments![0].Text);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public async Task TranscribeAsync_SseStream_AccumulatesDeltasAndReportsDoneSegments()
    {
        // OpenAI streaming format: a sequence of `event:` + `data:` lines
        // terminated by blank lines. ProcessSseEvent handles
        // transcript.text.delta (append + Progress<string>) and
        // transcript.text.done (segments + Progress<OpenAiCompatibleSegment>).
        const string sse =
            "event: transcript.text.delta\n" +
            "data: {\"type\":\"transcript.text.delta\",\"delta\":\"Hello\"}\n" +
            "\n" +
            "event: transcript.text.delta\n" +
            "data: {\"type\":\"transcript.text.delta\",\"delta\":\" there\"}\n" +
            "\n" +
            "event: transcript.text.done\n" +
            "data: {\"type\":\"transcript.text.done\",\"text\":\"Hello there\"," +
            "\"segments\":[{\"id\":0,\"start\":0,\"end\":1.2,\"text\":\"Hello there\"}]}\n" +
            "\n";

        using var handler = new StubHandler((req, ct) =>
            Task.FromResult(JsonResponse(sse, contentType: "text/event-stream")));
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, MakeSettings());

        var deltas = new List<string>();
        var reportedSegments = new List<OpenAiCompatibleSegment>();
        var deltaProgress = new Progress<string>(d => deltas.Add(d));
        var segmentProgress = new Progress<OpenAiCompatibleSegment>(s => reportedSegments.Add(s));

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            var response = await service.TranscribeAsync(wav, language: null, deltaProgress, segmentProgress, ct);

            // Both deltas should be appended in order.
            Assert.Equal("Hello there", response.Text);

            // Progress<T> marshals to a SynchronizationContext; in xunit the
            // continuation is queued, so allow it to drain.
            await WaitForAsync(() => deltas.Count >= 2 && reportedSegments.Count >= 1);
            Assert.Equal(new[] { "Hello", " there" }, deltas);

            Assert.NotNull(response.Segments);
            Assert.Single(response.Segments!);
            Assert.Equal("Hello there", response.Segments![0].Text);
            Assert.Equal(1.2, response.Segments[0].End);
            Assert.Single(reportedSegments);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public async Task TranscribeAsync_AttachesExtraHeaders_AndIgnoresMalformedLines()
    {
        // ExtraHeaders is a free-form multi-line string of "Name: value" pairs;
        // AddExtraHeaders should attach the well-formed ones, skip lines that
        // don't contain a colon, and never overwrite Authorization (set from ApiKey).
        var extra = string.Join("\n", new[]
        {
            "X-Custom-Header: custom-value",
            "X-Another: another-value",
            "not a header line",
            "Authorization: should-be-ignored",
        });

        HttpRequestMessage? capturedRequest = null;
        using var handler = new StubHandler((req, ct) =>
        {
            capturedRequest = req;
            return Task.FromResult(JsonResponse("""{"text":"ok"}"""));
        });
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, MakeSettings(extraHeaders: extra));

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            await service.TranscribeAsync(wav, cancellationToken: ct);

            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest!.Headers.TryGetValues("X-Custom-Header", out var custom));
            Assert.Equal("custom-value", custom!.Single());
            Assert.True(capturedRequest.Headers.TryGetValues("X-Another", out var another));
            Assert.Equal("another-value", another!.Single());

            // Authorization came from ApiKey ("Bearer test-key") and must not be
            // replaced by the ExtraHeaders entry.
            Assert.Equal("Bearer", capturedRequest.Headers.Authorization?.Scheme);
            Assert.Equal("test-key", capturedRequest.Headers.Authorization?.Parameter);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public void Constructor_DoesNotMutateHttpClientTimeout()
    {
        // Per-call timeouts are now applied via a linked CTS inside
        // TranscribeAsync; the service must NOT mutate the passed-in
        // HttpClient.Timeout so the instance is safe to share.
        using var handler = new StubHandler((req, ct) => Task.FromResult(JsonResponse("""{"text":"ok"}""")));
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(42) };

        _ = new OpenAiSttService(client, MakeSettings());

        Assert.Equal(TimeSpan.FromSeconds(42), client.Timeout);
    }

    [Fact]
    public async Task TranscribeAsync_HonorsPerCallTimeout_FromSettings()
    {
        // settings.TimeoutSeconds drives a CancelAfter on a linked CTS;
        // a handler that never completes should be cancelled by it.
        var requestStarted = new TaskCompletionSource();
        using var handler = new StubHandler(async (req, ct) =>
        {
            requestStarted.TrySetResult();
            // Wait forever — the per-call timeout should fire and cancel ct.
            await Task.Delay(Timeout.Infinite, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var settings = MakeSettings();
        settings.TimeoutSeconds = 1; // 1 second for the test
        var service = new OpenAiSttService(client, settings);

        var wav = MakeTinyWav();
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.TranscribeAsync(wav, cancellationToken: TestContext.Current.CancellationToken));
            Assert.True(requestStarted.Task.IsCompletedSuccessfully);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public void SharedHttpClient_IsStableAndHasInfiniteTimeout()
    {
        // The shared instance must be reused (single-instance) and must use
        // InfiniteTimeSpan, otherwise long SSE streams would be truncated by
        // the default 100-second HttpClient timeout.
        var a = OpenAiSttService.SharedHttpClient;
        var b = OpenAiSttService.SharedHttpClient;

        Assert.Same(a, b);
        Assert.Equal(Timeout.InfiniteTimeSpan, a.Timeout);
    }

    [Fact]
    public async Task TranscribeAsync_NonSuccessStatus_ThrowsHttpRequestException()
    {
        using var handler = new StubHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"error\":\"bad key\"}", Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(resp);
        });
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, MakeSettings());

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.TranscribeAsync(wav, cancellationToken: ct));
            Assert.Contains("401", ex.Message);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public async Task TranscribeAsync_DefaultSettings_DoesNotSendStreamPart()
    {
        // Groq and some other OpenAI-compatible providers reject the `stream`
        // multipart field with HTTP 400. The opt-in default must keep the
        // field out of the request entirely.
        var capturedNames = await CaptureMultipartFieldNamesAsync(MakeSettings());

        Assert.DoesNotContain("stream", capturedNames);
    }

    [Fact]
    public async Task TranscribeAsync_StreamEnabled_SendsStreamTruePart()
    {
        var settings = MakeSettings();
        settings.Stream = true;

        var (names, values) = await CaptureMultipartFieldNamesAndValuesAsync(settings);

        Assert.Contains("stream", names);
        Assert.Equal("true", values["stream"]);
    }

    [Fact]
    public async Task TranscribeAsync_NonSuccessStatus_InvokesLogger_WithSanitizedUrl()
    {
        var logEntries = new List<string>();
        var settings = MakeSettings();
        settings.EndpointUrl = "https://user:secret@api.example.com/v1/audio/transcriptions?api_key=SHOULD_NOT_LEAK&model=x";
        settings.Logger = msg => logEntries.Add(msg);

        using var handler = new StubHandler((req, ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"nope\"}", Encoding.UTF8, "application/json"),
            }));
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, settings);

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            var ex = await Assert.ThrowsAsync<HttpRequestException>(
                () => service.TranscribeAsync(wav, cancellationToken: ct));

            // Logger must have been invoked exactly once and the credentials
            // must not appear in either the log entry or the exception message.
            Assert.Single(logEntries);
            Assert.DoesNotContain("secret", logEntries[0]);
            Assert.DoesNotContain("SHOULD_NOT_LEAK", logEntries[0]);
            Assert.Contains("api_key=***", logEntries[0]);
            Assert.DoesNotContain("secret", ex.Message);
            Assert.DoesNotContain("SHOULD_NOT_LEAK", ex.Message);
        }
        finally
        {
            File.Delete(wav);
        }
    }

    [Fact]
    public void SanitizeEndpointForLog_RedactsUserInfoAndSensitiveQueryParams()
    {
        var sanitized = OpenAiSttService.SanitizeEndpointForLog(
            "https://user:pw@example.com/v1/audio/transcriptions?api_key=AAA&token=BBB&model=whisper&access_token=CCC");

        Assert.DoesNotContain("user", sanitized);
        Assert.DoesNotContain("pw@", sanitized);
        Assert.DoesNotContain("AAA", sanitized);
        Assert.DoesNotContain("BBB", sanitized);
        Assert.DoesNotContain("CCC", sanitized);
        Assert.Contains("api_key=***", sanitized);
        Assert.Contains("token=***", sanitized);
        Assert.Contains("access_token=***", sanitized);
        Assert.Contains("model=whisper", sanitized);
    }

    [Fact]
    public void SanitizeEndpointForLog_PassesThroughNonUrlOrEmpty()
    {
        Assert.Equal("", OpenAiSttService.SanitizeEndpointForLog(""));
        Assert.Equal("not-a-url", OpenAiSttService.SanitizeEndpointForLog("not-a-url"));
    }

    private static async Task<HashSet<string>> CaptureMultipartFieldNamesAsync(OpenAiCompatibleSettings settings)
    {
        var (names, _) = await CaptureMultipartFieldNamesAndValuesAsync(settings);
        return names;
    }

    private static async Task<(HashSet<string> Names, Dictionary<string, string> Values)>
        CaptureMultipartFieldNamesAndValuesAsync(OpenAiCompatibleSettings settings)
    {
        // We need to inspect the parts before the handler returns, since
        // MultipartFormDataContent is disposed once the request completes.
        var names = new HashSet<string>(StringComparer.Ordinal);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        using var handler = new StubHandler(async (req, ct) =>
        {
            if (req.Content is MultipartFormDataContent multipart)
            {
                foreach (var part in multipart)
                {
                    var name = part.Headers.ContentDisposition?.Name?.Trim('"') ?? string.Empty;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    names.Add(name);
                    // Only read string-valued parts, not the file stream.
                    if (part is StringContent)
                    {
                        values[name] = await part.ReadAsStringAsync(ct);
                    }
                }
            }
            return JsonResponse("""{"text":"ok"}""");
        });
        using var client = new HttpClient(handler);
        var service = new OpenAiSttService(client, settings);

        var ct = TestContext.Current.CancellationToken;
        var wav = MakeTinyWav();
        try
        {
            await service.TranscribeAsync(wav, cancellationToken: ct);
        }
        finally
        {
            File.Delete(wav);
        }
        return (names, values);
    }

    private static HttpResponseMessage JsonResponse(string body, string contentType = "application/json")
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        resp.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return resp;
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 2000)
    {
        var deadline = Environment.TickCount + timeoutMs;
        while (Environment.TickCount < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(10);
        }
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
