using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// The user's extra request JSON is merged into every request body last: its fields win over the
/// engine's, a null removes an engine field, and the two fields the protocol owns are refused.
/// </summary>
public class LlamaCppAdvancedClientRequestJsonTests
{
    private static JsonDocument Build(string requestJson, string? responseFormat = "{\"type\":\"json_object\"}", bool disableThinking = false)
    {
        var settings = new SeLlamaCppAdvanced { RequestJson = requestJson, TopK = 20 };
        return JsonDocument.Parse(LlamaCppAdvancedClient.BuildRequestJson(settings, "system", "user", responseFormat, "model-a", 100, disableThinking: disableThinking));
    }

    private static int Count(JsonDocument doc, string name)
    {
        return doc.RootElement.EnumerateObject().Count(p => p.Name == name);
    }

    [Fact]
    public void UserFieldsAreAddedAndWin()
    {
        using var doc = Build("{\"reasoning\": {\"enabled\": false}, \"max_tokens\": 4000, \"temperature\": 0.7, \"top_k\": 40, \"model\": \"model-b\"}");

        var root = doc.RootElement;
        Assert.False(root.GetProperty("reasoning").GetProperty("enabled").GetBoolean());
        Assert.Equal(4000, root.GetProperty("max_tokens").GetInt32());
        Assert.Equal(0.7, root.GetProperty("temperature").GetDouble());
        Assert.Equal(40, root.GetProperty("top_k").GetInt32());
        Assert.Equal("model-b", root.GetProperty("model").GetString());
        foreach (var name in new[] { "max_tokens", "temperature", "top_k", "model" })
        {
            Assert.Equal(1, Count(doc, name)); // the engine's own value is left out, not duplicated
        }

        // The engine's part is otherwise untouched.
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.True(root.GetProperty("cache_prompt").GetBoolean());
        Assert.Equal("json_object", root.GetProperty("response_format").GetProperty("type").GetString());
        Assert.Equal(2, root.GetProperty("messages").GetArrayLength());
    }

    [Fact]
    public void NullRemovesTheEngineField()
    {
        using var doc = Build("{\"max_tokens\": null, \"cache_prompt\": null}");

        Assert.False(doc.RootElement.TryGetProperty("max_tokens", out _));
        Assert.False(doc.RootElement.TryGetProperty("cache_prompt", out _));
        Assert.True(doc.RootElement.GetProperty("stream").GetBoolean());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptySettingLeavesTheBodyAsItWas(string requestJson)
    {
        var settings = new SeLlamaCppAdvanced();
        var plain = LlamaCppAdvancedClient.BuildRequestJson(settings, "system", "user", null, null, 100);
        settings.RequestJson = requestJson;

        Assert.Equal(plain, LlamaCppAdvancedClient.BuildRequestJson(settings, "system", "user", null, null, 100));
    }

    [Fact]
    public void AppliesToEveryFormatFallbackAndToThinkingOff()
    {
        foreach (var format in new[] { "{\"type\":\"json_schema\",\"json_schema\":{}}", "{\"type\":\"json_object\"}", null })
        {
            using var doc = Build("{\"top_k\": 40}", format, disableThinking: true);
            Assert.Equal(40, doc.RootElement.GetProperty("top_k").GetInt32());
            Assert.False(doc.RootElement.GetProperty("chat_template_kwargs").GetProperty("enable_thinking").GetBoolean());
        }
    }

    [Theory]
    [InlineData("{\"top_k\": ", "not valid JSON")]
    [InlineData("[1, 2]", "must be a JSON object")]
    [InlineData("\"text\"", "must be a JSON object")]
    [InlineData("{\"messages\": []}", "cannot set \"messages\"")]
    [InlineData("{\"response_format\": {\"type\": \"text\"}}", "cannot set \"response_format\"")]
    public void UnusableJsonIsRefusedUpFrontAndAtRequestTime(string requestJson, string expectedInMessage)
    {
        var error = LlamaCppAdvancedClient.ValidateRequestJson(requestJson);
        Assert.NotNull(error);
        Assert.Contains(expectedInMessage, error);

        var ex = Assert.Throws<InvalidOperationException>(() => Build(requestJson));
        Assert.Contains(expectedInMessage, ex.Message);
    }

    [Theory]
    [InlineData("{\"top_k\": 40}")]
    [InlineData("")]
    [InlineData(null)]
    public void UsableJsonValidates(string? requestJson)
    {
        Assert.Null(LlamaCppAdvancedClient.ValidateRequestJson(requestJson));
    }
}
