using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class UnknownFormatImporterEasyWinsTest
{
    private static Subtitle Import(string text, string fileName = "test.txt")
    {
        return new UnknownFormatImporter().AutoGuessImport(text.SplitToLines(), fileName);
    }

    [Theory]
    [InlineData("{0} --> {1} {2}")]
    [InlineData("{0} - {1} {2}")]
    [InlineData("[{0} --> {1}]  {2}")]
    [InlineData("{0} – {1}\t{2}")]
    public void SeparatorBetweenTimeCodesIsNotInText(string pattern)
    {
        var text = string.Join(Environment.NewLine,
            string.Format(pattern, "1:02:03.456", "1:02:05.000", "Long hours"),
            string.Format(pattern, "1:02:06.000", "1:02:08.000", "Second"),
            string.Format(pattern, "1:02:09.000", "1:02:10.500", "- Dialog dash stays"));

        var subtitle = Import(text);

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Long hours", subtitle.Paragraphs[0].Text);
        Assert.Equal("- Dialog dash stays", subtitle.Paragraphs[2].Text);
        Assert.Equal(new TimeCode(1, 2, 3, 456).TotalMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void TwoDigitsAfterDotAreHundredthsNotFrames()
    {
        var subtitle = Import(
            "Notes\n" +
            "0:00:01.50 - 0:00:03.25 Hello there\n" +
            "0:00:04.1 - 0:00:06.00 Second line\n" +
            "0:00:07,00 - 0:00:08,75 Third\n");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3250, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4100, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(8750, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void HtmlEntitiesAreDecoded()
    {
        var subtitle = Import(
            "1:00:01.000 - 1:00:03.000 Tom &amp; Jerry\n" +
            "1:00:04.000 - 1:00:06.000 He said &quot;hi&quot;\n" +
            "1:00:07.000 - 1:00:08.000 It&#39;s&nbsp;fine &lt;3\n");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Tom & Jerry", subtitle.Paragraphs[0].Text);
        Assert.Equal("He said \"hi\"", subtitle.Paragraphs[1].Text);
        Assert.Equal("It's fine <3", subtitle.Paragraphs[2].Text);
    }

    [Theory]
    [InlineData("start,end,text\n1.5,3.2,Hello\n3.5,5.0,World\n5.5,7.0,Third\n")]
    [InlineData("Start;End;Text\n1,5;3,2;Hello\n3,5;5,0;World\n5,5;7,0;Third\n")]
    [InlineData("start,duration,text\n1.5,1.7,Hello\n3.5,1.5,World\n5.5,1.5,Third\n")]
    [InlineData("start,end,text\n1500,3200,Hello\n3500,5000,World\n5500,7000,Third\n")]
    public void CsvPlainNumberTimes(string csv)
    {
        var subtitle = Import(csv, "test.csv");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3200, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(7000, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void CsvWholeSecondsStaySeconds()
    {
        var subtitle = Import("start,end,text\n1,3,Hello\n4,6,World\n7,9,Third\n", "test.csv");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(9000, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void JsonOffsetAndDuration()
    {
        var subtitle = Import(
            "[{\"offset\":1500,\"duration\":2000,\"text\":\"Offset one\"},{\"offset\":4000,\"duration\":1500,\"text\":\"Two\"},{\"offset\":6000,\"duration\":1000,\"text\":\"Three\"}]",
            "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Offset one", subtitle.Paragraphs[0].Text);
    }
}
