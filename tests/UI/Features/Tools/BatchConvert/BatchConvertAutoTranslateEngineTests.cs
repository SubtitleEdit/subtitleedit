using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;
using System.Reflection;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert builds its auto-translate view in code and keeps its own engine list, so an engine
/// that works in the Auto-translate window can still be missing here - and the "Advanced..." button
/// is only useful if its visibility actually follows the selected engine.
/// </summary>
public class BatchConvertAutoTranslateEngineTests
{
    [AvaloniaFact]
    public void EngineList_ContainsLlamaCppAdvanced()
    {
        var viewModel = MakeViewModel();

        Assert.Contains(viewModel.AutoTranslators, t => t is LlamaCppAdvancedTranslate);
    }

    /// <summary>
    /// A settings reset or import replaces the settings object; an engine created before that
    /// must translate with the new values, not the ones it saw at construction.
    /// </summary>
    [AvaloniaFact]
    public void AdvancedEngines_ReadReplacedSettingsLive()
    {
        var original = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        try
        {
            var engines = new AdvancedTranslatorBase[] { new LlamaCppAdvancedTranslate(), new OllamaAdvancedTranslate(), new LmStudioAdvancedTranslate() };
            var replaced = new SeLlamaCppAdvanced { BatchSize = 7 };
            Se.Settings.AutoTranslate.LlamaCppAdvanced = replaced;

            Assert.All(engines, e => Assert.Same(replaced, e.Settings));
        }
        finally
        {
            Se.Settings.AutoTranslate.LlamaCppAdvanced = original;
        }
    }

    [AvaloniaFact]
    public void EngineList_ContainsOllamaAdvanced()
    {
        var viewModel = MakeViewModel();

        Assert.Contains(viewModel.AutoTranslators, t => t is OllamaAdvancedTranslate);
    }

    [AvaloniaFact]
    public void AdvancedButton_IsShownForAdvancedEnginesOnly()
    {
        var viewModel = MakeViewModel();
        var view = ViewAutoTranslate.Make(viewModel);

        var button = view.GetLogicalDescendants().OfType<Button>().FirstOrDefault(b =>
            AutomationProperties.GetName(b) == Se.Language.General.AdvancedSettings);
        Assert.NotNull(button);

        foreach (var engine in viewModel.AutoTranslators)
        {
            viewModel.SelectedAutoTranslator = engine;
            viewModel.OnAutoTranslatorChanged();

            // Batch size, history, synopsis/glossary/style - shared settings, so every engine
            // running the advanced batch protocol opens the same window.
            Assert.Equal(engine is AdvancedTranslatorBase, viewModel.LlamaCppAdvancedButtonIsVisible);
        }
    }

    /// <summary>
    /// The advanced Ollama engine talks to the OpenAI-compatible endpoint, which is a different URL
    /// and a separately stored model from the classic Ollama engine - so both fields must be shown
    /// and filled from its own settings, not the classic engine's. The same goes for its API key.
    /// </summary>
    [AvaloniaFact]
    public void OllamaAdvanced_ShowsItsOwnUrlAndModel()
    {
        var viewModel = MakeViewModel();
        Se.Settings.AutoTranslate.OllamaAdvancedUrl = "http://example.local:11434/v1/chat/completions";
        Se.Settings.AutoTranslate.OllamaAdvancedModel = "qwen3:8b";
        Se.Settings.AutoTranslate.OllamaApiKey = "classic-key";
        Se.Settings.AutoTranslate.OllamaAdvancedApiKey = "advanced-key";

        viewModel.SelectedAutoTranslator = viewModel.AutoTranslators.First(t => t is OllamaAdvancedTranslate);
        viewModel.OnAutoTranslatorChanged();

        Assert.True(viewModel.AutoTranslateUrlIsVisible);
        Assert.True(viewModel.AutoTranslateModelIsVisible);
        Assert.True(viewModel.AutoTranslateModelBrowseIsVisible);
        Assert.True(viewModel.AutoTranslateApiKeyIsVisible);
        Assert.Equal("http://example.local:11434/v1/chat/completions", viewModel.AutoTranslateUrl);
        Assert.Equal("qwen3:8b", viewModel.AutoTranslateModel);
        Assert.Equal("advanced-key", viewModel.AutoTranslateApiKey);
    }

