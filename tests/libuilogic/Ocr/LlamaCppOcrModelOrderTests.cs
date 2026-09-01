using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using System.Linq;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The curated OCR model order is behavior, not presentation: the first entry is preselected in the
/// OCR/Video OCR/batch-convert dropdowns when nothing is saved, and the headless callers (seconv's
/// LlamaCppOcrEngine, BatchConverter) fall back to the first *installed* entry. Appending a new
/// model without ranking it would therefore hand real jobs to a worse model. See the measurements
/// on <see cref="LlamaCppServerManager.OcrModels"/>.
/// </summary>
public class LlamaCppOcrModelOrderTests
{
    [Fact]
    public void GlmOcr_IsFirst_SoItIsTheDefaultPick()
    {
        Assert.Equal("GLM-OCR-Q8_0.gguf", LlamaCppServerManager.OcrModels[0].FileName);
    }

    [Fact]
    public void LightOnOcr_IsLast_TheWeakestAndSlowestOfTheCuratedModels()
    {
        Assert.Equal("LightOnOCR-1B-1025-Q8_0.gguf", LlamaCppServerManager.OcrModels[^1].FileName);
    }

    [Fact]
    public void EveryCuratedOcrModel_ShipsAVisionProjector()
    {
        Assert.All(LlamaCppServerManager.OcrModels, m =>
        {
            Assert.False(string.IsNullOrEmpty(m.MmprojFileName), m.DisplayName + " needs an mmproj sidecar");
            Assert.False(string.IsNullOrEmpty(m.MmprojUrl), m.DisplayName + " needs an mmproj download URL");
        });
    }

    [Fact]
    public void CuratedOcrModels_HaveNoDuplicateFileNames()
    {
        var names = LlamaCppServerManager.OcrModels.Select(m => m.FileName).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
