using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Translate;

public class TranslationProfileModelBrowseTests
{
    private static TranslationProfile Profile(string url) => new()
    {
        Kind = TranslationEngineKind.Llm, EngineId = "Custom", ApiUrl = url,
        ApiKey = "profile-key", Model = "manual-model",
    };
    private static T Named<T>(Control root, string name) where T : Control => root.GetLogicalDescendants().OfType<T>().Single(c => c.Name == name);
    private static ComboBox Models(Control editor) => Named<ComboBox>(editor, "TranslationModel");
    private static Button Browse(Control editor) => Named<Button>(editor, "BrowseTranslationModels");
    private static async Task Until(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition() && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            await Task.Delay(10);
            Dispatcher.UIThread.RunJobs();
        }
        Assert.True(condition(), "Model browsing did not complete.");
    }

    [AvaloniaFact]
    public async Task OpeningProfilesDropdownsAndEditingConnectionsDoNotRequestModels()
    {
        using var server = new ModelServer();
        using var editor = new TranslationProfileEditorControl(Profile(server.Url));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            Models(editor).IsDropDownOpen = true;
            Models(editor).IsDropDownOpen = false;
            var key = editor.GetLogicalDescendants().OfType<TextBox>().Single(t => t.Text == "profile-key");
            key.Focus(); key.Text = "edited-key"; Models(editor).Focus();
            await Task.Delay(550); Dispatcher.UIThread.RunJobs();
            Assert.Empty(server.Requests);
            Assert.Equal("manual-model", Models(editor).Text);
            Assert.DoesNotContain(editor.GetLogicalDescendants(), c => c is Control { Name: "ModelListStatus" });
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void HostedProvidersUseBuiltInChoicesAndOnlyCustomOpenAiOffersBrowse()
    {
        using var editor = new TranslationProfileEditorControl(Profile(ChatGptTranslate.DefaultUrl));
        Assert.NotEmpty(Models(editor).ItemsSource!.Cast<string>());
        Assert.False(Browse(editor).IsVisible);
        var providers = Named<ComboBox>(editor, "ProviderPreset");
        providers.SelectedIndex = 2;
        Assert.Contains(AnthropicTranslate.Models[0], Models(editor).ItemsSource!.Cast<string>());
        Assert.False(Browse(editor).IsVisible);
        providers.SelectedIndex = 0;
        Assert.True(Browse(editor).IsVisible);
        Named<ComboBox>(editor, "ApiProtocol").SelectedIndex = 1;
        Assert.False(Browse(editor).IsVisible);
        Assert.Equal("manual-model", Models(editor).Text);
    }

    [AvaloniaTheory]
    [InlineData("Ollama", true)]
    [InlineData("LmStudio", true)]
    [InlineData("Custom", true)]
    [InlineData("LlamaCpp", false)]
    public void LocalModelBrowsingMatchesTheOriginalEngineWorkflow(string engineId, bool expected)
    {
        var profile = Profile("http://localhost:1234/v1/chat/completions");
        profile.Location = LlmLocation.Local; profile.EngineId = engineId;
        using var editor = new TranslationProfileEditorControl(profile);
        Assert.Equal(expected, Browse(editor).IsVisible);
    }

    [AvaloniaFact]
    public async Task ExplicitBrowseUsesOriginalPickerAndProfileCredentials()
    {
        using var server = new ModelServer();
        using var editor = new TranslationProfileEditorControl(Profile(server.Url));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            Assert.Empty(server.Requests);
            Browse(editor).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var picker = Assert.Single(host.OwnedWindows.OfType<PickOllamaModelWindow>());
            var vm = Assert.IsType<PickOllamaModelViewModel>(picker.DataContext);
            await Until(() => vm.Models.Contains("server-model"));
            var request = Assert.Single(server.Requests);
            Assert.Contains("GET /v1/models HTTP/1.1", request);
            Assert.Contains("Authorization: Bearer profile-key", request);
            Assert.Equal("manual-model", Models(editor).Text);
            vm.SelectedModel = "server-model";
            vm.OkCommand.Execute(null);
            await Until(() => Models(editor).Text == "server-model");
            Assert.Equal("server-model", editor.CaptureDraft().Model);
        }
        finally { foreach (var owned in host.OwnedWindows.ToArray()) owned.Close(); host.Close(); }
    }

    [AvaloniaFact]
    public async Task UnauthorizedModelListLeavesPickerEmptyAndManualModelIntact()
    {
        using var server = new ModelServer(401);
        using var editor = new TranslationProfileEditorControl(Profile(server.Url));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            Browse(editor).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var picker = Assert.Single(host.OwnedWindows.OfType<PickOllamaModelWindow>());
            var vm = Assert.IsType<PickOllamaModelViewModel>(picker.DataContext);
            await Until(() => server.Requests.Count == 1);
            await Task.Delay(100); Dispatcher.UIThread.RunJobs();
            Assert.Empty(vm.Models);
            vm.CancelCommand.Execute(null);
            await Until(() => !host.OwnedWindows.Contains(picker));
            Assert.Equal("manual-model", Models(editor).Text);
            Models(editor).Text = "another-manual-model";
            Assert.Equal("another-manual-model", editor.CaptureDraft().Model);
        }
        finally { foreach (var owned in host.OwnedWindows.ToArray()) owned.Close(); host.Close(); }
    }

    private sealed class ModelServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();
        public ConcurrentQueue<string> Requests { get; } = new();
        public string Url { get; }

        public ModelServer(int status = 200)
        {
            _listener.Start();
            Url = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/v1/chat/completions";
            var token = _stop.Token;
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var client = await _listener.AcceptTcpClientAsync(token);
                        using var stream = client.GetStream();
                        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                        var headers = new StringBuilder();
                        while (await reader.ReadLineAsync(token) is { Length: > 0 } line) headers.AppendLine(line);
                        Requests.Enqueue(headers.ToString());
                        var body = status == 200 ? "{\"data\":[{\"id\":\"server-model\"}]}" : "{\"error\":\"unauthorized\"}";
                        var payload = Encoding.UTF8.GetBytes(body);
                        var response = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} {(status == 200 ? "OK" : "Unauthorized")}\r\nContent-Type: application/json\r\nContent-Length: {payload.Length}\r\nConnection: close\r\n\r\n");
                        await stream.WriteAsync(response, token);
                        await stream.WriteAsync(payload, token);
                    }
                    catch (Exception) when (token.IsCancellationRequested) { return; }
                }
            });
        }

        public void Dispose() { _stop.Cancel(); _listener.Stop(); _stop.Dispose(); }
    }
}
