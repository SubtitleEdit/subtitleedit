using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// The advanced engines take whole-line "&lt;i&gt;...&lt;/i&gt;" and "♪ ... ♪" off before sending a
/// line and put them back around the answer: TranslateGemma 12B dropped &lt;i&gt; in 11 of 20
/// italic lines (#16869).
/// </summary>
public class TranslationWrapperStripTests
{
    [Theory]
    [InlineData("<i>Hello there.</i>", "Hello there.")]
    [InlineData("<I>Hello there.</I>", "Hello there.")]
    [InlineData("<b>Hello there.</b>", "Hello there.")]
    [InlineData("<i>Line one\r\nline two</i>", "Line one\r\nline two")]
    [InlineData("♪ La la la ♪", "La la la")]
    [InlineData("♪♪ La la la", "La la la")]
    [InlineData("La la la ♫", "La la la")]
    [InlineData("<i>♪ La la la ♪</i>", "La la la")]
    [InlineData("♪ <i>La la la</i> ♪", "La la la")]
    [InlineData("<i><b>Both</b></i>", "Both")]
    [InlineData(@"{\an8}<i>Up top</i>", "Up top")]
    public void WholeLineWrappers_AreStrippedAndRestored(string input, string expectedText)
    {
        var stripped = StrippedLine.StripForTranslation(input);

        Assert.Equal(expectedText, stripped.Text);
        Assert.Equal(input, stripped.Restore(stripped.Text));
    }

    [Fact]
    public void WrapperIsPutBackAroundTheTranslation()
    {
        var stripped = StrippedLine.StripForTranslation("<i>♪ Hello there ♪</i>");

        Assert.Equal("<i>♪ مرحبا ♪</i>", stripped.Restore("مرحبا"));
    }

    [Theory]
    [InlineData("<i>مرحبا</i>")]
    [InlineData("♪ مرحبا ♪")]
    [InlineData("<i>♪ مرحبا ♪</i>")]
    [InlineData("  <i>مرحبا</i> ")]
    public void WrapperKeptByTheModel_IsNotDoubled(string modelAnswer)
    {
        var stripped = StrippedLine.StripForTranslation("<i>♪ Hello there ♪</i>");

        Assert.Equal("<i>♪ مرحبا ♪</i>", stripped.Restore(modelAnswer));
    }

    [Theory]
    [InlineData("<i>One</i> and <i>two</i>")]
    [InlineData("<i>One</i>\r\n<i>two</i>")]
    [InlineData("He said <i>no</i>.")]
    [InlineData("<i>Half</i> italic")]
    [InlineData("Plain text")]
    public void PartialTags_StayInTheText(string input)
    {
        var stripped = StrippedLine.StripForTranslation(input);

        Assert.Equal(input, stripped.Text);
        Assert.False(stripped.HasWrappers);
        Assert.Equal("Hola", stripped.Restore("Hola"));
    }

    [Theory]
    [InlineData("♪")]
    [InlineData("♪ ♪")]
    [InlineData("<i>♪</i>")]
    [InlineData("<i></i>")]
    public void OnlyMarkup_IsLeftAsItIs(string input)
    {
        var stripped = StrippedLine.StripForTranslation(input);

        Assert.Equal(input, stripped.Text);
        Assert.False(stripped.HasWrappers);
    }

    [Fact]
    public void LineWithoutWrappers_KeepsTheModelsMarkup()
    {
        // No wrapper of our own, so a tag the model wrote is its translation of an inline tag.
        var stripped = StrippedLine.StripForTranslation("He said no.");

        Assert.Equal("<i>Han sagde nej.</i>", stripped.Restore("<i>Han sagde nej.</i>"));
    }

    [Fact]
    public void PlainStrip_StillLeavesHtmlTagsAlone()
    {
        // AI Review and the main-window callers use Strip, which only takes ASSA blocks off.
        Assert.Equal("<i>Hello</i>", StrippedLine.Strip("<i>Hello</i>").Text);
    }

    [Fact]
    public async Task Batch_SendsLinesWithoutWrappers_AndRestoresDroppedItalics()
    {
        var rows = new ObservableCollection<TranslateRow>
        {
            new() { Text = "<i>Where were you last night?</i>" },
            new() { Text = "I was at the office." },
            new() { Text = "<i>Who else was there?</i>" },
        };

        // The model drops the italics on line 1 and keeps them on line 3.
        const string modelReply = "{\"1\":\"أين كنت الليلة الماضية؟\",\"2\":\"كنت في المكتب.\",\"3\":\"<i>من كان هناك أيضا؟</i>\"}";
        var requestBody = string.Empty;
        var (listener, url) = StartListener();
        using (listener)
        {
            var serverTask = Task.Run(async () =>
            {
                var context = await listener.GetContextAsync();
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
                }

                var chunk = "{\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"finish_reason\":\"stop\"," +
                            "\"delta\":{\"role\":\"assistant\",\"content\":" + System.Text.Json.JsonSerializer.Serialize(modelReply) + "}}]}";
                var bytes = Encoding.UTF8.GetBytes("data: " + chunk + "\n\ndata: [DONE]\n\n");
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/event-stream";
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }, TestContext.Current.CancellationToken);

            using var translator = new FakeServerTranslator(url + "v1/chat/completions");
            translator.Settings = new SeLlamaCppAdvanced { BatchSize = 10, HistoryPairs = 0 };
            translator.Initialize();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var translated = await translator.TranslateBatchAsync(rows, 0, "English", "Arabic", null, timeout.Token);
            await serverTask;

            Assert.Equal(3, translated);
        }

        Assert.DoesNotContain("<i>", requestBody);
        Assert.Contains("Where were you last night?", requestBody);
        Assert.Equal("<i>أين كنت الليلة الماضية؟</i>", rows[0].TranslatedText);
        Assert.Equal("كنت في المكتب.", rows[1].TranslatedText);
        Assert.Equal("<i>من كان هناك أيضا؟</i>", rows[2].TranslatedText);
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
