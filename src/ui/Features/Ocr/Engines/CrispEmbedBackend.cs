using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// One OCR backend (model family) for the CrispEmbed engine, e.g. GLM-OCR. The VLM backends run
/// through crispembed-server (model loaded once per OCR session); PP-OCRv6 is a detector plus
/// recognizer pair and runs through the crispembed CLI instead - see <see cref="CrispEmbedOcr"/>.
/// </summary>
public class CrispEmbedBackend
{
    public string Name { get; set; } = string.Empty;
    public List<CrispEmbedModel> Models { get; set; } = new();

    /// <summary>
    /// True when this backend's models need a companion text detector, which also selects the
    /// CLI pipeline execution path instead of the crispembed-server one.
    /// </summary>
    public bool UsesTextDetector => Models.Count > 0 && Models[0].HasDetector;

    public string GetModelPath(CrispEmbedModel model)
    {
        return Path.Combine(CrispEmbedEngine.GetAndCreateModelFolder(), model.Name);
    }

    public string GetDetectorPath(CrispEmbedModel model)
    {
        return model.HasDetector
            ? Path.Combine(CrispEmbedEngine.GetAndCreateModelFolder(), model.DetectorName)
            : string.Empty;
    }

    public bool IsModelInstalled(CrispEmbedModel model)
    {
        if (!IsFilePresent(GetModelPath(model), model.MinimumSizeBytes))
        {
            return false;
        }

        return !model.HasDetector || IsFilePresent(GetDetectorPath(model), model.MinimumDetectorSizeBytes);
    }

    private static bool IsFilePresent(string fileName, long minimumSizeBytes)
    {
        return File.Exists(fileName) && new FileInfo(fileName).Length >= minimumSizeBytes;
    }

    public override string ToString()
    {
        return Name;
    }
}
