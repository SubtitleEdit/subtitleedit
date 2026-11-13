using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// The batch JSON is what the model reads: the server decodes only the request's outer JSON
/// layer, so an escaped character inside the user message reaches the model as the six
/// characters of the escape (a backslash, a "u" and four hex digits per glyph). With the
/// default encoder a 7B model answered a Chinese batch with text unrelated to the source.
/// </summary>
public class LlamaCppAdvancedProtocolEncodingTests
{
    [Fact]
    public void UserContent_KeepsNonAsciiTextReadable()
    {
        var history = new List<LlamaCppAdvancedProtocol.HistoryPair> { new("你昨晚在哪里？", "Where were you last night?") };
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, "埃里克。"), new(2, "Åse sagde: \"nej\" <i>nu</i> & så, don't") };

        var json = LlamaCppAdvancedProtocol.BuildUserContent(history, lines);

        Assert.Contains("你昨晚在哪里？", json);
        Assert.Contains("埃里克。", json);
        Assert.Contains("Åse sagde: \\\"nej\\\" <i>nu</i> & så, don't", json); // quotes escaped, nothing else
        Assert.DoesNotContain("\\u", json);
        using var doc = JsonDocument.Parse(json); // still valid JSON
        Assert.Equal("埃里克。", doc.RootElement.GetProperty("lines")[0].GetProperty("text").GetString());
        Assert.Equal("你昨晚在哪里？", doc.RootElement.GetProperty("history")[0].GetProperty("source").GetString());
    }

    [Fact]
    public void RequestJson_KeepsNonAsciiTextReadable()
    {
        var userContent = LlamaCppAdvancedProtocol.BuildUserContent(new(), new() { new(1, "こんにちは") });

        var body = LlamaCppAdvancedClient.BuildRequestJson(new SeLlamaCppAdvanced(), "Glossar: Straße = street", userContent, null, null, 100);

        Assert.Contains("こんにちは", body);
        Assert.Contains("Straße", body);
        Assert.DoesNotContain("\\u", body);
        using var doc = JsonDocument.Parse(body);
        var messages = doc.RootElement.GetProperty("messages");
        Assert.Equal("Glossar: Straße = street", messages[0].GetProperty("content").GetString());
        // One JSON layer decoded, as the server does it: the text the model reads is still Japanese.
        var user = messages[1].GetProperty("content").GetString()!;
        Assert.Contains("こんにちは", user);
        Assert.DoesNotContain("\\u", user);
    }

    [Fact]
    public void ResponseFormat_IsStillValidJsonSchema()
    {
        var json = LlamaCppAdvancedProtocol.BuildResponseFormatJson(new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, "a"), new(2, "b") });

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("json_schema", doc.RootElement.GetProperty("type").GetString());
    }
}
