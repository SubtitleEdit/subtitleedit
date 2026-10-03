using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// "--translate-to da,de" translates every input into each language, one output per language
/// ("in.da.srt", "in.de.srt"). Runs against a fake LibreTranslate server that tags the text with
/// the target code.
/// </summary>
public class MultipleTranslateTargetsTest : IDisposable
{
    private const string SrtContent = "1\n00:00:01,000 --> 00:00:03,000\nHello world.\n";

    private readonly string _tempRoot;
    private readonly HttpListener _listener = new();
    private readonly string _url;
    private readonly string _oldLibreUrl;

    public MultipleTranslateTargetsTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "MultiTranslate_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        _oldLibreUrl = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.AutoTranslateLibreUrl;

        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        _url = $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(_url);
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

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
            var q = request.RootElement.GetProperty("q").GetString();
            var target = request.RootElement.GetProperty("target").GetString();
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { translatedText = $"[{target}] {q}" }));
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(body);
            context.Response.Close();
        }
    }

    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
        Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.AutoTranslateLibreUrl = _oldLibreUrl;
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private ConversionOptions MakeOptions(string input, string outDir, string translateTo, bool noLanguageSuffix = false) => new()
    {
        Patterns = [input],
        Format = "SubRip",
        OutputFolder = outDir,
        Overwrite = true,
        TranslateTo = translateTo,
        TranslateFrom = "en",
        TranslateEngine = "libretranslate",
        TranslateUrl = _url,
        NoLanguageSuffix = noLanguageSuffix,
        Quiet = true,
    };

    [Fact]
    public async Task SeveralTargets_OneOutputPerLanguage()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var outDir = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outDir);

        var result = await new SubtitleConverter().ConvertAsync(MakeOptions(input, outDir, "da, de"));

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(2, result.SuccessfulFiles);
        Assert.Contains("[da] Hello world.", await File.ReadAllTextAsync(Path.Combine(outDir, "in.da.srt"), TestContext.Current.CancellationToken));
        Assert.Contains("[de] Hello world.", await File.ReadAllTextAsync(Path.Combine(outDir, "in.de.srt"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SeveralTargets_WithNoLanguageSuffix_IsRejected()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(MakeOptions(input, _tempRoot, "da,de", noLanguageSuffix: true));

        Assert.False(result.Success);
        Assert.Contains("--no-language-suffix", string.Join("; ", result.Errors));
    }

    [Theory]
    [InlineData("de", new[] { "de" })]
    [InlineData("de, fr ,de", new[] { "de", "fr" })]
    [InlineData(" ", new string[0])]
    public void SplitTranslateTargets_TrimsAndDedupes(string value, string[] expected)
    {
        Assert.Equal(expected, SubtitleConverter.SplitTranslateTargets(value));
    }
}
