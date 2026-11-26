using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class AdobeEncoreLineTabNewLineTest
{
    [Theory]
    [InlineData("First", "First")]
    [InlineData("First\nSecond", "First\nSecond")]
    [InlineData("First\nSecond\nThird", "First Second Third")]
    public void ExportRebalancesOnlyCuesWithMoreThanTwoLines(string text, string expected)
    {
        var settings = Configuration.Settings.General;
        var oldMaximumLength = settings.SubtitleLineMaximumLength;
        var oldMergeLength = settings.MergeLinesShorterThan;
        try
        {
            settings.SubtitleLineMaximumLength = 42;
            settings.MergeLinesShorterThan = 30;
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(text.Replace("\n", Environment.NewLine), 1000, 3000));
            var format = new AdobeEncoreLineTabNewLine();

            var output = format.ToText(subtitle, "title");
            var reloaded = new Subtitle();
            format.LoadSubtitle(reloaded, output.SplitToLines(), "test.txt");

            var paragraph = Assert.Single(reloaded.Paragraphs);
            Assert.Equal(expected.Replace("\n", Environment.NewLine), paragraph.Text);
            Assert.Equal(1000, paragraph.StartTime.TotalMilliseconds);
            Assert.Equal(3000, paragraph.EndTime.TotalMilliseconds);
        }
        finally
        {
            settings.SubtitleLineMaximumLength = oldMaximumLength;
            settings.MergeLinesShorterThan = oldMergeLength;
        }
    }
}
