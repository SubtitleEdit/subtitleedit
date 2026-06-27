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
    public void EbuStl_ToText_IsNotUsableForSaving()
    {
        // Guards the root cause: ToText is a stub for this binary format, so the save path must use
        // the IBinaryPersistableSubtitle writer instead (#11910).
        Assert.IsAssignableFrom<IBinaryPersistableSubtitle>(new Ebu());
        Assert.True(new Ebu().ToText(MakeSubtitle(), string.Empty).Length < 20);
    }
}
