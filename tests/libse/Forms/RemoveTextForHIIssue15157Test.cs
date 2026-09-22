using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

/// <summary>
/// Issue #15157: with no option selected, "Remove text for hearing impaired" still listed
/// hundreds of lines in a file that centers its text with leading spaces. The rebuild
/// collapsed the padding, that white-space-only difference counted as a change, and the
/// Fix Common Errors dash fixer then stripped or added dialog dashes.
/// </summary>
public class RemoveTextForHIIssue15157Test
{
    private static RemoveTextForHI MakeRemover(Subtitle subtitle)
    {
        var settings = new RemoveTextForHISettings(subtitle)
        {
            OnlyIfInSeparateLine = false,
            RemoveTextBetweenSquares = false,
            RemoveTextBetweenBrackets = false,
            RemoveTextBetweenParentheses = false,
            RemoveTextBetweenQuestionMarks = false,
            RemoveTextBetweenCustomTags = false,
            RemoveInterjections = false,
            RemoveIfAllUppercase = false,
            RemoveTextBeforeColon = false,
            RemoveWhereContains = false,
            RemoveIfOnlyMusicSymbols = false,
            CustomStart = "?",
            CustomEnd = "?",
        };
        return new RemoveTextForHI(settings);
    }

    private static Subtitle Make(params string[] texts)
    {
        var s = new Subtitle();
        var t = 0;
        foreach (var text in texts)
        {
            s.Paragraphs.Add(new Paragraph(text, t, t + 1000));
            t += 2000;
        }
        return s;
    }

    [Fact]
    public void NoOptions_SpacePaddedSingleSpeakerDash_Unchanged()
    {
        var text = "            - Tiens," + Environment.NewLine + "       ça va être bon, ça.";
        var sub = Make("   (stridulations de grillon)", text, " - Pourquoi on dort dans l'auto?");
        var remover = MakeRemover(sub);
        Assert.Equal(text, remover.RemoveTextFromHearImpaired(text, sub, 1, "fr"));
    }

    [Fact]
    public void NoOptions_SpacePaddedSecondLineDash_Unchanged()
    {
        var text = "       Êtes-vous bien, là?" + Environment.NewLine + "          - J'ai froid.";
        var sub = Make("x", text, "y");
        var remover = MakeRemover(sub);
        Assert.Equal(text, remover.RemoveTextFromHearImpaired(text, sub, 1, "fr"));
    }

    [Fact]
    public void NoOptions_SpacePaddedOneLiner_Unchanged()
    {
        var text = " - Pourquoi on dort dans l'auto?";
        var sub = Make("x", text, "y");
        var remover = MakeRemover(sub);
        Assert.Equal(text, remover.RemoveTextFromHearImpaired(text, sub, 1, "fr"));
    }

    [Fact]
    public void NoOptions_DoubleSpaceInsideLine_Unchanged()
    {
        var text = "- Hello  there," + Environment.NewLine + "how are you?";
        var sub = Make("x", text, "y");
        var remover = MakeRemover(sub);
        Assert.Equal(text, remover.RemoveTextFromHearImpaired(text, sub, 1, "en"));
    }

    [Fact]
    public void Squares_RemovedDialog_StillFixesDash()
    {
        var text = "- [Sighs]" + Environment.NewLine + "- Hello.";
        var sub = Make("x", text, "y");
        var remover = MakeRemover(sub);
        remover.Settings.RemoveTextBetweenSquares = true;
        Assert.Equal("Hello.", remover.RemoveTextFromHearImpaired(text, sub, 1, "en"));
    }

    [Fact]
    public void Squares_SpacePaddedRemovedDialog_StillFixesDash()
    {
        var text = "      - [Sighs]" + Environment.NewLine + "      - Hello.";
        var sub = Make("x", text, "y");
        var remover = MakeRemover(sub);
        remover.Settings.RemoveTextBetweenSquares = true;
        Assert.Equal("Hello.", remover.RemoveTextFromHearImpaired(text, sub, 1, "en"));
    }
}
