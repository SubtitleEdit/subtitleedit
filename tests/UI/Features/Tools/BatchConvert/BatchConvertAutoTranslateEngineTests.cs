using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic.Config;

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
            AutomationProperties.GetName(b) == Se.Language.Translate.AdvancedSettings);
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
    /// and filled from its own settings, not the classic engine's.
    /// </summary>
    [AvaloniaFact]
    public void OllamaAdvanced_ShowsItsOwnUrlAndModel()
    {
        var viewModel = MakeViewModel();
        Se.Settings.AutoTranslate.OllamaAdvancedUrl = "http://example.local:11434/v1/chat/completions";
        Se.Settings.AutoTranslate.OllamaAdvancedModel = "qwen3:8b";

        viewModel.SelectedAutoTranslator = viewModel.AutoTranslators.First(t => t is OllamaAdvancedTranslate);
        viewModel.OnAutoTranslatorChanged();

        Assert.True(viewModel.AutoTranslateUrlIsVisible);
        Assert.True(viewModel.AutoTranslateModelIsVisible);
        Assert.True(viewModel.AutoTranslateModelBrowseIsVisible);
        Assert.False(viewModel.AutoTranslateApiKeyIsVisible);
        Assert.Equal("http://example.local:11434/v1/chat/completions", viewModel.AutoTranslateUrl);
        Assert.Equal("qwen3:8b", viewModel.AutoTranslateModel);
    }

    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }
}
