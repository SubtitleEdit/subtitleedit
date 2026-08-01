using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// GigaAM-v3 (ai-sage) — a Russian-only ASR model, new as the <c>gigaam</c> backend in
/// CrispASR v0.8.25. Rotary Conformer encoder with four published revisions: CTC and RNN-T
/// heads, each in a character-level and an SPM ("e2e") variant.
///
/// The <c>e2e</c> heads emit punctuation and capitalisation natively, which is what subtitles
/// want — the plain character-level heads return a bare lowercase transcript. Verified against
/// the pinned v0.8.25 binary on Apple M4 / Metal with `say -v Milena` audio:
///   e2e-rnnt-q8_0 → "Привет." / "Это тест распознавания" / "русской речи." (27x realtime)
///   ctc-q4_k      → "привет это тест распознавания русской речи" (no case, no punctuation)
/// so the e2e revisions are listed first and RNN-T is the registry default upstream picks.
///
/// The backend reports Russian as its only language and CrispASR skips language detection for
/// it ("gigaam is ru-only — skipping language detection"). Passing <c>-l ru</c> is accepted;
/// any other code is ignored with a warning, so the language list holds Russian alone.
/// </summary>
public class CrispAsrGigaAm : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR GigaAM";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrGigaAm;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "gigaam";
    public override string DefaultLanguage => "ru";
    public override bool IncludeLanguage => true;
    public override bool HasNativeTimestamps => true;

    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("ru", "russian"),
        };

    public override List<WhisperModel> Models =>
       new()
       {
            // e2e (SPM) revisions first: punctuation + capitalisation straight out of the
            // model, so no separate punctuation-restoration pass is needed.
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-rnnt-q8_0.gguf",
                Size = "249 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-rnnt-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-rnnt-q4_k.gguf",
                Size = "154 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-rnnt-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-rnnt-f16.gguf",
                Size = "452 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-rnnt-f16.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-ctc-q8_0.gguf",
                Size = "245 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-ctc-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-ctc-q4_k.gguf",
                Size = "151 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-ctc-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-e2e-ctc-f16.gguf",
                Size = "449 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-e2e-ctc-f16.gguf",
                ],
            },

            // Character-level heads: lowercase, unpunctuated output. Kept for users who feed
            // the transcript through their own casing/punctuation step, or who want the raw
            // character alignment rather than SPM pieces.
            new WhisperModel
            {
                Name = "gigaam-v3-rnnt-q8_0.gguf",
                Size = "247 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-rnnt-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-rnnt-q4_k.gguf",
                Size = "153 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-rnnt-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-rnnt-f16.gguf",
                Size = "451 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-rnnt-f16.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-ctc-q8_0.gguf",
                Size = "245 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-ctc-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-ctc-q4_k.gguf",
                Size = "151 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-ctc-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "gigaam-v3-ctc-f16.gguf",
                Size = "449 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/gigaam-v3-ctc-f16.gguf",
                ],
            },
       };

    public override string Extension => string.Empty;
    public override string UnpackSkipFolder => string.Empty;

    public override bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return CrispAsrEngine.GetBackendDisplayName(this);
    }

    public override string GetAndCreateWhisperFolder()
    {
        var folder = Se.CrispAsrFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(folder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public override string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public override bool IsModelInstalled(WhisperModel model)
    {
        var modelFile = GetModelForCmdLine(model.Name);
        if (!File.Exists(modelFile))
        {
            return false;
        }

        return new FileInfo(modelFile).Length > 10_000_000;
    }

    public override string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        return modelFileName;
    }

    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    internal static string GetExecutableFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "crispasr.exe";
        }

        return "crispasr";
    }

    public override bool CanBeDownloaded()
    {
        return true;
    }

    public override string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrGigaAm;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrGigaAm = value;
    }
}