    /// <summary>
    /// A llama.cpp, Ollama or LM Studio server can ask for an API key. The key box goes with the URL
    /// box: shown with each engine's own saved key where the URL is editable, hidden for the server
    /// Subtitle Edit starts itself and for the classic Ollama engine, whose address (and key) come
    /// from the Auto-translate window.
    /// </summary>
    [AvaloniaFact]
    public void LocalServerEngines_ShowTheKeyBoxNextToTheUrlBox()
    {
        var keys = Se.Settings.AutoTranslate;
        var saved = (keys.LlamaCppApiKey, keys.OllamaApiKey, keys.LmStudioApiKey, keys.OllamaAdvancedApiKey, keys.LmStudioAdvancedApiKey,
            Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer);
        try
        {
            (keys.LlamaCppApiKey, keys.OllamaApiKey, keys.LmStudioApiKey, keys.OllamaAdvancedApiKey, keys.LmStudioAdvancedApiKey) =
                ("llamacpp-key", "ollama-key", "lmstudio-key", "ollama-advanced-key", "lmstudio-advanced-key");
            Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer = true;
            var viewModel = MakeViewModel();

            void Select(Type engine)
            {
                viewModel.SelectedAutoTranslator = viewModel.AutoTranslators.First(t => t.GetType() == engine);
                viewModel.OnAutoTranslatorChanged();
            }

            Select(typeof(LmStudioTranslate));
            Assert.True(viewModel.AutoTranslateApiKeyIsVisible);
            Assert.Equal("lmstudio-key", viewModel.AutoTranslateApiKey);

            Select(typeof(LmStudioAdvancedTranslate));
            Assert.True(viewModel.AutoTranslateApiKeyIsVisible);
            Assert.Equal("lmstudio-advanced-key", viewModel.AutoTranslateApiKey);

            Select(typeof(OllamaTranslate));
            Assert.False(viewModel.AutoTranslateUrlIsVisible);
            Assert.False(viewModel.AutoTranslateApiKeyIsVisible);

            foreach (var llamaCpp in new[] { typeof(LlamaCppTranslate), typeof(LlamaCppAdvancedTranslate) })
            {
                Select(llamaCpp);
                Assert.True(viewModel.AutoTranslateApiKeyIsVisible, llamaCpp.Name);
                Assert.Equal("llamacpp-key", viewModel.AutoTranslateApiKey);
            }

            // The "use external server" switch off: no URL box, no key box.
            viewModel.LlamaCppUseRemoteServer = false;
            Assert.False(viewModel.AutoTranslateUrlIsVisible);
            Assert.False(viewModel.AutoTranslateApiKeyIsVisible);
        }
        finally
        {
            (keys.LlamaCppApiKey, keys.OllamaApiKey, keys.LmStudioApiKey, keys.OllamaAdvancedApiKey, keys.LmStudioAdvancedApiKey,
                Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer) = saved;
        }
    }

    /// <summary>
    /// What a run is started with: the key typed for an external llama.cpp server reaches both
    /// llama.cpp engines and is saved; with the switch off the engines get no key, and the saved
    /// one stays for the external server.
    /// </summary>
    [AvaloniaFact]
    public void LlamaCpp_RunGetsTheKeyForAnExternalServerOnly()
    {
        var keys = Se.Settings.AutoTranslate;
        var tools = Configuration.Settings.Tools;
        var saved = (keys.LlamaCppApiKey, keys.LlamaCppApiUrl, tools.LlamaCppApiKey, tools.LlamaCppApiUrl, Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer);
        try
        {
            Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer = true;
            var viewModel = MakeViewModel();
            viewModel.SelectedAutoTranslator = viewModel.AutoTranslators.First(t => t is LlamaCppTranslate);
            viewModel.OnAutoTranslatorChanged();
            var updateSettings = typeof(BatchConvertViewModel).GetMethod("UpdateAutoTranslateSettings", BindingFlags.NonPublic | BindingFlags.Instance)!;

            viewModel.AutoTranslateApiKey = " external-key ";
            updateSettings.Invoke(viewModel, null);
            Assert.Equal("external-key", tools.LlamaCppApiKey);
            Assert.Equal("external-key", keys.LlamaCppApiKey);

            viewModel.LlamaCppUseRemoteServer = false;
            updateSettings.Invoke(viewModel, null);
            Assert.Equal(string.Empty, tools.LlamaCppApiKey);
            Assert.Equal("external-key", keys.LlamaCppApiKey);
        }
        finally
        {
            (keys.LlamaCppApiKey, keys.LlamaCppApiUrl, tools.LlamaCppApiKey, tools.LlamaCppApiUrl, Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer) = saved;
        }
    }

    /// <summary>
    /// The generic OpenAI-compatible engine (vLLM, a hand-started llama-server, hosted providers)
    /// has no model list to browse, so all three of URL, API key and model are free-text and come
    /// from its own settings.
    /// </summary>
    [AvaloniaFact]
    public void OpenAiCompatible_ShowsUrlApiKeyModelAndBrowse()
    {
        var viewModel = MakeViewModel();
        Se.Settings.AutoTranslate.OpenAiCompatibleUrl = "http://example.local:8000/v1/chat/completions";
        Se.Settings.AutoTranslate.OpenAiCompatibleApiKey = "sk-test";
        Se.Settings.AutoTranslate.OpenAiCompatibleModel = "gemma-3-12b";

        viewModel.SelectedAutoTranslator = viewModel.AutoTranslators.First(t => t is OpenAiCompatibleTranslate);
        viewModel.OnAutoTranslatorChanged();

        Assert.True(viewModel.AutoTranslateUrlIsVisible);
        Assert.True(viewModel.AutoTranslateApiKeyIsVisible);
        Assert.True(viewModel.AutoTranslateModelIsVisible);
        Assert.True(viewModel.AutoTranslateModelBrowseIsVisible);
        Assert.Equal("http://example.local:8000/v1/chat/completions", viewModel.AutoTranslateUrl);
        Assert.Equal("sk-test", viewModel.AutoTranslateApiKey);
        Assert.Equal("gemma-3-12b", viewModel.AutoTranslateModel);
    }

    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }
}
