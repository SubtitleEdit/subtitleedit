using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt (sweep 17): an unguarded index inside the
/// remove-hyphens helper that took down a whole Fix-Common-Errors / Remove-text-for-HI run.
/// </summary>
public class BugHunt17Test
{
    private static Subtitle MakeSubtitle(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 3000));
        subtitle.Renumber();
        return subtitle;
    }

    [Theory]
    // RemoveSpacesBeginLine's LineStartsWithHtmlTag(true) test is satisfied by a string that is
    // exactly "<i>", so text[3] read past the end. "<i>-" gets there because FixDash strips the
    // dash first and hands it the bare tag.
    [InlineData("<i>-")]
    [InlineData("<i>")]
    [InlineData("<i>- Hello")]
    [InlineData("<font color=\"red\">-")]
    [InlineData("-")]
    [InlineData("")]
    public void FixHyphensRemoveForSingleLine_ShortTaggedText_DoesNotThrow(string text)
    {
        var subtitle = MakeSubtitle(text);

        var exception = Record.Exception(
            () => Helper.FixHyphensRemoveForSingleLine(subtitle, subtitle.Paragraphs[0].Text, 0));

        Assert.Null(exception);
    }
}
