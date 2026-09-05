using Nikse.SubtitleEdit.UiLogic.Grammar;
using System.Collections.Generic;
using System.Text.Json;

namespace UITests.Features.Tools.GrammarCheck;

public class LanguageToolAnnotatedTextTests
{
    [Fact]
    public void Build_TextIsTheLinesJoinedWithOneSeparator()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He go to school.", "I has a apple" });

        Assert.Equal("He go to school.\nI has a apple", annotated.Text);
    }

    [Fact]
    public void Build_TagsAndMusicSymbolsBecomeMarkup()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "♪ <i>He go</i> {\\an8}home ♪" });

        var (texts, markups) = ReadAnnotation(annotated.Json);

        Assert.Contains("<i>", markups);
        Assert.Contains("</i>", markups);
        Assert.Contains("{\\an8}", markups);
        Assert.Contains("♪", markups);
        Assert.Contains("He go", texts);

        // the complete text keeps every character, so offsets can be applied to the original line
        Assert.Equal("♪ <i>He go</i> {\\an8}home ♪", annotated.Text);
    }

    [Fact]
    public void Build_LineBreakInsideASubtitleIsReadAsASpace()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "I has\na apple" });

        var entries = ReadEntries(annotated.Json);
        var lineBreak = Assert.Single(entries, e => e.Markup == "\n");
        Assert.Equal(" ", lineBreak.InterpretAs);
        Assert.Equal("I has\na apple", annotated.Text);
    }

    [Fact]
    public void Build_ContinuedSentenceIsJoinedWithASpace()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "If I were in your shoes,", "I has taken the deal." });

        var separator = Assert.Single(ReadEntries(annotated.Json), e => e.Markup == "\n");
        Assert.Equal(" ", separator.InterpretAs);
    }

    [Fact]
    public void Build_FinishedSentenceIsFollowedByAParagraphBreak()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He went home.", "She has stayed." });

        var separator = Assert.Single(ReadEntries(annotated.Json), e => e.Markup == "\n");
        Assert.Equal("\n\n", separator.InterpretAs);
    }

    [Fact]
    public void TryMapToLine_OffsetInSecondLine_MapsToThatLine()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He go to school.", "I has a apple" });

        // "has" sits at 19 in "He go to school.\nI has a apple"
        Assert.True(annotated.TryMapToLine(19, 3, out var lineIndex, out var lineOffset));
        Assert.Equal(1, lineIndex);
        Assert.Equal(2, lineOffset);
        Assert.Equal("has", annotated.Text.Substring(19, 3));
    }

    [Fact]
    public void TryMapToLine_MatchInsideItalicTags_MapsWithTheTagOffset()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He <i>go</i> to school." });

        Assert.True(annotated.TryMapToLine(6, 2, out var lineIndex, out var lineOffset));
        Assert.Equal(0, lineIndex);
        Assert.Equal(6, lineOffset);
    }

    [Fact]
    public void TryMapToLine_MatchOverlappingATag_IsRejected()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "a <i>apple</i>" });

        // "a <i>apple" - replacing this would swallow the italic tag
        Assert.False(annotated.TryMapToLine(0, 10, out _, out _));
    }

    [Fact]
    public void TryMapToLine_MatchSpanningTwoLines_IsRejected()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He go", "to school." });

        Assert.False(annotated.TryMapToLine(3, 5, out _, out _));
    }

    [Fact]
    public void TryMapToLine_OutsideTheText_IsRejected()
    {
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { "He go to school." });

        Assert.False(annotated.TryMapToLine(100, 2, out _, out _));
        Assert.False(annotated.TryMapToLine(0, 0, out _, out _));
    }

    [Fact]
    public void IsEmpty_OnlyWhenThereIsNothingToCheck()
    {
        Assert.True(LanguageToolAnnotatedText.Build(new List<string>()).IsEmpty);
        Assert.True(LanguageToolAnnotatedText.Build(new List<string> { "  " }).IsEmpty);
        Assert.False(LanguageToolAnnotatedText.Build(new List<string> { "Hi." }).IsEmpty);
    }

    [Fact]
    public void EndsSentence_TerminalPunctuation_True()
    {
        Assert.True(LanguageToolAnnotatedText.EndsSentence("Hello there."));
        Assert.True(LanguageToolAnnotatedText.EndsSentence("What?"));
        Assert.True(LanguageToolAnnotatedText.EndsSentence("Wait…"));
        Assert.True(LanguageToolAnnotatedText.EndsSentence("<i>Done.</i>"));
    }

    [Fact]
    public void EndsSentence_Continuation_False()
    {
        Assert.False(LanguageToolAnnotatedText.EndsSentence("If I were in your shoes,"));
        Assert.False(LanguageToolAnnotatedText.EndsSentence("and then he"));
    }

    private static (List<string> Texts, List<string> Markups) ReadAnnotation(string json)
    {
        var texts = new List<string>();
        var markups = new List<string>();
        foreach (var entry in ReadEntries(json))
        {
            if (entry.Text != null)
            {
                texts.Add(entry.Text);
            }

            if (entry.Markup != null)
            {
                markups.Add(entry.Markup);
            }
        }

        return (texts, markups);
    }

    private static List<AnnotationEntry> ReadEntries(string json)
    {
        var entries = new List<AnnotationEntry>();
        using var document = JsonDocument.Parse(json);
        foreach (var element in document.RootElement.GetProperty("annotation").EnumerateArray())
        {
            entries.Add(new AnnotationEntry(
                element.TryGetProperty("text", out var text) ? text.GetString() : null,
                element.TryGetProperty("markup", out var markup) ? markup.GetString() : null,
                element.TryGetProperty("interpretAs", out var interpretAs) ? interpretAs.GetString() : null));
        }

        return entries;
    }

    private record AnnotationEntry(string? Text, string? Markup, string? InterpretAs);
}
