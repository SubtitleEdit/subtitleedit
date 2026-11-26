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

    [Theory]
    [InlineData("0:01 – 0:03 First\n0:04 – 0:06 Second|Continued")]
    [InlineData("0:01 – 0:03\nFirst\n0:04 – 0:06\nSecond\nContinued")]
    [InlineData("{0:00:01}{0:00:03}First\n{0:00:04}{0:00:06}Second|Continued")]
    public void WholeSecondRanges(string text)
    {
        var subtitle = Import(text);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("First", subtitle.Paragraphs[0].Text);
        Assert.Equal(4000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void CsvExplicitMillisecondsAndNumberedTextColumns()
    {
        var subtitle = Import(
            "Start time in milliseconds;End time in milliseconds;Line 1;Line 2\n" +
            "100;150;\"Hello; there\";World\n",
            "test.csv");

        var cue = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(100, cue.StartTime.TotalMilliseconds);
        Assert.Equal(150, cue.EndTime.TotalMilliseconds);
        Assert.Equal("Hello; there" + Environment.NewLine + "World", cue.Text);
    }

    [Theory]
    [InlineData("tc1", "tc2")]
    [InlineData("PositionIn", "PositionOut")]
    public void XmlTimingAliasesAndCDataLineBreaks(string start, string end)
    {
        var subtitle = Import(
            $"<cues><cue {start}=\"1500\" {end}=\"3500\"><![CDATA[<sub>First<br />Continued</sub>]]></cue>" +
            $"<cue {start}=\"4200\" {end}=\"5600\" CaptionText=\"Second\" /></cues>",
            "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("First" + Environment.NewLine + "Continued", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void PairedDelimitersAndSlashBreaksPreserveUrlsAndDialogueDashes()
    {
        var subtitle = Import(
            "00:00:01:00,00:00:03:00,- Visit https://example.com/a\n" +
            "00:00:04:00,00:00:06:00,Second///Continued\n");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("- Visit https://example.com/a", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void ColonHundredthsApplyAcrossTheFile()
    {
        var subtitle = Import(
            "00:00:01:12\t00:00:03:20\tFirst\n" +
            "00:00:04:82\t00:00:06:68\tSecond\tContinued\n");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1120, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3200, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4820, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6680, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void TabularDurationColumnIsRemovedWhenItMatchesTheCue()
    {
        var subtitle = Import(
            "00:00:01.500\t00:00:03.500\t00:00:02.000\tFirst\n" +
            "00:00:04.820\t00:00:06.680\t00:00:01.860\tSecond\n");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("First", subtitle.Paragraphs[0].Text);
        Assert.Equal(4820, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6680, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void XmlPrefersTimedTextOverMoreNumerousLayoutEntries()
    {
        var subtitle = Import(
            "<doc><layout>" + string.Concat(Enumerable.Range(1, 8).Select(i =>
                $"<item position=\"{i}\"><textAlign>center</textAlign></item>")) +
            "</layout><cue start=\"1.5s\" end=\"3.5s\"><![CDATA[First\\nContinued]]></cue>" +
            "<cue start=\"4s\" end=\"6s\">Second</cue></doc>", "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("First" + Environment.NewLine + "Continued", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void RtfIsUnwrappedBeforeGuessingTimecodeLayout()
    {
        var subtitle = Import(
            @"{\rtf1\ansi 00:00:01.500 --> 00:00:03.500\par First\par " +
            @"00:00:04.820 --> 00:00:06.680\par Second\par}", "test.rtf");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("First", subtitle.Paragraphs[0].Text);
        Assert.Equal(4820, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6680, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void CommentedCsvMarkerHeaderPreservesQuotedMultilineText()
    {
        var subtitle = Import(
            "# Marker list\n# No, Anchor, Position, Duration, Comment\n" +
            "1,ON,\"00:00:01:500\",\"00:00:02:000\",\"First, line\nSecond line\"",
            "test.csv");

        var cue = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(1500, cue.StartTime.TotalMilliseconds);
        Assert.Equal(3500, cue.EndTime.TotalMilliseconds);
        Assert.Equal("First, line" + Environment.NewLine + "Second line", cue.Text);
    }

    [Fact]
    public void TimelineXmlUsesFrameRateAndTextParameter()
    {
        var subtitle = Import(
            "<xmeml><sequence><rate><timebase>25</timebase></rate><media><video><track>" +
            "<generatoritem><start>25</start><end>50</end><effect><parameter>" +
            "<parameterid>str</parameterid><value>First</value></parameter></effect></generatoritem>" +
            "<generatoritem><rate><timebase>30</timebase></rate><start>90</start><end>120</end>" +
            "<effect><parameter><parameterid>str</parameterid><value>Second&#13;Continued</value>" +
            "</parameter></effect></generatoritem></track></video></media></sequence></xmeml>",
            "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(3000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(4000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void JsonTextCanPrecedeNestedClockTimes()
    {
        var subtitle = Import(
            "{\"transcript\":[" +
            "{\"text\":\"First\",\"instances\":[{\"start\":\"00:00:01.500\",\"end\":\"00:00:03.500\"}]}," +
            "{\"text\":\"Second<br>Continued\",\"instances\":[{\"start\":\"00:00:04.820\",\"end\":\"00:00:06.680\"}]}," +
            "{\"text\":\"Third\",\"instances\":[{\"start\":\"00:00:10.000\",\"end\":\"00:00:12.720\"}]}]}",
            "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(4820, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6680, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("00:00:01:00 First\n00:00:03:00\n00:00:04:00 Second\n00:00:06:00")]
    [InlineData("00:00:01:00\nFirst\n00:00:03:00\n00:00:04:00\nSecond\n00:00:06:00")]
    [InlineData("00 00 01 00\n00 00 03 00\nFirst\n00 00 04 00\n00 00 06 00\nSecond")]
    [InlineData("First\n<00:00:01:00><00:00:03:00>\nSecond\n<00:00:04:00><00:00:06:00>")]
    [InlineData("First\n00:00:01:00\n00:00:03:00\nSecond\n00:00:04:00\n00:00:06:00")]
    public void PairedTimingMarkersCanPrecedeFollowOrSurroundText(string text)
    {
        var subtitle = Import(text);

        Assert.Collection(subtitle.Paragraphs,
            cue =>
            {
                Assert.Equal("First", cue.Text);
                Assert.Equal(1000, cue.StartTime.TotalMilliseconds);
                Assert.Equal(3000, cue.EndTime.TotalMilliseconds);
            },
            cue =>
            {
                Assert.Equal("Second", cue.Text);
                Assert.Equal(4000, cue.StartTime.TotalMilliseconds);
                Assert.Equal(6000, cue.EndTime.TotalMilliseconds);
            });
    }

    [Fact]
    public void WholeSecondStartsKeepTextAndEndBeforeTheNextCue()
    {
        var subtitle = Import("00:00:01:This is a longer caption.|Another line\n00:00:02:Second");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("This is a longer caption." + Environment.NewLine + "Another line", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1999, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(2000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void XmlDecimalSecondsApplyToWholeValuesInSameCueList()
    {
        var subtitle = Import(
            "<doc><list><cue start=\"1.5\" dur=\"2\">First</cue>" +
            "<cue start=\"10\" dur=\"2.5\">Second</cue></list><list>" +
            "<cue start=\"15000\" dur=\"2500\">Third</cue></list></doc>",
            "test.xml");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(10000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(12500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(15000, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal(17500, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void SamiSignatureUsesSyncMarkersWithoutAFileExtension()
    {
        var subtitle = Import(
            "<SAMI>\n<BODY>\n<SYNC Start=1000><P>First\n<SYNC Start=3000><P>&nbsp;\n" +
            "<SYNC Start=4000><P>Second\n<SYNC Start=6000><P>&nbsp;\n</BODY>\n</SAMI>",
            "captions.txt");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("First", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("1:  00:00:01.00  00:00:03.00 [5]\nFirst\n2:  00:00:04.00  00:00:06.00 [6]\nSecond")]
    [InlineData("0001.00-0003.00\nFirst\n0004.00-0006.00\nSecond")]
    [InlineData("[1.000 3.000] First\n[4.000 6.000] Second")]
    public void PrintedTimingRangesKeepTextAndDiscardReportMetadata(string text)
    {
        var subtitle = Import(text);

        Assert.Collection(subtitle.Paragraphs,
            cue =>
            {
                Assert.Equal("First", cue.Text);
                Assert.Equal(1000, cue.StartTime.TotalMilliseconds);
                Assert.Equal(3000, cue.EndTime.TotalMilliseconds);
            },
            cue =>
            {
                Assert.Equal("Second", cue.Text);
                Assert.Equal(4000, cue.StartTime.TotalMilliseconds);
                Assert.Equal(6000, cue.EndTime.TotalMilliseconds);
            });
    }

    [Fact]
    public void HeaderlessTimedCsvKeepsQuotedFieldsAndLineBreaks()
    {
        var subtitle = Import(
            "\"1\",\"00:00:01:00\",\"00:00:03:00\",\"A\",\"First, line\ncontinued\",\"True\"\n" +
            "\"2\",\"00:00:04:00\",\"00:00:06:00\",\"B\",\"Second\",\"False\"",
            "test.csv");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal("First, line" + Environment.NewLine + "continued", subtitle.Paragraphs[0].Text);
        Assert.Equal("A", subtitle.Paragraphs[0].Actor);
        Assert.Equal("True", subtitle.Paragraphs[0].Effect);
    }

    [Fact]
    public void JsonEnvelopeCanContainEscapedSubRip()
    {
        var subtitle = Import(
            "{\"data\":{\"websrt\":\"1\\n00:00:01,000 --> 00:00:03,000\\nFirst\\n\\n" +
            "2\\n00:00:04,000 --> 00:00:06,000\\nSecond\\n\\n3\\n00:00:07,000 --> 00:00:09,000\\nThird\\n\"}}",
            "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
        Assert.Equal(4000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void IndexedXmlRecordsWithTypedTextFormOneCueList()
    {
        var subtitle = Import(
            "<doc><rows><Data1><In>00:00:01.000</In><Out>00:00:03.000</Out>" +
            "<Field1><Type>Text</Type><Data>First</Data></Field1></Data1>" +
            "<Data2><In>00:00:04.000</In><Out>00:00:06.000</Out>" +
            "<Field1><Type>Text</Type><Data>Second|Continued</Data></Field1></Data2></rows></doc>",
            "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("First", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second" + Environment.NewLine + "Continued", subtitle.Paragraphs[1].Text);
        Assert.Equal(4000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void FcpxmlAnchoredTitlesUseTimelineTime()
    {
        var subtitle = Import(
            "<fcpxml version=\"1.8\"><library><event><project><sequence><spine>" +
            "<gap offset=\"0s\" start=\"3600s\" duration=\"20s\">" +
            "<title offset=\"3601s\" duration=\"2s\"><text><text-style>First</text-style></text></title></gap>" +
            "<asset-clip offset=\"20s\" start=\"100s\" duration=\"20s\">" +
            "<title lane=\"1\" offset=\"104s\" duration=\"2s\"><text><text-style>Second</text-style></text></title>" +
            "</asset-clip></spine></sequence></project></event></library></fcpxml>",
            "test.fcpxml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(24000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(26000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void XmlCDataKeepsWindowsPathBackslashes()
    {
        var subtitle = Import(
            "<cues><cue start=\"1.5s\" end=\"3.5s\"><![CDATA[Open C:\\new\\file]]></cue>" +
            "<cue start=\"4s\" end=\"6s\"><![CDATA[First\\nSecond]]></cue></cues>",
            "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Open C:\\new\\file", subtitle.Paragraphs[0].Text);
        Assert.Equal("First" + Environment.NewLine + "Second", subtitle.Paragraphs[1].Text);
    }
}
