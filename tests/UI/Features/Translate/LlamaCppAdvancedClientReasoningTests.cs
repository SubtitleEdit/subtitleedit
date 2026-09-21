using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Xunit;

namespace UITests.Features.Translate;

/// <summary>
/// KoboldCpp streams the whole reply as "reasoning_content" when the chat template opens a think
/// block (Qwen 3.5+ with Jinja): the JSON grammar never lets the model emit the closing tag, so
/// "content" - the only field the client read - stayed empty and every batch failed with a bare
/// "No usable translation" (#15009). The client now falls back to the reasoning text when
/// content is blank, and an unusable reply says why.
/// </summary>
public class LlamaCppAdvancedClientReasoningTests
{
    private const string TranslationJson = "{\"1\":\"Tobruk è sotto\\nun bombardamento continuo.\",\"2\":\"L'hai deciso tu?\"}";

    private static string Chunk(string field, string text)
    {
        return "{\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"finish_reason\":null," +
               "\"delta\":{\"role\":\"assistant\",\"" + field + "\":" + System.Text.Json.JsonSerializer.Serialize(text) + "}}]}";
    }

    [Fact]
    public void DeltaContentAndReasoningAreCollectedSeparately()
    {
        var content = new StringBuilder();
        var reasoning = new StringBuilder();

        Assert.True(LlamaCppAdvancedClient.TryAppendDelta(Chunk("reasoning_content", "Let me think. "), content, reasoning, out _));
        Assert.True(LlamaCppAdvancedClient.TryAppendDelta(Chunk("content", "{\"1\":"), content, reasoning, out _));
        Assert.True(LlamaCppAdvancedClient.TryAppendDelta(Chunk("content", "\"Hej\"}"), content, reasoning, out _));

        Assert.Equal("{\"1\":\"Hej\"}", content.ToString());
        Assert.Equal("Let me think. ", reasoning.ToString());
    }

    [Fact]
    public void UsageChunkWithoutChoicesIsIgnored()
    {
        var content = new StringBuilder();
        var reasoning = new StringBuilder();

        Assert.True(LlamaCppAdvancedClient.TryAppendDelta("{\"object\":\"chat.completion.chunk\",\"choices\":[],\"usage\":{\"total_tokens\":9}}", content, reasoning, out _));

        Assert.Equal(0, content.Length);
        Assert.Equal(0, reasoning.Length);
    }

    [Fact]
    public void InStreamErrorStillFailsTheRequest()
    {
        var ok = LlamaCppAdvancedClient.TryAppendDelta("{\"error\":{\"message\":\"context full\"}}", new StringBuilder(), new StringBuilder(), out var error);

        Assert.False(ok);
        Assert.Contains("context full", error);
    }

