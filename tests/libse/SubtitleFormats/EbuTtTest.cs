using System.IO;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class EbuTtTest
{
    // Minimal headless stand-in for the UI's EBU save helper so the binary writer can run in a test.
    private sealed class TestEbuUiHelper : Ebu.IEbuUiHelper
    {
        public byte JustificationCode { get; set; }
        public void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle) { }
        public bool ShowDialogOk() => true;
    }

    /// <summary>
    /// A minimal EBU STL GSI header marked as teletext level 1 - what a subtitle loaded from a
    /// teletext STL carries, and what makes the EBU-TT writer trust MarginV rows and the
    /// box/double height settings.
    /// </summary>
    private static string TeletextStlHeader()
    {
        return ("437" + "STL25.01" + "1").PadRight(1024, ' ');
    }

    private static Subtitle RoundTrip(Subtitle subtitle, out string raw)
    {
        var format = new EbuTt();
        raw = subtitle.ToText(format);
        var result = new Subtitle();
        format.LoadSubtitle(result, raw.SplitToLines(), null);
        return result;
    }

    [Fact]
    public void BasicRoundTrip()
    {
        var input = "Hello world!" + Environment.NewLine + "Second line.";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 1000, 3520));

        var result = RoundTrip(sub, out _);

        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
        Assert.Equal(1000, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3520, result.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ItalicAndBoldRoundTrip()
    {
        var input = "This is an <i>italic</i> and <b>bold</b> word!";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        var result = RoundTrip(sub, out _);

        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void TeletextColorRoundTrip()
    {
        var input = "<font color=\"Yellow\">Yellow line</font>" + Environment.NewLine +
                    "<font color=\"Cyan\">cyan line</font>";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        var result = RoundTrip(sub, out var raw);

        Assert.Contains("tts:color=\"#ffff00\"", raw);
        Assert.Contains("tts:color=\"#00ffff\"", raw);
        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void CustomColorRoundTrip()
    {
        // EBU-TT is not capped at the eight teletext colours - a free colour must survive.
        var input = "<font color=\"#ff8800\">Orange!</font>";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        var result = RoundTrip(sub, out var raw);

        Assert.Contains("tts:color=\"#ff8800\"", raw);
        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void TeletextRowAndBoxRoundTrip()
    {
        var oldBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
        var oldDouble = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight;
        try
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = true;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = true;

            var sub = new Subtitle { Header = TeletextStlHeader() };
            sub.Paragraphs.Add(new Paragraph("Row twenty line", 0, 3000) { MarginV = "20" });

            var format = new EbuTt();
            var raw = sub.ToText(format);

            // The row becomes a region on the 24 row teletext grid, the boxed look a black span
            // background, double height the 1c 2c cell font size of Tech 3360.
            Assert.Contains("rowRegion20", raw);
            Assert.Contains("83.333%", raw);
            Assert.Contains("tts:backgroundColor=\"#000000\"", raw);
            Assert.Contains("tts:fontSize=\"1c 2c\"", raw);

            // Reading back seeds the same settings an STL load seeds and restores the row.
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = false;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = false;
            var result = new Subtitle();
            format.LoadSubtitle(result, raw.SplitToLines(), null);

            Assert.Single(result.Paragraphs);
            Assert.Equal("20", result.Paragraphs[0].MarginV);
            Assert.Equal("rowRegion20", result.Paragraphs[0].Region);
            Assert.Equal("Row twenty line", result.Paragraphs[0].Text);
            Assert.True(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox);
            Assert.True(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = oldBox;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = oldDouble;
        }
    }

    [Fact]
    public void OpenSubtitlingBoxTagsRoundTrip()
    {
        // Box tags in the text (the open subtitling boxing of an STL) survive per span without
        // touching the teletext settings.
        var oldBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
        try
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = false;
            var input = "<box>Boxed part</box> and plain part";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

            var result = RoundTrip(sub, out var raw);

            Assert.Contains("tts:backgroundColor=\"#000000\"", raw);
            Assert.Single(result.Paragraphs);
            Assert.Equal(input, result.Paragraphs[0].Text);
            Assert.False(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox);
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = oldBox;
        }
    }

    [Fact]
    public void TopAlignmentRoundTrip()
    {
        var input = "{\\an8}Top text";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 2000));

        var result = RoundTrip(sub, out var raw);

        Assert.Contains("region=\"top\"", raw);
        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void LeftJustificationRoundTrip()
    {
        var input = "{\\an1}Left text";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 2000));

        var result = RoundTrip(sub, out var raw);

        Assert.Contains("tts:textAlign=\"left\"", raw);
        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void OutputIsConformantShape()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hi & bye <font color=\"Red\">now</font>", 0, 2000));

        RoundTrip(sub, out var raw);

        // Load-bearing EBU-TT Part 1 bits: the version marker, the teletext cell grid, media
        // timebase, referential styling only (no tts: attributes in the body), spans.
        Assert.Contains("<ebuttm:documentEbuttVersion>v1.0</ebuttm:documentEbuttVersion>", raw);
        Assert.Contains("ttp:cellResolution=\"40 24\"", raw);
        Assert.Contains("ttp:timeBase=\"media\"", raw);
        Assert.Contains("<span style=\"textRed\">now</span>", raw);
        Assert.DoesNotContain("urn:ebu:tt:distribution", raw);
        Assert.DoesNotContain("xmlns=\"\"", raw);

        var body = raw.Substring(raw.IndexOf("<body", StringComparison.Ordinal));
        Assert.DoesNotContain("tts:", body);
    }

    [Fact]
    public void DetectsAsEbuTtNotEbuTtDOrTimedText()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Detection test", 0, 2000));
        RoundTrip(sub, out var raw);

        var lines = raw.SplitToLines();
        SubtitleFormat? detected = null;
        foreach (var candidate in SubtitleFormat.AllSubtitleFormats)
        {
            if (candidate.IsMine(lines, null))
            {
                detected = candidate;
                break;
            }
        }

        Assert.NotNull(detected);
        Assert.Equal(new EbuTt().Name, detected.Name);
    }

    [Fact]
    public void EbuTtDOutputStillDetectsAsEbuTtD()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Distribution profile", 0, 2000));
        var raw = sub.ToText(new EbuTtD());

        var lines = raw.SplitToLines();
        SubtitleFormat? detected = null;
        foreach (var candidate in SubtitleFormat.AllSubtitleFormats)
        {
            if (candidate.IsMine(lines, null))
            {
                detected = candidate;
                break;
            }
        }

        Assert.NotNull(detected);
        Assert.Equal(new EbuTtD().Name, detected.Name);
    }

    [Fact]
    public void LoadsSmpteTimebaseSample()
    {
        // A Part 1 document from an STL conversion tool: smpte timebase, prefixed elements,
        // referential styles with colour and background.
        var raw = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt:tt xmlns:tt="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling"
                   xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xmlns:ebuttm="urn:ebu:tt:metadata"
                   xmlns:ebutts="urn:ebu:tt:style" ttp:timeBase="smpte" ttp:frameRate="25"
                   ttp:cellResolution="40 24" xml:lang="de">
              <tt:head>
                <tt:metadata>
                  <ebuttm:documentMetadata>
                    <ebuttm:documentEbuttVersion>v1.0</ebuttm:documentEbuttVersion>
                  </ebuttm:documentMetadata>
                </tt:metadata>
                <tt:styling>
                  <tt:style xml:id="defaultStyle" tts:fontFamily="monospaceSansSerif" tts:textAlign="center"/>
                  <tt:style xml:id="WhiteOnBlack" tts:color="#FFFFFF" tts:backgroundColor="#000000"/>
                  <tt:style xml:id="YellowOnBlack" tts:color="#FFFF00" tts:backgroundColor="#000000"/>
                </tt:styling>
                <tt:layout>
                  <tt:region xml:id="r20" tts:origin="0% 83.33%" tts:extent="100% 16.67%" tts:displayAlign="before"/>
                </tt:layout>
              </tt:head>
              <tt:body style="defaultStyle">
                <tt:div>
                  <tt:p xml:id="sub1" begin="00:00:05:12" end="00:00:07:00" region="r20">
                    <tt:span style="YellowOnBlack">Gelbe Zeile</tt:span>
                  </tt:p>
                </tt:div>
              </tt:body>
            </tt:tt>
            """;

        var format = new EbuTt();
        Assert.True(format.IsMine(raw.SplitToLines(), "sample.xml"));

        var sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        Assert.Single(sub.Paragraphs);
        Assert.Equal(5480, sub.Paragraphs[0].StartTime.TotalMilliseconds); // 12 frames at 25 fps
        Assert.Equal(7000, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("20", sub.Paragraphs[0].MarginV);
        Assert.Equal("<font color=\"Yellow\">Gelbe Zeile</font>", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void StlToEbuTtKeepsBoxTagsAndRows()
    {
        // Converting EBU STL -> EBU-TT must not strip what EBU-TT can carry.
        var sub = new Subtitle { Header = TeletextStlHeader() };
        sub.Paragraphs.Add(new Paragraph("<box>Boxed</box>", 0, 2000) { MarginV = "18" });

        new Ebu().RemoveNativeFormatting(sub, new EbuTt());

        Assert.Equal("<box>Boxed</box>", sub.Paragraphs[0].Text);
        Assert.Equal("18", sub.Paragraphs[0].MarginV);

        // ...while converting to a non teletext format still strips both.
        new Ebu().RemoveNativeFormatting(sub, new SubRip());
        Assert.Equal("Boxed", sub.Paragraphs[0].Text);
        Assert.Null(sub.Paragraphs[0].MarginV);
    }

    [Fact]
    public void StlMetadataTravelsToEbuTtAndBack()
    {
        // A GSI header with real metadata - the same shape Ebu.LoadSubtitle stores.
        var gsi = new Ebu.EbuGeneralSubtitleInformation
        {
            DisplayStandardCode = "1",
            OriginalProgrammeTitle = "Die Sendung".PadRight(32),
            OriginalEpisodeTitle = "Folge 7".PadRight(32),
            TranslatedProgrammeTitle = "The Programme".PadRight(32),
            TranslatorsName = "R. Weiss".PadRight(32),
            SubtitleListReferenceCode = "REF0001".PadRight(16),
            CreationDate = "260830",
            RevisionNumber = "02",
            TimeCodeStartOfProgramme = "10000000",
            CountryOfOrigin = "CHE",
            Publisher = "SRF".PadRight(32),
        };

        var sub = new Subtitle { Header = gsi.ToString() };
        sub.Paragraphs.Add(new Paragraph("Hallo", 0, 2000));

        var format = new EbuTt();
        var raw = sub.ToText(format);

        Assert.Contains("<ebuttm:documentOriginalProgrammeTitle>Die Sendung</ebuttm:documentOriginalProgrammeTitle>", raw);
        Assert.Contains("<ebuttm:documentOriginalEpisodeTitle>Folge 7</ebuttm:documentOriginalEpisodeTitle>", raw);
        Assert.Contains("<ebuttm:documentTranslatedProgrammeTitle>The Programme</ebuttm:documentTranslatedProgrammeTitle>", raw);
        Assert.Contains("<ebuttm:documentTranslatorsName>R. Weiss</ebuttm:documentTranslatorsName>", raw);
        Assert.Contains("<ebuttm:documentSubtitleListReferenceCode>REF0001</ebuttm:documentSubtitleListReferenceCode>", raw);
        Assert.Contains("<ebuttm:documentCreationDate>2026-08-30</ebuttm:documentCreationDate>", raw);
        Assert.Contains("<ebuttm:documentRevisionNumber>2</ebuttm:documentRevisionNumber>", raw);
        Assert.Contains("<ebuttm:documentStartOfProgramme>10:00:00:00</ebuttm:documentStartOfProgramme>", raw);
        Assert.Contains("<ebuttm:documentCountryOfOrigin>CHE</ebuttm:documentCountryOfOrigin>", raw);
        Assert.Contains("<ebuttm:documentPublisher>SRF</ebuttm:documentPublisher>", raw);

        // The metadata carries forward when re-saving an EBU-TT document...
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, raw.SplitToLines(), null);
        var resaved = loaded.ToText(format);
        Assert.Contains("<ebuttm:documentOriginalProgrammeTitle>Die Sendung</ebuttm:documentOriginalProgrammeTitle>", resaved);
        Assert.Contains("<ebuttm:documentStartOfProgramme>10:00:00:00</ebuttm:documentStartOfProgramme>", resaved);

        // ...and fills the GSI fields again on the way back to STL.
        Ebu.EbuUiHelper = new TestEbuUiHelper();
        using var ms = new MemoryStream();
        var ok = new Ebu().Save("test.stl", ms, loaded, batchMode: true, null);
        Assert.True(ok);
        var writtenGsi = Ebu.ReadHeader(ms.ToArray());
        Assert.Equal("Die Sendung", writtenGsi.OriginalProgrammeTitle.Trim());
        Assert.Equal("Folge 7", writtenGsi.OriginalEpisodeTitle.Trim());
        Assert.Equal("R. Weiss", writtenGsi.TranslatorsName.Trim());
        Assert.Equal("REF0001", writtenGsi.SubtitleListReferenceCode.Trim());
        Assert.Equal("02", writtenGsi.RevisionNumber);
        Assert.Equal("10000000", writtenGsi.TimeCodeStartOfProgramme);
        Assert.Equal("CHE", writtenGsi.CountryOfOrigin);
        Assert.Equal("SRF", writtenGsi.Publisher.Trim());
    }

    [Fact]
    public void DvbTeletextPageAndLanguageTravelThroughEbuTt()
    {
        var sub = new Subtitle { Header = DvbTeletext.CreateHeader(150, "ger") };
        sub.Paragraphs.Add(new Paragraph("Hallo", 0, 2000));

        var format = new EbuTt();
        var raw = sub.ToText(format);

        Assert.Contains("urn:subtitleedit:metadata", raw);
        Assert.Contains("page=\"150\"", raw);
        Assert.Contains("language=\"ger\"", raw);

        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, raw.SplitToLines(), null);
        Assert.True(EbuTt.TryGetTeletextPageAndLanguage(loaded.Header, out var page, out var language));
        Assert.Equal(150, page);
        Assert.Equal("ger", language);

        // The page also survives a re-save of the EBU-TT document itself.
        var resaved = loaded.ToText(format);
        Assert.Contains("page=\"150\"", resaved);

        // Non EBU-TT headers say no.
        Assert.False(EbuTt.TryGetTeletextPageAndLanguage(DvbTeletext.CreateHeader(150, "ger"), out _, out _));
        Assert.False(EbuTt.TryGetTeletextPageAndLanguage(null, out _, out _));
    }

    [Fact]
    public void MetadataAbsentStaysAbsent()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Plain", 0, 2000));

        var raw = sub.ToText(new EbuTt());

        Assert.DoesNotContain("documentOriginalProgrammeTitle", raw);
        Assert.DoesNotContain("urn:subtitleedit:metadata", raw);
        Assert.Contains("<ebuttm:documentOriginatingSystem>Subtitle Edit</ebuttm:documentOriginatingSystem>", raw);
    }

    [Fact]
    public void EbuTtToOtherFormatStripsTeletextExtras()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("<box>Boxed</box>", 0, 2000) { MarginV = "18", Region = "rowRegion18" });

        new EbuTt().RemoveNativeFormatting(sub, new Ebu());
        Assert.Equal("<box>Boxed</box>", sub.Paragraphs[0].Text);
        Assert.Equal("18", sub.Paragraphs[0].MarginV);

        new EbuTt().RemoveNativeFormatting(sub, new SubRip());
        Assert.Equal("Boxed", sub.Paragraphs[0].Text);
        Assert.Null(sub.Paragraphs[0].MarginV);
        Assert.Null(sub.Paragraphs[0].Region);
    }
}
