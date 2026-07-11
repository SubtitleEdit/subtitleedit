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
    /// Like CrispASR, the Windows variants differ wildly in size (CPU ~5 MB, Vulkan ~24 MB,
    /// CUDA ~691 MB) and the variant is picked at download time, so show a range.
    /// </summary>
    public static string DownloadSizeText
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "~5 MB – 691 MB";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "~8 MB";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "~6 MB";
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
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "crispembed-server.exe" : "crispembed-server";
    }

    public static string GetServerExecutable()
    {
        return Path.Combine(GetAndCreateFolder(), GetServerExecutableFileName());
    }

    public static bool IsEngineInstalled()
    {
        return File.Exists(GetServerExecutable());
    }

    /// <summary>
    /// The OCR backends offered in Subtitle Edit - the three CrispEmbed models best suited
    /// for subtitle OCR. Model URLs/sizes match cstr's HuggingFace repos as referenced by
    /// the CrispEmbed v0.14.0 model registry.
    /// </summary>
    public static List<CrispEmbedBackend> GetBackends()
    {
        return new List<CrispEmbedBackend>
        {
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
            new()
            {
                Name = "PaddleOCR-VL",
                Models = new List<CrispEmbedModel>
                {
                    new()
                    {
                        Name = "paddleocr-vl-0.9b-q4_k.gguf",
                        Size = "1.3 GB",
                        Url = "https://huggingface.co/cstr/paddleocr-vl-0.9b-GGUF/resolve/main/paddleocr-vl-0.9b-q4_k.gguf",
                    },
                    new()
                    {
                        Name = "paddleocr-vl-0.9b-q8_0.gguf",
                        Size = "1.48 GB",
                        Url = "https://huggingface.co/cstr/paddleocr-vl-0.9b-GGUF/resolve/main/paddleocr-vl-0.9b-q8_0.gguf",
                    },
                },
            },
        };
    }
}
