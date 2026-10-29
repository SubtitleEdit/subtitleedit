using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class FixRightToLeftGuardTest
{
    // The RTL operations only make sense for Hebrew/Arabic text: on a Latin-only subtitle
    // "ReverseRtlStartEnd" turns "Hello world." into ".Hello world". Same guard as Batch convert.

    private static Subtitle MakeSubtitle(params string[] texts)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < texts.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(texts[i], i * 2000, i * 2000 + 1500));
        }

        return subtitle;
    }

    [Fact]
    public void ReverseRtlStartEnd_EnglishSubtitle_IsLeftAlone()
    {
        var subtitle = MakeSubtitle("Hello world.", "Is that you?");

        LibSEIntegration.ApplyOperations(subtitle, new List<string> { "ReverseRtlStartEnd" });

        Assert.Equal("Hello world.", subtitle.Paragraphs[0].Text);
        Assert.Equal("Is that you?", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void ReverseRtlStartEnd_HebrewSubtitle_StillReversed()
    {
        var subtitle = MakeSubtitle("Credits in English first.", "שלום עולם.");

        LibSEIntegration.ApplyOperations(subtitle, new List<string> { "ReverseRtlStartEnd" });

        Assert.Equal(".Credits in English first", subtitle.Paragraphs[0].Text);
        Assert.Equal(".שלום עולם", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void FixRtlViaUnicodeChars_EnglishSubtitle_IsLeftAlone()
    {
        var subtitle = MakeSubtitle("Hello world.");

        LibSEIntegration.ApplyOperations(subtitle, new List<string> { "FixRtlViaUnicodeChars" });

        Assert.Equal("Hello world.", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void FixRtlViaUnicodeChars_HebrewSubtitle_StillWrapped()
    {
        var subtitle = MakeSubtitle("שלום עולם.");

        LibSEIntegration.ApplyOperations(subtitle, new List<string> { "FixRtlViaUnicodeChars" });

        Assert.NotEqual("שלום עולם.", subtitle.Paragraphs[0].Text);
        Assert.Contains("שלום עולם.", subtitle.Paragraphs[0].Text);
    }
}
