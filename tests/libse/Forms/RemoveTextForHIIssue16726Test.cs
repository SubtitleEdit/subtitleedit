using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

/// <summary>
/// Issue #16726: removing a speaker label before a colon from a dialog that uses en or em
/// dashes gave "- – Maltese." / "- Justice!" - the dialog logic only knew "-".
/// </summary>
public class RemoveTextForHIIssue16726Test
{
    private static string Run(string text)
    {
        text = text.Replace("\n", Environment.NewLine);
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("Before.", 0, 1000));
        s.Paragraphs.Add(new Paragraph(text, 2000, 3000));
        var settings = new RemoveTextForHISettings(s)
        {
            OnlyIfInSeparateLine = false,
            RemoveTextBetweenSquares = true,
            RemoveTextBetweenBrackets = true,
            RemoveTextBetweenParentheses = true,
            RemoveTextBetweenQuestionMarks = false,
            RemoveInterjections = false,
            RemoveIfAllUppercase = false,
            RemoveTextBeforeColon = true,
            RemoveTextBeforeColonOnlyUppercase = false,
            ColonSeparateLine = false,
        };
        return new RemoveTextForHI(settings).RemoveTextFromHearImpaired(text, s, 1, "en");
    }

    [Theory]
    [InlineData("– Maltese.\n– ALL: Justice!", "– Maltese.\n– Justice!")]
    [InlineData("– ALL: Justice!\n– Maltese.", "– Justice!\n– Maltese.")]
    [InlineData("— Maltese.\n— ALL: Justice!", "— Maltese.\n— Justice!")]
    [InlineData("—Maltese.\n—ALL: Justice!", "—Maltese.\n—Justice!")]
    [InlineData("<i>– Maltese.</i>\n<i>– ALL: Justice!</i>", "<i>– Maltese.</i>\n<i>– Justice!</i>")]
    [InlineData("– Maltese.\n– [shouts] Justice!", "– Maltese.\n– Justice!")]
    [InlineData("- Maltese.\n- ALL: Justice!", "- Maltese.\n- Justice!")]
    public void Dialog_KeepsDashStyle(string input, string expected)
    {
        Assert.Equal(expected.Replace("\n", Environment.NewLine), Run(input));
    }

    [Fact]
    public void NoChange_KeepsEnDashesAsIs()
    {
        var input = "– Maltese." + Environment.NewLine + "– Justice!";
        Assert.Equal(input, Run(input));
    }

    [Fact]
    public void EmDashAtLineEnd_NotTouched()
    {
        var input = "Well, I was going—" + Environment.NewLine + "JOHN: Stop!";
        Assert.DoesNotContain("JOHN", Run(input));
        Assert.Contains("going—", Run(input));
    }

    [Fact]
    public void ReplaceAndRestore_RoundTrip()
    {
        var input = "{\\an8}– One." + Environment.NewLine + "– Two.";
        var replaced = RemoveTextForHI.ReplaceLeadingDialogDashes(input, out var dash, out var space);
        Assert.Equal("{\\an8}- One." + Environment.NewLine + "- Two.", replaced);
        Assert.Equal('–', dash);
        Assert.True(space);
        Assert.Equal(input, RemoveTextForHI.RestoreLeadingDialogDashes(replaced, dash, space));
    }

    [Fact]
    public void MixedWithHyphen_NotReplaced()
    {
        var input = "- One." + Environment.NewLine + "– Two.";
        var replaced = RemoveTextForHI.ReplaceLeadingDialogDashes(input, out var dash, out _);
        Assert.Equal(input, replaced);
        Assert.Equal('\0', dash);
    }
}
