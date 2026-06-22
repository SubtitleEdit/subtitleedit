using System;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using Xunit;

namespace UITests.Logic.NetflixQualityCheck;

public class NetflixCheckTextForHiUseBracketsTests
{
    private static string RunFix(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000));
        var controller = new NetflixQualityController { Language = "en" };

        new NetflixCheckTextForHiUseBrackets("test").Check(subtitle, controller);

        // The checker records a fix only when it changes the text; otherwise the text is left as-is.
        return controller.Records.Count > 0 && controller.Records[0].FixedParagraph != null
            ? controller.Records[0].FixedParagraph!.Text
            : text;
    }

    [Theory]
    [InlineData("{SIGHING}", "[SIGHING]")]
    [InlineData("(SIGHING)", "[SIGHING]")]
    [InlineData("{DOOR CREAKS}", "[DOOR CREAKS]")]
    public void ConvertsHiSpeakerIdsAndSoundEffectsToSquareBrackets(string input, string expected)
    {
        Assert.Equal(expected, RunFix(input));
    }

    // Issue #11720: ASS/SSA override tags also use { } braces and must NOT be turned into [ ].
    [Fact]
    public void DoesNotMangleAssaOverrideTags_IssueExample()
    {
        var input = "{\\i1\\a6}Shot on location in Singapore" + Environment.NewLine +
                    "and all over the Korean peninsula. {\\i0}";

        Assert.Equal(input, RunFix(input));
    }

    [Theory]
    [InlineData("{\\i1}Hello{\\i0}")]
    [InlineData("{\\i1\\a6}Have you ever worked with a director?{\\i0}")]
    [InlineData("{\\an8}Top of screen{\\an8}")]
    public void DoesNotMangleSingleLineAssaOverride(string input)
    {
        Assert.Equal(input, RunFix(input));
    }

    [Fact]
    public void DoesNotMangleDashedAssaOverrideLines()
    {
        var input = "-{\\i1}foo{\\i0}" + Environment.NewLine + "-{\\i1}bar{\\i0}";

        Assert.Equal(input, RunFix(input));
    }

    [Theory]
    [InlineData("{\\a6}Please do NOT hardsub and/or stream")] // override tag, no trailing brace
    [InlineData("Plain dialogue line.")]
    public void LeavesNonBracketTextUnchanged(string input)
    {
        Assert.Equal(input, RunFix(input));
    }
}
