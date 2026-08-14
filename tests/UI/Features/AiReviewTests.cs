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

    private static Dictionary<int, string> Lines(params (int Number, string Text)[] lines)
    {
        return lines.ToDictionary(x => x.Number, x => x.Text);
    }

    [Fact]
    public void ParseChanges_ValidReply_Parsed()
    {
        var reply = """{"changes":[{"n":12,"text":"We received it.","reason":"typo","category":"spelling"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((12, "We recieved it.")));

        var change = Assert.Single(changes);
        Assert.Equal(12, change.Number);
        Assert.Equal("We received it.", change.NewText);
        Assert.Equal(ReviewCategory.Spelling, change.Category);
    }

    [Fact]
    public void ParseChanges_MarkdownFences_Parsed()
    {
        var reply = "Here you go:\n```json\n{\"changes\":[{\"n\":3,\"text\":\"Fixed.\",\"reason\":\"x\",\"category\":\"grammar\"}]}\n```";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((3, "Fixxed.")));

        Assert.Single(changes);
        Assert.Equal(ReviewCategory.Grammar, changes[0].Category);
    }

    [Fact]
    public void ParseChanges_HallucinatedLineNumber_Skipped()
    {
        var reply = """{"changes":[{"n":99,"text":"Nope.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "One."), (2, "Two."), (3, "Three.")));

        Assert.Empty(changes);
    }

    [Fact]
    public void ParseChanges_InvalidJson_Empty()
    {
        Assert.Empty(AiReviewProtocol.ParseChanges("I could not find any issues!", Lines((1, "One."))));
        Assert.Empty(AiReviewProtocol.ParseChanges(string.Empty, Lines((1, "One."))));
    }

    [Fact]
    public void ParseChanges_NewLinesNormalized()
    {
        var reply = """{"changes":[{"n":1,"text":"First line\nsecond line.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Frst line\nsecond line.")));

        Assert.Contains(System.Environment.NewLine, changes[0].NewText);
    }

    [Fact]
    public void ParseChanges_EchoConfirmsLineNumber_Kept()
    {
        var reply = """{"changes":[{"n":2,"orig":"We recieved it.","text":"We received it.","reason":"typo","category":"spelling"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Hello."), (2, "We recieved it.")));

        var change = Assert.Single(changes);
        Assert.Equal(2, change.Number);
    }

    [Fact]
    public void ParseChanges_EchoBelongsToOtherLine_Remapped()
    {
        // the issue-13628 shape: the model corrected line 645 but labeled it n=644
        var reply = """{"changes":[{"n":644,"orig":"Its really hot in there.","text":"It's really hot in there.","reason":"typo","category":"punctuation"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines(
            (644, "Whoa! Christmas Eve, that is..."),
            (645, "Its really hot in there.")));

        var change = Assert.Single(changes);
        Assert.Equal(645, change.Number);
        Assert.Equal("It's really hot in there.", change.NewText);
    }

    [Fact]
    public void ParseChanges_EchoMatchesNoLine_Dropped()
    {
        var reply = """{"changes":[{"n":1,"orig":"Something entirely different was here.","text":"Something entirely different was there.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Hello."), (2, "Goodbye.")));

        Assert.Empty(changes);
    }

    [Fact]
    public void ParseChanges_EchoAmbiguous_Dropped()
    {
        var reply = """{"changes":[{"n":1,"orig":"Yes.","text":"Yes!","reason":"","category":"punctuation"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "No."), (2, "Yes."), (3, "Yes.")));

        Assert.Empty(changes);
    }

    [Fact]
    public void ParseChanges_EchoNearlyMatchesOwnLine_Kept()
    {
        // model normalized the curly apostrophe while copying - still clearly the same line
        var reply = """{"changes":[{"n":1,"orig":"You're gonna like it here alot.","text":"You're going to like it here a lot.","reason":"","category":"grammar"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "You’re gonna like it here alot.")));

        var change = Assert.Single(changes);
        Assert.Equal(1, change.Number);
    }

    [Fact]
    public void ParseChanges_NoEcho_TrustsLineNumber()
    {
        var reply = """{"changes":[{"n":1,"text":"Fixed.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Fixxed.")));

        Assert.Single(changes);
    }

    [Fact]
    public void ParseChanges_EchoRepeatsCorrectedText_TrustsLineNumber()
    {
        // a useless echo (orig == text) carries no information and must not drop the change
        var reply = """{"changes":[{"n":1,"orig":"Fixed.","text":"Fixed.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Fixxed.")));

        var change = Assert.Single(changes);
        Assert.Equal(1, change.Number);
    }

    [Fact]
    public void ParseChanges_DuplicateLineNumber_FirstWins()
    {
        var reply = """{"changes":[{"n":1,"text":"First.","reason":"","category":"other"},{"n":1,"text":"Second.","reason":"","category":"other"}]}""";
        var changes = AiReviewProtocol.ParseChanges(reply, Lines((1, "Frst.")));

        var change = Assert.Single(changes);
        Assert.Equal("First.", change.NewText);
    }

    [Fact]
    public void LooksMisaligned_AfterIsCopyOfNeighbor_True()
    {
        // the issue-13628 screenshot: line 644's "After" was line 645's original text
        Assert.True(AiReviewProtocol.LooksMisaligned(
            "Whoa! Christmas Eve, that is...",
            "It's really hot in there. Is that ammonia?",
            new[] { "If I'm wrong, I'm really wrong.", "It's really hot in there. Is that ammonia?" }));
    }

    [Fact]
    public void LooksMisaligned_AfterIsEditedNeighbor_True()
    {
        // the shifted correction: "After" is a lightly edited copy of the next line
        Assert.True(AiReviewProtocol.LooksMisaligned(
            "It's really hot in there. Is that ammonia?",
            "Anyway, that's...",
            new[] { "Whoa! Christmas Eve, that is...", "Anyway, that is..." }));
    }

    [Fact]
    public void LooksMisaligned_LegitimateFix_False()
    {
        Assert.False(AiReviewProtocol.LooksMisaligned(
            "If we were gonna catch it,\nwhat would we do?",
            "If we were going to catch it,\nwhat would we do?",
            new[] { "What the fudge, fudge?", "If I'm wrong, I'm really wrong." }));
    }

    [Fact]
    public void LooksMisaligned_RepetitiveDialogue_False()
    {
        // a legit fix may equal a neighbor in repetitive dialogue - high own-similarity keeps it
        Assert.False(AiReviewProtocol.LooksMisaligned("Yes", "Yes.", new[] { "Yes.", "No." }));
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

    // A whole batch shifted by 3 lines, with the echo repeating the corrected text (so it
    // carries no information) - the field failure behind the "Trouble Mum" screenshot: the
    // suggestion for line 368 was really the correction of line 365, three lines back.
    [Fact]
    public void ParseChanges_ShiftedNumbersUselessEcho_RemappedByContent()
    {
        var lines = Lines(
            (365, "Go! Did I bring her diapers?"),
            (366, "I'm all set"),
            (367, "You can pick Paolo up between 5:00 and 6:00 pm."),
            (368, "I'm staying if you don't mind. I need to watch him."));
        var reply = """
            {"changes":[
              {"n":368,"orig":"Go! Did I bring her diapers","text":"Go! Did I bring her diapers","reason":"x","category":"punctuation"},
              {"n":369,"orig":"I'm all set.","text":"I'm all set.","reason":"x","category":"punctuation"},
              {"n":371,"orig":"I'm staying, if you don't mind. I need to watch him.","text":"I'm staying, if you don't mind. I need to watch him.","reason":"x","category":"punctuation"}
            ]}
            """;

        var changes = AiReviewProtocol.ParseChanges(reply, lines);

        // 369/371 are not editable numbers and would have been dropped outright; the echo
        // remap already rescues those. The dangerous one is 368: an editable number whose
        // echo equals its text - it used to keep the wrong line. Content remap moves it to 365.
        Assert.Contains(changes, c => c.Number == 365 && c.NewText == "Go! Did I bring her diapers");
        Assert.DoesNotContain(changes, c => c.Number == 368);
    }

    [Fact]
    public void ParseChanges_LegitCorrection_NotRemapped()
    {
        // A real correction resembles its own line - it must stay put even when another
        // line in the batch is similar.
        var lines = Lines(
            (10, "We recieved it yesterday."),
            (11, "We received it today."));
        var reply = """{"changes":[{"n":10,"text":"We received it yesterday.","reason":"typo","category":"spelling"}]}""";

        var changes = AiReviewProtocol.ParseChanges(reply, lines);

        var change = Assert.Single(changes);
        Assert.Equal(10, change.Number);
    }

    [Fact]
    public void ParseChanges_ContentRemapAmbiguous_KeepsModelNumber()
    {
        // Two near-identical lines (song refrain) - remapping would be a guess, so the
        // model's number is kept and the low own-similarity is flagged in the UI instead.
        var lines = Lines(
            (20, "Something completely different here."),
            (21, "La la la, here we go again!"),
            (22, "La la la, here we go again"));
        var reply = """{"changes":[{"n":20,"text":"La la la, here we go again!","reason":"x","category":"punctuation"}]}""";

        var changes = AiReviewProtocol.ParseChanges(reply, lines);

        var change = Assert.Single(changes);
        Assert.Equal(20, change.Number);
    }

    [Fact]
    public void ParseChanges_ContentRemapTargetTaken_DropsDuplicate()
    {
        var lines = Lines(
            (30, "Hello there, my old friend."),
            (31, "Unrelated text on this line."));
        var reply = """
            {"changes":[
              {"n":30,"text":"Hello there, my old friend!","reason":"x","category":"punctuation"},
              {"n":31,"text":"Hello there, my old friend!","reason":"x","category":"punctuation"}
            ]}
            """;

        var changes = AiReviewProtocol.ParseChanges(reply, lines);

        // the second change remaps onto line 30, which already has one - dropped
        var change = Assert.Single(changes);
        Assert.Equal(30, change.Number);
    }
}
