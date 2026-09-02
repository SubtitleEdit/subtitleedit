using Nikse.SubtitleEdit.Features.Tools.AiReview;
using System.Collections.Generic;

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
