using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrEngine : CrispAsrEngineBase
{
    private const string NamePrefix = "Crisp ASR ";
    private readonly List<CrispAsrEngineBase> _backends;

    public static string StaticName => "Crisp ASR";
    public IReadOnlyList<CrispAsrEngineBase> Backends => _backends;
    public CrispAsrEngineBase SelectedBackend { get; private set; }
    public CrispAsrEngine()
    {
        // Ordered by quality, best first; the first backend is the default. General-purpose
        // backends come first, ranked by published English Open ASR Leaderboard WER (vendor
        // numbers, not measured in SE) - except where SE's own FLEURS runs disagree: Parakeet
        // (Ultra) measured best in SE (default since #16988), and Granite measured at roughly twice
        // Parakeet's WER despite its published score, so it closes the group. Specialists (one
        // language family, diarization, translation, coverage over accuracy) follow.
        _backends = new List<CrispAsrEngineBase>
        {
            new CrispAsrParakeet(),   // Ultra 5.80, 25 languages - SE-measured best
            new CrispAsrArk(),        // 5.04, 20 languages
            new CrispAsrQwen3(),      // 5.40, 31 languages
            new CrispAsrCohere(),     // 5.42, 15 languages
            new CrispAsrKyutai(),     // 2.6B 6.40, English/French
            new CrispAsrCanary(),     // 7.15, 25 languages
            new CrispAsrVoxtral(),    // Mini 3B, 9 languages
            new CrispAsrGranite(),    // published 5.33, SE-measured 9-12% on FLEURS en/de

            // Chinese: FireRedASR2 3.05 vs Fun-ASR-Nano 4.55 average Mandarin CER (FireRed report).
            new CrispAsrFireRed(),
            new CrispAsrFunAsrNano(),
            //new CrispAsrFunAsrMltNano(), does not work well enough
            new CrispAsrSenseVoice(), // zh/yue/ja/ko/en
            new CrispAsrGlm(),        // zh/yue/en

            new CrispAsrGigaAm(),     // Russian only
            new CrispAsrMega(),       // Qwen3-ASR fine-tune for noisy audio, slightly worse on clean speech
            new CrispAsrOmni(),       // broadest coverage (150 listed), accuracy second
            new CrispAsrMossDiarize(),// transcription with speaker labels
            new CrispAsrIndexEcho(),  // Chinese speech -> translated subtitles
        };

        SelectedBackend = _backends[0];
    }

    public bool TrySelectBackendChoice(string choice)
    {
        var backend = _backends.FirstOrDefault(p =>
            string.Equals(p.Choice, choice, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, choice, StringComparison.OrdinalIgnoreCase));

        if (backend == null)
        {
            return false;
        }

        SelectBackend(backend);
        return true;
    }

    public void SelectBackend(CrispAsrEngineBase backend)
    {
        var match = _backends.FirstOrDefault(p => ReferenceEquals(p, backend) || string.Equals(p.Choice, backend.Choice, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            SelectedBackend = match;
        }
    }

    public static string GetBackendDisplayName(ICrispAsrEngine backend)
    {
        return backend.Name.StartsWith(NamePrefix, StringComparison.OrdinalIgnoreCase)
            ? backend.Name[NamePrefix.Length..]
            : backend.Name;
    }

    public override string Name => StaticName;
    public override string Choice => SelectedBackend.Choice;
    public override string Url => SelectedBackend.Url;
    public override string BackendName => SelectedBackend.BackendName;
    public override string DefaultLanguage => SelectedBackend.DefaultLanguage;
    public override bool IncludeLanguage => SelectedBackend.IncludeLanguage;
    public override bool HasNativeTimestamps => SelectedBackend.HasNativeTimestamps;
    public override Version? GetMinimumCrispAsrVersion(WhisperModel model) => SelectedBackend.GetMinimumCrispAsrVersion(model);
    public override List<WhisperLanguage> Languages => SelectedBackend.Languages;
    public override List<WhisperModel> Models => SelectedBackend.Models;
    public override string Extension => SelectedBackend.Extension;
    public override string UnpackSkipFolder => SelectedBackend.UnpackSkipFolder;
    public override string CommandLineParameter
    {
        get => SelectedBackend.CommandLineParameter;
        set => SelectedBackend.CommandLineParameter = value;
    }

    public override bool IsEngineInstalled() => SelectedBackend.IsEngineInstalled();
    public override bool CanBeDownloaded() => SelectedBackend.CanBeDownloaded();
    public override string DownloadSizeText => SelectedBackend.DownloadSizeText;
    public override string GetAndCreateWhisperFolder() => SelectedBackend.GetAndCreateWhisperFolder();
    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => SelectedBackend.GetAndCreateWhisperModelFolder(whisperModel);
    public override string GetExecutable() => SelectedBackend.GetExecutable();
    public override bool IsModelInstalled(WhisperModel model) => SelectedBackend.IsModelInstalled(model);
    public override string GetModelForCmdLine(string modelName) => SelectedBackend.GetModelForCmdLine(modelName);
    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => SelectedBackend.GetWhisperModelDownloadFileName(whisperModel, url);
    public override Task<string> GetHelpText() => SelectedBackend.GetHelpText();

    public override string ToString()
    {
        return Name;
    }
}
