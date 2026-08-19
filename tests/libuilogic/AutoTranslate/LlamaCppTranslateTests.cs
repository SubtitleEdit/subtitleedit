using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using System.Linq;

namespace LibUiLogicTests.AutoTranslate;

public class LlamaCppTranslateTests
{
    [Fact]
    public void TranslateModels_MiLmMtEntries_AreCompletionOnlyWithEmbeddedTextTemplate()
    {
        var models = LlamaCppServerManager.TranslateModels
            .Where(m => m.FileName.Contains("MiLMMT", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(models);
        foreach (var model in models)
        {
            Assert.True(model.CompletionOnly);
            Assert.NotNull(model.PromptTemplate);
            Assert.Contains("{2}", model.PromptTemplate);
            // The GGUF's embedded chat template is a pure passthrough, which is exactly what the
            // raw-completion format needs - a forced template (e.g. "gemma") would wrap the
            // prompt in turn tokens the model was never trained on.
            Assert.Null(model.ChatTemplate);
            Assert.False(model.NoJinja);
            Assert.Equal(0, model.Temperature);
        }
    }

    [Fact]
    public void TranslateModels_NonMiLmMtEntries_AreNotCompletionOnly()
    {
        foreach (var model in LlamaCppServerManager.TranslateModels
                     .Where(m => !m.FileName.Contains("MiLMMT", System.StringComparison.OrdinalIgnoreCase)))
        {
            Assert.False(model.CompletionOnly);
        }
    }

    [Theory]
    [InlineData("MiLMMT-46-12B-v1.0.i1-Q4_K_M.gguf")]
    [InlineData("milmmt-46-1b-v0.1-q8_0.gguf")]
    public void CreateCustomModel_MiLmMtFileName_InfersCompletionPromptAndSampling(string fileName)
    {
        var model = LlamaCppServerManager.CreateCustomModel(fileName, fileName, "1.0 GB");

        Assert.True(model.CompletionOnly);
        Assert.NotNull(model.PromptTemplate);
        Assert.Contains("{2}", model.PromptTemplate);
        Assert.Equal(0, model.Temperature);
        Assert.Null(model.ChatTemplate);
    }

    [Fact]
    public void CreateCustomModel_OtherFileNames_KeepChatTemplateInferenceAndNoPrompt()
    {
        var gemma = LlamaCppServerManager.CreateCustomModel(
            "translategemma-27b-it.Q4_K_M.gguf", "translategemma-27b-it.Q4_K_M.gguf", "16 GB");
        Assert.Equal("gemma", gemma.ChatTemplate);
        Assert.True(gemma.NoJinja);
        Assert.Null(gemma.PromptTemplate);
        Assert.False(gemma.CompletionOnly);

        var plain = LlamaCppServerManager.CreateCustomModel("my-model.gguf", "my-model.gguf", "1 GB");
        Assert.Null(plain.ChatTemplate);
        Assert.Null(plain.PromptTemplate);
        Assert.False(plain.CompletionOnly);
        Assert.Equal(-1, plain.Temperature);
    }
}
