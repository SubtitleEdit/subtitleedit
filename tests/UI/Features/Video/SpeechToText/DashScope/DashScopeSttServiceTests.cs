using System.Net;
using System.Text;
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

    [Fact]
    public async Task TranscribeAsync_UploadUsesOssCompatibleMultipartSyntax()
    {
        var ct = TestContext.Current.CancellationToken;
        var audioBytes = new byte[] { 0, 1, 2, 13, 10, 255 };
        var audioPath = Path.GetTempFileName();
        var fileName = Path.GetFileName(audioPath);
        var requestCount = 0;
        using var handler = new StubHandler(async (request, token) =>
        {
            requestCount++;
            switch (requestCount)
            {
                case 1:
                    Assert.Equal(HttpMethod.Get, request.Method);
                    Assert.Contains("action=getPolicy&model=qwen3-asr-flash-filetrans", request.RequestUri!.Query);
                    Assert.Equal("Bearer test-key", request.Headers.Authorization!.ToString());
                    return JsonResponse("""
                        {"data":{"upload_host":"https://upload.example.com/","upload_dir":"test/dir",
                        "oss_access_key_id":"test-id","policy":"test-policy","signature":"test-signature",
                        "x_oss_object_acl":"private","x_oss_forbid_overwrite":"true"}}
                        """);
                case 2:
                    Assert.Equal(HttpMethod.Post, request.Method);
                    Assert.Equal("upload.example.com", request.RequestUri!.Host);
                    var form = Assert.IsType<MultipartFormDataContent>(request.Content);
                    var boundary = form.Headers.ContentType!.Parameters.Single(p => p.Name == "boundary").Value!;
                    Assert.DoesNotContain("\"", boundary);
                    var wire = Encoding.Latin1.GetString(await form.ReadAsByteArrayAsync(token));
                    var parts = wire.Split("--" + boundary, StringSplitOptions.None);
                    Assert.Equal(10, parts.Length); // Preamble, eight fields, closing boundary.
                    var fields = form.ToArray();
                    var expectedNames = new[] { "OSSAccessKeyId", "policy", "Signature", "key",
                        "x-oss-object-acl", "x-oss-forbid-overwrite", "success_action_status", "file" };
                    for (var i = 0; i < expectedNames.Length; i++)
                    {
                        Assert.StartsWith($"\r\nContent-Disposition: form-data; name=\"{expectedNames[i]}\"", parts[i + 1]);
                    }
                    var expectedValues = new[] { "test-id", "test-policy", "test-signature", $"test/dir/{fileName}",
                        "private", "true", "200" };
                    for (var i = 0; i < expectedValues.Length; i++)
                    {
                        Assert.Equal(expectedValues[i], await fields[i].ReadAsStringAsync(token));
                    }

                    Assert.Equal(audioBytes, await fields[^1].ReadAsByteArrayAsync(token));
                    Assert.Equal("application/octet-stream", fields[^1].Headers.ContentType!.MediaType);
                    Assert.Contains($"; filename=\"{fileName}\"\r\n", parts[^2]);
                    Assert.DoesNotContain("filename*", parts[^2]);
                    Assert.EndsWith("\r\n--" + boundary + "--\r\n", wire);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                case 3:
                    Assert.Equal("enable", Assert.Single(request.Headers.GetValues("X-DashScope-OssResourceResolve")));
                    using (var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token)))
                    {
                        Assert.Equal($"oss://test/dir/{fileName}", body.RootElement.GetProperty("input").GetProperty("file_url").GetString());
                    }
                    return JsonResponse("""{"output":{"task_id":"test-task"}}""");
                case 4:
                    return JsonResponse("""{"output":{"task_status":"SUCCEEDED","result":{"transcription_url":"https://result.example.com/result.json"}}}""");
                case 5:
                    return JsonResponse("""{"transcripts":[{"sentences":[{"begin_time":0,"end_time":1000,"text":"Hello."}]}]}""");
                default:
                    throw new InvalidOperationException("Unexpected request");
            }
        });
        using var client = new HttpClient(handler);
        var service = new DashScopeSttService(client, MakeSettings());
        try
        {
            await File.WriteAllBytesAsync(audioPath, audioBytes, ct);
            var result = await service.TranscribeAsync(audioPath, null, null, null, ct);
            Assert.Equal("Hello.", result.Text);
            Assert.Equal(5, requestCount);
        }
        finally
        {
            File.Delete(audioPath);
        }
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

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
