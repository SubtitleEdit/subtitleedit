using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class DCinemaSmpteTest
{
    // Regression: MsToFramesMaxFrameRate used to clamp against the global
    // Configuration.Settings.General.CurrentFrameRate instead of the parameter,
    // so calling the helper at a different frame rate than the active project
    // setting produced an out-of-range frame number on the last frame of a
    // second.
    [Fact]
    public void MsToFramesMaxFrameRate_2014_ClampsAgainstParameter_NotGlobal()
    {
        var originalGlobal = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            // Global is 25 fps, but we ask for frames at 24 fps. 999 ms at 24 fps
            // rounds to frame 24 — invalid (valid range 0..23) — and must clamp
            // to 23. Previously this saw `24 >= 25` (false) and returned 24.
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var frames = DCinemaSmpte2014.MsToFramesMaxFrameRate(999, 24);
            Assert.Equal(23, frames);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalGlobal;
        }
    }

    [Fact]
    public void MsToFramesMaxFrameRate_2010_ClampsAgainstParameter_NotGlobal()
    {
        var originalGlobal = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var frames = DCinemaSmpte2010.MsToFramesMaxFrameRate(999, 24);
            Assert.Equal(23, frames);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalGlobal;
        }
    }

    // Regression for discussion #13869: a MacCaption-generated SMPTE reel spells the text
    // attributes the Interop way ("VPosition"/"VAlign"/"HAlign") instead of the SMPTE way
    // ("Vposition"/"Valign"/"Halign"). The lookup was case sensitive, so no vertical position
    // was ever seen, the line break between the two <Text> elements was lost, and both lines
    // ran together without even a space.
    private static string SmpteReel(string dcstYear, string vPositionAttribute, string vAlignAttribute, string timeCodeRate = "24") =>
        $"""
         <?xml version="1.0" encoding="UTF-8"?>
         <dcst:SubtitleReel xmlns:dcst="http://www.smpte-ra.org/schemas/428-7/{dcstYear}/DCST">
           <Id>urn:uuid:cbfae863-db46-464a-b421-51c873d16c50</Id>
           <ContentTitleText>Subtitle Content Title</ContentTitleText>
           <IssueDate>2026-08-13T19:56:28.770-04:00</IssueDate>
           <ReelNumber>Reel 1</ReelNumber>
           <Language>English</Language>
           <EditRate>{timeCodeRate} 1</EditRate>
           <dcst:TimeCodeRate>{timeCodeRate}</dcst:TimeCodeRate>
           <StartTime>00:00:08:01</StartTime>
           <SubtitleList>
             <Font Color="FFFFFFFF" Effect="shadow" EffectColor="FF000000" Size="42" Italic="no">
               <Subtitle SpotNumber="1" TimeIn="00:00:28:21" TimeOut="00:00:32:12">
                 <Text Direction="horizontal" {vAlignAttribute}="bottom" {vPositionAttribute}="85.00">- How do you see</Text>
                 <Text Direction="horizontal" {vAlignAttribute}="bottom" {vPositionAttribute}="79.29">empathy just on a whole,</Text>
               </Subtitle>
             </Font>
           </SubtitleList>
         </dcst:SubtitleReel>
         """;

    public static TheoryData<SubtitleFormat, string> SmpteFormats => new()
    {
        { new DCinemaSmpte2007(), "2007" },
        { new DCinemaSmpte2010(), "2010" },
        { new DCinemaSmpte2014(), "2014" },
    };

    [Theory]
    [MemberData(nameof(SmpteFormats))]
    public void Smpte_InteropCasedAttributes_KeepsTwoLines(SubtitleFormat format, string dcstYear)
    {
        var lines = SmpteReel(dcstYear, "VPosition", "VAlign").SplitToLines();
        Assert.True(format.IsMine(lines, null));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);

        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("- How do you see" + Environment.NewLine + "empathy just on a whole,", paragraph.Text);
    }

    [Theory]
    [MemberData(nameof(SmpteFormats))]
    public void Smpte_SchemaCasedAttributes_KeepsTwoLines(SubtitleFormat format, string dcstYear)
    {
        var lines = SmpteReel(dcstYear, "Vposition", "Valign").SplitToLines();

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);

        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("- How do you see" + Environment.NewLine + "empathy just on a whole,", paragraph.Text);
    }

    // TimeIn/TimeOut count ticks per second at TimeCodeRate, but loading always divided the
    // frame field by the hardcoded default of 24, so every non-24 reel got the wrong ms.
    [Theory]
    [MemberData(nameof(SmpteFormats))]
    public void Smpte_TimeCodeRate25_IsUsedForFrameToMs(SubtitleFormat format, string dcstYear)
    {
        var originalGlobal = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            var lines = SmpteReel(dcstYear, "Vposition", "Valign", "25").SplitToLines();

            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, lines, null);

            var paragraph = Assert.Single(subtitle.Paragraphs);

            // 21 frames at 25 fps is 840 ms (at 24 fps it would have been 875 ms)
            Assert.Equal(840, paragraph.StartTime.Milliseconds);
            Assert.Equal(480, paragraph.EndTime.Milliseconds);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalGlobal;
        }
    }

    // The mirror-image case: an Interop file spelling the attributes the SMPTE way.
    [Fact]
    public void Interop_SmpteCasedAttributes_KeepsTwoLines()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           <DCSubtitle Version="1.0">
                             <SubtitleID>4eb245b8-4d3a-4158-9516-95dd20e8322e</SubtitleID>
                             <MovieTitle>Unknown</MovieTitle>
                             <ReelNumber>1</ReelNumber>
                             <Language>English</Language>
                             <Font Id="Font1" Color="FFFFFFFF" Size="42" Italic="no">
                               <Subtitle SpotNumber="1" TimeIn="00:00:06:040" TimeOut="00:00:08:040" FadeUpTime="20" FadeDownTime="20">
                                 <Text Direction="horizontal" Valign="bottom" Vposition="85.00">- How do you see</Text>
                                 <Text Direction="horizontal" Valign="bottom" Vposition="79.29">empathy just on a whole,</Text>
                               </Subtitle>
                             </Font>
                           </DCSubtitle>
                           """;

        var lines = xml.SplitToLines();
        var format = new DCinemaInterop();
        Assert.True(format.IsMine(lines, null));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);

        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("- How do you see" + Environment.NewLine + "empathy just on a whole,", paragraph.Text);
    }
}
