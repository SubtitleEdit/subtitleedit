using Nikse.SubtitleEdit.Features.Tools.AiReview;

namespace UITests.Features.Tools.AiReview;

public class AiReviewProtocolTests
{
    [Fact]
    public void BuildUserContent_WritesActorAndStyleOnlyWhenPresent()
    {
        var chunk = new ReviewChunk();
        chunk.Lines.Add(new ReviewLine(1, "Overboard", "Narrator", "Sign"));
        chunk.Lines.Add(new ReviewLine(2, "Hello", null, "  "));

        var json = AiReviewProtocol.BuildUserContent(chunk);

        Assert.Contains("\"n\":1,\"text\":\"Overboard\",\"actor\":\"Narrator\",\"style\":\"Sign\"", json);
        Assert.Contains("\"n\":2,\"text\":\"Hello\"}", json);
        Assert.DoesNotContain("\"actor\":\"\"", json);
    }

    // The chunk JSON is what the model reads (the server decodes only the request's outer layer),
    // so non-ASCII text must stay readable instead of becoming \uXXXX escapes.
    [Fact]
    public void BuildUserContent_KeepsNonAsciiTextReadable()
    {
        var chunk = new ReviewChunk();
        chunk.Lines.Add(new ReviewLine(1, "Åse sagde: \"nej\" – 你好", "Günther"));

        var json = AiReviewProtocol.BuildUserContent(chunk);

        Assert.Contains("Åse sagde: \\\"nej\\\" – 你好", json);
        Assert.Contains("\"actor\":\"Günther\"", json);
        Assert.DoesNotContain("\\u", json);
    }

    [Fact]
    public void ParseChanges_StrippedText_EchoMatchesStrippedLine()
    {
        var editable = new Dictionary<int, string> { { 1, "Overbored" } };

        var changes = AiReviewProtocol.ParseChanges(
            "{\"changes\":[{\"n\":1,\"orig\":\"Overbored\",\"text\":\"Overboard\",\"reason\":\"typo\",\"category\":\"spelling\"}]}",
            editable);

        Assert.Single(changes);
        Assert.Equal("Overboard", changes[0].NewText);
    }
}
