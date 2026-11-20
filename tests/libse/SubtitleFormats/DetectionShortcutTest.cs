using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Formats whose readers now reject a file before the expensive part (joining all lines, a full
/// json/xml parse) - they must still read their own output, and still reject what they rejected.
/// </summary>
[Collection("NonParallelTests")]
public class DetectionShortcutTest
{
    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 4000, 6000));
        subtitle.Paragraphs.Add(new Paragraph("Third line", 7000, 9000));
        return subtitle;
    }

    public static TheoryData<string> Formats => new()
    {
        nameof(JsonType14),
        nameof(GooglePlayJson),
        nameof(JsonAeneas),
        nameof(TranscriberXml),
        nameof(Rtf1),
        nameof(Rtf2),
        nameof(UnknownSubtitle101),
        nameof(UnknownSubtitle41),
        nameof(EZTSubtitlesProject),
        nameof(Spruce),
        nameof(InqScribe),
    };

    private static SubtitleFormat Create(string name)
    {
        return SubtitleFormat.AllSubtitleFormats.Concat(SubtitleFormat.GetTextOtherFormats())
            .First(f => f.GetType().Name == name).GetType()
            .GetConstructor(Type.EmptyTypes)!
            .Invoke(null) as SubtitleFormat ?? throw new InvalidOperationException(name);
    }

    [Theory]
    [MemberData(nameof(Formats))]
    public void ReadsItsOwnOutput(string name)
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var format = Create(name);
            var lines = format.ToText(MakeSubtitle(), "test").SplitToLines();

            Assert.True(format.IsMine(lines, "test" + format.Extension));
            var loaded = new Subtitle();
            format.LoadSubtitle(loaded, lines, "test" + format.Extension);
            Assert.Equal(3, loaded.Paragraphs.Count);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Theory]
    [MemberData(nameof(Formats))]
    public void RejectsOtherJsonAndXml(string name)
    {
        var format = Create(name);
        var json = "{\n  \"title\": \"x\",\n  \"lines\": [\"a\", \"b\"],\n  \"data\": { \"text\": \"y\" }\n}".SplitToLines();
        var xml = "<?xml version=\"1.0\"?>\n<Language>\n  <TranslatedBy>Someone</TranslatedBy>\n  <Turn>no</Turn>\n</Language>".SplitToLines();

        Assert.False(format.IsMine(json, "test.json"));
        Assert.False(format.IsMine(xml, "test.xml"));
    }

    [Fact]
    public void RtfHeaderAfterBlankLinesIsStillRtf()
    {
        var format = new Rtf1();
        var lines = format.ToText(MakeSubtitle(), "test").SplitToLines();
        lines.InsertRange(0, new[] { string.Empty, "   " });

        Assert.True(format.IsMine(lines, "test.rtf"));
    }
}
