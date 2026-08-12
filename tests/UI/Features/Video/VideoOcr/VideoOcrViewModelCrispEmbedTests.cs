using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;
using System.Reflection;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// Tests for the CrispEmbed engine option in Video OCR - the engine list, the backend/model
/// combos, and how they follow the per-feature Video OCR settings (deliberately NOT the OCR
/// window's shared settings: the best backend for burned-in video is not the best one for
/// subtitle bitmaps).
/// </summary>
public class VideoOcrViewModelCrispEmbedTests
{
    [AvaloniaFact]
    public void Engines_ContainCrispEmbed_WhenAvailableOnPlatform()
    {
        var engines = VideoOcrEngineItem.GetEngines();

        Assert.Equal(CrispEmbedEngine.CanBeDownloaded(),
            engines.Any(p => p.EngineType == OcrEngineType.CrispEmbed));
    }

    [AvaloniaFact]
    public void SelectingCrispEmbed_ShowsItsPanel_AndHidesTheOthers()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var viewModel = MakeViewModel();

        viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType == OcrEngineType.CrispEmbed);

        Assert.True(viewModel.IsCrispEmbedEngine);
        Assert.False(viewModel.IsPaddleEngine);
        Assert.False(viewModel.IsOllamaEngine);
        Assert.False(viewModel.IsGlmEngine);
        Assert.False(viewModel.IsLlamaCppEngine);
        Assert.NotNull(viewModel.SelectedCrispEmbedBackend);
        Assert.NotNull(viewModel.SelectedCrispEmbedModel);
    }

    [AvaloniaFact]
    public void BackendCombo_FollowsVideoOcrSettings_AndSwitchRepopulatesModels()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var settings = Se.Settings.Video.VideoOcr;
        var savedBackend = settings.CrispEmbedBackend;
        var savedModel = settings.CrispEmbedModel;
        try
        {
            settings.CrispEmbedBackend = "GLM-OCR";
            settings.CrispEmbedModel = "glm-ocr-q4_k.gguf";

            var viewModel = MakeViewModel();
            viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType == OcrEngineType.CrispEmbed);

            Assert.Equal("GLM-OCR", viewModel.SelectedCrispEmbedBackend?.Name);
            Assert.Equal("glm-ocr-q4_k.gguf", viewModel.SelectedCrispEmbedModel?.Model.Name);

            var otherBackend = viewModel.CrispEmbedBackends.First(p => p.Name == "GOT-OCR2");
            viewModel.SelectedCrispEmbedBackend = otherBackend;

            Assert.All(viewModel.CrispEmbedModels, m => Assert.Equal(otherBackend, m.Backend));
            Assert.NotNull(viewModel.SelectedCrispEmbedModel);

            // Browsing the combos must not touch the saved choice - the window may still be
            // cancelled. It lands in the Video OCR settings (not the OCR window's) on save.
            Assert.Equal("GLM-OCR", settings.CrispEmbedBackend);

            Invoke(viewModel, "SaveSettings");
            Assert.Equal("GOT-OCR2", settings.CrispEmbedBackend);
            Assert.Equal(viewModel.SelectedCrispEmbedModel?.Model.Name, settings.CrispEmbedModel);
        }
        finally
        {
            settings.CrispEmbedBackend = savedBackend;
            settings.CrispEmbedModel = savedModel;
        }
    }

    [AvaloniaFact]
    public void CrispEmbedSettings_AreSeparateFromTheOcrWindowSettings()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var videoSettings = Se.Settings.Video.VideoOcr;
        var savedVideoBackend = videoSettings.CrispEmbedBackend;
        var savedOcrBackend = Se.Settings.Ocr.CrispEmbedBackend;
        try
        {
            Se.Settings.Ocr.CrispEmbedBackend = "GLM-OCR";
            videoSettings.CrispEmbedBackend = "GOT-OCR2";

            var viewModel = MakeViewModel();
            viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType == OcrEngineType.CrispEmbed);

            Assert.Equal("GOT-OCR2", viewModel.SelectedCrispEmbedBackend?.Name);
            Assert.Equal("GLM-OCR", Se.Settings.Ocr.CrispEmbedBackend);
        }
        finally
        {
            videoSettings.CrispEmbedBackend = savedVideoBackend;
            Se.Settings.Ocr.CrispEmbedBackend = savedOcrBackend;
        }
    }

    private static void Invoke(object viewModel, string method) =>
        viewModel.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(viewModel, null);

    private static VideoOcrViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<VideoOcrViewModel>();
    }
}
