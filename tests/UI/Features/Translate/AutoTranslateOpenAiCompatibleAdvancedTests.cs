using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace UITests.Features.Translate;

/// <summary>
/// The OpenAI Compatible advanced engine, seconv's openai-compatible-advanced in the GUI: it takes
/// url, key and model like the regular OpenAI Compatible engine, from its own settings, and speaks
/// plain OpenAI chat-completions, without the llama.cpp request fields.
/// </summary>
public class AutoTranslateOpenAiCompatibleAdvancedTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class Probe : OpenAiCompatibleAdvancedTranslate
    {
        public string ApiUrl => GetApiUrl();
        public string? Model => GetModel();
        public string? ApiKey => GetApiKey();
        public bool LlamaCppExtensions => UsesLlamaCppExtensions;
    }

    /// <summary>Restores the settings these tests write.</summary>
    private sealed class SettingsScope : IDisposable
    {
        private static SeAutoTranslate Gui => Se.Settings.AutoTranslate;

        private readonly (string, string, string, string) _saved =
            (Gui.OpenAiCompatibleAdvancedUrl, Gui.OpenAiCompatibleAdvancedModel, Gui.OpenAiCompatibleAdvancedApiKey, Gui.AutoTranslateLastName);

        public void Dispose()
        {
            (Gui.OpenAiCompatibleAdvancedUrl, Gui.OpenAiCompatibleAdvancedModel, Gui.OpenAiCompatibleAdvancedApiKey, Gui.AutoTranslateLastName) = _saved;
        }
    }

    [Fact]
    public void Engine_ReadsItsOwnSettings_AndLeavesOutTheLlamaCppFields()
    {
        // The regular engine's default url, not a provider's.
        Assert.Equal(OpenAiCompatibleTranslate.DefaultUrl, new SeAutoTranslate().OpenAiCompatibleAdvancedUrl);

        using var _ = new SettingsScope();
        var settings = Se.Settings.AutoTranslate;
        settings.OpenAiCompatibleAdvancedUrl = "http://hosted.example:8080";
        settings.OpenAiCompatibleAdvancedModel = " vendor/model ";
        settings.OpenAiCompatibleAdvancedApiKey = "advanced-key";

        var probe = new Probe();
        Assert.Equal("http://hosted.example:8080/v1/chat/completions", probe.ApiUrl); // a base url is completed, as for the regular engine
        Assert.Equal("vendor/model", probe.Model);
        Assert.Equal("advanced-key", probe.ApiKey);
        Assert.False(probe.LlamaCppExtensions);

        // The model is optional: a one-model server needs none, and no "model" field is sent.
        settings.OpenAiCompatibleAdvancedModel = string.Empty;
        Assert.Null(new Probe().Model);
    }

    [AvaloniaFact]
    public void Window_ShowsItsOwnUrlKeyAndModel_AndSavesThem()
    {
        using var _ = new SettingsScope();
        var settings = Se.Settings.AutoTranslate;
        settings.OpenAiCompatibleAdvancedUrl = "https://hosted.example/api/v1/chat/completions";
        settings.OpenAiCompatibleAdvancedModel = "vendor/model";
        settings.OpenAiCompatibleAdvancedApiKey = "advanced-key";

        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello.", 0, 1000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        vm.OnLoaded();
        Dispatcher.UIThread.RunJobs();
        try
        {
            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is OpenAiCompatibleAdvancedTranslate);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.ApiUrlIsVisible);
            Assert.Equal("https://hosted.example/api/v1/chat/completions", vm.ApiUrlText);
            Assert.True(vm.ApiKeyIsVisible);
            Assert.Equal("advanced-key", vm.ApiKeyText);
            Assert.True(vm.ModelIsVisible);
            Assert.True(vm.ModelBrowseIsVisible);
            Assert.Equal("vendor/model", vm.ModelText);
            Assert.True(vm.LlamaCppAdvancedButtonIsVisible);

            vm.ApiUrlText = " http://typed.example/v1/chat/completions ";
            vm.ApiKeyText = " typed-key ";
            vm.ModelText = " typed/model ";
            vm.SaveSettings();
            Assert.Equal("http://typed.example/v1/chat/completions", settings.OpenAiCompatibleAdvancedUrl);
            Assert.Equal("typed-key", settings.OpenAiCompatibleAdvancedApiKey);
            Assert.Equal("typed/model", settings.OpenAiCompatibleAdvancedModel);
        }
        finally
        {
            window.Close();
        }
    }
}
