using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The Apple Vision option in batch convert settings.
///
/// The one trap an engine with its own language combo hits here is the shared "Language" label:
/// it is shown for every engine that is not on an exclusion list, so an engine that brings its
/// own combo shows two language controls at once unless it is added to that list. That is the
/// mistake worth a test, and it is checked on every OS - the exclusion is a string comparison
/// with no framework behind it, so it can go wrong on Linux CI just as easily.
/// </summary>
public class BatchConvertSettingsViewModelAppleVisionTests
{
    [AvaloniaFact]
    public void OcrEngines_ContainAppleVision_OnlyWhereItRuns()
    {
        var viewModel = MakeViewModel();

        Assert.Equal(AppleVisionOcr.IsAvailable(), viewModel.OcrEngines.Contains(AppleVisionOcr.StaticName));
    }

    [AvaloniaFact]
    public void SelectingAppleVision_ShowsItsOwnLanguage_AndHidesTheSharedOne()
    {
        var viewModel = MakeViewModel();

        viewModel.SelectedOcrEngine = AppleVisionOcr.StaticName;
        viewModel.OnOcrEngineChanged();

        Assert.True(viewModel.IsAppleVisionVisible);
        Assert.False(viewModel.IsOcrLanguageVisible);

        // Nothing else may claim the row at the same time.
        Assert.False(viewModel.IsCrispEmbedVisible);
        Assert.False(viewModel.IsTesseractOcrVisible);
        Assert.False(viewModel.IsPaddleOCrVisible);
    }

    [AvaloniaFact]
    public void SelectingAnotherEngine_HidesAppleVision()
    {
        var viewModel = MakeViewModel();

        viewModel.SelectedOcrEngine = AppleVisionOcr.StaticName;
        viewModel.OnOcrEngineChanged();
        viewModel.SelectedOcrEngine = "nOcr";
        viewModel.OnOcrEngineChanged();

        Assert.False(viewModel.IsAppleVisionVisible);
    }

    [AvaloniaFact]
    public void LanguageCombo_FollowsTheSavedBatchSetting()
    {
        if (!AppleVisionOcr.IsAvailable())
        {
            Assert.Skip("Apple Vision is macOS only.");
            return;
        }

        var saved = Se.Settings.Tools.BatchConvert.AppleVisionLanguage;
        try
        {
            // Any second language this machine's Vision supports; the point is that the combo
            // restores what was saved rather than always landing on the en-US default.
            var other = AppleVisionOcr.GetLanguages().First(l => l.Code != "en-US").Code;
            Se.Settings.Tools.BatchConvert.AppleVisionLanguage = other;

            var viewModel = MakeViewModel();
            viewModel.SelectedOcrEngine = AppleVisionOcr.StaticName;
            viewModel.OnOcrEngineChanged();

            Assert.Equal(other, viewModel.SelectedAppleVisionLanguage?.Code);
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.AppleVisionLanguage = saved;
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
