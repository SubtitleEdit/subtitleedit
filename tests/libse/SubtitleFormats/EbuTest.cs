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
        var header = new Ebu.EbuGeneralSubtitleInformation { DisplayStandardCode = "1" };
        var headerBytes = Ebu.GetEncoding(header.CodePageNumber).GetBytes(header.ToString());

        var tti = new byte[128];
        tti[3] = 0xff; // extension block number: last block in cue
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
}
