using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

// The llamacpp tests plant a fake install in AppContext.BaseDirectory/llama.cpp - the first
// (highest-priority) probe candidate - so resolution never depends on a real Subtitle Edit
// install or a llama-server on PATH. Static server-manager overrides are reset on dispose.
public class AutoTranslateRunnerTest : IDisposable
{
    private readonly string _fakeLlamaFolder = Path.Combine(AppContext.BaseDirectory, "llama.cpp");

    private readonly string _defaultLlamaCppPrompt = Configuration.Settings.Tools.LlamaCppPrompt;
    private readonly string _defaultOllamaPrompt = Configuration.Settings.Tools.OllamaPrompt;
    private readonly string _defaultLmStudioPrompt = Configuration.Settings.Tools.LmStudioPrompt;

    public AutoTranslateRunnerTest()
    {
        LlamaCppServerManager.FolderOverride = null;
        LlamaCppServerManager.ExecutableOverride = null;
    }

    public void Dispose()
    {
        LlamaCppServerManager.FolderOverride = null;
        LlamaCppServerManager.ExecutableOverride = null;

        // The prompt settings are process-wide; put them back for the next test.
        Configuration.Settings.Tools.LlamaCppPrompt = _defaultLlamaCppPrompt;
        Configuration.Settings.Tools.LlamaCppModelPrompt = string.Empty;
        Configuration.Settings.Tools.OllamaPrompt = _defaultOllamaPrompt;
        Configuration.Settings.Tools.LmStudioPrompt = _defaultLmStudioPrompt;
        if (Directory.Exists(_fakeLlamaFolder))
            Directory.Delete(_fakeLlamaFolder, recursive: true);
    }

    private static ConversionOptions MakeOptions(string engine = "llamacpp", string? url = null, string? model = null, string to = "de", string? prompt = null)
    {
        return new ConversionOptions
        {
            Patterns = ["in.srt"],
            Format = "subrip",
            TranslateTo = to,
            TranslateEngine = engine,
            TranslateUrl = url,
            TranslateModel = model,
            TranslatePrompt = prompt,
            Quiet = true,
        };
    }

    private void PlantFakeInstall(params string[] modelFileNames)
    {
        Directory.CreateDirectory(_fakeLlamaFolder);
        var exeName = OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server";
        File.WriteAllText(Path.Combine(_fakeLlamaFolder, exeName), "fake");

        var models = Path.Combine(_fakeLlamaFolder, "models");
        Directory.CreateDirectory(models);
        foreach (var name in modelFileNames)
        {
            // IsModelInstalled requires > 10 MB; a sparse file keeps this fast.
            using var fs = File.Create(Path.Combine(models, name));
            fs.SetLength(11_000_000);
        }
    }

