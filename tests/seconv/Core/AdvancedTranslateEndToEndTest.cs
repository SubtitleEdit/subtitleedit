using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// The advanced engines through the whole seconv pipeline, against a fake OpenAI-compatible
/// chat-completions server that answers the batch protocol: every line comes back tagged with
/// the target language, the batches carry the rolling history, and the system prompt carries the
/// synopsis, glossary and formality given on the command line.
/// </summary>
public class AdvancedTranslateEndToEndTest : IDisposable
{
    private const string SrtContent =
        "1\n00:00:01,000 --> 00:00:02,000\nWhere were you last night?\n\n" +
        "2\n00:00:03,000 --> 00:00:04,000\nI was at the office until late.\n\n" +
        "3\n00:00:05,000 --> 00:00:06,000\nThe Chief wants to see you.\n\n" +
        "4\n00:00:07,000 --> 00:00:08,000\nDid he say why?\n\n" +
        "5\n00:00:09,000 --> 00:00:10,000\nNo, but he did not sound happy.\n\n" +
        "6\n00:00:11,000 --> 00:00:12,000\nFine, I will be there at eight.\n\n" +
        "7\n00:00:13,000 --> 00:00:14,000\nDo not be late this time.\n\n" +
        "8\n00:00:15,000 --> 00:00:16,000\nI will not.\n";

    private readonly string _tempRoot;
    private readonly HttpListener _listener = new();
    private readonly string _url;
    private readonly List<(string System, int HistoryCount, int LineCount)> _requests = new();

    public AdvancedTranslateEndToEndTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "AdvancedTranslate_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);

        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        _url = $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(_url);
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    /// <summary>
    /// Answers like llama-server/LM Studio: a streamed chat completion whose content is the JSON
    /// object the protocol asks for, mapping every requested line number to "[de] " + its text.
    /// </summary>
    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch
            {
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            using var request = JsonDocument.Parse(await reader.ReadToEndAsync());
            var messages = request.RootElement.GetProperty("messages");
            var system = messages[0].GetProperty("content").GetString() ?? string.Empty;
            using var user = JsonDocument.Parse(messages[1].GetProperty("content").GetString() ?? "{}");
            var history = user.RootElement.GetProperty("history");
            var lines = user.RootElement.GetProperty("lines");
            lock (_requests)
            {
                _requests.Add((system, history.GetArrayLength(), lines.GetArrayLength()));
            }

            var translations = new Dictionary<string, string>();
            foreach (var line in lines.EnumerateArray())
            {
                translations[line.GetProperty("n").GetInt32().ToString()] = "[de] " + line.GetProperty("text").GetString();
            }

            var chunk = JsonSerializer.Serialize(new { choices = new[] { new { delta = new { content = JsonSerializer.Serialize(translations) } } } });
            var body = Encoding.UTF8.GetBytes("data: " + chunk + "\n\ndata: [DONE]\n\n");
            context.Response.ContentType = "text/event-stream";
            await context.Response.OutputStream.WriteAsync(body);
            context.Response.Close();
        }
    }

    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public async Task LmStudioAdvanced_TranslatesInBatchesWithContext()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var glossary = Path.Combine(_tempRoot, "glossary.txt");
        await File.WriteAllTextAsync(glossary, "the Chief = der Chef\n", TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = _tempRoot,
            Overwrite = true,
            TranslateTo = "de",
            TranslateFrom = "en",
            TranslateEngine = "lmstudio-advanced",
            TranslateUrl = _url, // base only; completed to /v1/chat/completions
            TranslateBatchSize = 3,
            TranslateHistory = 2,
            TranslateSynopsis = "A Scandinavian crime drama.",
            TranslateGlossary = glossary,
            TranslateFormality = "informal",
            Quiet = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var output = await File.ReadAllTextAsync(Path.Combine(_tempRoot, "in.de.srt"), TestContext.Current.CancellationToken);
        Assert.Contains("[de] Where were you last night?", output);
        Assert.Contains("[de] I will not.", output);
        Assert.DoesNotContain("\nWhere were you last night?", output); // every line translated

        // 8 lines in batches of 3: 3 + 3 + 2, each later batch with the 2 previous pairs as
        // history (a batch of one line would go context-free by design, hence 8 lines).
        List<(string System, int HistoryCount, int LineCount)> requests;
        lock (_requests)
        {
            requests = _requests.ToList();
        }

        Assert.Equal(new[] { 3, 3, 2 }, requests.Select(r => r.LineCount));
        Assert.Equal(new[] { 0, 2, 2 }, requests.Select(r => r.HistoryCount));
        Assert.All(requests, r =>
        {
            Assert.Contains("from English to German", r.System);
            Assert.Contains("A Scandinavian crime drama.", r.System);
            Assert.Contains("the Chief = der Chef", r.System);
            Assert.Contains("informal address", r.System);
        });
    }
}
