using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Adobe format compatibility sweep - round-trips plus real-world EDL/XMP shapes
/// (Premiere, Avid and Nucoda EDL exports; Premiere tick-based XMP markers).
/// </summary>
public class AdobeFormatsSweepTest
{
    private static Subtitle MakeReference()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("Hello, Adobe world!", 1000, 3000));
        s.Paragraphs.Add(new Paragraph("Second line" + Environment.NewLine + "with a line break.", 4000, 6500));
        return s;
    }

    // NLEs pad EDL event rows with trailing spaces (Premiere, Avid and Nucoda all do) -
    // the row regex used to demand the last time code at end-of-line, so whole files
    // fell through to the unknown-format importer as garbage.
    [Fact]
    public void EdlToleratesTrailingSpacesOnEventRows()
    {
        var lines = new List<string>
        {
            "TITLE:   gap test",
            "FCM: NON-DROP FRAME",
            "001  SHOT1    V     C        00:10:00:00 00:10:01:00 00:00:00:00 00:00:01:00 ",
            "FROM CLIP NAME: shot1",
            "002  SHOT2    V     C        23:00:00:00 23:00:01:00 00:00:01:16 00:00:02:16 ",
            "FROM CLIP NAME: shot2",
        };
        var format = new Edl();
        Assert.True(format.IsMine(lines, "gap_test.edl"));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "gap_test.edl");
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("shot1", subtitle.Paragraphs[0].Text);
        Assert.Equal("shot2", subtitle.Paragraphs[1].Text);
    }

    // Avid writes "* FROM CLIP: path", Nucoda "* FROM FILE: path", screening EDLs
    // "*SOURCE FILE: ..." - those comment lines must not leak into the cue text, and
    // the clip name prefix appears with "* ", "*" and no prefix at all.
    [Fact]
    public void EdlSkipsCommentLinesAndStripsClipNamePrefixVariants()
    {
        var lines = new List<string>
        {
            "TITLE:   Avid_Example.01",
            "001  ZZ100_50 V     C        01:00:04:05 01:00:05:12 00:59:53:11 00:59:54:18",
            "* FROM CLIP NAME:  take_1",
            "* FROM CLIP: S:\\path\\to\\ZZ100_501.take_1.0001.exr",
            "002  ZZ100_50 V     C        01:00:06:13 01:00:08:15 00:59:54:18 00:59:56:20",
            "*FROM CLIP NAME:  take_2",
            "*SOURCE FILE: ZZ100_502A.LAY3.01",
        };
        var format = new Edl();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "avid.edl");
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("take_1", subtitle.Paragraphs[0].Text);
        Assert.Equal("take_2", subtitle.Paragraphs[1].Text);
    }

    // SE writes ':' time codes with no drop-frame arithmetic - that is non-drop time
    // code at every rate. The old header said DROP FRAME for integer rates (backwards),
    // and BL filler events reused the neighboring cue's event number.
    [Fact]
    public void EdlWritesNonDropHeaderAndSequentialEventNumbers()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("One", 5000, 7000));    // >1s start => leading BL
        s.Paragraphs.Add(new Paragraph("Two", 10000, 12000));  // gap => BL between
        var raw = new Edl().ToText(s, "title");

        Assert.Contains("FCM: NON-DROP FRAME", raw);
        Assert.DoesNotContain("DROP FRAME\r\nFCM", raw);

        var eventNumbers = raw.SplitToLines()
            .Where(l => l.Length > 6 && char.IsDigit(l[0]))
            .Select(l => int.Parse(l.Substring(0, 6)))
            .ToList();
        Assert.Equal(new List<int> { 1, 2, 3, 4 }, eventNumbers);

        var reloaded = new Subtitle();
        new Edl().LoadSubtitle(reloaded, raw.SplitToLines(), "file.edl");
        Assert.Equal(2, reloaded.Paragraphs.Count);
        Assert.Equal("One", reloaded.Paragraphs[0].Text);
    }

    // The loader used to cap continuation lines at 200 characters of accumulated text,
    // silently truncating long (e.g. SDH) cues.
    [Fact]
    public void AdobeEncoreTabsKeepsLongCues()
    {
        var longLine1 = new string('a', 150);
        var longLine2 = new string('b', 150);
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph(longLine1 + Environment.NewLine + longLine2, 1000, 3000));
        var format = new AdobeEncoreTabs();
        var raw = format.ToText(s, "title");

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, raw.SplitToLines(), "file.txt");
        Assert.Single(reloaded.Paragraphs);
        Assert.Equal(longLine1 + Environment.NewLine + longLine2, reloaded.Paragraphs[0].Text);
    }

    // AdobeEncoreLineTabs existed but was never registered in AllSubtitleFormats, so its
    // own file shape had no handler.
    [Fact]
    public void AdobeEncoreLineTabsIsRegisteredAndRoundTrips()
    {
        var format = new AdobeEncoreLineTabs();
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("<i>Italic</i> start", 1000, 3000));
        s.Paragraphs.Add(new Paragraph("Second line" + Environment.NewLine + "with a line break.", 4000, 6500));
        var raw = format.ToText(s, "title");

        var parsed = Subtitle.Parse(raw.SplitToLines(), ".txt");
        Assert.NotNull(parsed);
        Assert.Equal(format.Name, parsed.OriginalFormat.Name);
        Assert.Equal(2, parsed.Paragraphs.Count);
        Assert.Equal("<i>Italic</i> start", parsed.Paragraphs[0].Text);
        Assert.Equal("Second line" + Environment.NewLine + "with a line break.", parsed.Paragraphs[1].Text);
    }

    // Premiere stores 254016000000 ticks per second; a real .prproj puts a 4-second
    // clip start at 1016064000000. The old conversion divided by 200000000 - both the
    // wrong constant and ~27% off.
    [Fact]
    public void AdobePremierePrProjConvertsTicksCorrectly()
    {
        var xml = """
            <PremiereData Version="3">
              <VideoComponentChain ObjectID="10">
                <ComponentChain>
                  <Components>
                    <Component ObjectRef="20"/>
                  </Components>
                </ComponentChain>
              </VideoComponentChain>
              <VideoClipTrackItem ObjectID="30">
                <ClipTrackItem>
                  <ComponentOwner>
                    <Components ObjectRef="10"/>
                  </ComponentOwner>
                  <TrackItem>
                    <Start>1016064000000</Start>
                    <End>1524096000000</End>
                  </TrackItem>
                </ClipTrackItem>
              </VideoClipTrackItem>
              <VideoFilterComponent ObjectID="20">
                <Component>
                  <DisplayName>Text</DisplayName>
                  <InstanceName>Hello title</InstanceName>
                </Component>
              </VideoFilterComponent>
            </PremiereData>
            """;
        var subtitle = new Subtitle();
        new AdobePremierePrProj().LoadSubtitle(subtitle, xml.SplitToLines(), "x.xml");
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello title", subtitle.Paragraphs[0].Text);
        Assert.Equal(4000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal(6000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
    }

    // Premiere writes XMP marker times as ticks with frameRate "f254016000000" - read
    // as frame numbers they would be off by ten orders of magnitude.
    [Fact]
    public void XmpHonorsDeclaredTickFrameRate()
    {
        var xmp = """
            <?xml version="1.0" encoding="utf-8"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description rdf:about="" xmlns:xmpDM="http://ns.adobe.com/xmp/1.0/DynamicMedia/">
                  <xmpDM:Tracks>
                    <rdf:Bag>
                      <rdf:li rdf:parseType="Resource">
                        <xmpDM:frameRate>f254016000000</xmpDM:frameRate>
                        <xmpDM:markers>
                          <rdf:Seq>
                            <rdf:li rdf:parseType="Resource">
                              <xmpDM:startTime>1016064000000</xmpDM:startTime>
                              <xmpDM:duration>508032000000</xmpDM:duration>
                              <xmpDM:comment>Marker at four seconds</xmpDM:comment>
                            </rdf:li>
                          </rdf:Seq>
                        </xmpDM:markers>
                      </rdf:li>
                    </rdf:Bag>
                  </xmpDM:Tracks>
                </rdf:Description>
              </rdf:RDF>
            </x:xmpmeta>
            """;
        var subtitle = new Subtitle();
        new Xmp().LoadSubtitle(subtitle, xmp.SplitToLines(), "markers.xmp");
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Marker at four seconds", subtitle.Paragraphs[0].Text);
        Assert.Equal(4000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal(6000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
    }

    // The header used to hardcode f25 while the marker times were written at the current
    // frame rate - Adobe apps then read wrong times at any other rate.
    [Fact]
    public void XmpDeclaresTheFrameRateItWritesWith()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 29.97;
            var format = new Xmp();
            var raw = format.ToText(MakeReference(), "title");
            Assert.Contains("<xmpDM:frameRate>f30000s1001</xmpDM:frameRate>", raw);

            var reloaded = new Subtitle();
            format.LoadSubtitle(reloaded, raw.SplitToLines(), "file.xmp");
            Assert.Equal(2, reloaded.Paragraphs.Count);
            Assert.Equal(1000, reloaded.Paragraphs[0].StartTime.TotalMilliseconds, 40.0);
            Assert.Equal(3000, reloaded.Paragraphs[0].EndTime.TotalMilliseconds, 40.0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    [Theory]
    [InlineData("f25", 25, 1)]
    [InlineData("f24000s1001", 24000, 1001)]
    [InlineData("f254016000000", 254016000000, 1)]
    public void XmpFrameRateParsing(string input, double expectedNumerator, double expectedDenominator)
    {
        Assert.True(Xmp.TryParseFrameRate(input, out var numerator, out var denominator));
        Assert.Equal(expectedNumerator, numerator);
        Assert.Equal(expectedDenominator, denominator);
    }

    // Premiere's Markers panel "Export Markers as CSV" is tab-separated despite the
    // extension; point markers have Out == In and a zero duration.
    [Fact]
    public void PremiereMarkersCsvImports()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var lines = new List<string>
            {
                "Marker Name\tDescription\tIn\tOut\tDuration\tMarker Type",
                "Intro\tOpening narration starts\t00:00:01:00\t00:00:03:00\t00:00:02:00\tComment",
                "Point\t\t00:00:04:12\t00:00:04:12\t00:00:00:00\tComment",
            };

            var parsed = Subtitle.Parse(lines, ".csv");
            Assert.NotNull(parsed);
            Assert.Equal(new AdobePremiereMarkersCsv().Name, parsed.OriginalFormat.Name);
            Assert.Equal(2, parsed.Paragraphs.Count);
            Assert.Equal("Opening narration starts", parsed.Paragraphs[0].Text);
            Assert.Equal(1000, parsed.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
            Assert.Equal(3000, parsed.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
            // Point marker: text falls back to the marker name, and it gets a usable duration.
            Assert.Equal("Point", parsed.Paragraphs[1].Text);
            Assert.Equal(4480, parsed.Paragraphs[1].StartTime.TotalMilliseconds, 1.0);
            Assert.Equal(5480, parsed.Paragraphs[1].EndTime.TotalMilliseconds, 1.0);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }

    [Fact]
    public void PremiereMarkersCsvRoundTrips()
    {
        var format = new AdobePremiereMarkersCsv();
        var raw = format.ToText(MakeReference(), "title");

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, raw.SplitToLines(), "markers.csv");
        Assert.Equal(2, reloaded.Paragraphs.Count);
        Assert.Equal("Hello, Adobe world!", reloaded.Paragraphs[0].Text);
        Assert.Equal(1000, reloaded.Paragraphs[0].StartTime.TotalMilliseconds, 25.0);
        Assert.Equal(3000, reloaded.Paragraphs[0].EndTime.TotalMilliseconds, 25.0);
    }
}
