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
    public void AdvancedButton_IsShownForLlamaCppAdvancedOnly()
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

            Assert.Equal(engine is LlamaCppAdvancedTranslate, viewModel.LlamaCppAdvancedButtonIsVisible);
        }
    }

    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }
}
