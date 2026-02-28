using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class OcrEngineItem
{
    public string Name { get; set; }
    public OcrEngineType EngineType { get; set; }
    public string Description { get; set; }
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }

    public OcrEngineItem(string name, OcrEngineType engineType, string description, string apiKey, string endpoint)
    {
        Name = name;
        EngineType = engineType;
        Description = description;
        ApiKey = apiKey;
        Endpoint = endpoint;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<OcrEngineItem> GetOcrEngines()
    {
        var list = new List<OcrEngineItem>();

        list.Add(new("nOcr", OcrEngineType.nOcr, "nOcr is an internal OCR engine (free/open source)", "", ""));
        list.Add(new("Binary image compare", OcrEngineType.BinaryImageCompare, "OCR via image compare (free/open source)", "", ""));
        list.Add(new("Tesseract", OcrEngineType.Tesseract, "Tesseract is an open-source OCR engine", "", ""));

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            list.Add(new("Paddle OCR Standalone", OcrEngineType.PaddleOcrStandalone, "Paddle OCR Standalone", "", ""));
        }

        list.Add(new("Paddle OCR Python", OcrEngineType.PaddleOcrPython, "Paddle OCR Python", "", ""));
        list.Add(new("Ollama", OcrEngineType.Ollama, "Ollama e.g. via llama-vision", "", "http://localhost:11434/api/chat"));
        list.Add(new("Google Vision", OcrEngineType.GoogleVision, "Google Vision is a cloud-based OCR engine by Google", "", ""));

        //list.Add(new("Azure Vision", OcrEngineType.AzureVision, "Azure Vision is a cloud-based OCR engine by Microsoft", "", ""));
        //list.Add(new("Amazon Rekognition", OcrEngineType.AmazonRekognition, "Amazon Rekognition is a cloud-based OCR engine by Amazon", "", ""));

        list.Add(new("Mistral OCR", OcrEngineType.Mistral, "Mistral OCR is a cloud-based OCR engine", "", ""));

        list.Add(new("Google Lens Sharp", OcrEngineType.GoogleLensSharp, "Google Lens (free, but capped) cloud-based OCR engine by Google", "", ""));
        if (OperatingSystem.IsWindows())
        {
            list.Add(new("Google Lens Standalone", OcrEngineType.GoogleLens, "Google Lens (free, but capped) cloud-based OCR engine by Google", "", ""));
        }

        return list;
    }
}