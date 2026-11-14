using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// A runaway length test that assumes one character of translation per character of source fails
/// correct Chinese-to-English translations. Once a line is on its own there is nothing to measure
/// the expansion against, and the line is accepted instead of failing the file; leaked JSON and
/// the prompt's example still fail (#16885).
/// </summary>
public class AdvancedTranslatorLengthOnlyTests
{
    private const string ChineseSource = "把他带回来 这次我要亲自问他昨天晚上的每一分钟";
    private const string EnglishTranslation = "Bring him back. This time, I'll personally question him about every single minute of last night.";

    [Fact]
    public void LongCorrectTranslation_TripsOnlyAnAssumedLengthTest()
    {
        // One character per character, the old assumption: far too long. The 3.3 a Chinese batch
        // measures: a normal line. Alone, with nothing to measure against: not checked for length.
        Assert.True(LlamaCppAdvancedProtocol.IsBrokenValue(ChineseSource, EnglishTranslation, expansion: 1));
        Assert.False(LlamaCppAdvancedProtocol.IsBrokenValue(ChineseSource, EnglishTranslation, expansion: 3.3));
        Assert.False(LlamaCppAdvancedProtocol.IsMalformedValue(ChineseSource, EnglishTranslation));
    }

    [Theory]
    [InlineData("Come on up.", "Komm hoch.\n2\":\"Was gibt's?")]
    [InlineData("Come on up.", "Komm hoch.\"}]}")]
    [InlineData("Come on up.", "<translation of line 1>")]
    public void LeakOrExample_IsStillMalformed(string source, string value)
    {
        Assert.True(LlamaCppAdvancedProtocol.IsMalformedValue(source, value));
    }

    [Fact]
    public async Task LoneLine_LongCorrectTranslation_IsAcceptedWithoutRetry()
    {
        var rows = new ObservableCollection<TranslateRow> { new() { Text = ChineseSource } };

        var (translated, requests) = await TranslateWithReplyAsync(rows, "{\"1\":" + System.Text.Json.JsonSerializer.Serialize(EnglishTranslation) + "}");

        Assert.Equal(1, translated);
        Assert.Equal(1, requests);
        Assert.Equal(EnglishTranslation, rows[0].TranslatedText);
    }

    [Fact]
    public async Task LoneLine_LeakedJson_StillFails()
    {
        var rows = new ObservableCollection<TranslateRow> { new() { Text = "Come on up." } };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            TranslateWithReplyAsync(rows, "{\"1\":\"Komm hoch.\\n2\\\":\\\"Was gibt's?\"}"));
        Assert.True(string.IsNullOrEmpty(rows[0].TranslatedText));
    }

    private static async Task<(int Translated, int Requests)> TranslateWithReplyAsync(ObservableCollection<TranslateRow> rows, string modelReply)
    {
        var requests = 0;
        var (listener, url) = StartListener();
        using (listener)
        {
            using var stop = new CancellationTokenSource();
            var serverTask = Task.Run(async () =>
            {
                while (!stop.IsCancellationRequested)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await listener.GetContextAsync().WaitAsync(stop.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    Interlocked.Increment(ref requests);
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        await reader.ReadToEndAsync();
                    }

                    var chunk = "{\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"finish_reason\":\"stop\"," +
                                "\"delta\":{\"role\":\"assistant\",\"content\":" + System.Text.Json.JsonSerializer.Serialize(modelReply) + "}}]}";
                    var bytes = Encoding.UTF8.GetBytes("data: " + chunk + "\n\ndata: [DONE]\n\n");
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/event-stream";
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
            });

            try
            {
                using var translator = new FakeServerTranslator(url + "v1/chat/completions");
                translator.Settings = new SeLlamaCppAdvanced { BatchSize = 1, HistoryPairs = 0 };
                translator.Initialize();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var translated = await translator.TranslateBatchAsync(rows, 0, "Chinese", "English", null, timeout.Token);
                return (translated, requests);
            }
            finally
            {
                stop.Cancel();
                await serverTask;
            }
        }
    }

    private sealed class FakeServerTranslator(string apiUrl) : AdvancedTranslatorBase
    {
        public override string Name => "Fake";
        public override string Url => string.Empty;
        protected override string GetApiUrl() => apiUrl;
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
