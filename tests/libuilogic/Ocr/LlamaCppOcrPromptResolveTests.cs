using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System.Linq;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// LFM2.5-VL is a general vision model that answers the shared OCR prompt's "identify the number
/// of lines" clause literally (a bare "2" line before the text), so it carries its own prompt.
/// That prompt must apply whenever the user has left the shared prompt alone, and must yield to
/// an edited shared prompt - the user's wording drives every model then.
/// </summary>
public class LlamaCppOcrPromptResolveTests
{
    private static LlamaCppModel Lfm => LlamaCppServerManager.OcrModels.Single(m => m.FileName == "LFM2.5-VL-3B-Q8_0.gguf");
    private static LlamaCppModel Glm => LlamaCppServerManager.OcrModels.Single(m => m.FileName == "GLM-OCR-Q8_0.gguf");

    [Fact]
    public void Lfm_ShipsItsOwnPrompt_WithLanguagePlaceholder()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfm25Vl, Lfm.PromptTemplate);
        Assert.Contains("{language}", Lfm.PromptTemplate);
        Assert.DoesNotContain("number of lines", Lfm.PromptTemplate);
    }

    [Fact]
    public void ModelPrompt_Wins_WhenSharedPromptIsDefault()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfm25Vl,
            LlamaCppServerManager.ResolveOcrPrompt(Lfm, SeOcrDefaults.LlamaCppOcrPrompt));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfm25Vl,
            LlamaCppServerManager.ResolveOcrPrompt(Lfm, "  " + SeOcrDefaults.LlamaCppOcrPrompt + "\n"));
    }

    [Fact]
    public void ModelPrompt_Wins_WhenSharedPromptIsBlank()
    {
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfm25Vl, LlamaCppServerManager.ResolveOcrPrompt(Lfm, null));
        Assert.Equal(SeOcrDefaults.LlamaCppOcrPromptLfm25Vl, LlamaCppServerManager.ResolveOcrPrompt(Lfm, "   "));
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
    public void Lfm_IsRankedSecond_BehindGlmOcr()
    {
        Assert.Equal("LFM2.5-VL-3B-Q8_0.gguf", LlamaCppServerManager.OcrModels[1].FileName);
    }
}
