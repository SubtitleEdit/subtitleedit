using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;

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
    public void LightOnOcr3_4B_IsSecond_AsAccurateAsGlmOcrButFourTimesTheDownload()
    {
        Assert.Equal("LightOnOCR-3-4B.Q8_0.gguf", LlamaCppServerManager.OcrModels[1].FileName);
    }

    [Fact]
    public void LightOnOcr3_08B_IsLast_TheFastestButLeastAccurateOnRealSubtitles()
    {
        Assert.Equal("LightOnOCR-3-0.8B-Q8_0.gguf", LlamaCppServerManager.OcrModels[^1].FileName);
    }

    [Theory]
    [InlineData("LightOnOCR-3-4B.Q8_0.gguf")]
    [InlineData("LightOnOCR-3-0.8B-Q8_0.gguf")]
    public void LightOnOcr3_RunsWithoutThinkingAndWithTheBorder(string fileName)
    {
        // Qwen 3.5 VL thinks by default and then answers into reasoning_content, leaving the
        // content field the OCR engines read empty; the square pad cost accuracy on every corpus.
        var model = LlamaCppServerManager.OcrModels.Single(m => m.FileName == fileName);
        Assert.True(model.NoThinking);
        Assert.Equal(AiOcrPadding.Border, LlamaCppServerManager.GetOcrPadding(System.IO.Path.GetFileNameWithoutExtension(fileName)));
    }

    [Fact]
    public void QariOcr_RanksBehindTheGeneralModels_BecauseItIsAnArabicSpecialist()
    {
        // Best on Arabic, but it merges every two-line subtitle, so it must not become the
        // headless fallback ahead of GLM-OCR/LFM2.5-VL/PaddleOCR-VL/HunyuanOCR.
        Assert.Equal("Qari-OCR-0.2.2.1-VL-2B-Instruct-merged.Q8_0.gguf", LlamaCppServerManager.OcrModels[^2].FileName);
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
