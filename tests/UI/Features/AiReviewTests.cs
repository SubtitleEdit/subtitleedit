using Nikse.SubtitleEdit.Features.Tools.AiReview;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features;

public class AiReviewTests
{
    [Fact]
    public void EndsSentence_TerminalPunctuation_True()
    {
        Assert.True(AiReviewChunker.EndsSentence("Hello there."));
        Assert.True(AiReviewChunker.EndsSentence("What?"));
        Assert.True(AiReviewChunker.EndsSentence("Stop!"));
        Assert.True(AiReviewChunker.EndsSentence("Wait…"));
        Assert.True(AiReviewChunker.EndsSentence("\"He said so.\""));
        Assert.True(AiReviewChunker.EndsSentence("<i>Done.</i>"));
    }

    [Fact]
    public void EndsSentence_Continuation_False()
    {
        Assert.False(AiReviewChunker.EndsSentence("If I were in your shoes,"));
        Assert.False(AiReviewChunker.EndsSentence("and then he"));
        Assert.False(AiReviewChunker.EndsSentence("It was the"));
    }

    [Fact]
    public void BuildUnitIds_SentenceAcrossTwoLines_SameUnit()
    {
        var lines = new List<ReviewLine>
        {
            new(1, "Hello there."),
            new(2, "If I were in your shoes,"),
            new(3, "I would have taken the deal."),
            new(4, "Goodbye."),
        };

        var ids = AiReviewChunker.BuildUnitIds(lines);

        Assert.NotEqual(ids[0], ids[1]);
        Assert.Equal(ids[1], ids[2]);
        Assert.NotEqual(ids[2], ids[3]);
    }

    [Fact]
    public void BuildChunks_NeverSplitsAUnit()
    {
        var lines = new List<ReviewLine>();
        for (var i = 1; i <= 40; i++)
        {
            // every odd line continues into the next -> units of two lines
            lines.Add(new ReviewLine(i, i % 2 == 1 ? "First half of a sentence" : "and the second half."));
        }

        var unitIds = AiReviewChunker.BuildUnitIds(lines);
        var unitIdByNumber = lines.Select((line, i) => (line.Number, Id: unitIds[i])).ToDictionary(x => x.Number, x => x.Id);
        var chunks = AiReviewChunker.BuildChunks(lines, 5);

        Assert.Equal(lines.Count, chunks.Sum(c => c.Lines.Count));
        foreach (var chunk in chunks)
        {
            // a unit id must not span two chunks
            var first = chunk.Lines[0].Number;
            var last = chunk.Lines[^1].Number;
            if (first > 1)
            {
                Assert.NotEqual(unitIdByNumber[first - 1], unitIdByNumber[first]);
            }

            if (last < lines.Count)
            {
                Assert.NotEqual(unitIdByNumber[last], unitIdByNumber[last + 1]);
            }
        }
    }

    [Fact]
    public void BuildChunks_AddsContext()
    {
        var lines = Enumerable.Range(1, 30).Select(i => new ReviewLine(i, $"Line {i}.")).ToList();
        var chunks = AiReviewChunker.BuildChunks(lines, 10);

        Assert.True(chunks.Count >= 3);
        Assert.Empty(chunks[0].ContextBefore);
        Assert.Equal(2, chunks[0].ContextAfter.Count);
        Assert.Equal(2, chunks[1].ContextBefore.Count);
        Assert.Empty(chunks[^1].ContextAfter);
    }

    [Fact]
    public void ParseChanges_ValidReply_Parsed()
    {
        var reply = """{"changes":[{"n":12,"text":"We received it.","reason":"typo","category":"spelling"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, new HashSet<int> { 12 });

        var change = Assert.Single(changes);
        Assert.Equal(12, change.Number);
        Assert.Equal("We received it.", change.NewText);
        Assert.Equal(ReviewCategory.Spelling, change.Category);
    }

    [Fact]
    public void ParseChanges_MarkdownFences_Parsed()
    {
        var reply = "Here you go:\n```json\n{\"changes\":[{\"n\":3,\"text\":\"Fixed.\",\"reason\":\"x\",\"category\":\"grammar\"}]}\n```";
        var changes = AiReviewProtocol.ParseChanges(reply, new HashSet<int> { 3 });

        Assert.Single(changes);
        Assert.Equal(ReviewCategory.Grammar, changes[0].Category);
    }

    [Fact]
    public void ParseChanges_HallucinatedLineNumber_Skipped()
    {
        var reply = """{"changes":[{"n":99,"text":"Nope.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, new HashSet<int> { 1, 2, 3 });

        Assert.Empty(changes);
    }

    [Fact]
    public void ParseChanges_InvalidJson_Empty()
    {
        Assert.Empty(AiReviewProtocol.ParseChanges("I could not find any issues!", new HashSet<int> { 1 }));
        Assert.Empty(AiReviewProtocol.ParseChanges(string.Empty, new HashSet<int> { 1 }));
    }

    [Fact]
    public void ParseChanges_NewLinesNormalized()
    {
        var reply = """{"changes":[{"n":1,"text":"First line\nsecond line.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, new HashSet<int> { 1 });

        Assert.Contains(System.Environment.NewLine, changes[0].NewText);
    }

    [Fact]
    public void TagsMatch_DetectsRemovedTags()
    {
        Assert.True(AiReviewProtocol.TagsMatch("<i>Hello.</i>", "<i>Hello there.</i>"));
        Assert.False(AiReviewProtocol.TagsMatch("<i>Hello.</i>", "Hello there."));
        Assert.True(AiReviewProtocol.TagsMatch("{\\an8}Up here.", "{\\an8}Up here!"));
        Assert.False(AiReviewProtocol.TagsMatch("{\\an8}Up here.", "Up here."));
    }

    [Fact]
    public void ExtractJsonObject_ProseAroundObject_Found()
    {
        Assert.NotNull(AiReviewProtocol.ExtractJsonObject("Sure! {\"changes\":[]} Hope that helps."));
        Assert.Null(AiReviewProtocol.ExtractJsonObject("No json here"));
    }
}
