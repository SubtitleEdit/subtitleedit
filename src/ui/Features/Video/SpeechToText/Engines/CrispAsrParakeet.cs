using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrParakeet : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Parakeet";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrParakeet;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "parakeet";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => true;
    public override bool HasNativeTimestamps => true;

    /// <summary>
    /// crispasr's <c>parakeet</c> backend is the transducer (TDT / RNN-T) runtime. A pure-CTC
    /// Parakeet GGUF (NeMo EncDecCTCModelBPE: encoder + CTC head, no prediction network or joint)
    /// makes it bail out with "this GGUF has no RNN-T decoder/joint tensors"; those models run on
    /// the <c>fastconformer-ctc</c> backend instead, and crispasr's own auto-detect picks that by
    /// the same filename rule used here - "parakeet" and "ctc" without "tdt". The tdt_ctc hybrids
    /// keep their TDT head and stay on <c>parakeet</c>. Timestamps still come for free (CTC frame
    /// times), so the built-in aligner option keeps applying.
    /// </summary>
    public const string CtcBackendName = "fastconformer-ctc";

    public static bool IsPureCtcModel(string modelName)
    {
        return modelName.Contains("parakeet", StringComparison.OrdinalIgnoreCase) &&
               modelName.Contains("ctc", StringComparison.OrdinalIgnoreCase) &&
               !modelName.Contains("tdt", StringComparison.OrdinalIgnoreCase);
    }

    public override string GetBackendName(string modelName)
    {
        return IsPureCtcModel(modelName) ? CtcBackendName : BackendName;
    }

    /// <summary>
    /// crispasr auto-enables FireRedPunc punctuation restoration for the fastconformer-ctc
    /// backend, because its usual models emit bare lowercase text. The Vietnamese CTC model is
    /// trained with punctuation and casing already, and FireRedPunc's Chinese/English vocabulary
    /// re-joins the Vietnamese words it does not know without spaces ("thời tiếtởHà Nội"), so the
    /// stage has to stay off. <c>--no-punctuation</c> is the wrong lever: with it crispasr strips
    /// the model's own punctuation too. <c>--punc-model none</c> only skips the restoration.
    /// </summary>
    public override string GetModelArguments(string modelName, string? userArguments)
    {
        if (!IsPureCtcModel(modelName))
        {
            return string.Empty;
        }

        if (Regex.IsMatch(userArguments ?? string.Empty, @"(^|\s)(--punc-model|--no-punctuation|--require-punctuation)\b"))
        {
            return string.Empty;
        }

        return "--punc-model none";
    }

    public override List<WhisperLanguage> Languages =>
       new()
       {
            new WhisperLanguage("auto", "Auto detect"),
            new WhisperLanguage("en", "english"),
            new WhisperLanguage("es", "spanish"),
            new WhisperLanguage("fr", "french"),
            new WhisperLanguage("de", "german"),
            new WhisperLanguage("it", "italian"),
            new WhisperLanguage("pt", "portuguese"),
            new WhisperLanguage("zh", "chinese"),
            new WhisperLanguage("ja", "japanese"),
            new WhisperLanguage("ko", "korean"),
            new WhisperLanguage("ru", "russian"),
            new WhisperLanguage("pl", "polish"),
            new WhisperLanguage("tr", "turkish"),
            new WhisperLanguage("nl", "dutch"),
            new WhisperLanguage("vi", "vietnamese"),
       };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v3-q4_k.gguf",
                Size = "489 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v3-GGUF/resolve/main/parakeet-tdt-0.6b-v3-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v3-q5_0.gguf",
                Size = "541 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v3-GGUF/resolve/main/parakeet-tdt-0.6b-v3-q5_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v3-q8_0.gguf",
                Size = "745 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v3-GGUF/resolve/main/parakeet-tdt-0.6b-v3-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v3.gguf",
                Size = "1.26 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v3-GGUF/resolve/main/parakeet-tdt-0.6b-v3.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-ja-q4_k.gguf",
                Size = "476 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-ja-GGUF/resolve/main/parakeet-tdt-0.6b-ja-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                // Upstream's recommended default for ja since CrispASR v0.8.8:
                // TDT output byte-identical to F16 at roughly half the size.
                Name = "parakeet-tdt-0.6b-ja-q8_0.gguf",
                Size = "744 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-ja-GGUF/resolve/main/parakeet-tdt-0.6b-ja-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-ja.gguf",
                Size = "1.25 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-ja-GGUF/resolve/main/parakeet-tdt-0.6b-ja.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-rnnt-0.6b-q4_k.gguf",
                Size = "468 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-rnnt-0.6b-GGUF/resolve/main/parakeet-rnnt-0.6b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-rnnt-0.6b-f16.gguf",
                Size = "1.24 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-rnnt-0.6b-GGUF/resolve/main/parakeet-rnnt-0.6b-f16.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-rnnt-1.1b-q4_k.gguf",
                Size = "808 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-rnnt-1.1b-GGUF/resolve/main/parakeet-rnnt-1.1b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-rnnt-1.1b-f16.gguf",
                Size = "2.14 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-rnnt-1.1b-GGUF/resolve/main/parakeet-rnnt-1.1b-f16.gguf",
                ],
            },

            // Largest TDT variant - the accuracy ceiling for this backend. Roughly
            // 1.3x-1.6x the download of tdt-0.6b-v3 depending on quant (652 MB vs
            // 489 MB at q4_k, 2.00 GB vs 1.26 GB at F16).
            new WhisperModel
            {
                Name = "parakeet-tdt-1.1b-q4_k.gguf",
                Size = "652 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-1.1b-GGUF/resolve/main/parakeet-tdt-1.1b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-1.1b-q8_0.gguf",
                Size = "1.07 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-1.1b-GGUF/resolve/main/parakeet-tdt-1.1b-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-1.1b.gguf",
                Size = "2.00 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-1.1b-GGUF/resolve/main/parakeet-tdt-1.1b.gguf",
                ],
            },

            // v2 predates the multilingual v3: English only, but trained for mixed-case
            // output with punctuation, which suits subtitling better than a bare lowercase
            // transcript. Worth keeping alongside v3 for English-only jobs.
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v2-q4_k.gguf",
                Size = "379 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v2-GGUF/resolve/main/parakeet-tdt-0.6b-v2-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v2-q8_0.gguf",
                Size = "634 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v2-GGUF/resolve/main/parakeet-tdt-0.6b-v2-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt-0.6b-v2.gguf",
                Size = "1.15 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt-0.6b-v2-GGUF/resolve/main/parakeet-tdt-0.6b-v2.gguf",
                ],
            },

            // Smallest model on this backend by a wide margin - the q4_k build is 75 MB,
            // for low-end CPUs where even the 0.6b variants are too slow. TDT head, so
            // native timestamps still apply.
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-110m-q4_k.gguf",
                Size = "75 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-110m-GGUF/resolve/main/parakeet-tdt_ctc-110m-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-110m-q8_0.gguf",
                Size = "121 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-110m-GGUF/resolve/main/parakeet-tdt_ctc-110m-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-110m.gguf",
                Size = "219 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-110m-GGUF/resolve/main/parakeet-tdt_ctc-110m.gguf",
                ],
            },

            // Hybrid TDT+CTC at 1.1b - same TDT decoding path (and native timestamps) as
            // parakeet-tdt-1.1b, with a CTC head available to the runtime.
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-1.1b-q4_k.gguf",
                Size = "654 MB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-1.1b-GGUF/resolve/main/parakeet-tdt_ctc-1.1b-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-1.1b-q8_0.gguf",
                Size = "1.07 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-1.1b-GGUF/resolve/main/parakeet-tdt_ctc-1.1b-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-tdt_ctc-1.1b.gguf",
                Size = "2.00 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/parakeet-tdt_ctc-1.1b-GGUF/resolve/main/parakeet-tdt_ctc-1.1b.gguf",
                ],
            },

            // nvidia/parakeet-ctc-0.6b-Vietnamese (#14496): the one Parakeet trained on Vietnamese
            // (2,000+ h, with punctuation and casing). Pure CTC - no TDT head - so GetBackendName
            // sends it to fastconformer-ctc and GetModelArguments keeps crispasr's punctuation
            // restoration off; timestamps come from the CTC frames. Converted from the .nemo with
            // CrispASR's convert-stt-fastconformer-ctc-to-gguf.py and crispasr-quantize 0.8.31; the
            // four quants agree to within a word on FLEURS vi_vn clips, q8_0 is the safe default.
            new WhisperModel
            {
                Name = "parakeet-ctc-0.6b-vi-q4_k.gguf",
                Size = "384 MB",
                Urls =
                [
                    "https://huggingface.co/niksedk/parakeet-ctc-0.6b-vi-GGUF/resolve/main/parakeet-ctc-0.6b-vi-q4_k.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-ctc-0.6b-vi-q5_0.gguf",
                Size = "450 MB",
                Urls =
                [
                    "https://huggingface.co/niksedk/parakeet-ctc-0.6b-vi-GGUF/resolve/main/parakeet-ctc-0.6b-vi-q5_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-ctc-0.6b-vi-q8_0.gguf",
                Size = "650 MB",
                Urls =
                [
                    "https://huggingface.co/niksedk/parakeet-ctc-0.6b-vi-GGUF/resolve/main/parakeet-ctc-0.6b-vi-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "parakeet-ctc-0.6b-vi.gguf",
                Size = "1.22 GB",
                Urls =
                [
                    "https://huggingface.co/niksedk/parakeet-ctc-0.6b-vi-GGUF/resolve/main/parakeet-ctc-0.6b-vi.gguf",
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
        var modelsFolder = Se.CrispAsrModelsFolder;
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
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrParakeet;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrParakeet = value;
    }
}