    [Fact]
    public void Create_UnsupportedEngine_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.Create(MakeOptions(engine: "bing")));
        Assert.Contains("not supported", ex.Message);
        Assert.Contains("llamacpp", ex.Message);
    }

    [Fact]
    public void Create_Ollama_AppliesUrlAndModel()
    {
        var runner = AutoTranslateRunner.Create(MakeOptions(engine: "ollama", url: "http://myhost:11434/api/generate", model: "gemma2"));

        Assert.NotNull(runner);
        Assert.Equal("http://myhost:11434/api/generate", Configuration.Settings.Tools.OllamaApiUrl);
        Assert.Equal("gemma2", Configuration.Settings.Tools.OllamaModel);
    }

    [Fact]
    public void Create_LmStudio_DefaultsUrl()
    {
        AutoTranslateRunner.Create(MakeOptions(engine: "lmstudio"));

        Assert.Equal("http://localhost:1234/v1/chat/completions", Configuration.Settings.Tools.LmStudioApiUrl);
    }

    [Theory]
    [InlineData("http://myhost:8080", "http://myhost:8080/v1/chat/completions")]
    [InlineData("http://myhost:8080/", "http://myhost:8080/v1/chat/completions")]
    [InlineData("http://myhost:8080/v1/chat/completions", "http://myhost:8080/v1/chat/completions")]
    public void Create_LlamaCppWithUrl_CompletesEndpoint(string url, string expected)
    {
        AutoTranslateRunner.Create(MakeOptions(url: url));

        Assert.Equal(expected, Configuration.Settings.Tools.LlamaCppApiUrl);
    }

    [Fact]
    public void Create_LlamaCppLocal_NoInstall_ThrowsWithInstructions()
    {
        // No fake install planted; guard against a real llama-server on the test machine's
        // PATH or in the user's SE data folder making this env-dependent - if one exists,
        // Create legitimately succeeds, so only assert the error when it throws.
        try
        {
            AutoTranslateRunner.Create(MakeOptions());
        }
        catch (InvalidOperationException ex)
        {
            Assert.True(
                ex.Message.Contains("llama-server not found") || ex.Message.Contains("No llama.cpp translate model"),
                $"Unexpected message: {ex.Message}");
        }
    }

    [Fact]
    public void Create_LlamaCppLocal_PicksFirstInstalledCuratedModel()
    {
        PlantFakeInstall("translategemma-4b_Q5_K_M.gguf");

        var runner = AutoTranslateRunner.Create(MakeOptions());

        Assert.NotNull(runner.LlamaCppModel);
        Assert.Equal("translategemma-4b_Q5_K_M.gguf", runner.LlamaCppModel!.FileName);
        // Curated TranslateGemma entries carry the gemma chat-template override.
        Assert.Equal("gemma", runner.LlamaCppModel.ChatTemplate);
        Assert.True(runner.LlamaCppModel.NoJinja);
    }

    [Fact]
    public void Create_LlamaCppLocal_CustomModelByName()
    {
        PlantFakeInstall("my-own-model.gguf");

        var runner = AutoTranslateRunner.Create(MakeOptions(model: "my-own-model"));

        Assert.NotNull(runner.LlamaCppModel);
        Assert.Equal("my-own-model.gguf", runner.LlamaCppModel!.FileName);
        // Unknown family: leave the embedded template alone.
        Assert.Null(runner.LlamaCppModel.ChatTemplate);
        Assert.False(runner.LlamaCppModel.NoJinja);
    }

    [Fact]
    public void Create_LlamaCppLocal_ModelByFullPath_InheritsCuratedTemplateFlags()
    {
        PlantFakeInstall();
        var path = Path.Combine(_fakeLlamaFolder, "models", "Qwen_Qwen3-8B-Q4_K_M.gguf");
        using (var fs = File.Create(path))
        {
            fs.SetLength(11_000_000);
        }

        var runner = AutoTranslateRunner.Create(MakeOptions(model: path));

        Assert.NotNull(runner.LlamaCppModel);
        Assert.Equal(path, runner.LlamaCppModel!.FileName);
        Assert.Null(runner.LlamaCppModel.ChatTemplate);
        Assert.False(runner.LlamaCppModel.NoJinja);
        Assert.True(runner.LlamaCppModel.NoThinking);
    }

    // A TranslateGemma size/quant we do not curate (issue #12440 asked for the 27B). Whether it is
    // dropped into the models folder or passed as a full path, it still needs gemma + --no-jinja:
    // TranslateGemma's embedded Jinja template is non-standard, so serving it unflagged is broken.
    [Fact]
    public void Create_LlamaCppLocal_UncuratedTranslateGemmaByName_GetsGemmaTemplate()
    {
        PlantFakeInstall("translategemma-27b-it.Q4_K_M.gguf");

        var runner = AutoTranslateRunner.Create(MakeOptions(model: "translategemma-27b-it.Q4_K_M"));

        Assert.NotNull(runner.LlamaCppModel);
        Assert.Equal("translategemma-27b-it.Q4_K_M.gguf", runner.LlamaCppModel!.FileName);
        Assert.Equal("gemma", runner.LlamaCppModel.ChatTemplate);
        Assert.True(runner.LlamaCppModel.NoJinja);
    }

    [Fact]
    public void Create_LlamaCppLocal_UncuratedTranslateGemmaByFullPath_GetsGemmaTemplate()
    {
        PlantFakeInstall();
        var path = Path.Combine(_fakeLlamaFolder, "models", "translategemma-27b-it.Q5_K_M.gguf");
        using (var fs = File.Create(path))
        {
            fs.SetLength(11_000_000);
        }

        var runner = AutoTranslateRunner.Create(MakeOptions(model: path));

        Assert.NotNull(runner.LlamaCppModel);
        Assert.Equal("gemma", runner.LlamaCppModel!.ChatTemplate);
        Assert.True(runner.LlamaCppModel.NoJinja);
    }

    [Theory]
    [InlineData("translategemma-4b_Q5_K_M.gguf", "gemma", true, false)]   // curated: exact match
    [InlineData("translategemma-27b-it.Q4_K_M.gguf", "gemma", true, false)] // uncurated: inferred
    [InlineData("google_gemma-3-27b-it-Q4_K_M.gguf", "gemma", true, false)]
    [InlineData("Qwen_Qwen3.5-32B-Q4_K_M.gguf", null, false, true)]
    [InlineData("aya-expanse-8b-Q4_K_M.gguf", null, false, false)]        // curated: embedded template
    [InlineData("Meta-Llama-3.1-70B-Instruct-Q4_K_M.gguf", null, false, false)]
    [InlineData("some-unknown-model.gguf", null, false, false)]
    // Gemma 4 keeps its embedded template but must not think - both spellings of the family, and
    // both the curated entry and a self-supplied quant (e.g. a fine-tune) we do not list.
    [InlineData("google_gemma-4-E4B-it-Q4_K_M.gguf", null, false, true)]  // curated
    [InlineData("gemma-4-27B-it-Q4_K_M.gguf", null, false, true)]         // uncurated size
    [InlineData("translate_gemma4_sub-E4B-Q4_K_XL.gguf", null, false, true)]
    // "translategemma-4b" is TranslateGemma at 4B, not Gemma 4 - it still needs the gemma template.
    [InlineData("translategemma-4b_Q6_K.gguf", "gemma", true, false)]
    public void InferChatTemplate_PicksFlagsByFamily(string fileName, string? expectedTemplate, bool expectedNoJinja, bool expectedNoThinking)
    {
        var (chatTemplate, noJinja, noThinking) = LlamaCppServerManager.InferChatTemplate(fileName);

        Assert.Equal(expectedTemplate, chatTemplate);
        Assert.Equal(expectedNoJinja, noJinja);
        Assert.Equal(expectedNoThinking, noThinking);
    }

    [Fact]
    public void Create_LlamaCppLocal_UnknownModelName_Throws()
    {
        PlantFakeInstall("translategemma-4b_Q5_K_M.gguf");

        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.Create(MakeOptions(model: "no-such-model")));
        Assert.Contains("not found", ex.Message);
    }

    [Theory]
    [InlineData(null, "llamacpp")]
    [InlineData("", "llamacpp")]
    [InlineData("llama.cpp", "llamacpp")]
    [InlineData("LlamaCpp", "llamacpp")]
    [InlineData("ollama", "ollama")]
    public void NormalizeEngine_ResolvesAliasesAndDefault(string? engine, string expected)
    {
        Assert.Equal(expected, AutoTranslateRunner.NormalizeEngine(engine));
    }

    [Theory]
    [InlineData(null, true)] // default engine is llamacpp
    [InlineData("llama.cpp", true)]
    [InlineData("ollama", true)]
    [InlineData("lmstudio", true)]
    [InlineData("libretranslate", false)]
    [InlineData("nllb-serve", false)]
    [InlineData("nllb-api", false)]
    public void SupportsPrompt_OnlyForLlmEngines(string? engine, bool expected)
    {
        Assert.Equal(expected, AutoTranslateRunner.SupportsPrompt(engine));
    }

    [Fact]
    public void ReadPromptOption_NotGiven_ReturnsNull()
    {
        Assert.Null(AutoTranslateRunner.ReadPromptOption(null));
    }

    [Fact]
    public void ReadPromptOption_InlineText_UnescapesLineBreaks()
    {
        var prompt = AutoTranslateRunner.ReadPromptOption("Translate this from {0} to {1}:\\n{0}: {2}\\n{1}:");

        Assert.Equal("Translate this from {0} to {1}:\n{0}: {2}\n{1}:", prompt);
    }

    [Fact]
    public void ReadPromptOption_KeepsUnknownEscapesAndBackslashes()
    {
        Assert.Equal("keep \\d and \\ here", AutoTranslateRunner.ReadPromptOption("keep \\d and \\\\ here"));
    }

    [Fact]
    public void ReadPromptOption_Empty_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ReadPromptOption("   "));
        Assert.Contains("empty", ex.Message);
    }

    [Fact]
    public void ReadPromptOption_File_IsReadFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        File.WriteAllText(path, "Translate this from {0} to {1}:\n{0}: {2}\n{1}:\n");
        try
        {
            Assert.Equal("Translate this from {0} to {1}:\n{0}: {2}\n{1}:", AutoTranslateRunner.ReadPromptOption(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    // A path-shaped value is a path even when it does not exist - saying so beats sending
    // "prompts/mine.tmpl" to the model as the prompt, which would silently translate a whole
    // batch under a garbage instruction.
    [Theory]
    [InlineData("no-such-prompt.txt")]      // prompt-file extension
    [InlineData("prompts/mine.tmpl")]       // any extension, no spaces
    [InlineData("./missing")]               // no extension at all
    [InlineData("my prompts/milmmt.txt")]   // spaces in the path, prompt-file extension
    public void ReadPromptOption_MissingPromptFile_Throws(string value)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ReadPromptOption(value));
        Assert.Contains("not found", ex.Message);
    }

    // ... while anything sentence-shaped stays inline text, placeholders included.
    [Theory]
    [InlineData("Translate from {0} to {1}:")]
    [InlineData("{0}->{1}:")]                       // terse, no spaces, but has placeholders
    [InlineData("Translate this. Keep line breaks.")]
    public void ReadPromptOption_SentenceShapedValue_StaysInline(string value)
    {
        Assert.Equal(value, AutoTranslateRunner.ReadPromptOption(value));
    }

    [Fact]
    public void ReadPromptOption_ExistingFile_WinsOverAnyShape()
    {
        // No extension and no spaces, but it exists - read it.
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", string.Empty));
        File.WriteAllText(path, "Translate from {0} to {1}:");
        try
        {
            Assert.Equal("Translate from {0} to {1}:", AutoTranslateRunner.ReadPromptOption(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadPromptOption_HugeFile_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        using (var fs = File.Create(path))
        {
            fs.SetLength(300 * 1024); // e.g. --translate-prompt pointed at a data file by mistake
        }

        try
        {
            var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ReadPromptOption(path));
            Assert.Contains("too large", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Create_Ollama_AppliesPrompt()
    {
        AutoTranslateRunner.Create(MakeOptions(engine: "ollama", prompt: "Translate {0} to {1} like a pirate:"));

        Assert.Equal("Translate {0} to {1} like a pirate:", Configuration.Settings.Tools.OllamaPrompt);
    }

    [Fact]
    public void Create_LmStudio_AppliesPrompt()
    {
        AutoTranslateRunner.Create(MakeOptions(engine: "lmstudio", prompt: "Translate {0} to {1} like a pirate:"));

        Assert.Equal("Translate {0} to {1} like a pirate:", Configuration.Settings.Tools.LmStudioPrompt);
    }

    [Fact]
    public void Create_LlamaCppRemote_AppliesPrompt()
    {
        AutoTranslateRunner.Create(MakeOptions(url: "http://myhost:8080", prompt: "Translate {0} to {1}:"));

        Assert.Equal("Translate {0} to {1}:", Configuration.Settings.Tools.LlamaCppPrompt);
        Assert.Equal("Translate {0} to {1}:", Configuration.Settings.Tools.LlamaCppModelPrompt);
    }

    // LlamaCppTranslate prefers the per-model template over the plain prompt, and the curated
    // template is re-applied before every file - so an explicit --translate-prompt has to win
    // there too, or it would silently do nothing for exactly the models people run headless.
    [Fact]
    public void ApplyPromptSettings_CustomPrompt_BeatsCuratedModelTemplate()
    {
        PlantFakeInstall("MiLMMT-46-4B-v1.0.Q4_K_M.gguf");

        var runner = AutoTranslateRunner.Create(MakeOptions(prompt: "Translate {0} to {1}:"));
        Assert.NotNull(runner.LlamaCppModel);
        Assert.False(string.IsNullOrEmpty(runner.LlamaCppModel!.PromptTemplate));

        runner.ApplyPromptSettings(); // what TranslateAsync does before each file

        Assert.Equal("Translate {0} to {1}:", Configuration.Settings.Tools.LlamaCppModelPrompt);
        Assert.Equal("Translate {0} to {1}:", Configuration.Settings.Tools.LlamaCppPrompt);
        // The model's sampling recommendations still apply - only the prompt is overridden.
        Assert.Equal(0, Configuration.Settings.Tools.LlamaCppModelTemperature);
    }

    [Fact]
    public void ApplyPromptSettings_NoCustomPrompt_KeepsCuratedModelTemplate()
    {
        PlantFakeInstall("MiLMMT-46-4B-v1.0.Q4_K_M.gguf");

        var runner = AutoTranslateRunner.Create(MakeOptions());
        runner.ApplyPromptSettings();

        Assert.Equal(runner.LlamaCppModel!.PromptTemplate, Configuration.Settings.Tools.LlamaCppModelPrompt);
    }

    [Fact]
    public void Create_PromptWithEngineThatHasNone_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => AutoTranslateRunner.Create(MakeOptions(engine: "libretranslate", prompt: "Translate {0} to {1}:")));

        Assert.Contains("--translate-prompt is not supported", ex.Message);
        Assert.Contains("llamacpp, ollama, lmstudio", ex.Message);
    }

    [Fact]
    public void ResolveLanguage_MatchesCodeAndName_CaseInsensitive()
    {
        var languages = new OllamaTranslate().GetSupportedTargetLanguages();

        Assert.Equal("de", AutoTranslateRunner.ResolveLanguage(languages, "de", "target").TwoLetterIsoLanguageName);
        Assert.Equal("de", AutoTranslateRunner.ResolveLanguage(languages, "german", "target").TwoLetterIsoLanguageName);
        Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ResolveLanguage(languages, "klingon", "target"));
    }

    // The OpenAI-style ".../v1" base (no trailing slash) used to fail the old "/v1/" check
    // and come out as ".../v1/v1/chat/completions".
    [Theory]
    [InlineData("http://localhost:8080", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/v1", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/v1/", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/v1/chat/completions", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/V1/Chat/Completions", "http://localhost:8080/V1/Chat/Completions")]
    public void CompleteChatCompletionsUrl_CompletesWithoutDoublingV1(string input, string expected)
    {
        Assert.Equal(expected, AutoTranslateRunner.CompleteChatCompletionsUrl(input));
    }
}
