using System.Net;
using System.Text;
using System.Text.Json;
using LibUiLogicTests.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// The client with <see cref="LlamaCppAdvancedClient.LlamaCppExtensions"/> off, for an endpoint
/// that is only known to speak OpenAI chat-completions (a hosted provider): the llama.cpp request
/// fields are left out, and a model that reasons gets room. The engine's max_tokens is sized for
/// the visible translation and reasoning tokens count against it, so such a model can spend the
/// whole budget on thoughts and end on the limit with no content at all (1036 of 1036 completion
/// tokens in a run against a hosted reasoning model). There is no thinking switch to send to such
/// an endpoint, so the reply is asked for again with a larger budget.
/// </summary>
public class LlamaCppAdvancedClientPlainOpenAiTests
{
    private const string TranslationJson = "{\"1\":\"Inspector: Where were you last night?\",\"2\":\"What time?\"}";

    /// <summary>Thoughts under "reasoning", no content, then the limit with the reasoning tokens in the usage.</summary>
    private const string StarvedSse =
        "data: {\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\",\"reasoning\":\"We need to translate line 1...\"},\"finish_reason\":null}]}\n\n" +
        "data: {\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning\":\" The speaker is the inspector...\"},\"finish_reason\":null}]}\n\n" +
        "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"length\"}],\"usage\":{\"prompt_tokens\":474,\"completion_tokens\":562,\"completion_tokens_details\":{\"reasoning_tokens\":562}}}\n\n" +
        "data: [DONE]\n\n";

    /// <summary>A model that runs away inside the JSON grammar also ends on the limit, without any reasoning.</summary>
    private const string RunawaySse =
        "data: {\"choices\":[{\"index\":0,\"delta\":{\"content\":\"{\\\"1\\\":\\\"Hi\\\" \\t\\t\\t\\t\"},\"finish_reason\":null}]}\n\n" +
        "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"length\"}],\"usage\":{\"completion_tokens\":562,\"completion_tokens_details\":{\"reasoning_tokens\":0}}}\n\n" +
        "data: [DONE]\n\n";

    private static readonly string GoodSse =
        "data: " + JsonSerializer.Serialize(new { choices = new[] { new { index = 0, delta = new { content = TranslationJson }, finish_reason = "stop" } } }) + "\n\n" +
        "data: [DONE]\n\n";

    [Fact]
    public void RequestLeavesOutTheLlamaCppFields()
    {
        var json = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced(), "system", "user", null, "some/model", 100, disableThinking: true, llamaCppExtensions: false);

        using var doc = JsonDocument.Parse(json);
        Assert.False(doc.RootElement.TryGetProperty("cache_prompt", out _));
        Assert.False(doc.RootElement.TryGetProperty("chat_template_kwargs", out _));
        Assert.Equal("some/model", doc.RootElement.GetProperty("model").GetString());
        Assert.True(doc.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal(100, doc.RootElement.GetProperty("max_tokens").GetInt32());
    }

