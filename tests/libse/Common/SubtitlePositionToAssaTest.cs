using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

public class SubtitlePositionToAssaTest
{
    private const string TtmlHeader = @"<?xml version=""1.0"" encoding=""utf-8""?>
<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"">
  <head>
    <layout>
      <region xml:id=""bottom"" tts:origin=""10% 80%"" tts:extent=""80% 15%"" tts:displayAlign=""after"" tts:textAlign=""center"" />
      <region xml:id=""top"" tts:origin=""10% 10%"" tts:extent=""80% 15%"" tts:displayAlign=""before"" tts:textAlign=""start"" />
    </layout>
  </head>
  <body><div /></body>
</tt>";

    private static Subtitle SubtitleWith(Paragraph paragraph)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(paragraph);
        return subtitle;
    }

    private static Paragraph ParagraphWithRegion(string text, string region)
    {
        return new Paragraph(text, 1000, 3000) { Region = region };
    }

    [Fact]
    public void TtmlBottomRegionKeepsTheLineJustAboveTheRegionBottom()
    {
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "bottom"));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, TtmlHeader));

        var p = subtitle.Paragraphs[0];
        Assert.Equal(@"{\an2}Hi there!", p.Text);
        Assert.Equal("14", p.MarginV); // 100% - (80% + 15%) of 288
        Assert.Equal("38", p.MarginL); // 10% of 384
        Assert.Equal("38", p.MarginR);
    }

    [Fact]
    public void TtmlTopLeftRegionIsMeasuredFromTheTop()
    {
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "top"));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, TtmlHeader));

        var p = subtitle.Paragraphs[0];
        Assert.Equal(@"{\an7}Hi there!", p.Text); // before + start
        Assert.Equal("29", p.MarginV); // 10% of 288
    }

    [Fact]
    public void TtmlRegionInPixelsUsesTheRootExtent()
    {
        const string header = @"<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" tts:extent=""1920px 1080px"">
  <head><layout><region xml:id=""r1"" tts:origin=""192px 108px"" tts:extent=""1536px 216px"" tts:displayAlign=""before"" /></layout></head>
</tt>";
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "r1"));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, header));

        var p = subtitle.Paragraphs[0];
        Assert.Equal(@"{\an8}Hi there!", p.Text);
        Assert.Equal("29", p.MarginV); // 108 of 1080 = 10% of 288
    }

    [Fact]
    public void TtmlRegionStyleReferenceIsResolved()
    {
        const string header = @"<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"">
  <head>
    <styling><style xml:id=""s1"" tts:origin=""10% 5%"" tts:extent=""80% 20%"" tts:displayAlign=""before"" tts:textAlign=""end"" /></styling>
    <layout><region xml:id=""r1"" style=""s1"" /></layout>
  </head>
</tt>";
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "r1"));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, header));

        var p = subtitle.Paragraphs[0];
        Assert.Equal(@"{\an9}Hi there!", p.Text);
        Assert.Equal("14", p.MarginV); // 5% of 288
    }

    [Fact]
    public void AnAlignmentTagInTheTextWins()
    {
        // TimedTextImsc11 already puts a tag in front when reading - it must not get a second one.
        var subtitle = SubtitleWith(ParagraphWithRegion(@"{\an8}Hi there!", "bottom"));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, TtmlHeader));

        var p = subtitle.Paragraphs[0];
        Assert.Equal(@"{\an8}Hi there!", p.Text);
        Assert.Equal("230", p.MarginV); // top of the region, 80% of 288
    }

    [Fact]
    public void UnknownRegionIsLeftAlone()
    {
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "no-such-region"));

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, TtmlHeader));

        var p = subtitle.Paragraphs[0];
        Assert.Equal("Hi there!", p.Text);
        Assert.Null(p.MarginV);
    }

    // The header of the file a subtitle was read from stays on it when the format is switched in
    // the toolbar, so a subtitle now shown as SubRip asks for the positioning to be left off - see
    // SubtitleFormat.HasPositionSupport.
    [Fact]
    public void TtmlRegionIsIgnoredWhenPositionsAreOff()
    {
        var subtitle = SubtitleWith(ParagraphWithRegion("Hi there!", "top"));

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, TtmlHeader, false));

        var p = subtitle.Paragraphs[0];
        Assert.Equal("Hi there!", p.Text);
        Assert.Null(p.MarginV);
        Assert.Null(p.MarginL);
    }

    [Fact]
    public void PacPercentageBecomesAMargin()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "16.6666666666667%" });

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, null));

        Assert.Equal("48", subtitle.Paragraphs[0].MarginV); // 1/6 of 288
    }

    [Fact]
    public void PacPercentageIsRemovedWhenPositionsAreOff()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "50%" });

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, null, false));

        Assert.Null(subtitle.Paragraphs[0].MarginV);
    }

    [Fact]
    public void EbuTeletextRowNearTheBottomLeavesTheRowsBelowItFree()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi" + Environment.NewLine + "there!", 1000, 3000) { MarginV = "20" });

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, MakeEbuHeader()));

        var p = subtitle.Paragraphs[0];
        Assert.StartsWith(@"{\an2}", p.Text, StringComparison.Ordinal);
        Assert.Equal("13", p.MarginV); // one row of 23 left below the two lines (rows 20 and 22)
    }

    [Fact]
    public void EbuTeletextRowNearTheTopIsMeasuredFromTheTop()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "3" });

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, MakeEbuHeader()));

        var p = subtitle.Paragraphs[0];
        Assert.StartsWith(@"{\an8}", p.Text, StringComparison.Ordinal);
        Assert.Equal("25", p.MarginV); // 2 of 23 rows
    }

    [Fact]
    public void EbuTeletextRowIsNotLeftBehindAsAPixelMargin()
    {
        // A row number means nothing to libass - it used to move every line by a near random amount.
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "20" });

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, MakeEbuHeader(), false));

        Assert.Null(subtitle.Paragraphs[0].MarginV);
    }

    [Fact]
    public void DvbTeletextRowNearTheBottomLeavesTheRowsBelowItFree()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi" + Environment.NewLine + "there!", 1000, 3000) { MarginV = "20" });

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, DvbTeletext.CreateHeader(888, "eng")));

        var p = subtitle.Paragraphs[0];
        Assert.StartsWith(@"{\an2}", p.Text, StringComparison.Ordinal);
        Assert.Equal("13", p.MarginV); // one row of 23 left below the two lines (rows 20 and 22)
    }

    [Fact]
    public void DvbTeletextWithoutARowLandsOnTheWritersDefaultRow()
    {
        // ManzanitaTeletextWriter puts a single line on row 22 (a double height row covers 22
        // and 23) - the preview should show it there, not wherever the EBU margins point.
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000));

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, DvbTeletext.CreateHeader(888, "eng")));

        var p = subtitle.Paragraphs[0];
        Assert.StartsWith(@"{\an2}", p.Text, StringComparison.Ordinal);
        Assert.Equal("13", p.MarginV); // row 22 of 23, one row left below
    }

    [Fact]
    public void DvbTeletextRowIsNotLeftBehindAsAPixelMargin()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "20" });

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, DvbTeletext.CreateHeader(888, "eng"), false));

        Assert.Null(subtitle.Paragraphs[0].MarginV);
    }

    [Fact]
    public void AssaMarginsAreLeftAlone()
    {
        var subtitle = SubtitleWith(new Paragraph("Hi there!", 1000, 3000) { MarginV = "60" });

        Assert.False(SubtitlePositionToAssa.ApplyPositions(subtitle, AdvancedSubStationAlphaHeader));

        Assert.Equal("60", subtitle.Paragraphs[0].MarginV);
    }

    [Fact]
    public void TimedTextFileIsPositionedFromItsRegions()
    {
        var lines = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" xml:lang=""en"">
 <head>
  <layout>
   <region xml:id=""speaker"" tts:origin=""10% 10%"" tts:extent=""80% 20%"" tts:displayAlign=""before"" tts:textAlign=""center""/>
   <region xml:id=""subtitle"" tts:origin=""10% 70%"" tts:extent=""80% 25%"" tts:displayAlign=""after"" tts:textAlign=""center""/>
  </layout>
 </head>
 <body><div>
   <p begin=""00:00:01.000"" end=""00:00:03.000"" region=""subtitle"">Down here at the bottom.</p>
   <p begin=""00:00:04.000"" end=""00:00:06.000"" region=""speaker"">Up here, out of the way.</p>
 </div></body>
</tt>".SplitToLines();

        var subtitle = new Subtitle();
        new TimedText10().LoadSubtitle(subtitle, lines, null);

        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, subtitle.Header));

        Assert.Equal(@"{\an2}Down here at the bottom.", subtitle.Paragraphs[0].Text);
        Assert.Equal("14", subtitle.Paragraphs[0].MarginV); // 5% left below the region
        Assert.Equal(@"{\an8}Up here, out of the way.", subtitle.Paragraphs[1].Text);
        Assert.Equal("29", subtitle.Paragraphs[1].MarginV); // region starts 10% down
    }

    [Fact]
    public void Imsc11FileIsPositionedFromItsRegions()
    {
        var lines = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" xml:lang=""en"">
 <head>
  <layout>
   <region xml:id=""r1"" tts:origin=""10% 60%"" tts:extent=""80% 30%"" tts:displayAlign=""after"" tts:textAlign=""center""/>
  </layout>
 </head>
 <body><div>
   <p begin=""00:00:01.000"" end=""00:00:03.000"" region=""r1"">Hi there!</p>
 </div></body>
</tt>".SplitToLines();

        var subtitle = new Subtitle();
        new TimedTextImsc11().LoadSubtitle(subtitle, lines, null);

        Assert.Equal("r1", subtitle.Paragraphs[0].Region);
        Assert.True(SubtitlePositionToAssa.ApplyPositions(subtitle, subtitle.Header));

        // The region bottom is at 90%, where the reader-snapped-to-thirds alignment tag alone
        // would have left the line at the preview margin.
        Assert.Equal("29", subtitle.Paragraphs[0].MarginV);
    }

    private const string AdvancedSubStationAlphaHeader = @"[Script Info]
ScriptType: v4.00+

[V4+ Styles]
Format: Name, Fontname
Style: Default,Arial

[Events]";

    private static string MakeEbuHeader()
    {
        // Code page number + disk format code, padded to the 1024 byte GSI block Ebu.LoadSubtitle keeps.
        return ("850" + "STL25.01").PadRight(1024, ' ');
    }
}
