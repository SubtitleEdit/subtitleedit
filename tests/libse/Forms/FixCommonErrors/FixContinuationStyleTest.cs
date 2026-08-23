using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixContinuationStyleTest
{
    // The rule walks paragraph pairs and carries paragraph i+1's sanitized text forward to use
    // as paragraph i's on the next iteration, rather than sanitizing every paragraph twice.
    // Carrying the wrong side of the pair still produces the right answer on a plain
    // continuation chain, so this pins a case that actually discriminates: only paragraph 2
    // continues into paragraph 3 (40 ms gap), while paragraph 1 ends a sentence and paragraph 4
    // is too far away (1600 ms). Get the carry wrong and paragraph 2 keeps its dots-less text.
    [Fact]
    public void OnlyTheContinuingParagraphGetsTrailingDots()
    {
        var previousStyle = Configuration.Settings.General.ContinuationStyle;
        try
        {
            Configuration.Settings.General.ContinuationStyle = ContinuationStyle.OnlyTrailingDots;

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Wait!", 0, 1122));
            subtitle.Paragraphs.Add(new Paragraph("I was going to tell you", 1162, 3083));
            subtitle.Paragraphs.Add(new Paragraph("and now it is too late", 3123, 4220));
            subtitle.Paragraphs.Add(new Paragraph("[door slams]", 5820, 7720));
            subtitle.Renumber();

            new FixContinuationStyle { FixAction = "act" }.Fix(subtitle, new EmptyFixCallback());

            Assert.Equal("Wait!", subtitle.Paragraphs[0].Text);
            Assert.Equal("I was going to tell you...", subtitle.Paragraphs[1].Text);
            Assert.Equal("and now it is too late", subtitle.Paragraphs[2].Text);
            Assert.Equal("[door slams]", subtitle.Paragraphs[3].Text);
        }
        finally
        {
            Configuration.Settings.General.ContinuationStyle = previousStyle;
        }
    }
}
