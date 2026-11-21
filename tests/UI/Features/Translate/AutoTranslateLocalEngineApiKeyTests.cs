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
/// A llama.cpp, Ollama or LM Studio server can be set up to ask for an API key. The Auto-translate
/// window shows the key box for these engines and their advanced variants and keeps one key per
/// URL setting, so a key only goes to the address it was entered for. The server Subtitle Edit
/// starts itself asks for no key and is never sent the one saved for an external llama.cpp server.
/// </summary>
public class AutoTranslateLocalEngineApiKeyTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class LlamaCppAdvancedProbe : LlamaCppAdvancedTranslate
    {
        public string? ApiKey => GetApiKey();
    }

    private sealed class OllamaAdvancedProbe : OllamaAdvancedTranslate
    {
        public string? ApiKey => GetApiKey();
    }

    private sealed class LmStudioAdvancedProbe : LmStudioAdvancedTranslate
    {
        public string? ApiKey => GetApiKey();
    }

    /// <summary>Restores every setting these tests write, so they leave no trace.</summary>
    private sealed class KeySettingsScope : IDisposable
    {
        private static SeAutoTranslate Gui => Se.Settings.AutoTranslate;
        private static Nikse.SubtitleEdit.Core.Settings.ToolsSettings Tools => Configuration.Settings.Tools;

        private readonly (string, string, string, string, string) _guiKeys =
            (Gui.LlamaCppApiKey, Gui.OllamaApiKey, Gui.LmStudioApiKey, Gui.OllamaAdvancedApiKey, Gui.LmStudioAdvancedApiKey);
        private readonly (string, string, string) _engineKeys = (Tools.LlamaCppApiKey, Tools.OllamaApiKey, Tools.LmStudioApiKey);
        private readonly (bool, string) _other = (Gui.LlamaCppUseRemoteServer, Gui.AutoTranslateLastName);

        public void Dispose()
        {
            (Gui.LlamaCppApiKey, Gui.OllamaApiKey, Gui.LmStudioApiKey, Gui.OllamaAdvancedApiKey, Gui.LmStudioAdvancedApiKey) = _guiKeys;
            (Tools.LlamaCppApiKey, Tools.OllamaApiKey, Tools.LmStudioApiKey) = _engineKeys;
            (Gui.LlamaCppUseRemoteServer, Gui.AutoTranslateLastName) = _other;
        }
    }

    private static (AutoTranslateViewModel, AutoTranslateWindow) OpenWindow()
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello.", 0, 1000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        vm.OnLoaded();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    private static void Select(AutoTranslateViewModel vm, Type engine)
    {
        vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t.GetType() == engine);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void EachEngine_ShowsAndSavesTheKeyOfItsOwnUrlSetting()
    {
        using var _ = new KeySettingsScope();
        var settings = Se.Settings.AutoTranslate;
        var tools = Configuration.Settings.Tools;
        settings.LlamaCppUseRemoteServer = true;
        settings.LlamaCppApiKey = "saved-llamacpp";
        settings.OllamaApiKey = "saved-ollama";
        settings.LmStudioApiKey = "saved-lmstudio";
        settings.OllamaAdvancedApiKey = "saved-ollama-advanced";
        settings.LmStudioAdvancedApiKey = "saved-lmstudio-advanced";

        var (vm, window) = OpenWindow();
        try
        {
            // engine, the key it shows, and where the edited key must end up
            var engines = new (Type Engine, string Saved, Func<string> Setting)[]
            {
                (typeof(LlamaCppTranslate), "saved-llamacpp", () => settings.LlamaCppApiKey),
                (typeof(OllamaTranslate), "saved-ollama", () => settings.OllamaApiKey),
                (typeof(LmStudioTranslate), "saved-lmstudio", () => settings.LmStudioApiKey),
                (typeof(OllamaAdvancedTranslate), "saved-ollama-advanced", () => settings.OllamaAdvancedApiKey),
                (typeof(LmStudioAdvancedTranslate), "saved-lmstudio-advanced", () => settings.LmStudioAdvancedApiKey),
            };

            foreach (var (engine, saved, setting) in engines)
            {
                Select(vm, engine);
                Assert.True(vm.ApiKeyIsVisible, engine.Name);
                Assert.Equal(saved, vm.ApiKeyText);

                vm.ApiKeyText = " key-of-" + engine.Name + " ";
                vm.SaveSettings();
                Assert.Equal("key-of-" + engine.Name, setting());
            }

            // Saving one engine's key left the others alone.
            Assert.Equal("key-of-LlamaCppTranslate", settings.LlamaCppApiKey);
            Assert.Equal("key-of-OllamaTranslate", settings.OllamaApiKey);
            Assert.Equal("key-of-LmStudioTranslate", settings.LmStudioApiKey);
            Assert.Equal("key-of-OllamaAdvancedTranslate", settings.OllamaAdvancedApiKey);
            Assert.Equal("key-of-LmStudioAdvancedTranslate", settings.LmStudioAdvancedApiKey);

            // The regular engines read their key from the libse settings when a run starts.
            Assert.Equal("key-of-OllamaTranslate", tools.OllamaApiKey);
            Assert.Equal("key-of-LmStudioTranslate", tools.LmStudioApiKey);

            // The two llama.cpp engines share the URL setting, so they share the key too.
            Select(vm, typeof(LlamaCppAdvancedTranslate));
            Assert.True(vm.ApiKeyIsVisible);
            Assert.Equal("key-of-LlamaCppTranslate", vm.ApiKeyText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LlamaCpp_KeyIsForTheExternalServerOnly()
    {
        using var _ = new KeySettingsScope();
        var settings = Se.Settings.AutoTranslate;
        var tools = Configuration.Settings.Tools;
        settings.LlamaCppUseRemoteServer = true;
        settings.LlamaCppApiKey = "external-key";

        var (vm, window) = OpenWindow();
        try
        {
            Select(vm, typeof(LlamaCppTranslate));
            Assert.True(vm.ApiKeyIsVisible);
            vm.SaveSettings();
            Assert.Equal("external-key", tools.LlamaCppApiKey);
            Assert.Equal("external-key", new LlamaCppAdvancedProbe().ApiKey);

            // The "use external server" switch off: Subtitle Edit starts the server itself.
            vm.LlamaCppUseRemoteServer = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.ApiKeyIsVisible);

            vm.SaveSettings();
            Assert.Equal(string.Empty, tools.LlamaCppApiKey);
            Assert.Equal(string.Empty, new LlamaCppAdvancedProbe().ApiKey);
            Assert.Equal("external-key", settings.LlamaCppApiKey);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void AdvancedEngines_SendTheKeyOfTheirOwnSetting()
    {
        using var _ = new KeySettingsScope();
        Se.Settings.AutoTranslate.OllamaAdvancedApiKey = "ollama-advanced-key";
        Se.Settings.AutoTranslate.LmStudioAdvancedApiKey = "lmstudio-advanced-key";

        Assert.Equal("ollama-advanced-key", new OllamaAdvancedProbe().ApiKey);
        Assert.Equal("lmstudio-advanced-key", new LmStudioAdvancedProbe().ApiKey);
    }
}