    [Fact]
    public void RequestKeepsTheLlamaCppFieldsByDefault()
    {
        var json = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced(), "system", "user", null, null, 100, disableThinking: true);

        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("cache_prompt").GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("chat_template_kwargs", out _));
    }

    /// <summary>
    /// A local llama.cpp run with a curated model leaves that model's sampling values in the
    /// settings for the rest of the session. Without the extensions they are left out - they are
    /// for another model, and a strict server refuses top_k and repeat_penalty - while the user's
    /// own values are still sent. The local engines keep the curated values.
    /// </summary>
    [Fact]
    public void CuratedModelSamplingGoesToTheLocalEnginesOnly()
    {
        var tools = Configuration.Settings.Tools;
        var saved = (tools.LlamaCppModelTemperature, tools.LlamaCppModelTopP, tools.LlamaCppModelTopK, tools.LlamaCppModelRepeatPenalty);
        string plain, plainWithUserTopK, local;
        try
        {
            (tools.LlamaCppModelTemperature, tools.LlamaCppModelTopP, tools.LlamaCppModelTopK, tools.LlamaCppModelRepeatPenalty) = (0.7, 0.6, 20, 1.05);
            plain = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced(), "system", "user", null, "some/model", 100, llamaCppExtensions: false);
            plainWithUserTopK = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced { TopK = 40 }, "system", "user", null, "some/model", 100, llamaCppExtensions: false);
            local = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced(), "system", "user", null, null, 100);
        }
        finally
        {
            (tools.LlamaCppModelTemperature, tools.LlamaCppModelTopP, tools.LlamaCppModelTopK, tools.LlamaCppModelRepeatPenalty) = saved;
        }

        using var plainDoc = JsonDocument.Parse(plain);
        Assert.Equal(0.2, plainDoc.RootElement.GetProperty("temperature").GetDouble());
        foreach (var name in new[] { "top_p", "top_k", "repeat_penalty" })
        {
            Assert.False(plainDoc.RootElement.TryGetProperty(name, out _), name);
        }

        using var userDoc = JsonDocument.Parse(plainWithUserTopK);
        Assert.Equal(40, userDoc.RootElement.GetProperty("top_k").GetInt32());

        using var localDoc = JsonDocument.Parse(local);
        Assert.Equal(0.7, localDoc.RootElement.GetProperty("temperature").GetDouble());
        Assert.Equal(0.6, localDoc.RootElement.GetProperty("top_p").GetDouble());
        Assert.Equal(20, localDoc.RootElement.GetProperty("top_k").GetInt32());
        Assert.Equal(1.05, localDoc.RootElement.GetProperty("repeat_penalty").GetDouble());
    }

    [Theory]
    // thoughts, under each of the names servers use
    [InlineData("{\"choices\":[{\"delta\":{\"content\":\"\",\"reasoning\":\"We need to...\"},\"finish_reason\":null}]}", false, true)]
    [InlineData("{\"choices\":[{\"delta\":{\"reasoning_content\":\"We need to...\"},\"finish_reason\":null}]}", false, true)]
    [InlineData("{\"choices\":[{\"delta\":{\"content\":\"\",\"reasoning_details\":[{\"type\":\"reasoning.text\",\"text\":\"We\"}]},\"finish_reason\":null}]}", false, true)]
    // the end of a starved reply: the limit, and reasoning tokens in the usage
    [InlineData("{\"choices\":[{\"delta\":{},\"finish_reason\":\"length\"}],\"usage\":{\"completion_tokens\":562,\"completion_tokens_details\":{\"reasoning_tokens\":562}}}", true, true)]
    // the limit without reasoning, an ordinary end, ordinary content, empty reasoning fields
    [InlineData("{\"choices\":[{\"delta\":{},\"finish_reason\":\"length\"}],\"usage\":{\"completion_tokens\":562,\"completion_tokens_details\":{\"reasoning_tokens\":0}}}", true, false)]
    [InlineData("{\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":{\"completion_tokens\":90}}", false, false)]
    [InlineData("{\"choices\":[{\"delta\":{\"content\":\"{\\\"1\\\":\"},\"finish_reason\":null}]}", false, false)]
    [InlineData("{\"choices\":[{\"delta\":{\"content\":\"Hi\",\"reasoning\":\"\",\"reasoning_details\":[]},\"finish_reason\":null}]}", false, false)]
    [InlineData("{\"choices\":[],\"usage\":{\"total_tokens\":9}}", false, false)]
    public void ChunkTellsWhetherTheReplyHitTheLimitAndWhetherTheModelReasoned(string chunk, bool expectedHitTokenLimit, bool expectedReasoned)
    {
        Assert.True(LlamaCppAdvancedClient.TryAppendDelta(chunk, new StringBuilder(), new StringBuilder(), out _, out var hitTokenLimit, out var reasoned));

        Assert.Equal(expectedHitTokenLimit, hitTokenLimit);
        Assert.Equal(expectedReasoned, reasoned);
    }

    /// <summary>Thoughts under "reasoning" are a sign only: they are never handed back as the reply.</summary>
    [Fact]
    public void ReasoningFieldIsNotCollectedAsText()
    {
        var content = new StringBuilder();
        var reasoning = new StringBuilder();

        LlamaCppAdvancedClient.TryAppendDelta("{\"choices\":[{\"delta\":{\"content\":\"\",\"reasoning\":\"We need to...\"}}]}", content, reasoning, out _, out _, out _);

        Assert.Equal(0, content.Length);
        Assert.Equal(0, reasoning.Length);
    }

    [Theory]
    [InlineData(562, false, 562)]
    [InlineData(562, true, 2248)] // four times
    [InlineData(300, true, 2048)] // and never below 2048: the thoughts do not shrink with the batch
    [InlineData(-1, true, -1)] // no budget to raise
    public void ReplyBudgetGrowsOnlyOnceTheModelNeedsRoom(int defaultMaxTokens, bool reasoningNeedsRoom, int expected)
    {
        Assert.Equal(expected, LlamaCppAdvancedClient.ReplyBudget(defaultMaxTokens, reasoningNeedsRoom));
    }

    [Fact]
    public async Task StarvedReplyIsAskedForAgainWithALargerBudgetThatIsKept()
    {
        using var server = new FakeServer(StarvedSse, GoodSse, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced()) { LlamaCppExtensions = false };

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.Equal(TranslationJson, reply);
        Assert.False(client.ReplyRanOutWhileReasoning);
        Assert.False(client.DisableThinking); // thoughts under "reasoning" do not flip the llama.cpp switch
        Assert.Equal(new[] { 562, 2248 }, server.MaxTokens);

        // The next batch does not pay for a starved request again.
        Assert.Equal(TranslationJson, await server.ChatAsync(client, defaultMaxTokens: 400));
        Assert.Equal(new[] { 562, 2248, 2048 }, server.MaxTokens);
    }

    /// <summary>
    /// A local server has the thinking switch, which costs nothing - the engines for llama.cpp,
    /// Ollama and LM Studio keep what they did: one request, no larger budget.
    /// </summary>
    [Fact]
    public async Task WithTheLlamaCppExtensionsTheReplyIsNotAskedForAgain()
    {
        using var server = new FakeServer(StarvedSse, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced());

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.Equal(string.Empty, reply);
        Assert.True(client.ReplyRanOutWhileReasoning);
        Assert.Equal(new[] { 562 }, server.MaxTokens);
    }

    /// <summary>A cap the user set is the user's: it is not raised, and the same request is not sent twice.</summary>
    [Theory]
    [InlineData(300, "")]
    [InlineData(-1, "{\"max_tokens\": 300}")]
    public async Task UserSetTokenCapIsNotRaised(int maxTokensSetting, string requestJson)
    {
        using var server = new FakeServer(StarvedSse, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced { MaxTokens = maxTokensSetting, RequestJson = requestJson }) { LlamaCppExtensions = false };

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.Equal(string.Empty, reply);
        Assert.True(client.ReplyRanOutWhileReasoning);
        Assert.Equal(new[] { 300 }, server.MaxTokens);
    }

    [Fact]
    public async Task ReplyThatStillRunsOutIsReportedAsSuch()
    {
        using var server = new FakeServer(StarvedSse, StarvedSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced()) { LlamaCppExtensions = false };

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.Equal(string.Empty, reply);
        Assert.True(client.ReplyRanOutWhileReasoning);
        Assert.Equal(new[] { 562, 2248 }, server.MaxTokens);

        var description = AdvancedTranslatorBase.DescribeUnusableReply(reply, client.ReplyFromReasoning, client.LastRequestDisabledThinking, client.ReplyRanOutWhileReasoning);
        Assert.Contains("ran out of tokens, which the model spent on reasoning", description);
        Assert.Contains("extra request JSON", description);
    }

    /// <summary>
    /// The larger budget can be more than the model may write, and a server answers a max_tokens
    /// above that cap with a 400. The first reply then stands, with no response-format fallbacks
    /// on the refusal, and the next request is back on the usual budget - with the larger one
    /// kept, every later request of the file would be refused as well.
    /// </summary>
    [Fact]
    public async Task LargerBudgetTheServerRefusesIsDroppedAndTheFirstReplyStands()
    {
        const string refusal = "{\"error\":{\"message\":\"max_tokens is too large: 2248. This model supports at most 2000 completion tokens.\",\"type\":\"invalid_request_error\"}}";
        using var server = new FakeServer(StarvedSse, refusal, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced()) { LlamaCppExtensions = false };

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562, responseFormatJson: "{\"type\":\"json_object\"}");

        Assert.Equal(string.Empty, reply);
        Assert.True(client.ReplyRanOutWhileReasoning);
        Assert.Equal(new[] { 562, 2248 }, server.MaxTokens);

        Assert.Equal(TranslationJson, await server.ChatAsync(client, defaultMaxTokens: 562));
        Assert.Equal(new[] { 562, 2248, 562 }, server.MaxTokens);
    }

    /// <summary>
    /// An answer under "reasoning_content" only, with the extensions off: no "enable_thinking" is
    /// sent to such an endpoint, so the advice names the provider's field and not that switch.
    /// </summary>
    [Fact]
    public void ReasoningOnlyReplyWithoutAThinkingSwitchPointsAtTheProvidersField()
    {
        var description = AdvancedTranslatorBase.DescribeUnusableReply("thoughts only", true, requestDisabledThinking: false, hasThinkingSwitch: false);

        Assert.Contains("\"reasoning_content\" only", description);
        Assert.Contains("extra request JSON", description);
        Assert.DoesNotContain("enable_thinking", description);
        Assert.DoesNotContain("--reasoning off", description);
        Assert.EndsWith(": thoughts only", description);
    }

    /// <summary>More tokens would only make a runaway longer (#13830): without a sign of reasoning the limit is left to the batch retry.</summary>
    [Fact]
    public async Task RunawayWithoutReasoningIsNotAskedForAgain()
    {
        using var server = new FakeServer(RunawaySse, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced()) { LlamaCppExtensions = false };

        await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.False(client.ReplyRanOutWhileReasoning);
        Assert.Equal(new[] { 562 }, server.MaxTokens);
    }

    /// <summary>
    /// A cut-off answer after reasoning is the same shortage: part of the budget went to thoughts.
    /// </summary>
    [Fact]
    public async Task ReplyCutOffAfterReasoningIsAskedForAgainToo()
    {
        const string cutOffSse =
            "data: {\"choices\":[{\"delta\":{\"content\":\"\",\"reasoning\":\"We need to...\"},\"finish_reason\":null}]}\n\n" +
            "data: {\"choices\":[{\"delta\":{\"content\":\"{\\\"1\\\":\\\"Inspector: Whe\"},\"finish_reason\":null}]}\n\n" +
            "data: {\"choices\":[{\"delta\":{},\"finish_reason\":\"length\"}],\"usage\":{\"completion_tokens\":562,\"completion_tokens_details\":{\"reasoning_tokens\":540}}}\n\n" +
            "data: [DONE]\n\n";
        using var server = new FakeServer(cutOffSse, GoodSse);
        using var client = new LlamaCppAdvancedClient(new SeLlamaCppAdvanced()) { LlamaCppExtensions = false };

        var reply = await server.ChatAsync(client, defaultMaxTokens: 562);

        Assert.Equal(TranslationJson, reply);
        Assert.Equal(new[] { 562, 2248 }, server.MaxTokens);
    }

    /// <summary>
    /// Answers the requests it gets with the given SSE bodies, in order, and keeps each request's
    /// max_tokens. A body that is not SSE is an error body and goes out with a 400.
    /// </summary>
    private sealed class FakeServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _url;
        private readonly List<int> _maxTokens = new();

        public FakeServer(params string[] sseReplies)
        {
            (_listener, _url) = LlamaCppTranslateEchoTests.StartListener();
            _ = Task.Run(async () =>
            {
                foreach (var sse in sseReplies)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await _listener.GetContextAsync();
                    }
                    catch
                    {
                        return; // disposed with replies left over
                    }

                    using (var reader = new StreamReader(context.Request.InputStream))
                    using (var request = JsonDocument.Parse(await reader.ReadToEndAsync()))
                    {
                        lock (_maxTokens)
                        {
                            _maxTokens.Add(request.RootElement.TryGetProperty("max_tokens", out var maxTokens) ? maxTokens.GetInt32() : -1);
                        }
                    }

                    var bytes = Encoding.UTF8.GetBytes(sse);
                    context.Response.StatusCode = sse.StartsWith("data:", StringComparison.Ordinal) ? 200 : 400;
                    context.Response.ContentType = "text/event-stream";
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
            });
        }

        public int[] MaxTokens
        {
            get
            {
                lock (_maxTokens)
                {
                    return _maxTokens.ToArray();
                }
            }
        }

        public async Task<string> ChatAsync(LlamaCppAdvancedClient client, int defaultMaxTokens, string? responseFormatJson = null)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            return await client.ChatAsync(_url + "v1/chat/completions", "system", "{\"history\":[],\"lines\":[]}", responseFormatJson, timeout.Token, "some/model", defaultMaxTokens);
        }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
