using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Text;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// These formats got a cheap check that rejects files they could never load before the full
/// parse (opening a large file that is no subtitle asks every format). A file each one wrote
/// itself must still be recognized and read back in full, and plain prose must not be.
/// </summary>
public class NonSubtitlePrecheckRoundTripTest
{
    public static TheoryData<string> Formats() => new TheoryData<string>
    {
        nameof(CsvNuendo), nameof(CsvDaVinci), nameof(MacCaption10), nameof(Sami), nameof(SamiModern),
        nameof(SamiYouTube), nameof(SamiAvDicPlayer), nameof(SubStationAlpha), nameof(AdvancedSubStationAlpha),
        nameof(FinalCutProXml), nameof(FinalCutProTest2Xml), nameof(FinalCutProTestXml), nameof(FinalCutProXCM),
        nameof(FinalCutProXmlGap), nameof(FinalCutProXXml), nameof(FinalCutProXmlName), nameof(TimedTextNoNs),
    };

    private static SubtitleFormat Create(string name) =>
        (SubtitleFormat)System.Activator.CreateInstance(typeof(SubtitleFormat).Assembly.GetType("Nikse.SubtitleEdit.Core.SubtitleFormats." + name));

    [Theory]
    [MemberData(nameof(Formats))]
    public void OwnOutputIsStillRecognizedAndReadBack(string formatName)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("How are you?", 4000, 6000));
        subtitle.Paragraphs.Add(new Paragraph("Fine, thanks.", 7000, 9000));

        var format = Create(formatName);
        var lines = format.ToText(subtitle, "title").SplitToLines();

        Assert.True(format.IsMine(lines, "test" + format.Extension));
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, lines, "test" + format.Extension);
        Assert.Equal(3, loaded.Paragraphs.Count);
    }

    [Theory]
    [MemberData(nameof(Formats))]
    public void ProseIsNotRecognized(string formatName)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 200; i++)
        {
            sb.AppendLine("This is a line of plain text, number " + i + ", with a comma or two.");
        }

        Assert.False(Create(formatName).IsMine(sb.ToString().SplitToLines(), "notes.txt"));
    }

    [Fact]
    public void JsonType9ObjectsWithoutEndAreErrorsNotParagraphs()
    {
        var lines = new List<string>
        {
            "[",
            "{\"start\": \"00:00:01.000\", \"end\": \"00:00:02.000\", \"text\": [\"One\"]},",
            "{\"start\": \"00:00:03.000\", \"text\": [\"No end\"]},",
            "{\"start\": \"00:00:05.000\", \"end\": \"00:00:06.000\", \"text\": [\"Three\"]}",
            "]",
        };

        var format = new JsonType9();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "test.json");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1, format.ErrorCount);
    }
}
