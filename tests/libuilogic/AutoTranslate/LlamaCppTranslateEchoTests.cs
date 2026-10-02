using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// TranslateGemma 12B answered merged en -> de requests with the English source, and
/// LlamaCppTranslate returned that as the German translation. An echo is now retried once
/// (warmer when the model pins a temperature); a second echo is kept, as a line can legitimately
/// read the same in both languages and failing it aborted the whole translation.
/// </summary>
public class LlamaCppTranslateEchoTests : IDisposable
{
    private const string Source = "Where were you last night?\nI was at the office until late.";
    private const string German = "Wo warst du letzte Nacht?\nIch war bis spät im Büro.";

    private readonly string _oldUrl;
    private readonly double _oldTemperature;
    private readonly string _oldModelPrompt;

    public LlamaCppTranslateEchoTests()
    {
        var tools = Configuration.Settings.Tools;
        _oldUrl = tools.LlamaCppApiUrl;
        _oldTemperature = tools.LlamaCppModelTemperature;
        _oldModelPrompt = tools.LlamaCppModelPrompt;
        tools.LlamaCppModelPrompt = string.Empty;
    }

    public void Dispose()
    {
        var tools = Configuration.Settings.Tools;
        tools.LlamaCppApiUrl = _oldUrl;
        tools.LlamaCppModelTemperature = _oldTemperature;
        tools.LlamaCppModelPrompt = _oldModelPrompt;
    }

    [Fact]
    public async Task EchoThenTranslation_RetriesOnceWithWarmerTemperature()
    {
        Configuration.Settings.Tools.LlamaCppModelTemperature = 0;

        var (result, requests, error) = await TranslateAgainstFakeServerAsync(Source, German);

        Assert.Equal(German.Replace("\n", Environment.NewLine), result);
        Assert.Equal(2, requests.Count);
        Assert.Contains("\"temperature\": 0", requests[0]);
        Assert.DoesNotContain("\"temperature\": 0.3", requests[0]);
        Assert.Contains("\"temperature\": 0.3", requests[1]);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public async Task EchoThenTranslation_WithoutModelTemperature_KeepsServerDefault()
    {
        Configuration.Settings.Tools.LlamaCppModelTemperature = -1;

        var (result, requests, _) = await TranslateAgainstFakeServerAsync(Source, German);

        Assert.Equal(German.Replace("\n", Environment.NewLine), result);
        Assert.Equal(2, requests.Count);
        Assert.DoesNotContain("temperature", requests[1]);
    }

    [Fact]
    public async Task EchoTwice_KeepsTheEchoInsteadOfFailing()
    {
        Configuration.Settings.Tools.LlamaCppModelTemperature = -1;

        var (result, requests, error) = await TranslateAgainstFakeServerAsync(Source, Source);

        Assert.Equal(Source.Replace("\n", Environment.NewLine), result);
        Assert.Equal(2, requests.Count);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public async Task MergedBlockMostlyEchoed_IsRetried()
    {
        Configuration.Settings.Tools.LlamaCppModelTemperature = -1;
        const string block = "Where were you last night?\nI was at the office until late.\nDon't lie to me, I called them.";
        const string mostlyEchoed = "Wo warst du letzte Nacht?\nI was at the office until late.\nDon't lie to me, I called them.";
        const string german = "Wo warst du letzte Nacht?\nIch war bis spät im Büro.\nLüg mich nicht an, ich habe sie angerufen.";

        var (result, requests, _) = await TranslateAgainstFakeServerAsync(block, german, firstReply: mostlyEchoed);

        Assert.Equal(german.Replace("\n", Environment.NewLine), result);
        Assert.Equal(2, requests.Count);
    }

    [Fact]
    public async Task VariantOfSameLanguage_IsNotRetried()
    {
        var (result, requests, _) = await TranslateAgainstFakeServerAsync(Source, "unused", firstReply: Source,
            sourceLanguage: "Spanish", targetLanguage: "Spanish (Latin America)");

        Assert.Equal(Source.Replace("\n", Environment.NewLine), result);
        Assert.Single(requests);
    }

    [Fact]
    public async Task TranslatedReply_IsNotRetried()
    {
        Configuration.Settings.Tools.LlamaCppModelTemperature = -1;

        var (result, requests, _) = await TranslateAgainstFakeServerAsync(Source, "unused", firstReply: German);

        Assert.Equal(German.Replace("\n", Environment.NewLine), result);
        Assert.Single(requests);
    }

    [Fact]
    public async Task ShortLineKeptAsIs_IsNotRetried()
    {
        var (result, requests, _) = await TranslateAgainstFakeServerAsync("Erik.", "unused", firstReply: "Erik.");

        Assert.Equal("Erik.", result);
        Assert.Single(requests);
    }

    /// <summary>
    /// Serves the first request with <paramref name="firstReply"/> (default: the source echoed)
    /// and any later one with <paramref name="laterReply"/>, collecting the request bodies.
    /// </summary>
    private static async Task<(string result, List<string> requests, string error)> TranslateAgainstFakeServerAsync(
        string text, string laterReply, string? firstReply = null, string sourceLanguage = "English", string targetLanguage = "German")
    {
        var requests = new List<string>();
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
                        context = await listener.GetContextAsync();
                    }
                    catch (Exception)
                    {
                        return; // listener closed
                    }

                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        requests.Add(await reader.ReadToEndAsync());
                    }

                    var reply = requests.Count == 1 ? firstReply ?? text : laterReply;
                    var json = "{\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":" +
                               System.Text.Json.JsonSerializer.Serialize(reply) + "}}]}";
                    var bytes = Encoding.UTF8.GetBytes(json);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
            });

            Configuration.Settings.Tools.LlamaCppApiUrl = url + "v1/chat/completions";
            using var translator = new LlamaCppTranslate();
            translator.Initialize();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await translator.Translate(text, sourceLanguage, targetLanguage, timeout.Token);
            stop.Cancel();
            listener.Stop();
            await serverTask;
            return (result, requests, translator.Error);
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
