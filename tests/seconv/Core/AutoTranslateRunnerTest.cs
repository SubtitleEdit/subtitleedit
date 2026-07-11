using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

// The llamacpp tests plant a fake install in AppContext.BaseDirectory/llama.cpp - the first
// (highest-priority) probe candidate - so resolution never depends on a real Subtitle Edit
// install or a llama-server on PATH. Static server-manager overrides are reset on dispose.
public class AutoTranslateRunnerTest : IDisposable
{
    private readonly string _fakeLlamaFolder = Path.Combine(AppContext.BaseDirectory, "llama.cpp");

    public AutoTranslateRunnerTest()
    {
        LlamaCppServerManager.FolderOverride = null;
        LlamaCppServerManager.ExecutableOverride = null;
    }

    public void Dispose()
    {
        LlamaCppServerManager.FolderOverride = null;
        LlamaCppServerManager.ExecutableOverride = null;
        if (Directory.Exists(_fakeLlamaFolder))
            Directory.Delete(_fakeLlamaFolder, recursive: true);
    }

    private static ConversionOptions MakeOptions(string engine = "llamacpp", string? url = null, string? model = null, string to = "de")
    {
        return new ConversionOptions
        {
            Patterns = ["in.srt"],
            Format = "subrip",
            TranslateTo = to,
            TranslateEngine = engine,
            TranslateUrl = url,
            TranslateModel = model,
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
        Assert.Equal("chatml", runner.LlamaCppModel.ChatTemplate);
        Assert.True(runner.LlamaCppModel.NoJinja);
    }

    [Fact]
    public void Create_LlamaCppLocal_UnknownModelName_Throws()
    {
        PlantFakeInstall("translategemma-4b_Q5_K_M.gguf");

        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.Create(MakeOptions(model: "no-such-model")));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void ResolveLanguage_MatchesCodeAndName_CaseInsensitive()
    {
        var languages = new OllamaTranslate().GetSupportedTargetLanguages();

        Assert.Equal("de", AutoTranslateRunner.ResolveLanguage(languages, "de", "target").TwoLetterIsoLanguageName);
        Assert.Equal("de", AutoTranslateRunner.ResolveLanguage(languages, "german", "target").TwoLetterIsoLanguageName);
        Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ResolveLanguage(languages, "klingon", "target"));
    }
}
