using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class PerplexityTranslateTests
{
    private const string Message =
        "{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"completed\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hej, hvordan g\\u00e5r det?\",\"annotations\":[]}]}";

    [Fact]
    public void GetOutputText_MessageOnly_ReturnsText()
    {
        var json = "{\"id\":\"resp_1\",\"object\":\"response\",\"status\":\"completed\",\"output\":[" + Message + "]}";

        Assert.Equal("Hej, hvordan går det?", PerplexityTranslate.GetOutputText(json));
    }

    [Fact]
    public void GetOutputText_SearchResultsBeforeMessage_ReturnsMessageText()
    {
        // Sonar runs web search by default, so output[0] is a search_results item.
        var json = "{\"id\":\"resp_1\",\"status\":\"completed\",\"output\":[" +
                   "{\"type\":\"search_results\",\"queries\":[\"hello\"],\"results\":[{\"id\":1,\"url\":\"https://example.com\",\"title\":\"t\",\"snippet\":\"s\"}]}," +
                   "{\"type\":\"fetch_url_results\",\"contents\":[{\"url\":\"https://example.com\",\"title\":\"t\",\"snippet\":\"s\"}]}," +
                   Message + "]}";

        Assert.Equal("Hej, hvordan går det?", PerplexityTranslate.GetOutputText(json));
    }

    [Fact]
    public void GetOutputText_NoMessage_FallsBackToOutputText()
    {
        var json = "{\"status\":\"completed\",\"output\":[],\"output_text\":\"Hej\"}";

        Assert.Equal("Hej", PerplexityTranslate.GetOutputText(json));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("{\"status\":\"failed\",\"error\":{\"message\":\"boom\"},\"output\":[]}")]
    public void GetOutputText_NoText_ReturnsEmpty(string json)
    {
        Assert.Equal(string.Empty, PerplexityTranslate.GetOutputText(json));
    }
}
