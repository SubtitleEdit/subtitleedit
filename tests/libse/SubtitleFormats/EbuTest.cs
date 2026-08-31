using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;

namespace LibSETests.SubtitleFormats;

public class EbuTest
{
    // Minimal headless stand-in for the UI's EBU save helper so the binary writer can run in a test.
    private sealed class TestEbuUiHelper : Ebu.IEbuUiHelper
    {
        public byte JustificationCode { get; set; }
        public void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle) { }
        public bool ShowDialogOk() => true;
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 4000, 6000));
        return subtitle;
    }

    // The first-manual-save prompt and the vertical-position handling both hinge on this check:
    // it must accept the header the options dialog stores on the subtitle (a bare
    // EbuGeneralSubtitleInformation.ToString()) and reject anything else.
    [Fact]
    public void IsStlHeader_AcceptsDialogStoredHeader_RejectsOtherHeaders()
    {
        Assert.False(Ebu.IsStlHeader(null));
        Assert.False(Ebu.IsStlHeader(string.Empty));
        Assert.False(Ebu.IsStlHeader("WEBVTT"));
        Assert.False(Ebu.IsStlHeader(new string(' ', 1024)));

        var stored = new Ebu.EbuGeneralSubtitleInformation().ToString();
        Assert.True(Ebu.IsStlHeader(stored));

        // Every disk format code the save options dialog offers must be recognized - STL23 was not,
        // so picking it made the save discard the header the dialog had just stored.
        foreach (var diskFormatCode in new[] { "STL23.01", "STL24.01", "STL25.01", "STL29.01", "STL30.01" })
        {
            var header = new Ebu.EbuGeneralSubtitleInformation { DiskFormatCode = diskFormatCode }.ToString();
            Assert.True(Ebu.IsStlHeader(header), diskFormatCode + " is not recognized as an STL header");
        }

        // A 1024-character header of some other format that happens to mention STL25 is not one.
        Assert.False(Ebu.IsStlHeader("STL25".PadRight(1024)));
    }

    // Regression for #11910: EBU STL Save produced a 14-byte invalid file ("Not supported!")
    // because the binary format went through the text save path. The binary writer must emit a real
    // EBU file (1024-byte GSI header + TTI blocks) that reads back.
    [Fact]
    public void EbuStl_BinarySave_ProducesValidFileNotFourteenByteStub()
    {
        Ebu.EbuUiHelper = new TestEbuUiHelper();
        var subtitle = MakeSubtitle();

        using var ms = new MemoryStream();
        var ok = ((IBinaryPersistableSubtitle)new Ebu()).Save("test.stl", ms, subtitle, batchMode: true);
        var bytes = ms.ToArray();

        Assert.True(ok);
        Assert.True(bytes.Length >= 1024, $"EBU file is only {bytes.Length} bytes (regression #11910)");
    }

    [Fact]
    public void EbuStl_BinarySave_RoundTripsParagraphs()
    {
        Ebu.EbuUiHelper = new TestEbuUiHelper();
        var subtitle = MakeSubtitle();

        using var ms = new MemoryStream();
        ((IBinaryPersistableSubtitle)new Ebu()).Save("test.stl", ms, subtitle, batchMode: true);

        var loaded = new Subtitle();
        new Ebu().LoadSubtitle(loaded, ms.ToArray());

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Contains("Hello world", loaded.Paragraphs[0].Text);
        Assert.Contains("Second line", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void EbuStl_Load_ExposesHeaderFrameRateOnTheParsingInstance()
    {
        // The SE5 main view reads the frame rate off the parsing instance's header after open, to
        // show the file's own frame numbers in the forced HH:MM:SS:FF display (#14076).
        Ebu.EbuUiHelper = new TestEbuUiHelper();
        using var ms = new MemoryStream();
        ((IBinaryPersistableSubtitle)new Ebu()).Save("test.stl", ms, MakeSubtitle(), batchMode: true);

        var ebu = new Ebu();
        ebu.LoadSubtitle(new Subtitle(), ms.ToArray());

        Assert.NotNull(ebu.Header);
        Assert.StartsWith("STL25", ebu.Header.DiskFormatCode);
        Assert.Equal(25.0, ebu.Header.FrameRate);
    }

    [Fact]
    public void EbuStl_ToText_IsNotUsableForSaving()
    {
        // Guards the root cause: ToText is a stub for this binary format, so the save path must use
        // the IBinaryPersistableSubtitle writer instead (#11910).
        Assert.IsAssignableFrom<IBinaryPersistableSubtitle>(new Ebu());
        Assert.True(new Ebu().ToText(MakeSubtitle(), string.Empty).Length < 20);
    }

    // ReadHeader used to skip GSI offsets 264-372, so publisher/editor metadata and the disk
    // fields were silently reset to the defaults on every load-save round trip.
    [Fact]
    public void EbuStl_ReadHeader_ReadsPublisherEditorAndDiskFields()
    {
        var header = new Ebu.EbuGeneralSubtitleInformation
        {
            TimeCodeFirstInCue = "10000000",
            TotalNumberOfDisks = "3",
            DiskSequenceNumber = "2",
            Publisher = "Acme Broadcasting".PadRight(32),
            EditorsName = "Jane Editor".PadRight(32),
            EditorsContactDetails = "jane@example.com".PadRight(32),
        };
        var buffer = Ebu.GetEncoding(header.CodePageNumber).GetBytes(header.ToString());
        Assert.Equal(1024, buffer.Length);

        var loaded = Ebu.ReadHeader(buffer);

        Assert.Equal("10000000", loaded.TimeCodeFirstInCue);
        Assert.Equal("3", loaded.TotalNumberOfDisks);
        Assert.Equal("2", loaded.DiskSequenceNumber);
        Assert.Equal("Acme Broadcasting", loaded.Publisher.TrimEnd());
        Assert.Equal("Jane Editor", loaded.EditorsName.TrimEnd());
        Assert.Equal("jane@example.com", loaded.EditorsContactDetails.TrimEnd());
    }

    // Builds a minimal teletext STL: a 1024-byte GSI header plus one TTI block whose text
    // field starts with the given bytes (rest is 8Fh padding).
    private static byte[] MakeTeletextStl(params byte[] textFieldBytes)
    {
        return MakeStl("1", textFieldBytes);
    }

    private static byte[] MakeStl(string displayStandardCode, params byte[] textFieldBytes)
    {
        var header = new Ebu.EbuGeneralSubtitleInformation { DisplayStandardCode = displayStandardCode };
        var headerBytes = Ebu.GetEncoding(header.CodePageNumber).GetBytes(header.ToString());

        var tti = new byte[128];
        tti[3] = 0xff; // extension block number: last block in cue
        tti[13] = 20; // vertical position: bottom, so no {\an} prefix is added
        for (var i = 16; i < tti.Length; i++)
        {
            tti[i] = 0x8f;
        }
        textFieldBytes.CopyTo(tti, 16);

        var buffer = new byte[headerBytes.Length + tti.Length];
        headerBytes.CopyTo(buffer, 0);
        tti.CopyTo(buffer, headerBytes.Length);
        return buffer;
    }

    // Whether a teletext STL uses boxes/double height must come from the loaded file, not from
    // whatever the previous export left in the global settings - otherwise a plain load-save
    // round trip of a boxless file adds boxes that were never there.
    [Fact]
    public void EbuStl_Load_SeedsBoxAndDoubleHeightFromFile()
    {
        var withCodes = MakeTeletextStl(0x0d, 0x0b, 0x0b, (byte)'H', (byte)'i', 0x0a, 0x0a);
        var withoutCodes = MakeTeletextStl((byte)'H', (byte)'i');

        Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = false;
        Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = false;
        new Ebu().LoadSubtitle(new Subtitle(), withCodes);
        Assert.True(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox);
        Assert.True(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight);

        new Ebu().LoadSubtitle(new Subtitle(), withoutCodes);
        Assert.False(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox);
        Assert.False(Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight);
    }

    // Adobe Premiere exports teletext control codes (colors, boxes) but stamps the header with
    // "open subtitling" (DSC=0), where 00h-1Fh is not defined - so reading it strictly dropped
    // every color and the user's colored subtitles imported as plain white (reported by email
    // against 5.2.0-beta25).
    [Fact]
    public void EbuStl_Load_ReadsTeletextColorsInOpenSubtitlingFile()
    {
        var buffer = MakeStl("0", 0x0b, 0x0b, 0x06, (byte)'Z', (byte)'Y', (byte)'A', (byte)'N', 0x0a, 0x0a);

        var subtitle = new Subtitle();
        new Ebu().LoadSubtitle(subtitle, buffer);

        Assert.Equal("<font color=\"Cyan\">ZYAN</font>", subtitle.Paragraphs[0].Text);
    }

    // ...but an open subtitling file without any color code must stay untouched: its 80h-85h
    // italic/underline codes are the ones that count, and no font tag may appear out of nowhere.
    [Fact]
    public void EbuStl_Load_OpenSubtitlingWithoutColorsIsUnchanged()
    {
        var buffer = MakeStl("0", 0x80, (byte)'H', (byte)'i', 0x81);

        var subtitle = new Subtitle();
        new Ebu().LoadSubtitle(subtitle, buffer);

        Assert.Equal("<i>Hi</i>", subtitle.Paragraphs[0].Text);
    }

    // The teletext color writer assumed the font tag's color value ends with a double quote and
    // took a Substring up to it. For an unquoted value ("<font color=#ffff00>" is common in
    // SubRip files) with any double quote later in the line - a quotation in the dialogue - that
    // Substring length came out as -13 and the whole save crashed, so no STL file was written at
    // all (reported by email against 5.2.0-beta24 as "The value -13 must be positive").
    [Theory]
    [InlineData("<font color=#ffff00>He said \"hello\" to me</font>")]
    [InlineData("<font color='#ffff00'>He said \"hello\" to me</font>")]
    [InlineData("<font color=\"#ffff00\">He said \"hello\" to me</font>")]
    [InlineData("<font color=yellow size=\"12\">He said hello</font>")]
    public void TeletextSave_FontColorValueQuotingVariants_AllWriteTheColor(string text)
    {
        Ebu.EbuUiHelper = new TestEbuUiHelper();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));

        var header = new Ebu.EbuGeneralSubtitleInformation { DisplayStandardCode = "1" };
        using var ms = new MemoryStream();
        var ok = new Ebu().Save("test.stl", ms, subtitle, batchMode: true, header);
        var bytes = ms.ToArray();

        Assert.True(ok);
        Assert.True(bytes.Length >= 1024 + 128, "no TTI block was written");

        // Yellow's teletext color code must sit in the TTI text field (offset 16..127).
        var textField = new byte[112];
        Array.Copy(bytes, 1024 + 16, textField, 0, 112);
        Assert.Contains((byte)0x03, textField);
    }

    // An STL file carries eight teletext colors, so the writer snaps anything else to the nearest
    // one. The UI asks the same question before it writes a color tag, so what the grid and the
    // video preview show is what the file will get - these are the answers it relies on.
    [Theory]
    [InlineData("#FF0000", "Red")]
    [InlineData("ff0000", "Red")]
    [InlineData("Red", "Red")]
    [InlineData("#FFA500", "Yellow")]  // orange
    [InlineData("#FFC0CB", "White")]   // pink
    [InlineData("#003300", "Black")]   // very dark green
    [InlineData("#00FFFF", "Cyan")]
    public void GetNearestColorName_SnapsToTheEightTeletextColors(string color, string expected)
    {
        Assert.Equal(expected, Ebu.GetNearestColorName(color));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a color")]
    [InlineData("#12345")]
    public void GetNearestColorName_IsNullWhenTheValueIsNotAColor(string color)
    {
        Assert.Null(Ebu.GetNearestColorName(color));
    }

    // Switching the format in the toolbar (or converting in batch convert) leaves the STL specific
    // bits behind: the box tags used to show up as text in the video preview and in the saved file,
    // and a teletext row in MarginV counts as an ASSA pixel margin.
    [Fact]
    public void RemoveNativeFormatting_DropsBoxTagsAndTeletextRows()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<box>Hello world</box>", 1000, 3000) { MarginV = "20" });
        subtitle.Paragraphs.Add(new Paragraph("<i>Second line</i>", 4000, 6000) { MarginV = "18" });

        new Ebu().RemoveNativeFormatting(subtitle, new SubRip());

        Assert.Equal("Hello world", subtitle.Paragraphs[0].Text);
        Assert.Equal("<i>Second line</i>", subtitle.Paragraphs[1].Text);
        Assert.All(subtitle.Paragraphs, p => Assert.Null(p.MarginV));
    }
}
