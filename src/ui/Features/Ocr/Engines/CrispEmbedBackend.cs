using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// One OCR backend (model family) for the CrispEmbed engine, e.g. GLM-OCR. All backends run
/// through the same crispembed-server executable; only the GGUF model differs.
/// </summary>
public class CrispEmbedBackend
{
    public string Name { get; set; } = string.Empty;
    public List<CrispEmbedModel> Models { get; set; } = new();

    public string GetModelPath(CrispEmbedModel model)
    {
        return Path.Combine(CrispEmbedEngine.GetAndCreateModelFolder(), model.Name);
    }

    public bool IsModelInstalled(CrispEmbedModel model)
    {
        var modelFile = GetModelPath(model);
        if (!File.Exists(modelFile))
        {
            return false;
        }

        return new FileInfo(modelFile).Length > 10_000_000;
    }

    public override string ToString()
    {
        return Name;
    }
}
