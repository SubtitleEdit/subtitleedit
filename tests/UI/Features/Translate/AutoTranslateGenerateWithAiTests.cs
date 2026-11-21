using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UITests.Features.Tools.BatchConvert;

namespace UITests.Features.Translate;

/// <summary>
/// "Generate with AI" in the advanced settings asks the server a run of the selected engine asks, with
/// the settings of the same dialog. With "Use external server (URL)" on, llama.cpp advanced runs against
/// that server and Subtitle Edit starts no model of its own, so the generator must not reach for the
/// local one either; and the dialog's Extra request JSON goes into its requests as into a run's.
/// </summary>
public class AutoTranslateGenerateWithAiTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public async Task LlamaCppAdvancedWithAnExternalServer_AsksThatServer()
    {
        using var snapshot = new BatchConvertAutoTranslateEngineTests.TranslateSettingsSnapshot();
        using var server = new FakeChatServer();

        // Saved in this mode, so the window opens in it and fills no local model list, as the app does.
        Se.Settings.AutoTranslate.LlamaCppUseRemoteServer = true;
        Se.Settings.AutoTranslate.LlamaCppApiUrl = server.Url + "v1/chat/completions";
        Se.Settings.AutoTranslate.LlamaCppApiKey = "external-key";
        var (vm, window) = Open(t => t is LlamaCppAdvancedTranslate);
        try
        {
            Assert.True(vm.LlamaCppUseRemoteServer);

            var result = await vm.GenerateLlamaCppAdvancedSynopsisAsync(window, (_, _) => { }, TestContext.Current.CancellationToken);

            Assert.Contains("Erik is asked where he was.", result);
            var request = Assert.Single(server.Requests);
            Assert.Equal("/v1/chat/completions", request.Path);
            Assert.Equal("Bearer external-key", request.Authorization);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task ExtraRequestJsonOfTheDialog_IsMergedIntoTheRequest()
    {
        using var snapshot = new BatchConvertAutoTranslateEngineTests.TranslateSettingsSnapshot();
        using var server = new FakeChatServer();
        UseOpenAiCompatibleAdvanced(server);
        var (vm, window) = Open(t => t is OpenAiCompatibleAdvancedTranslate);
        try
        {
            // As typed in the dialog's box: a provider's switch, and a null that removes the temperature.
            var result = await vm.GenerateLlamaCppAdvancedSynopsisAsync(window, (_, _) => { }, TestContext.Current.CancellationToken,
                "{\"reasoning\": {\"enabled\": false}, \"temperature\": null}");

            Assert.Contains("Erik is asked where he was.", result);
            using var body = JsonDocument.Parse(Assert.Single(server.Requests).Body);
            Assert.False(body.RootElement.GetProperty("reasoning").GetProperty("enabled").GetBoolean());
            Assert.False(body.RootElement.TryGetProperty("temperature", out _));
            Assert.Equal("vendor/model", body.RootElement.GetProperty("model").GetString());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task InvalidExtraRequestJson_IsReported_AndNothingIsSent()
    {
        using var snapshot = new BatchConvertAutoTranslateEngineTests.TranslateSettingsSnapshot();
        using var server = new FakeChatServer();
        UseOpenAiCompatibleAdvanced(server);
        var (vm, window) = Open(t => t is OpenAiCompatibleAdvancedTranslate);
        try
        {
            // Not awaited yet: the message box waits for its OK.
            var generate = vm.GenerateLlamaCppAdvancedSynopsisAsync(window, (_, _) => { }, TestContext.Current.CancellationToken, "{\"reasoning\": ");
            Dispatcher.UIThread.RunJobs();

            // The dialog's own check and message, as on OK.
            var messageBox = Assert.Single(window.OwnedWindows.OfType<MessageBox>());
            var texts = messageBox.GetVisualDescendants().OfType<TextBlock>().Select(p => p.Text ?? string.Empty).ToList();
            Assert.Contains(texts, p => p.Contains("not valid JSON"));
            messageBox.Close();

            Assert.Null(await generate);
            Assert.Empty(server.Requests);
        }
        finally
        {
            window.Close();
        }
    }

    private static void UseOpenAiCompatibleAdvanced(FakeChatServer server)
    {
        Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedUrl = server.Url + "v1/chat/completions";
        Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedModel = "vendor/model";
        Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedApiKey = string.Empty;
    }

    /// <summary>The Auto-translate window on a two-line subtitle, with the engine <paramref name="pick"/> finds.</summary>
    private static (AutoTranslateViewModel Vm, AutoTranslateWindow Window) Open(Func<object, bool> pick)
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Where were you last night, Erik?", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("I was at the office until late.", 2000, 4000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        vm.OnLoaded();
        Dispatcher.UIThread.RunJobs();
        vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => pick(t));
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    /// <summary>A stand-in chat server that answers every request with the names and summary of a part.</summary>
    private sealed class FakeChatServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly List<(string Path, string? Authorization, string Body)> _requests = new();

        public FakeChatServer()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            Url = "http://127.0.0.1:" + ((IPEndPoint)probe.LocalEndpoint).Port + "/";
            probe.Stop();
            _listener.Prefixes.Add(Url);
            _listener.Start();
            var answer = "{\"names\":[\"Erik\"],\"terms\":[],\"summary\":\"Erik is asked where he was.\"}";
            var reply = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { role = "assistant", content = answer }, finish_reason = "stop" } },
            }));
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await _listener.GetContextAsync();
                    }
                    catch (Exception e) when (e is HttpListenerException or ObjectDisposedException or InvalidOperationException)
                    {
                        return; // closed
                    }

                    using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                    {
                        var body = await reader.ReadToEndAsync();
                        lock (_requests)
                        {
                            _requests.Add((context.Request.Url!.AbsolutePath, context.Request.Headers["Authorization"], body));
                        }
                    }

                    context.Response.ContentType = "application/json";
                    await context.Response.OutputStream.WriteAsync(reply);
                    context.Response.Close();
                }
            });
        }

        public string Url { get; }

        public List<(string Path, string? Authorization, string Body)> Requests
        {
            get
            {
                lock (_requests)
                {
                    return _requests.ToList();
                }
            }
        }

        public void Dispose()
        {
            _listener.Close();
        }
    }
}
