using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UITests.Features.Translate;

/// <summary>
/// The Auto-translate window and Batch convert refuse to start an engine whose API key box is
/// shown but empty. An engine that also runs without a key has to be let through: the OpenAI
/// Compatible engine was not, so a local server that asks for no key needed dummy text in the box.
/// </summary>
public class AutoTranslateOptionalApiKeyTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [Fact]
    public void EnginesThatRunWithoutAKey_MayLeaveTheBoxEmpty()
    {
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new OpenAiCompatibleTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new LibreTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new MyMemoryApi()));
    }

    /// <summary>
    /// A llama.cpp, Ollama or LM Studio server asks for a key only when it was set up to; most do not.
    /// </summary>
    [Fact]
    public void LocalServerEngines_MayLeaveTheBoxEmpty()
    {
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new LlamaCppTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new OllamaTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new LmStudioTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new LlamaCppAdvancedTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new OllamaAdvancedTranslate()));
        Assert.True(AutoTranslateViewModel.ApiKeyIsOptional(new LmStudioAdvancedTranslate()));
    }

    [Fact]
    public void EnginesThatNeedAKey_StillRequireIt()
    {
        Assert.False(AutoTranslateViewModel.ApiKeyIsOptional(new DeepLTranslate()));
        Assert.False(AutoTranslateViewModel.ApiKeyIsOptional(new ChatGptTranslate()));
    }

    [AvaloniaFact]
    public async Task OpenAiCompatible_WithAnEmptyKeyBox_TranslatesAndSendsNoAuthorization()
    {
        var tools = Configuration.Settings.Tools;
        var (oldUrl, oldKey, oldModel) = (tools.OpenAiCompatibleTranslateUrl, tools.OpenAiCompatibleTranslateApiKey, tools.OpenAiCompatibleTranslateModel);
        var oldLastName = Se.Settings.AutoTranslate.AutoTranslateLastName;

        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var url = "http://127.0.0.1:" + ((IPEndPoint)probe.LocalEndpoint).Port + "/";
        probe.Stop();
        using var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        var authorizations = new List<string?>();
        var server = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            authorizations.Add(context.Request.Headers["Authorization"]);
            var bytes = Encoding.UTF8.GetBytes("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hallo.\"}}]}");
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }, TestContext.Current.CancellationToken);

        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello.", 0, 1000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        try
        {
            window.Show();
            vm.OnLoaded();
            Dispatcher.UIThread.RunJobs();

            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is OpenAiCompatibleTranslate);
            Dispatcher.UIThread.RunJobs();
            vm.SelectedSourceLanguage = vm.SourceLanguages.First(p => p.Name == "English");
            vm.SelectedTargetLanguage = vm.TargetLanguages.First(p => p.Name == "German");
            vm.ApiUrlText = url + "v1/chat/completions";
            vm.ApiKeyText = string.Empty;
            Assert.True(vm.ApiKeyIsVisible);

            // Not awaited: a refused start waits on its message box, which nobody closes here.
            _ = vm.TranslateCommand.ExecuteAsync(null);
            for (var i = 0; i < 500 && (string.IsNullOrEmpty(vm.Rows[0].TranslatedText) || !vm.IsTranslateEnabled); i++)
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);
                Dispatcher.UIThread.RunJobs();
            }

            Assert.Equal("Hallo.", vm.Rows[0].TranslatedText);
            Assert.Null(Assert.Single(authorizations));
        }
        finally
        {
            window.Close();
            listener.Close();
            tools.OpenAiCompatibleTranslateUrl = oldUrl;
            tools.OpenAiCompatibleTranslateApiKey = oldKey;
            tools.OpenAiCompatibleTranslateModel = oldModel;
            Se.Settings.AutoTranslate.AutoTranslateLastName = oldLastName;
        }

        Assert.True(server.IsCompleted);
    }
}
