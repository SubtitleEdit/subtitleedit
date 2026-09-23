using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// anime.ja is fine-tuned on transcription only and ignores the translate task, so "Translate to
/// English" produced Japanese (#15223). Selecting it hides and clears the option.
/// </summary>
public class SpeechToTextTranscribeOnlyModelTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static SpeechToTextModelDisplay Display(WhisperModel model) => new()
    {
        Model = model,
        Engine = new WhisperEnginePurfviewFasterWhisperXxl(),
    };

    [AvaloniaFact]
    public void TranscribeOnlyModel_HidesAndClearsTranslate()
    {
        var vm = new SpeechToTextViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());
        var models = new WhisperPurfviewFasterWhisperModel().Models;
        vm.SelectedModel = Display(models.Single(m => m.Name == "large-v3"));
        var translateVisibleForLargeV3 = vm.IsTranslateVisible;
        Assert.True(translateVisibleForLargeV3);
        vm.DoTranslateToEnglish = translateVisibleForLargeV3;

        vm.SelectedModel = Display(models.Single(m => m.Name == "anime.ja"));

        Assert.False(vm.IsTranslateVisible);
        Assert.False(vm.DoTranslateToEnglish);

        vm.SelectedModel = Display(models.Single(m => m.Name == "large-v3"));

        Assert.Equal(translateVisibleForLargeV3, vm.IsTranslateVisible);
    }

    [AvaloniaFact]
    public void AnimeJa_IsTranscribeOnlyForBothEngines()
    {
        Assert.True(new WhisperPurfviewFasterWhisperModel().Models.Single(m => m.Name == "anime.ja").TranscribeOnly);
        Assert.True(new WhisperCppModel().Models.Single(m => m.Name == "anime.ja").TranscribeOnly);
    }
}
