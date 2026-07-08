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

    public static List<VideoOcrEngineItem> GetEngines()
    {
        var list = new List<VideoOcrEngineItem>();

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            list.Add(new("Paddle OCR Standalone", OcrEngineType.PaddleOcrStandalone, "Local OCR engine (downloaded automatically) - fast and accurate"));
        }

        list.Add(new("Paddle OCR Python", OcrEngineType.PaddleOcrPython, "Local OCR engine via Python (requires \"pip install paddleocr\")"));
        list.Add(new("Ollama vision", OcrEngineType.Ollama, "Local vision model via Ollama - e.g. glm-ocr"));
        list.Add(new("llama.cpp", OcrEngineType.LlamaCpp, "Local vision model via llama.cpp (downloaded automatically)"));
        list.Add(new("GLM API", OcrEngineType.Glm, "GLM vision model via Z.ai / bigmodel.cn API (requires API key)"));

        return list;
    }
}
