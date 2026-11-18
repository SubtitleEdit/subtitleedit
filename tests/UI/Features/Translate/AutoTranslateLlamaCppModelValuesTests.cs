using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using UITests.Features.Tools.BatchConvert;

namespace UITests.Features.Translate;

/// <summary>
/// A local llama.cpp run sets the trained-in prompt and recommended sampling of the model it starts
/// (Tools.LlamaCppModel*). The regular llama.cpp engine prefers that prompt, and the advanced engines
/// send that sampling for every field the Advanced dialog leaves unset. Nothing saves the values, so
/// every other run, in either window, starts with them cleared: another engine, or llama.cpp against
/// an external server, does not run that model.
/// </summary>
public class AutoTranslateLlamaCppModelValuesTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    /// <summary>The settings snapshot restores strings and bools; this puts the numbers back.</summary>
    private sealed class SamplingScope : IDisposable
    {
        private readonly (double, double, int, double) _saved =
            (Tools.LlamaCppModelTemperature, Tools.LlamaCppModelTopP, Tools.LlamaCppModelTopK, Tools.LlamaCppModelRepeatPenalty);

        public void Dispose() =>
            (Tools.LlamaCppModelTemperature, Tools.LlamaCppModelTopP, Tools.LlamaCppModelTopK, Tools.LlamaCppModelRepeatPenalty) = _saved;
    }

    private static Nikse.SubtitleEdit.Core.Settings.ToolsSettings Tools => Configuration.Settings.Tools;

    // A local model with a trained-in prompt and sampling of its own, as curated entries have.
    private static readonly LlamaCppModel LocalModel = new("Local model", "local-model.gguf", "1 GB", string.Empty,
        PromptTemplate: "Translate from {0} to {1}: {2}", Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05);

    [AvaloniaTheory]
    [InlineData(typeof(OllamaAdvancedTranslate))]
    [InlineData(typeof(LmStudioAdvancedTranslate))]
    public void AutoTranslateWindow_RunOfAnotherEngine_StartsWithoutTheLocalModelsValues(Type engine)
    {
        using var snapshot = new BatchConvertAutoTranslateEngineTests.TranslateSettingsSnapshot();
        using var sampling = new SamplingScope();
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello.", 0, 1000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        try
        {
            window.Show();
            vm.OnLoaded();
            Dispatcher.UIThread.RunJobs(); // the window's own start-up, which restores the last engine
            Select(vm, typeof(LlamaCppTranslate));
            vm.LlamaCppUseRemoteServer = false;
            Dispatcher.UIThread.RunJobs();
            var local = new LlamaCppModelDisplay(LocalModel);
            vm.LlamaCppModels.Add(local); // the drop-down only keeps a selection from its list
            vm.SelectedLlamaCppModel = local;

            vm.SaveSettings(); // as a local llama.cpp run starts
            Assert.Equal(LocalModel.PromptTemplate, Tools.LlamaCppModelPrompt);
            Assert.Equal(LocalModel.Temperature, Tools.LlamaCppModelTemperature);

            Select(vm, engine);
            vm.SaveSettings(); // as a run of the other engine starts

            AssertCleared();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)] // another engine
    [InlineData(true)] // llama.cpp against an external server
    public async Task BatchConvert_RunOfAnotherEngineOrServer_StartsWithoutTheLocalModelsValues(bool llamaCppExternal)
    {
        using var snapshot = new BatchConvertAutoTranslateEngineTests.TranslateSettingsSnapshot();
        using var sampling = new SamplingScope();
        var useRemoteServer = Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer;
        try
        {
            var services = new ServiceCollection();
            services.AddSubtitleEditServices();
            using var provider = services.BuildServiceProvider();
            var viewModel = provider.GetRequiredService<BatchConvertViewModel>();
            viewModel.Window = new Window();
            viewModel.LlamaCppUseRemoteServer = true;
            var config = new BatchConvertConfig();
            config.AutoTranslate.IsActive = true;
            config.AutoTranslate.Translator = llamaCppExternal ? new LlamaCppTranslate() : new LmStudioAdvancedTranslate();
            LlamaCppServerManager.ApplyTranslatePromptSettings(LocalModel); // left by a local llama.cpp run in either window

            Assert.True(await viewModel.EnsureLlamaCppAvailable(config));

            AssertCleared();
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.LlamaCppUseRemoteServer = useRemoteServer;
        }
    }

    private static void Select(AutoTranslateViewModel vm, Type engine)
    {
        vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t.GetType() == engine);
        Dispatcher.UIThread.RunJobs();
    }

    private static void AssertCleared()
    {
        Assert.Equal(string.Empty, Tools.LlamaCppModelPrompt);
        Assert.Equal(-1, Tools.LlamaCppModelTemperature);
        Assert.Equal(-1, Tools.LlamaCppModelTopP);
        Assert.Equal(-1, Tools.LlamaCppModelTopK);
        Assert.Equal(-1, Tools.LlamaCppModelRepeatPenalty);
    }
}
