using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

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

    // PLaMo 3 Translate is driven through its chat template's PFML3 pass-through: the prompt must
    // start with the exact header the template checks for, and cue the output with a language
    // code. It cannot follow the advanced engine's JSON protocol, so it is completion-only too.
    [Fact]
    public void TranslateModels_Plamo3Entries_AreCompletionOnlyWithPfml3PassThroughTemplate()
    {
        var models = LlamaCppServerManager.TranslateModels
            .Where(m => m.FileName.StartsWith("plamo-3-translate", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(models);
        foreach (var model in models)
        {
            Assert.True(model.CompletionOnly);
            Assert.NotNull(model.PromptTemplate);
            Assert.StartsWith("<|plamo:tag|>dataset<|plamo:msg|>translation", model.PromptTemplate);
            Assert.EndsWith("<|plamo:tag|>output<|plamo:key|>lang<|plamo:val|>{4}<|plamo:msg|>", model.PromptTemplate);
            Assert.Null(model.ChatTemplate);
            Assert.False(model.NoJinja);
            Assert.Equal(0, model.Temperature);
        }
    }

    [Fact]
    public void TranslateModels_OtherEntries_AreNotCompletionOnly()
    {
        foreach (var model in LlamaCppServerManager.TranslateModels
                     .Where(m => !m.FileName.Contains("MiLMMT", System.StringComparison.OrdinalIgnoreCase) &&
                                 !m.FileName.StartsWith("plamo-3-translate", System.StringComparison.OrdinalIgnoreCase)))
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

    // A PLaMo 3 Translate quant SE does not curate (the 0.8B, the 31B, a Q2_K) gets the PFML3
    // pass-through prompt; under SE's generic chat prompt the embedded template would translate
    // the instruction along with the text.
    [Theory]
    [InlineData("plamo-3-translate-0.8b-Q4_K_M.gguf")]
    [InlineData("plamo-3-translate-31b-Q4_K_M.gguf")]
    public void CreateCustomModel_Plamo3TranslateFileName_InfersPfml3Prompt(string fileName)
    {
        var model = LlamaCppServerManager.CreateCustomModel(fileName, fileName, "1.0 GB");

        Assert.True(model.CompletionOnly);
        Assert.NotNull(model.PromptTemplate);
        Assert.StartsWith("<|plamo:tag|>dataset<|plamo:msg|>translation", model.PromptTemplate);
        Assert.Contains("{2}", model.PromptTemplate);
        Assert.Contains("{4}", model.PromptTemplate);
        Assert.Equal(0, model.Temperature);
    }

    // A Hy-MT file SE no longer curates (Hy-MT2, downloaded while it was in the list) or a Hy-MT3
    // quant it does not ship keeps Tencent's trained prompt - under SE's generic prompt the model
    // answers in Chinese. Chat models, so not completion-only.
    [Theory]
    [InlineData("Hy-MT2-30B-A3B.i1-IQ3_XXS.gguf")]
    [InlineData("HY-MT2-7B-Q8_0.gguf")]
    [InlineData("Hy-MT3-7B-IQ2.gguf")]
    public void CreateCustomModel_HyMtFileName_InfersTrainedPrompt(string fileName)
    {
        var model = LlamaCppServerManager.CreateCustomModel(fileName, fileName, "1.0 GB");

        Assert.NotNull(model.PromptTemplate);
        Assert.StartsWith("Translate the following text into {1}.", model.PromptTemplate);
        Assert.False(model.CompletionOnly);
        Assert.Equal(0.7, model.Temperature);
    }

    // Tencent's sampling differs per size: the Hy-MT3 30B-A3B wants top_p 1.0 and top_k off,
    // the dense models (and every Hy-MT2) top_p 0.6 / top_k 20 / repetition penalty 1.05.
    [Fact]
    public void CreateCustomModel_HyMt3MoeFileName_GetsMoeSampling()
    {
        var moe = LlamaCppServerManager.CreateCustomModel(
            "Hy-MT3-30B-A3B.i1-IQ3_XXS.gguf", "Hy-MT3-30B-A3B.i1-IQ3_XXS.gguf", "11.6 GB");
        Assert.Equal(1.0, moe.TopP);
        Assert.Equal(0, moe.TopK);
        Assert.Equal(1.0, moe.RepeatPenalty);

        var hyMt2Moe = LlamaCppServerManager.CreateCustomModel(
            "Hy-MT2-30B-A3B.i1-IQ3_XXS.gguf", "Hy-MT2-30B-A3B.i1-IQ3_XXS.gguf", "11.6 GB");
        Assert.Equal(0.6, hyMt2Moe.TopP);
        Assert.Equal(20, hyMt2Moe.TopK);
        Assert.Equal(1.05, hyMt2Moe.RepeatPenalty);
    }

    // PLaMo 2 Translate speaks the older "<|plamo:op|>" format - the PLaMo 3 prompt must not
    // be forced on it.
    [Fact]
    public void CreateCustomModel_Plamo2TranslateFileName_GetsNoPlamo3Prompt()
    {
        var model = LlamaCppServerManager.CreateCustomModel(
            "plamo-2-translate-Q4_K_S.gguf", "plamo-2-translate-Q4_K_S.gguf", "5.5 GB");

        Assert.Null(model.PromptTemplate);
        Assert.False(model.CompletionOnly);
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
