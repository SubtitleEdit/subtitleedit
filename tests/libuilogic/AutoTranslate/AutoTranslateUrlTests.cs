using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class AutoTranslateUrlTests
{
    [Theory]
    // The #13044 case: DeepSeek documents "https://api.deepseek.com" as the base url for the
    // OpenAI SDK, so that is what users paste - and posting to the origin root gives 404.
    [InlineData("https://api.deepseek.com", "https://api.deepseek.com/chat/completions")]
    [InlineData("https://api.deepseek.com/", "https://api.deepseek.com/chat/completions")]
    [InlineData("https://api.openai.com", "https://api.openai.com/v1/chat/completions")]
    // A leading part of the endpoint path is completed too, without doubling it.
    [InlineData("https://api.openai.com/v1", "https://api.openai.com/v1/chat/completions")]
    [InlineData("https://api.openai.com/v1/", "https://api.openai.com/v1/chat/completions")]
    public void Complete_AddsMissingEndpointPath(string url, string expected)
    {
        var defaultUrl = url.Contains("deepseek")
            ? DeepSeekTranslate.DefaultUrl
            : ChatGptTranslate.DefaultUrl;

        Assert.Equal(expected, AutoTranslateUrl.Complete(url, defaultUrl));
    }

    [Theory]
    [InlineData("https://api.groq.com", "https://api.groq.com/openai/v1/chat/completions")]
    [InlineData("https://api.groq.com/openai", "https://api.groq.com/openai/v1/chat/completions")]
    [InlineData("https://api.groq.com/openai/v1", "https://api.groq.com/openai/v1/chat/completions")]
    public void Complete_HandlesMultiSegmentEndpointPaths(string url, string expected)
    {
        Assert.Equal(expected, AutoTranslateUrl.Complete(url, GroqTranslate.DefaultUrl));
    }

    [Theory]
    // Already complete - only the trailing slash goes.
    [InlineData("http://localhost:8080/v1/chat/completions")]
    [InlineData("http://localhost:8080/v1/chat/completions/")]
    // A deliberate endpoint that is not part of the default path must survive untouched, e.g.
    // llama.cpp's native completion route or a proxy with its own routing.
    [InlineData("http://localhost:8080/completion")]
    [InlineData("https://my-proxy.example.com/openai/deployments/gpt/chat/completions")]
    public void Complete_LeavesAnExplicitPathAlone(string url)
    {
        Assert.Equal(url.TrimEnd('/'), AutoTranslateUrl.Complete(url, LlamaCppTranslate.DefaultUrl));
    }

    [Fact]
    public void Complete_KeepsQueryStringUrlsVerbatim()
    {
        const string url = "https://my-proxy.example.com?route=chat";

        Assert.Equal(url, AutoTranslateUrl.Complete(url, ChatGptTranslate.DefaultUrl));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Complete_FallsBackToTheDefaultWhenEmpty(string? url)
    {
        Assert.Equal(ChatGptTranslate.DefaultUrl, AutoTranslateUrl.Complete(url, ChatGptTranslate.DefaultUrl));
    }

    [Fact]
    public void Complete_LeavesNonUrlTextAlone()
    {
        Assert.Equal("not a url", AutoTranslateUrl.Complete("not a url", ChatGptTranslate.DefaultUrl));
    }

    [Fact]
    public void Complete_CompletesNativeEndpointsToo()
    {
        // Ollama and KoboldCpp are not chat/completions services - the default path decides.
        Assert.Equal("http://localhost:11434/api/generate", AutoTranslateUrl.Complete("http://localhost:11434", OllamaTranslate.DefaultUrl));
        Assert.Equal("http://localhost:5001/api/generate", AutoTranslateUrl.Complete("http://localhost:5001", KoboldCppTranslate.DefaultUrl));
        Assert.Equal("https://api.anthropic.com/v1/messages", AutoTranslateUrl.Complete("https://api.anthropic.com", AnthropicTranslate.DefaultUrl));
    }
}
