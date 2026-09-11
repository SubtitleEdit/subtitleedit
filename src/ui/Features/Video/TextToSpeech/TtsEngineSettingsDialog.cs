using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.CosyVoice3CrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Confucius4TtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DotsTtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.F5TtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AudioCppTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25AudioCppSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.KokoroTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.MossTtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.OmniVoiceCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.OmniVoiceSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.PiperSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.PocketTtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VibeVoiceCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoxCPM2CrispAsrSettings;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Per-engine settings dialog routing, shared by the main TTS window and the review window.
/// The review window regenerates single lines with the same engine, so the knobs that change
/// how a line sounds (emotion, speed, instruction, ...) have to be reachable from there too —
/// otherwise a bad take can only be fixed by closing the review and starting over.
/// </summary>
public static class TtsEngineSettingsDialog
{
    /// <summary>
    /// Whether this engine has a settings dialog worth showing a button for. Cloud engines
    /// other than ElevenLabs have nothing to configure here.
    /// </summary>
    public static bool HasSettings(ITtsEngine? engine) => engine is
        OmniVoiceTtsCpp or
        Qwen3TtsCpp or
        Qwen3TtsCrispAsr or
        VibeVoiceCrispAsr or
        IndexTts25AudioCpp or
        HiggsTtsAudioCpp or
        FishTtsAudioCpp or
        FireRedTts3AudioCpp or
        IndexTtsCrispAsr or
        DotsTtsCrispAsr or
        Confucius4TtsCrispAsr or
        CosyVoice3CrispAsr or
        PocketTtsCrispAsr or
        F5TtsCrispAsr or
        OmniVoiceCrispAsr or
        VoxCPM2CrispAsr or
        MossTtsCrispAsr or
        KokoroTtsCpp or
        ChatterboxTtsCpp or
        Piper or
        ElevenLabs;

    public static async Task ShowAsync(ITtsEngine? engine, Window window, IWindowService windowService)
    {
        if (engine is OmniVoiceTtsCpp)
        {
            await windowService.ShowDialogAsync<OmniVoiceSettingsWindow, OmniVoiceSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is Qwen3TtsCpp)
        {
            await windowService.ShowDialogAsync<Qwen3TtsSettingsWindow, Qwen3TtsSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is Qwen3TtsCrispAsr)
        {
            await windowService.ShowDialogAsync<Qwen3TtsCrispAsrSettingsWindow, Qwen3TtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is VibeVoiceCrispAsr)
        {
            await windowService.ShowDialogAsync<VibeVoiceCrispAsrSettingsWindow, VibeVoiceCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is IndexTts25AudioCpp)
        {
            await windowService.ShowDialogAsync<IndexTts25AudioCppSettingsWindow, IndexTts25AudioCppSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is HiggsTtsAudioCpp)
        {
            await windowService.ShowDialogAsync<AudioCppTtsSettingsWindow, AudioCppTtsSettingsViewModel>(window, vm => vm.Initialize(AudioCppTtsSettingsAdapters.Higgs));
        }
        else if (engine is FishTtsAudioCpp)
        {
            await windowService.ShowDialogAsync<AudioCppTtsSettingsWindow, AudioCppTtsSettingsViewModel>(window, vm => vm.Initialize(AudioCppTtsSettingsAdapters.Fish));
        }
        else if (engine is FireRedTts3AudioCpp)
        {
            await windowService.ShowDialogAsync<AudioCppTtsSettingsWindow, AudioCppTtsSettingsViewModel>(window, vm => vm.Initialize(AudioCppTtsSettingsAdapters.FireRedTts3));
        }
        else if (engine is IndexTtsCrispAsr)
        {
            await windowService.ShowDialogAsync<IndexTtsCrispAsrSettingsWindow, IndexTtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is DotsTtsCrispAsr)
        {
            await windowService.ShowDialogAsync<DotsTtsCrispAsrSettingsWindow, DotsTtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is Confucius4TtsCrispAsr)
        {
            await windowService.ShowDialogAsync<Confucius4TtsCrispAsrSettingsWindow, Confucius4TtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is CosyVoice3CrispAsr)
        {
            await windowService.ShowDialogAsync<CosyVoice3CrispAsrSettingsWindow, CosyVoice3CrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is PocketTtsCrispAsr)
        {
            await windowService.ShowDialogAsync<PocketTtsCrispAsrSettingsWindow, PocketTtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is F5TtsCrispAsr)
        {
            await windowService.ShowDialogAsync<F5TtsCrispAsrSettingsWindow, F5TtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is OmniVoiceCrispAsr)
        {
            await windowService.ShowDialogAsync<OmniVoiceCrispAsrSettingsWindow, OmniVoiceCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is VoxCPM2CrispAsr)
        {
            await windowService.ShowDialogAsync<VoxCPM2CrispAsrSettingsWindow, VoxCPM2CrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is MossTtsCrispAsr)
        {
            await windowService.ShowDialogAsync<MossTtsCrispAsrSettingsWindow, MossTtsCrispAsrSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is KokoroTtsCpp)
        {
            await windowService.ShowDialogAsync<KokoroTtsSettingsWindow, KokoroTtsSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is ChatterboxTtsCpp)
        {
            await windowService.ShowDialogAsync<ChatterboxTtsSettingsWindow, ChatterboxTtsSettingsViewModel>(window, vm => vm.Initialize());
        }
        else if (engine is Piper)
        {
            await windowService.ShowDialogAsync<PiperSettingsWindow, PiperSettingsViewModel>(window, vm => vm.Initialize());
        }
        else
        {
            await windowService.ShowDialogAsync<ElevenLabsSettingsWindow, ElevenLabsSettingsViewModel>(window, vm => { });
        }
    }
}