    [Fact]
    public void ContentWinsOverReasoning()
    {
        var reply = LlamaCppAdvancedClient.PickReply("{\"1\":\"Hej\"}", "The user wants Danish.", out var fromReasoning);

        Assert.Equal("{\"1\":\"Hej\"}", reply);
        Assert.False(fromReasoning);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n\n")]
    public void BlankContentFallsBackToReasoning(string content)
    {
        var reply = LlamaCppAdvancedClient.PickReply(content, TranslationJson, out var fromReasoning);

        Assert.Equal(TranslationJson, reply);
        Assert.True(fromReasoning);
    }

    [Fact]
    public void NothingAtAllIsNotFlaggedAsReasoning()
    {
        var reply = LlamaCppAdvancedClient.PickReply(string.Empty, string.Empty, out var fromReasoning);

        Assert.Equal(string.Empty, reply);
        Assert.False(fromReasoning);
    }

    [Fact]
    public void NonStreamedReplyFallsBackToMessageReasoning()
    {
        var body = "{\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"\",\"reasoning_content\":" +
                   System.Text.Json.JsonSerializer.Serialize(TranslationJson) + "}}]}";

        var reply = LlamaCppAdvancedClient.ExtractContent(body, out var fromReasoning);

        Assert.Equal(TranslationJson, reply);
        Assert.True(fromReasoning);
    }

    [Fact]
    public void NonStreamedReplyWithNullContentFallsBackToMessageReasoning()
    {
        var body = "{\"choices\":[{\"message\":{\"content\":null,\"reasoning_content\":\"{\\\"1\\\":\\\"Hej\\\"}\"}}]}";

        var reply = LlamaCppAdvancedClient.ExtractContent(body, out var fromReasoning);

        Assert.Equal("{\"1\":\"Hej\"}", reply);
        Assert.True(fromReasoning);
    }

    [Fact]
    public void NonStreamedReplyPrefersContent()
    {
        var body = "{\"choices\":[{\"message\":{\"content\":\"{\\\"1\\\":\\\"Hej\\\"}\",\"reasoning_content\":\"thoughts\"}}]}";

        var reply = LlamaCppAdvancedClient.ExtractContent(body, out var fromReasoning);

        Assert.Equal("{\"1\":\"Hej\"}", reply);
        Assert.False(fromReasoning);
    }

    [Fact]
    public void ReasoningOnlyReplyParsesIntoTranslations()
    {
        var reply = LlamaCppAdvancedClient.PickReply(string.Empty, TranslationJson, out _);

        var map = LlamaCppAdvancedProtocol.ParseTranslations(reply);

        Assert.Equal(2, map.Count);
        Assert.Equal("Tobruk è sotto" + Environment.NewLine + "un bombardamento continuo.", map[1]);
        Assert.Equal("L'hai deciso tu?", map[2]);
    }

    [Fact]
    public void RealThoughtsWithoutJsonStillYieldNoTranslations()
    {
        var reply = LlamaCppAdvancedClient.PickReply(string.Empty, "The user wants Italian. Line 1 is about Tobruk", out _);

        Assert.Empty(LlamaCppAdvancedProtocol.ParseTranslations(reply));
    }

    [Fact]
    public void EmptyReplyIsDescribed()
    {
        var description = AdvancedTranslatorBase.DescribeUnusableReply(string.Empty, false);

        Assert.Contains("empty reply", description);
    }

    [Fact]
    public void ReasoningOnlyReplyIsDescribedWithTheReply()
    {
        var description = AdvancedTranslatorBase.DescribeUnusableReply("thoughts only", true);

        Assert.Contains("reasoning_content", description);
        Assert.Contains("thinking", description);
        Assert.EndsWith("thoughts only", description);
    }

    [Fact]
    public void OrdinaryUnusableReplyIsPassedThrough()
    {
        Assert.Equal("{\"1\":\"Hej\"}", AdvancedTranslatorBase.DescribeUnusableReply("{\"1\":\"Hej\"}", false));
    }

    /// <summary>The #15009 stream end to end: every token arrives as reasoning_content.</summary>
    [Fact]
    public async Task KoboldCppStyleReasoningOnlyStreamIsAssembled()
    {
        var sse = new StringBuilder();
        for (var i = 0; i < TranslationJson.Length; i += 7)
        {
            sse.Append("data: ").Append(Chunk("reasoning_content", TranslationJson.Substring(i, Math.Min(7, TranslationJson.Length - i)))).Append("\n\n");
        }

        sse.Append("data: {\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"finish_reason\":\"stop\",\"delta\":{}}]}\n\n");
        sse.Append("data: [DONE]\n\n");

        using var client = new LlamaCppAdvancedClient();
        var reply = await ChatAgainstFakeServerAsync(client, sse.ToString());

        Assert.Equal(TranslationJson, reply);
        Assert.True(client.ReplyFromReasoning);
    }

    [Fact]
    public async Task OrdinaryContentStreamIsNotFlaggedAsReasoning()
    {
        var sse = "data: " + Chunk("content", TranslationJson) + "\n\ndata: [DONE]\n\n";

        using var client = new LlamaCppAdvancedClient();
        var reply = await ChatAgainstFakeServerAsync(client, sse);

        Assert.Equal(TranslationJson, reply);
        Assert.False(client.ReplyFromReasoning);
    }

    private static async Task<string> ChatAgainstFakeServerAsync(LlamaCppAdvancedClient client, string sseBody)
    {
        var (listener, url) = StartListener();
        using (listener)
        {
            var serverTask = Task.Run(async () =>
            {
                var context = await listener.GetContextAsync();
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    await reader.ReadToEndAsync();
                }

                var bytes = Encoding.UTF8.GetBytes(sseBody);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/event-stream";
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            });

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var reply = await client.ChatAsync(url + "v1/chat/completions", "system", "{\"history\":[],\"lines\":[]}", null, timeout.Token);
            await serverTask;
            return reply;
        }
    }

    private static (HttpListener listener, string url) StartListener()
    {
        for (var attempt = 0; ; attempt++)
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            var port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();

            var url = "http://127.0.0.1:" + port + "/";
            var listener = new HttpListener();
            listener.Prefixes.Add(url);
            try
            {
                listener.Start();
                return (listener, url);
            }
            catch (HttpListenerException) when (attempt < 5)
            {
                listener.Close(); // port taken between probe and start - try another
            }
        }
    }
}
