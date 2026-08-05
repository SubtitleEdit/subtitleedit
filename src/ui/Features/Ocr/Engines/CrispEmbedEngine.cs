using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// Shared install/paths/backends for the CrispEmbed OCR engine
/// (https://github.com/CrispStrobe/CrispEmbed) - a local ggml-based OCR engine with
/// multiple model backends, same family as CrispASR. The engine binaries live in
/// &lt;OcrFolder&gt;/CrispEmbed and the GGUF models in a "models" subfolder; OCR runs
/// through crispembed-server so the model is only loaded once per OCR session.
/// </summary>
public static class CrispEmbedEngine
{
    public const string StaticName = "CrispEmbed";
    public const string Url = "https://github.com/CrispStrobe/CrispEmbed";

    public static bool CanBeDownloaded()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            return true;
        }

        // Only an arm64 build is published for macOS.
        return OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
    }

    /// <summary>
    /// Like CrispASR, the Windows variants differ wildly in size (CPU ~6 MB, Vulkan ~25 MB,
    /// CUDA ~684 MB) and the variant is picked at download time, so show a range.
    /// </summary>
    public static string DownloadSizeText
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return "~6 MB – 684 MB";
            }

            if (OperatingSystem.IsLinux())
            {
                return "~10 MB";
            }

            if (OperatingSystem.IsMacOS())
            {
                return "~8 MB";
            }

            return string.Empty;
        }
    }

    public static string GetAndCreateFolder()
    {
        var folder = Se.CrispEmbedFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetAndCreateModelFolder()
    {
        var folder = Path.Combine(GetAndCreateFolder(), "models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// Base names (no .exe suffix) of the executables shipped in a CrispEmbed release archive.
    /// Shared by the post-download chmod loop and the stale-binary cleanup so the lists cannot
    /// drift apart.
    /// </summary>
    public static readonly string[] BinaryBaseNames = { "crispembed-server", "crispembed", "crispembed-quantize" };

    public static string GetServerExecutableFileName()
    {
        return OperatingSystem.IsWindows() ? "crispembed-server.exe" : "crispembed-server";
    }

    public static string GetServerExecutable()
    {
        return Path.Combine(GetAndCreateFolder(), GetServerExecutableFileName());
    }

    public static string GetCliExecutableFileName()
    {
        return OperatingSystem.IsWindows() ? "crispembed.exe" : "crispembed";
    }

    /// <summary>
    /// The single-shot CLI from the same archive. PP-OCRv6 runs through it because
    /// crispembed-server only exposes the detector+recognizer orchestrator when it is also given
    /// a primary embedding model (-m), which Subtitle Edit has no use for - see
    /// <see cref="CrispEmbedOcr"/>.
    /// </summary>
    public static string GetCliExecutable()
    {
        return Path.Combine(GetAndCreateFolder(), GetCliExecutableFileName());
    }

    public static bool IsEngineInstalled()
    {
        return File.Exists(GetServerExecutable());
    }

    /// <summary>
    /// The OCR backends offered in Subtitle Edit - the CrispEmbed models best suited for
    /// subtitle OCR. Model URLs/sizes match cstr's HuggingFace repos as referenced by the
    /// CrispEmbed model registry.
    /// </summary>
    public static List<CrispEmbedBackend> GetBackends()
    {
        return new List<CrispEmbedBackend>
        {
            // PP-OCRv6 (CrispEmbed v0.17.0) is a detector + recognizer pair rather than a VLM:
            // two small GGUFs instead of one ~1 GB model, and it keeps punctuation and per-line
            // breaks on subtitle bitmaps. Only the "medium" tier is offered - "tiny" returns
            // garbage on subtitle-sized text and "small", while as accurate here, saves too
            // little to be worth a second entry (verified 2026-08-04).
            new()
            {
                Name = "PP-OCRv6",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "PP-OCRv6_medium_rec-f16.gguf",
                        Size = "79 MB",
                        Url = "https://huggingface.co/cstr/PP-OCRv6-medium-rec-GGUF/resolve/main/PP-OCRv6_medium_rec-f16.gguf",
                        DetectorName = "PP-OCRv6_medium_det-f16.gguf",
                        DetectorUrl = "https://huggingface.co/cstr/PP-OCRv6-medium-det-GGUF/resolve/main/PP-OCRv6_medium_det-f16.gguf",
                        MinimumSizeBytes = 30_000_000,
                        MinimumDetectorSizeBytes = 30_000_000,
                    },
                },
            },
            new()
            {
                Name = "GLM-OCR",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "glm-ocr-q4_k.gguf",
                        Size = "889 MB",
                        Url = "https://huggingface.co/cstr/glm-ocr-crispembed-GGUF/resolve/main/glm-ocr-q4_k.gguf",
                    },
                    new()
                    {
                        Name = "glm-ocr-q8_0.gguf",
                        Size = "1.18 GB",
                        Url = "https://huggingface.co/cstr/glm-ocr-crispembed-GGUF/resolve/main/glm-ocr-q8_0.gguf",
                    },
                },
            },
            new()
            {
                Name = "GOT-OCR2",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "got-ocr2-q4_k.gguf",
                        Size = "445 MB",
                        Url = "https://huggingface.co/cstr/got-ocr2-crispembed-GGUF/resolve/main/got-ocr2-q4_k.gguf",
                    },
                    new()
                    {
                        Name = "got-ocr2-q8_0.gguf",
                        Size = "600 MB",
                        Url = "https://huggingface.co/cstr/got-ocr2-crispembed-GGUF/resolve/main/got-ocr2-q8_0.gguf",
                    },
                },
            },
            // PaddleOCR-VL (109 languages) omitted for now: in CrispEmbed v0.14.0 both the
            // q4_k and q8_0 quants truncate or hallucinate on subtitle-style single-line
            // images via both the server and CLI single-model paths - verified 2026-07-11.
            new()
            {
                Name = "Qwen3-VL-2B",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "qwen3-vl-2b-q4_k.gguf",
                        Size = "1.59 GB",
                        Url = "https://huggingface.co/cstr/qwen3-vl-2b-crispembed-gguf/resolve/main/qwen3-vl-2b-q4_k.gguf",
                    },
                    new()
                    {
                        Name = "qwen3-vl-2b-q8_0.gguf",
                        Size = "2.29 GB",
                        Url = "https://huggingface.co/cstr/qwen3-vl-2b-crispembed-gguf/resolve/main/qwen3-vl-2b-q8_0.gguf",
                    },
                },
            },
            // DeepSeek-OCR-2 (CrispEmbed v0.17.5): the most accurate backend here on
            // subtitle-style images - the only one exact on a 9-image EN/DE/FR/ES/IT/ZH/JA/RU
            // corpus where GLM-OCR scored 6/9 and GOT-OCR2 0/9 (verified 2026-08-05). ~2 s/image
            // on Apple Silicon with the single-view mode CrispEmbedOcr requests via
            // DS2_CROP_MODE=0. Only the q4_k "stacked" quant is offered: q8_0 measured slightly
            // worse on the same corpus (8/9, drops an umlaut) while being 1.3 GB larger and no
            // faster.
            new()
            {
                Name = "DeepSeek-OCR-2",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "deepseek-ocr2-q4_k-stacked.gguf",
                        Size = "2.31 GB",
                        Url = "https://huggingface.co/cstr/deepseek-ocr2-crispembed-GGUF/resolve/main/deepseek-ocr2-q4_k-stacked.gguf",
                        MinimumSizeBytes = 2_000_000_000,
                    },
                },
            },
        };
    }
}
