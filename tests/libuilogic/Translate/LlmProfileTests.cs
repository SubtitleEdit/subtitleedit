using System.Net;
using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;

namespace LibUiLogicTests.Translate;

public class LlmProfileTests
{
    [Fact]
    public void CopySeparatesConnectionAndPolicy()
    {
        var original = new TranslationProfile { ApiKey = "account-a" };
        original.Policy.Glossary = "Alice = 艾丽丝";
        original.Parameters["key"] = "one";
        var copy = original.Copy("Other account");
        copy.ApiKey = "account-b";
        copy.Policy.Glossary = "different";
        copy.Parameters["key"] = "two";
        Assert.NotEqual(original.Id, copy.Id);
        Assert.Equal("account-a", original.ApiKey);
        Assert.Equal("Alice = 艾丽丝", original.Policy.Glossary);
        Assert.Equal("one", original.Parameters["key"]);
    }

    [Fact]
    public async Task OpenAiAuthenticatesWithoutLocalServerFields()
    {
        var handler = new RecordingHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"你好\"}}]}") });
        using var client = new OpenAiLlmClient(new() { TopK = 10, RepeatPenalty = 1.2 }, "secret", false, new HttpClient(handler));
        Assert.Equal("你好", await client.ChatAsync("https://example.test/custom/chat", "system", "hello", null, TestContext.Current.CancellationToken, "model"));
        Assert.Equal("Bearer secret", handler.Authorization);
        Assert.DoesNotContain("cache_prompt", handler.Bodies[0]);
        Assert.DoesNotContain("repeat_penalty", handler.Bodies[0]);
        Assert.DoesNotContain("top_k", handler.Bodies[0]);
    }

    [Fact]
    public async Task AuthenticationFailureDoesNotTryWeakerFormats()
    {
        var handler = new RecordingHandler(_ => new(HttpStatusCode.Unauthorized) { Content = new StringContent("invalid API key") });
        using var client = new OpenAiLlmClient(new(), "bad", false, new HttpClient(handler));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.ChatAsync("https://example.test/chat", "system", "hello", "{}", TestContext.Current.CancellationToken));
        Assert.Single(handler.Bodies);
    }

    [Fact]
    public async Task ExplicitFormatRejectionFallsBackToJsonObject()
    {
        var handler = new RecordingHandler(n => n == 1
            ? new(HttpStatusCode.BadRequest) { Content = new StringContent("response_format json_schema unsupported") }
            : new(HttpStatusCode.OK) { Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"{}\"}}]}") });
        using var client = new OpenAiLlmClient(new(), "", false, new HttpClient(handler));
        await client.ChatAsync("https://example.test/chat", "system", "hello", "{\"type\":\"json_schema\"}", TestContext.Current.CancellationToken);
        Assert.Equal(2, handler.Bodies.Count);
        Assert.Contains("json_object", handler.Bodies[1]);
    }

    [Fact]
    public async Task AnthropicUsesMessagesAndOnlyTextBlocks()
    {
        var handler = new RecordingHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent("{\"content\":[{\"type\":\"thinking\",\"thinking\":\"private\"},{\"type\":\"text\",\"text\":\"你好\"}],\"stop_reason\":\"end_turn\"}") });
        using var client = new AnthropicLlmClient(new(), "anthropic-secret", new HttpClient(handler));
        Assert.Equal("你好", await client.ChatAsync("https://example.test/messages", "system", "hello", null, TestContext.Current.CancellationToken, "claude"));
        Assert.Equal("anthropic-secret", handler.ApiKey);
        using var json = JsonDocument.Parse(handler.Bodies[0]);
        Assert.Equal("system", json.RootElement.GetProperty("system").GetString());
        Assert.True(json.RootElement.GetProperty("max_tokens").GetInt32() > 0);
    }

    private sealed class RecordingHandler(Func<int, HttpResponseMessage> response) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = new();
        public string? Authorization { get; private set; }
        public string? ApiKey { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            ApiKey = request.Headers.TryGetValues("x-api-key", out var keys) ? keys.Single() : null;
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return response(Bodies.Count);
        }
    }
}
