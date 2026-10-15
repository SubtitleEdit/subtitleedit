using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// With --ocr-url, --ocr-model is only a name, and it is what the curated padding and prompt
/// lookups match on. A dotted name given without ".gguf" ("LightOnOCR-3-0.8B-Q8_0") used to lose
/// everything after its last dot, so the model was sent the square instead of its border.
/// </summary>
public class LlamaCppOcrModelStemTest
{
    [Theory]
    [InlineData("LightOnOCR-3-0.8B-Q8_0", "LightOnOCR-3-0.8B-Q8_0")]
    [InlineData("LightOnOCR-3-0.8B-Q8_0.gguf", "LightOnOCR-3-0.8B-Q8_0")]
    [InlineData("LightOnOCR-3-4B.Q8_0", "LightOnOCR-3-4B.Q8_0")]
    [InlineData("LightOnOCR-3-4B.Q8_0.GGUF", "LightOnOCR-3-4B.Q8_0")]
    [InlineData(" GLM-OCR-Q8_0.gguf ", "GLM-OCR-Q8_0")]
    public void StripsOnlyTheGgufExtension(string model, string expected)
    {
        Assert.Equal(expected, LlamaCppOcrEngine.ModelStem(model));
    }

    [Theory]
    [InlineData("LightOnOCR-3-0.8B-Q8_0")]
    [InlineData("LightOnOCR-3-4B.Q8_0")]
    public void DottedCuratedNamesWithoutExtension_GetTheirBorderPadding(string model)
    {
        Assert.Equal(AiOcrPadding.Border, LlamaCppServerManager.GetOcrPadding(LlamaCppOcrEngine.ModelStem(model)));
    }
}
