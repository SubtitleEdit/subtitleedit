using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public class VideoOcrEngineItem
{
    public string Name { get; set; }
    public OcrEngineType EngineType { get; set; }
    public string Description { get; set; }

    public VideoOcrEngineItem(string name, OcrEngineType engineType, string description)
    {
        Name = name;
        EngineType = engineType;
        Description = description;
    }

    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// Best measured engine first (burned-in subtitle clips of real footage with real SRTs
    /// as ground truth, 2026-08-26): Apple Vision led on accuracy and speed with zero setup;
    /// Paddle Standalone was next and is by far the fastest batch engine on Windows/Linux;
    /// the local VLM engines (CrispEmbed / llama.cpp, both defaulting to GLM-OCR) follow at
    /// roughly 1 s/frame; Ollama needs a user-managed install and the GLM API needs a key,
    /// so they close the list. The pip-based Paddle Python engine was dropped from this list:
    /// getting a matching Python + paddle install working is the most support-heavy path,
    /// and every remaining engine is either built in or downloaded automatically.
    /// </summary>
    public static List<VideoOcrEngineItem> GetEngines()
    {
        var list = new List<VideoOcrEngineItem>();

        // Nothing to download and no key: on macOS this works the moment the window opens.
        if (AppleVisionOcr.IsAvailable())
        {
            list.Add(new(AppleVisionOcr.StaticName, OcrEngineType.AppleVision,
                "macOS's built-in OCR engine - local, no download needed"));
        }

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            list.Add(new("Paddle OCR Standalone", OcrEngineType.PaddleOcrStandalone, "Local OCR engine (downloaded automatically) - fast and accurate"));
        }

        if (CrispEmbedEngine.CanBeDownloaded())
        {
            list.Add(new(CrispEmbedEngine.StaticName, OcrEngineType.CrispEmbed,
                "Local OCR engine with multiple model backends (downloaded automatically)"));
        }

        list.Add(new("llama.cpp", OcrEngineType.LlamaCpp, "Local vision model via llama.cpp (downloaded automatically)"));
        list.Add(new("Ollama vision", OcrEngineType.Ollama, "Local vision model via Ollama - e.g. glm-ocr"));
        list.Add(new("GLM API", OcrEngineType.Glm, "GLM vision model via Z.ai / bigmodel.cn API (requires API key)"));

        return list;
    }
}
