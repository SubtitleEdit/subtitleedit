using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Globalization;
using System.Text;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Apple format/program compatibility sweep - round-trips and real-world shapes
/// validated against Apple's FCPXML DTDs and AVFoundation-authored movies.
/// </summary>
public class AppleFormatsSweepTest
{
    private static Subtitle MakeReference()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("Hello, Apple world!", 1000, 3000));
        s.Paragraphs.Add(new Paragraph("Second line" + Environment.NewLine + "with a line break.", 4000, 6500));
        return s;
    }

    [Fact]
    public void SpruceWithSpaceRoundTripsItsOwnExport()
    {
        var format = new SpruceWithSpace();
        var raw = format.ToText(MakeReference(), "title");
        var lines = raw.SplitToLines();

        // The header used to contain "\\Colour" comment lines the loader counted as
        // errors, so SE could not re-open its own export.
        Assert.True(format.IsMine(lines, "file.stl"));

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, lines, "file.stl");
        Assert.Equal(2, reloaded.Paragraphs.Count);
        Assert.Equal("Hello, Apple world!", reloaded.Paragraphs[0].Text);
        Assert.Equal("Second line" + Environment.NewLine + "with a line break.", reloaded.Paragraphs[1].Text);
    }

    [Fact]
    public void SpruceWithSpaceStyleTogglesAreClosed()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("<i>Italic</i> and <b>bold</b> text.", 1000, 3000));
        var format = new SpruceWithSpace();
        var raw = format.ToText(s, "title");
        Assert.Contains("^IItalic^I and ^Bbold^B text.", raw);

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, raw.SplitToLines(), "file.stl");
        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("<i>Italic</i> and <b>bold</b> text.", reloaded.Paragraphs[0].Text);
    }

    // .STL (uppercase) never matched the lowercased extension in the priority pass, so
    // Spruce claimed DVD Studio Pro one-space files first and kept a leading space on
    // every line.
    [Fact]
    public void DvdStudioProSpaceOneExportIsDetectedAsItself()
    {
        var format = new DvdStudioProSpaceOne();
        var raw = format.ToText(MakeReference(), "title");

        var parsed = Subtitle.Parse(raw.SplitToLines(), ".STL");
        Assert.NotNull(parsed);
        Assert.Equal(format.Name, parsed.OriginalFormat.Name);
        Assert.Equal("Hello, Apple world!", parsed.Paragraphs[0].Text);
    }

    [Fact]
    public void MacCaption10KeepsLineBreaksAndCurrentFrameRate()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var format = new MacCaption10();
            var raw = format.ToText(MakeReference(), "title");

            // Exporting must not permanently switch the project frame rate to 29.97.
            Assert.Equal(25.0, Configuration.Settings.General.CurrentFrameRate);

            var reloaded = new Subtitle();
            format.LoadSubtitle(reloaded, raw.SplitToLines(), "file.mcc");
            Assert.Equal(2, reloaded.Paragraphs.Count);
            var text = reloaded.Paragraphs[1].Text;
            Assert.Equal("Second line" + Environment.NewLine + "with a line break.", text.Trim());
            // The retained pen-location commands used to append stray blank lines on
            // each end-of-caption flush.
            Assert.False(text.EndsWith('\n') || text.EndsWith('\r'), "no trailing line breaks expected: " + text.Replace("\r", "\\r").Replace("\n", "\\n"));
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    // The generatoritem template hardcoded timebase 25 while frames were computed with
    // the current frame rate, so a 23.976/24 fps export came back with all times x24/25.
    [Fact]
    public void FinalCutProTest2XmlTimingSurvivesRoundTrip()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 24.0;
            var raw = new FinalCutProTest2Xml().ToText(MakeReference(), "title");
            Assert.DoesNotContain("<ntsc>>", raw);

            var reloaded = new Subtitle();
            var reader = new FinalCutProXml();
            reader.LoadSubtitle(reloaded, raw.SplitToLines(), "file.xml");
            Assert.Equal(2, reloaded.Paragraphs.Count);
            Assert.Equal(1000, reloaded.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
            Assert.Equal(3000, reloaded.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    // chapter-marker only exists from FCPXML 1.2 on - a 1.1 file with chapter markers
    // fails Apple's DTD validation.
    [Fact]
    public void FinalCutProXcmWritesDtdValidVersion()
    {
        var raw = new FinalCutProXCM().ToText(MakeReference(), "title");
        Assert.Contains("<fcpxml version=\"1.2\">", raw);
    }

    // FCPXML 1.4's DTD requires duration on clip elements.
    [Fact]
    public void FinalCutProXml14ClipHasRequiredDuration()
    {
        var raw = new FinalCutProXml14().ToText(MakeReference(), "title");
        Assert.Contains("<clip offset=\"0s\" name=\"Subtitles\" duration=", raw);
    }

    private const string CaptionFcpXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE fcpxml>
        <fcpxml version="1.13">
            <resources>
                <format id="r1" name="FFVideoFormat1080p25" frameDuration="100/2500s" width="1920" height="1080"/>
            </resources>
            <library>
                <event name="Event">
                    <project name="Project">
                        <sequence format="r1" duration="150000/2500s" tcStart="3600s" tcFormat="NDF">
                            <spine>
                                <asset-clip ref="r1" offset="3600s" name="Clip" start="7200s" duration="150000/2500s">
                                    <caption lane="1" offset="7201s" name="c1" start="3600s" duration="2s" role="iTT?captionFormat=ITT.en">
                                        <text placement="bottom">
                                            <text-style ref="ts1">Hello caption</text-style>
                                        </text>
                                        <text-style-def id="ts1">
                                            <text-style font=".AppleSystemUIFont" fontSize="13" fontColor="1 1 1 1"/>
                                        </text-style-def>
                                    </caption>
                                    <caption lane="1" offset="7204s" name="c2" start="3600s" duration="5/2s" role="iTT?captionFormat=ITT.en">
                                        <text placement="top">
                                            <text-style ref="ts2">Line one
        line two</text-style>
                                            <text-style ref="ts3"> - italic tail</text-style>
                                        </text>
                                        <text-style-def id="ts2">
                                            <text-style font=".AppleSystemUIFont" fontSize="13" fontColor="1 1 1 1"/>
                                        </text-style-def>
                                        <text-style-def id="ts3">
                                            <text-style font=".AppleSystemUIFont" fontSize="13" fontColor="1 1 1 1" italic="1"/>
                                        </text-style-def>
                                    </caption>
                                </asset-clip>
                            </spine>
                        </sequence>
                    </project>
                </event>
            </library>
        </fcpxml>
        """;

    // Final Cut Pro's native caption workflow (File > Export > Captions / caption lanes
    // in fcpxml) was completely unreadable - no importer looked at caption elements.
    [Fact]
    public void FinalCutProCaptionsAreImported()
    {
        var lines = CaptionFcpXml.SplitToLines();
        var parsed = Subtitle.Parse(lines, ".fcpxml");
        Assert.NotNull(parsed);
        Assert.Equal(new FinalCutProXmlCaptions().Name, parsed.OriginalFormat.Name);
        Assert.Equal(2, parsed.Paragraphs.Count);

        // timeline time = parent offset + (caption offset - parent start) - sequence tcStart
        Assert.Equal(1000, parsed.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal(3000, parsed.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
        Assert.Equal("Hello caption", parsed.Paragraphs[0].Text);

        Assert.Equal(4000, parsed.Paragraphs[1].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal("{\\an8}Line one" + Environment.NewLine + "line two<i> - italic tail</i>", parsed.Paragraphs[1].Text);
    }

    // The newest known version class also opens files from future Final Cut Pro
    // releases, so a new fcpxml version does not make the file unreadable.
    [Fact]
    public void FinalCutProXml114AcceptsFutureVersions()
    {
        var raw = new FinalCutProXml114().ToText(MakeReference(), "title")
            .Replace("<fcpxml version=\"1.14\">", "<fcpxml version=\"1.15\">");

        var parsed = Subtitle.Parse(raw.SplitToLines(), ".fcpxml");
        Assert.NotNull(parsed);
        Assert.Equal("Final Cut Pro Xml 1.14", parsed.OriginalFormat.Name);
        Assert.Equal(2, parsed.Paragraphs.Count);
    }

    [Fact]
    public void TitleBasedFcpXml113IsImported()
    {
        var raw = new FinalCutProXml113().ToText(MakeReference(), "title");
        Assert.Contains("<fcpxml version=\"1.13\">", raw);

        var parsed = Subtitle.Parse(raw.SplitToLines(), ".fcpxml");
        Assert.NotNull(parsed);
        Assert.Equal("Final Cut Pro Xml 1.13", parsed.OriginalFormat.Name);
        Assert.Equal(2, parsed.Paragraphs.Count);
        Assert.Equal(1000, parsed.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
    }

    // The title export always wrote styled runs, but the importer flattened any italic/bold
    // in the title onto the whole paragraph.
    [Fact]
    public void FinalCutProTitleKeepsPartialItalicBoldOnRoundTrip()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("<i>Italic</i> and <b>bold</b> text.", 1000, 3000));
        var format = new FinalCutProXml19();
        var raw = format.ToText(s, "title");

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, raw.SplitToLines(), "file.fcpxml");
        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("<i>Italic</i> and <b>bold</b> text.", reloaded.Paragraphs[0].Text);
    }

    // A comma-decimal locale used to write fontColor="0,960784 ..." - Final Cut Pro
    // cannot parse that.
    [Fact]
    public void FinalCutProTitleColorsUseInvariantDecimals()
    {
        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");
            var raw = new FinalCutProXml19().ToText(MakeReference(), "title");
            var colors = System.Text.RegularExpressions.Regex.Matches(raw, "fontColor=\"[^\"]*\"");
            Assert.NotEmpty(colors);
            Assert.All(colors, m => Assert.DoesNotContain(",", m.Value));
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }

    // Caption-based export: Final Cut Pro imports these via File > Import > XML as real
    // captions (the native workflow), instead of the title-based exports.
    [Fact]
    public void FinalCutProXmlCaptionsExportRoundTrips()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var s = MakeReference();
            s.Paragraphs.Add(new Paragraph("{\\an8}<i>Italic</i> and <b>bold</b> runs", 8000, 9500));
            var format = new FinalCutProXmlCaptions();
            var raw = format.ToText(s, "My project");

            Assert.Contains("<fcpxml version=\"1.8\">", raw);
            Assert.Contains("role=\"iTT?captionFormat=ITT.", raw);
            Assert.Contains("FFVideoFormat1080p25", raw);
            Assert.Contains("italic=\"1\"", raw);
            Assert.Contains("bold=\"1\"", raw);
            Assert.Contains("placement=\"top\"", raw);

            var parsed = Subtitle.Parse(raw.SplitToLines(), ".fcpxml");
            Assert.NotNull(parsed);
            Assert.Equal(format.Name, parsed.OriginalFormat.Name);
            Assert.Equal(3, parsed.Paragraphs.Count);
            Assert.Equal(1000, parsed.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
            Assert.Equal(3000, parsed.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
            Assert.Equal("Second line" + Environment.NewLine + "with a line break.", parsed.Paragraphs[1].Text);
            Assert.Equal("{\\an8}<i>Italic</i> and <b>bold</b> runs", parsed.Paragraphs[2].Text);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    // 23.976 fps writes 1001/24000s frame durations; caption times must stay frame-aligned
    // multiples of it or Final Cut Pro complains about off-frame boundaries.
    [Fact]
    public void FinalCutProXmlCaptionsTimesAreFrameAligned()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            var raw = new FinalCutProXmlCaptions().ToText(MakeReference(), "t");
            Assert.Contains("frameDuration=\"1001/24000s\"", raw);
            // 1000 ms -> 24 frames -> 24024/24000s
            Assert.Contains("offset=\"24024/24000s\"", raw);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    // tx3g displayFlags 0x80000000/0x40000000 is how QuickTime/AVFoundation marks a
    // forced-subtitles track.
    [Theory]
    [InlineData(0x80000000u, true)]
    [InlineData(0x40000000u, true)]
    [InlineData(0x00000000u, false)]
    public void StsdReadsTx3gForcedDisplayFlags(uint displayFlags, bool expectedForced)
    {
        var payload = new List<byte>();
        payload.AddRange(new byte[6]); // reserved
        payload.AddRange(new byte[] { 0, 1 }); // data reference index
        payload.AddRange(new[] { (byte)(displayFlags >> 24), (byte)(displayFlags >> 16), (byte)(displayFlags >> 8), (byte)displayFlags });
        payload.AddRange(new byte[20]); // rest of tx3g text description

        var box = new List<byte>();
        box.AddRange(new byte[] { 0, 0, 0, 0 }); // version + flags
        box.AddRange(new byte[] { 0, 0, 0, 1 }); // entry count
        var entrySize = 8 + payload.Count;
        box.AddRange(new[] { (byte)(entrySize >> 24), (byte)(entrySize >> 16), (byte)(entrySize >> 8), (byte)entrySize });
        box.AddRange(Encoding.ASCII.GetBytes("tx3g"));
        box.AddRange(payload);

        using var ms = new MemoryStream(box.ToArray());
        var stsd = new Stsd(ms, (ulong)ms.Length);
        Assert.Equal(expectedForced, stsd.IsForcedSubtitle);
    }
}
