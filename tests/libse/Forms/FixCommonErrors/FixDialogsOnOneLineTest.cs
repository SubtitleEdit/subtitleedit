using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixDialogsOnOneLineTest
{
    private static string Fix(string input)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new FixDialogsOnOneLine().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs[0].Text;
    }

    // Issue #11521: a single line holding two dialog parts must split AND get the
    // dash spacing of the configured dialog style in one run.
    [Fact]
    public void SplitsAndAddsDashSpaces_WhenBothLinesWithSpace()
    {
        var old = Configuration.Settings.General.DialogStyle;
        Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithSpace;
        try
        {
            var result = Fix("-David Smith? -David Bowie, numbnuts.");
            Assert.Equal("- David Smith?" + Environment.NewLine + "- David Bowie, numbnuts.", result);
        }
        finally
        {
            Configuration.Settings.General.DialogStyle = old;
        }
    }

    [Fact]
    public void SplitsWithoutDashSpaces_WhenBothLinesWithoutSpace()
    {
        var old = Configuration.Settings.General.DialogStyle;
        Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithoutSpace;
        try
        {
            var result = Fix("-David Smith? -David Bowie, numbnuts.");
            Assert.Equal("-David Smith?" + Environment.NewLine + "-David Bowie, numbnuts.", result);
        }
        finally
        {
            Configuration.Settings.General.DialogStyle = old;
        }
    }
}
