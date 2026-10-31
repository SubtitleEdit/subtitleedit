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

    // LFM2.5-VL left the curated list, but it still answers the shared prompt's line count literally.
    [Theory]
    [InlineData("LFM3-VL-2B-Q4_K_M")]
    [InlineData("LFM2.5-VL-3B-Q8_0")]
    public void UrlMode_LfmVisionModels_GetTheLfmPrompt(string modelName)
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, LlamaCppOcrEngine.ResolveUrlModePrompt(modelName, null));
    }

    [Fact]
    public void UrlMode_OtherModels_KeepTheSharedOrUserPrompt()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, LlamaCppOcrEngine.ResolveUrlModePrompt("glmocr", null));
        Assert.Equal("my prompt", LlamaCppOcrEngine.ResolveUrlModePrompt("LFM2.5-VL-3B-Q8_0", "my prompt"));
    }

    [Fact]
    public void FullPathToAnLfmVisionModel_GetsTheLfmPrompt()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        try
        {
            var modelPath = Path.Combine(folder, "LFM2.5-VL-3B-Q8_0.gguf");
            File.WriteAllText(modelPath, string.Empty);
            File.WriteAllText(Path.Combine(folder, "mmproj-LFM2.5-VL-3B-Q8_0.gguf"), string.Empty);

            var model = LlamaCppOcrEngine.ResolveOcrModel(modelPath);

            Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, model.PromptTemplate);
            Assert.Equal(modelPath, model.FileName);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
