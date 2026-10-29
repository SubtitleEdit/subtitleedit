using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// A bare host:port for --ocr-url / --ollama-url used to reach HttpClient without a scheme,
/// which fails with "An invalid request URI was provided".
/// </summary>
public class OcrUrlCompletionTest
{
    [Theory]
    [InlineData("127.0.0.1:8098", "http://127.0.0.1:8098/v1/chat/completions")]
    [InlineData(" localhost:8080/ ", "http://localhost:8080/v1/chat/completions")]
    [InlineData("http://127.0.0.1:8098", "http://127.0.0.1:8098/v1/chat/completions")]
    [InlineData("http://127.0.0.1:8098/v1/chat/completions", "http://127.0.0.1:8098/v1/chat/completions")]
    [InlineData("127.0.0.1:8098/v1/chat/completions", "http://127.0.0.1:8098/v1/chat/completions")]
    [InlineData("https://ocr.example.com", "https://ocr.example.com/v1/chat/completions")]
    [InlineData("https://ocr.example.com/v1/chat/completions", "https://ocr.example.com/v1/chat/completions")]
    public void LlamaCppOcrUrl_IsCompletedWithSchemeAndPath(string input, string expected)
    {
        Assert.Equal(expected, LlamaCppOcrEngine.CompleteUrl(input));
    }

    [Theory]
    [InlineData("localhost:11434/api/chat", "http://localhost:11434/api/chat")]
    [InlineData("http://localhost:11434/api/chat", "http://localhost:11434/api/chat")]
    [InlineData("https://ollama.example.com/api/chat", "https://ollama.example.com/api/chat")]
    public void OllamaUrl_GetsHttpSchemeWhenMissing(string input, string expected)
    {
        Assert.Equal(expected, OllamaOcrEngine.CompleteScheme(input));
    }
}
