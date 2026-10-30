using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The LFM vision models (LFM3-VL, LFM2.5-VL before it) answer the shared OCR prompt's "identify the
/// number of lines" clause literally ("There are two lines..." before the text), so they carry their own prompt.
/// That prompt must apply whenever the user has left the shared prompt alone, and must yield to
/// an edited shared prompt - the user's wording drives every model then.
/// </summary>
public class LlamaCppOcrPromptResolveTests
{
    private static LlamaCppModel Lfm => LlamaCppServerManager.OcrModels.Single(m => m.FileName == "LFM3-VL-2B-Q4_K_M.gguf");
    private static LlamaCppModel Glm => LlamaCppServerManager.OcrModels.Single(m => m.FileName == "GLM-OCR-Q8_0.gguf");

    [Fact]
    public void Lfm_ShipsItsOwnPrompt_WithLanguagePlaceholder()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, Lfm.PromptTemplate);
        Assert.Contains("{language}", Lfm.PromptTemplate);
        Assert.DoesNotContain("number of lines", Lfm.PromptTemplate);
    }

    [Fact]
    public void ModelPrompt_Wins_WhenSharedPromptIsDefault()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl,
            LlamaCppServerManager.ResolveOcrPrompt(Lfm, SeOcrDefaults.LlamaCppOcrPrompt));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl,
            LlamaCppServerManager.ResolveOcrPrompt(Lfm, "  " + SeOcrDefaults.LlamaCppOcrPrompt + "\n"));
    }

    [Fact]
    public void ModelPrompt_Wins_WhenSharedPromptIsBlank()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, LlamaCppServerManager.ResolveOcrPrompt(Lfm, null));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, LlamaCppServerManager.ResolveOcrPrompt(Lfm, "   "));
    }

    [Fact]
    public void EditedSharedPrompt_Wins_OverModelPrompt()
    {
        const string custom = "Read the text. The language is {language}.";
        Assert.Equal(custom, LlamaCppServerManager.ResolveOcrPrompt(Lfm, custom));
    }

    [Fact]
    public void ModelWithoutOwnPrompt_GetsSharedPrompt()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, LlamaCppServerManager.ResolveOcrPrompt(Glm, SeOcrDefaults.LlamaCppOcrPrompt));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, LlamaCppServerManager.ResolveOcrPrompt(Glm, null));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, LlamaCppServerManager.ResolveOcrPrompt(null, ""));
    }

    [Fact]
    public void Lfm_KeepsThinking_WhichStopsItMergingLinesAndSwappingNoteMarks()
    {
        Assert.False(Lfm.NoThinking);
    }

    [Theory]
    [InlineData("LFM2.5-VL-3B-Q8_0.gguf")] // downloaded while it was curated, now a custom model
    [InlineData("LFM3-VL-2B-Q8_0.gguf")]
    [InlineData("lfm3-vl-2b-qad-q4_0.gguf")]
    public void CustomLfmVisionModel_GetsTheLfmPrompt(string fileName)
    {
        var model = LlamaCppServerManager.CreateCustomOcrModel(fileName, fileName, "1 GB", "mmproj-" + fileName);
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl, model.PromptTemplate);
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfmVl,
            LlamaCppServerManager.ResolveOcrPrompt(model, SeOcrDefaults.LlamaCppOcrPrompt));
    }

    [Theory]
    [InlineData("my-vision-model.gguf")]
    [InlineData("GLM-OCR-Q4_K_M.gguf")]
    [InlineData("LFM2.5-1.2B-Instruct-Q4_K_M.gguf")] // text-only LFM, no "-VL"
    public void OtherCustomModel_GetsTheSharedPrompt(string fileName)
    {
        var model = LlamaCppServerManager.CreateCustomOcrModel(fileName, fileName, "1 GB", "mmproj-" + fileName);
        Assert.Null(model.PromptTemplate);
    }
}
