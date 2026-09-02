using SeConv.Core;
using System.Globalization;
using System.Text.Json;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Issue #14310: the CLI's Ollama OCR sent no generation options at all, while the GUI's
/// OllamaOcr caps the tokens, zeroes the temperature, adds a repeat penalty and turns off
/// thinking - tuned to stop a thinking model from emitting its reasoning and repeating lines.
/// Uncapped, the CLI was both slower per image and more prone to garbage output.
///
/// The payload is hand-built JSON, so these check it parses and carries the same values the
/// GUI sends. A malformed body would otherwise surface as an Ollama 400 per image.
/// </summary>
public class OllamaOcrRequestTest
{
    private static JsonElement Build()
    {
        var json = OllamaOcrEngine.BuildRequestBody("glm-ocr", "Transcribe this", "QUJD");
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    [Fact]
    public void RequestBody_IsValidJson_WithModelPromptAndImage()
    {
        var root = Build();

        Assert.Equal("glm-ocr", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());

        var message = root.GetProperty("messages")[0];
        Assert.Equal("user", message.GetProperty("role").GetString());
        Assert.Equal("Transcribe this", message.GetProperty("content").GetString());
        Assert.Equal("QUJD", message.GetProperty("images")[0].GetString());
    }

    // The GUI's numbers, which is the whole point of the issue.
    [Fact]
    public void RequestBody_CarriesTheGuiGenerationLimits()
    {
        var options = Build().GetProperty("options");

        Assert.Equal(0, options.GetProperty("temperature").GetDouble());
        Assert.Equal(1.1, options.GetProperty("repeat_penalty").GetDouble(), 3);
        Assert.Equal(96, options.GetProperty("num_predict").GetInt32());
    }

    [Fact]
    public void RequestBody_DisablesThinking()
    {
        Assert.False(Build().GetProperty("think").GetBoolean());
    }

    // repeat_penalty is a double written into JSON by hand: under a comma-decimal culture an
    // uninvariant ToString() emits 1,1 and the whole request becomes unparseable.
    [Fact]
    public void RequestBody_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("da-DK");
        try
        {
            var json = OllamaOcrEngine.BuildRequestBody("m", "p", "QUJD");
            Assert.Contains("\"repeat_penalty\": 1.1", json);
            JsonDocument.Parse(json);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    // A quote or backslash in the model name or prompt must not break out of the JSON string.
    [Fact]
    public void RequestBody_EscapesQuotesAndBackslashes()
    {
        var json = OllamaOcrEngine.BuildRequestBody("mo\"del", "say \"hi\"\\ now", "QUJD");
        var root = JsonDocument.Parse(json).RootElement;

        Assert.Equal("mo\"del", root.GetProperty("model").GetString());
        Assert.Equal("say \"hi\"\\ now", root.GetProperty("messages")[0].GetProperty("content").GetString());
    }
}
