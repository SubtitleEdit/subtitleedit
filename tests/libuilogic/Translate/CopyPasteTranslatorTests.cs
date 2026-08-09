using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

public class CopyPasteTranslatorTests
{
    private const string Separator = "@";

    private static List<Paragraph> MakeParagraphs(params string[] texts)
    {
        var paragraphs = new List<Paragraph>();
        for (var i = 0; i < texts.Length; i++)
        {
            paragraphs.Add(new Paragraph(texts[i], i * 2000, i * 2000 + 1500));
        }

        return paragraphs;
    }

    [Fact]
    public void GetTranslationResult_SecondBlock_DoesNotGetFirstBlocksFormatting()
    {
        var paragraphs = MakeParagraphs(
            "<i>Hello there my good friend</i>",
            "This is the second line.",
            "This is the third line.",
            "This is the fourth line.");
        var translator = new CopyPasteTranslator(paragraphs, Separator);

        var blocks = translator.BuildBlocks(60, string.Empty, 0);
        Assert.Equal(2, blocks.Count);
        Assert.Equal(2, blocks[0].Paragraphs.Count);
        Assert.Equal(2, blocks[1].Paragraphs.Count);

        // Simulate a translation that returns the block text unchanged.
        var firstBlockResult = translator.GetTranslationResult(string.Empty, blocks[0].TargetText, blocks[0]);
        var secondBlockResult = translator.GetTranslationResult(string.Empty, blocks[1].TargetText, blocks[1]);

        Assert.Equal(2, firstBlockResult.Count);
        Assert.StartsWith("<i>", firstBlockResult[0]);
        Assert.EndsWith("</i>", firstBlockResult[0]);

        // The second block starts a fresh formatting sequence - it must not inherit
        // the italics of the first block's first paragraph.
        Assert.Equal(2, secondBlockResult.Count);
        Assert.Equal("This is the third line.", secondBlockResult[0]);
        Assert.Equal("This is the fourth line.", secondBlockResult[1]);
    }

    [Fact]
    public void GetTranslationResult_KeepsAllConsecutiveAssaTagGroups()
    {
        // Reporter's ASSA lines carry several adjacent {\...} groups (e.g.
        // {\pos(960,480)}{\fs-1.5}{\fs75}{\fscx120}{\fsc110}{\fad(0,0)}{\bord0});
        // SetTagsAndReturnTrimmed must strip ALL of them into the formatting
        // (not only the first), so the clipboard text is clean and the
        // re-applied translation keeps every group (#10627).
        var paragraphs = MakeParagraphs(
            "{\\pos(960,480)}{\\fs75}{\\bord0}The World Of Sword And Magic",
            "This is the second line.");
        var translator = new CopyPasteTranslator(paragraphs, Separator);

        var blocks = translator.BuildBlocks(60, string.Empty, 0);
        Assert.Single(blocks);
        Assert.DoesNotContain("{", blocks[0].TargetText);

        var result = translator.GetTranslationResult(string.Empty, blocks[0].TargetText, blocks[0]);
        Assert.Equal(2, result.Count);
        Assert.StartsWith("{\\pos(960,480)}{\\fs75}{\\bord0}", result[0]);
        Assert.Equal("This is the second line.", result[1]);
    }

    [Fact]
    public void GetTranslationResult_SecondBlock_IsNotReplacedByFirstBlocksMusicNotes()
    {
        var paragraphs = MakeParagraphs(
            "♪♪",
            "This is the second line.",
            "This is the third line.",
            "This is the fourth line.");
        var translator = new CopyPasteTranslator(paragraphs, Separator);

        var blocks = translator.BuildBlocks(55, string.Empty, 0);
        Assert.Equal(2, blocks.Count);

        var secondBlockResult = translator.GetTranslationResult(string.Empty, blocks[1].TargetText, blocks[1]);

        // A ♪♪-only paragraph in block 1 sets whole-line replacement formatting; that
        // must never leak into block 2 and overwrite a real translation.
        Assert.DoesNotContain(secondBlockResult, line => line.Contains('♪'));
        Assert.Contains("This is the fourth line.", secondBlockResult);
    }
}
