using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
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
/// "Generate with AI" in the advanced settings asks the server a run of the selected engine asks. With
/// "Use external server (URL)" on, llama.cpp advanced runs against that server and Subtitle Edit starts
/// no model of its own, so the generator must not reach for the local one either.
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
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var url = "http://127.0.0.1:" + ((IPEndPoint)probe.LocalEndpoint).Port + "/";
        probe.Stop();
        using var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        var requests = new List<(string Path, string? Authorization)>();
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
                    context = await listener.GetContextAsync();
                }
                catch (Exception e) when (e is HttpListenerException or ObjectDisposedException or InvalidOperationException)
                {
                    return; // closed
                }

                lock (requests)
                {
                    requests.Add((context.Request.Url!.AbsolutePath, context.Request.Headers["Authorization"]));
                }

                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(reply);
                context.Response.Close();
            }
        }, TestContext.Current.CancellationToken);

        // Saved in this mode, so the window opens in it and fills no local model list, as the app does.
        Se.Settings.AutoTranslate.LlamaCppUseRemoteServer = true;
        Se.Settings.AutoTranslate.LlamaCppApiUrl = url + "v1/chat/completions";
        Se.Settings.AutoTranslate.LlamaCppApiKey = "external-key";
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Where were you last night, Erik?", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("I was at the office until late.", 2000, 4000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        try
        {
            window.Show();
            vm.OnLoaded();
            Dispatcher.UIThread.RunJobs();
            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is LlamaCppAdvancedTranslate);
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.LlamaCppUseRemoteServer);

            var result = await vm.GenerateLlamaCppAdvancedSynopsisAsync(window, (_, _) => { }, TestContext.Current.CancellationToken);

            Assert.Contains("Erik is asked where he was.", result);
            lock (requests)
            {
                var request = Assert.Single(requests);
                Assert.Equal("/v1/chat/completions", request.Path);
                Assert.Equal("Bearer external-key", request.Authorization);
            }
        }
        finally
        {
            window.Close();
            listener.Close();
        }
    }
}
