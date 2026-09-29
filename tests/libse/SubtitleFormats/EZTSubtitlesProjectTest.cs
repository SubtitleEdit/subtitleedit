using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class EZTSubtitlesProjectTest
{
    private static Subtitle Load(string xml)
    {
        var subtitle = new Subtitle();
        new EZTSubtitlesProject().LoadSubtitle(subtitle, xml.SplitToLines(), "test.eztxml");
        return subtitle;
    }

    private static string Project(string videoFrameRate, string timeCodeStandard, string inCue, string outCue) =>
        $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<EZTSubtitlesProject version=""1.3"">
  <ProjectConfiguration>
    <VideoFrameRate>{videoFrameRate}</VideoFrameRate>
    <TimeCodeStandard>{timeCodeStandard}</TimeCodeStandard>
  </ProjectConfiguration>
  <Subtitles count=""1"">
    <Subtitle id=""sub1"" number=""1"" incue=""{inCue}"" outcue=""{outCue}"">
      <Rows><Row><Text>Hello</Text></Row></Rows>
    </Subtitle>
  </Subtitles>
</EZTSubtitlesProject>";

    // A 23.976 fps project can carry "30drop" cues (frames up to 29); the cues must be decoded
    // with the time code standard, not the video frame rate.
    [Fact]
    public void CuesFollowTimeCodeStandardNotVideoFrameRate()
    {
        var old = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            var subtitle = Load(Project("23.976 fps", "30drop", "00:00:01:29", "00:00:04:15"));

            Assert.Single(subtitle.Paragraphs);
            Assert.Equal(29.97, Configuration.Settings.General.CurrentFrameRate);
            Assert.Equal(1000 + SubtitleFormat.FramesToMilliseconds(29, 29.97), subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(4000 + SubtitleFormat.FramesToMilliseconds(15, 29.97), subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = old;
        }
    }

    [Fact]
    public void FallsBackToVideoFrameRateWithoutTimeCodeStandard()
    {
        var old = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            var subtitle = Load(Project("25 fps", "", "00:00:01:24", "00:00:02:00"));

            Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);
            Assert.Equal(1960, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = old;
        }
    }

    [Theory]
    [InlineData("30drop", 29.97)]
    [InlineData("60drop", 59.94)]
    [InlineData("25", 25)]
    [InlineData("24", 24)]
    [InlineData("23.976 fps", 23.976)]
    public void ParsesTimeCodeStandard(string text, double expected)
    {
        Assert.True(EZTSubtitlesProject.TryParseTimeCodeStandard(text, out var rate));
        Assert.Equal(expected, rate);
    }

    [Theory]
    [InlineData(29.97, "30drop")]
    [InlineData(59.94, "60drop")]
    [InlineData(23.976, "24")]
    [InlineData(25, "25")]
    public void WritesTimeCodeStandard(double rate, string expected)
    {
        Assert.Equal(expected, EZTSubtitlesProject.ToTimeCodeStandard(rate));
    }

    [Fact]
    public void RoundTripsAt2997()
    {
        var old = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 29.97;
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 1000 + SubtitleFormat.FramesToMilliseconds(29, 29.97), 4000));

            var xml = new EZTSubtitlesProject().ToText(subtitle, "t");
            Assert.Contains("<TimeCodeStandard>30drop</TimeCodeStandard>", xml);
            Assert.DoesNotContain("drop</TimeCodeStandard>", xml.Replace("30drop", ""));

            var loaded = Load(xml);
            Assert.Equal(subtitle.Paragraphs[0].StartTime.TotalMilliseconds, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = old;
        }
    }

    // Sample exported by EZTitles: italic/colour on the row or an inline span, alignment in
    // VisualAttributes, and "--:--:--:--" cues on subtitles without timing.
    [Fact]
    public void LoadsFormattingAndAlignment()
    {
        var subtitle = Load(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<EZTSubtitlesProject version=""1.3"">
  <ProjectConfiguration><TimeCodeStandard>24</TimeCodeStandard></ProjectConfiguration>
  <Subtitles count=""4"">
    <Subtitle id=""sub1"" number=""1"" incue=""--:--:--:--"" outcue=""--:--:--:--"">
      <VisualAttributes row_position=""center"" row_justification=""center"" vertical_align=""bottom""/>
      <Rows><Row foreground_color=""red"" italic=""true""><Text>Red italic text</Text></Row></Rows>
    </Subtitle>
    <Subtitle id=""sub2"" number=""1"" index=""a"" incue=""--:--:--:--"" outcue=""--:--:--:--"">
      <VisualAttributes row_position=""left"" row_justification=""left"" vertical_align=""top""/>
      <Rows><Row foreground_color=""white""><Text>Top left</Text></Row></Rows>
    </Subtitle>
    <Subtitle id=""sub3"" number=""1"" index=""b"" incue=""--:--:--:--"" outcue=""--:--:--:--"">
      <VisualAttributes row_position=""right"" row_justification=""right"" vertical_align=""bottom""/>
      <Rows><Row foreground_color=""yellow""><Text>Bottom right</Text><Style foreground_color=""yellow""/></Row></Rows>
    </Subtitle>
    <Subtitle id=""sub4"" number=""1"" index=""c"" incue=""--:--:--:--"" outcue=""--:--:--:--"">
      <VisualAttributes row_position=""center"" row_justification=""center"" vertical_align=""bottom""/>
      <Rows><Row foreground_color=""white""><Text>Only <span italic=""true"">THIS</span> in italic</Text></Row></Rows>
    </Subtitle>
  </Subtitles>
</EZTSubtitlesProject>");

        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("<font color=\"red\"><i>Red italic text</i></font>", subtitle.Paragraphs[0].Text);
        Assert.Equal("{\\an7}Top left", subtitle.Paragraphs[1].Text);
        Assert.Equal("{\\an3}<font color=\"yellow\">Bottom right</font>", subtitle.Paragraphs[2].Text);
        Assert.Equal("Only <i>THIS</i> in italic", subtitle.Paragraphs[3].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(0, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
