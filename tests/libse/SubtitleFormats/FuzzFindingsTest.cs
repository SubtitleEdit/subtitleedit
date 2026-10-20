using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for crashes and hangs found by mutation fuzzing the libse parsers (2026-10-20).
/// </summary>
public class FuzzFindingsTest
{
    private const string NetflixBareAmpersand =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xml:lang=\"en\">\n" +
        "  <head><metadata><ttm:title>Netflix Subtitle</ttm:title></metadata></head>\n" +
        "  <body><div>\n" +
        "    <p begin=\"00:00:01.000\" end=\"00:00:03.500\">Tom & Jerry</p>\n" +
        "  </div></body>\n" +
        "</tt>";

    private const string SmpteTt2052BareAmpersand =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:smpte=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt\" xmlns:m608=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt#cea608\" xml:lang=\"en\">\n" +
        "  <body><div>\n" +
        "    <p begin=\"00:00:01.000\" end=\"00:00:03.500\">Tom & Jerry</p>\n" +
        "  </div></body>\n" +
        "</tt>";

    [Theory]
    [InlineData(NetflixBareAmpersand, ".dfxp", NetflixTimedText.NameOfFormat)]
    [InlineData(SmpteTt2052BareAmpersand, ".xml", SmpteTt2052.NameOfFormat)]
    public void TimedText_RepairedXml_IsDetectedWithoutStackOverflow(string xml, string extension, string expectedFormat)
    {
        // TimedText10.IsMine left repaired (FixBadXml) files to Netflix/SMPTE-TT 2052 by calling
        // their IsMine, which called TimedText10.IsMine again - an endless recursion that killed
        // the app with a stack overflow on a file with a bare '&'.
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        File.WriteAllText(fileName, xml);
        try
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out _, null);

            Assert.NotNull(format);
            Assert.Equal(expectedFormat, format.Name);
            Assert.Single(subtitle.Paragraphs);
            Assert.Equal("Tom & Jerry", subtitle.Paragraphs[0].Text);
            Assert.False(new TimedText10().IsMine(xml.SplitToLines(), fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
