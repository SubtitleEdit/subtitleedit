using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Tests for the CrispEmbed OCR engine option in the batch convert settings - the engine list,
/// the backend/model combos, and how they follow the OCR settings shared with the OCR window.
/// </summary>
public class BatchConvertSettingsViewModelCrispEmbedTests
{
    [AvaloniaFact]
    public void OcrEngines_ContainCrispEmbed_WhenAvailableOnPlatform()
    {
        var viewModel = MakeViewModel();

        Assert.Equal(CrispEmbedEngine.CanBeDownloaded(),
            viewModel.OcrEngines.Contains(CrispEmbedEngine.StaticName));
    }

    [AvaloniaFact]
    public void SelectingCrispEmbed_ShowsBackendAndModel_AndHidesLanguage()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var viewModel = MakeViewModel();

        viewModel.SelectedOcrEngine = CrispEmbedEngine.StaticName;
        viewModel.OnOcrEngineChanged();

        Assert.True(viewModel.IsCrispEmbedVisible);
        Assert.False(viewModel.IsOcrLanguageVisible);
        Assert.NotNull(viewModel.SelectedCrispEmbedBackend);
        Assert.NotNull(viewModel.SelectedCrispEmbedModel);
    }

    [AvaloniaFact]
    public void BackendCombo_FollowsOcrSettings_AndSwitchRepopulatesModels()
    {
        var savedBackend = Se.Settings.Ocr.CrispEmbedBackend;
        var savedModel = Se.Settings.Ocr.CrispEmbedModel;
        try
        {
            Se.Settings.Ocr.CrispEmbedBackend = "GLM-OCR";
            Se.Settings.Ocr.CrispEmbedModel = "glm-ocr-q4_k.gguf";

            var viewModel = MakeViewModel();

            Assert.Equal("GLM-OCR", viewModel.SelectedCrispEmbedBackend?.Name);
            Assert.Equal("glm-ocr-q4_k.gguf", viewModel.SelectedCrispEmbedModel?.Model.Name);

            var otherBackend = viewModel.CrispEmbedBackends.First(p => p.Name == "GOT-OCR2");
            viewModel.SelectedCrispEmbedBackend = otherBackend;

            Assert.All(viewModel.CrispEmbedModels, m => Assert.Equal(otherBackend, m.Backend));
            Assert.NotNull(viewModel.SelectedCrispEmbedModel);
        }
        finally
        {
            Se.Settings.Ocr.CrispEmbedBackend = savedBackend;
            Se.Settings.Ocr.CrispEmbedModel = savedModel;
        }
    }

    private static BatchConvertSettingsViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertSettingsViewModel>();
    }
}
