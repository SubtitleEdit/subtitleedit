using Nikse.SubtitleEdit.Core.Common;
using System.Diagnostics;

namespace LibSETests.Common;

public class UnknownFormatImporterTest
{
    [Fact]
    public void AutoGuessImportParsesSubtitleWithNoLineBreaks()
    {
        var importer = new UnknownFormatImporter();
        var text = "1 00:00:01.502 --> 00:00:03.604 Hello there my good friend. " +
                   "2 00:00:04.000 --> 00:00:06.000 How are you doing today? " +
                   "3 00:00:07.000 --> 00:00:09.000 I am doing fine.";

        var subtitle = importer.AutoGuessImport([text], "test.txt");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1502, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3604, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        // The winning importer does not consume the "2"/"3" sequence numbers, so they
        // trail the preceding paragraph's text - long-standing behavior, asserted here
        // so the O(n²)→O(n) rewrite of the no-line-break importers is provably neutral.
        Assert.Equal("Hello there my good friend. 2", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you doing today? 3", subtitle.Paragraphs[1].Text);
        Assert.Equal("I am doing fine.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void AutoGuessImportParsesSubtitleWithNoLineBreaksWithExtraSpaces()
    {
        var importer = new UnknownFormatImporter();
        var text = "00: 00 : 01, 502 --> 00: 00 : 03, 604 Hello there my good friend. " +
                   "00: 00 : 04, 000 --> 00: 00 : 06, 000 How are you doing today? " +
                   "00: 00 : 07, 000 --> 00: 00 : 09, 000 I am doing fine.";

        var subtitle = importer.AutoGuessImport([text], "test.txt");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1502, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3604, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there my good friend.", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you doing today?", subtitle.Paragraphs[1].Text);
        Assert.Equal("I am doing fine.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void AutoGuessImportParsesXmlWithSecondsAttributes()
    {
        // TTML-like dialect with seconds-with-unit attributes and inline text
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<tt><body><div>",
            "<p begin=\"1.5s\" end=\"4.3s\">Hello there.</p>",
            "<p begin=\"5.5s\" end=\"8.3s\">How are <span>you</span>?</p>",
            "<p begin=\"9.5s\" end=\"12.3s\">Line one<br/>line two.</p>",
            "</div></body></tt>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(4300, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you?", subtitle.Paragraphs[1].Text);
        Assert.Equal("Line one" + Environment.NewLine + "line two.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void AutoGuessImportParsesXmlWithTimeCodeChildElementsAndNestedText()
    {
        // Matroska-chapters-like dialect: time codes and text in (nested) child elements
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<Chapters><EditionEntry>",
            "<ChapterAtom><ChapterUID>1</ChapterUID><ChapterTimeStart>00:00:01.500</ChapterTimeStart><ChapterDisplay><ChapterString>First chapter</ChapterString></ChapterDisplay></ChapterAtom>",
            "<ChapterAtom><ChapterUID>2</ChapterUID><ChapterTimeStart>00:00:05.500</ChapterTimeStart><ChapterDisplay><ChapterString>Second chapter</ChapterString></ChapterDisplay></ChapterAtom>",
            "</EditionEntry></Chapters>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal("First chapter", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second chapter", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void AutoGuessImportParsesXmlWithTimeChildAttributesAndTextSibling()
    {
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<titles>",
            "<title id=\"1\"><time start=\"00:00:01,500\" end=\"00:00:04,300\" /><text1>Hello there.</text1></title>",
            "<title id=\"2\"><time start=\"00:00:05,500\" end=\"00:00:08,300\" /><text1>How are you?</text1></title>",
            "</titles>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(4300, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void AutoGuessImportParsesXmlWithStartOnlySamplesAndEmptyEndMarkers()
    {
        // GPAC TTXT-like dialect: an empty sample closes the previous one
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<TextStream>",
            "<TextSample sampleTime=\"00:00:01.500\">Hello there.</TextSample>",
            "<TextSample sampleTime=\"00:00:04.300\" />",
            "<TextSample sampleTime=\"00:00:05.500\">How are you?</TextSample>",
            "<TextSample sampleTime=\"00:00:08.300\" />",
            "</TextStream>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(4300, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(5500, subtitle.Paragraphs[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(8300, subtitle.Paragraphs[1].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void AutoGuessImportParsesXmlWithTextInAttribute()
    {
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<Subtitle>",
            "<Clip start=\"1.5\" end=\"4.3\" text=\"Hello there.\" />",
            "<Clip start=\"5.5\" end=\"8.3\" text=\"How are you?\" />",
            "</Subtitle>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void AutoGuessImportIgnoresXmlWithoutTimeCodes()
    {
        var importer = new UnknownFormatImporter();
        var lines = new List<string>
        {
            "<configuration>",
            "<appSettings><add key=\"a\" value=\"b\" /><add key=\"c\" value=\"d\" /></appSettings>",
            "</configuration>",
        };

        var subtitle = importer.AutoGuessImport(lines, "test.xml");

        Assert.Empty(subtitle.Paragraphs);
    }

    [Fact]
    public void AutoGuessImportJsonIsNotHijackedByValuesMatchingTimeTagNames()
    {
        // "lineAlign":"start" must not be read as the start time of the paragraph
        var importer = new UnknownFormatImporter();
        var line = "{\"events\":[" +
                   "{\"startTime\":1.5,\"endTime\":4.3,\"positionAlign\":\"middle\",\"lineAlign\":\"start\",\"text\":\"Hello there.\"}," +
                   "{\"startTime\":5.5,\"endTime\":8.3,\"positionAlign\":\"middle\",\"lineAlign\":\"start\",\"text\":\"How are you?\"}," +
                   "{\"startTime\":9.5,\"endTime\":12.3,\"positionAlign\":\"middle\",\"lineAlign\":\"start\",\"text\":\"I am fine.\"}]}";

        var subtitle = importer.AutoGuessImport([line], "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(4300, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void AutoGuessImportJsonReadsTextFromNestedObject()
    {
        // time codes in the outer object, text in a nested one
        var importer = new UnknownFormatImporter();
        var line = "[" +
                   "{\"startTime\":1.5,\"endTime\":4.3,\"metadata\":{\"Text\":\"Hello there.\",\"Language\":\"en\"}}," +
                   "{\"startTime\":5.5,\"endTime\":8.3,\"metadata\":{\"Text\":\"How are you?\",\"Language\":\"en\"}}," +
                   "{\"startTime\":9.5,\"endTime\":12.3,\"metadata\":{\"Text\":\"I am fine.\",\"Language\":\"en\"}}]";

        var subtitle = importer.AutoGuessImport([line], "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you?", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void AutoGuessImportJsonReadsStartTimeOnlyFormat()
    {
        var importer = new UnknownFormatImporter();
        var line = "[" +
                   "{\"milliseconds\":1500,\"line\":\"Hello there.\"}," +
                   "{\"milliseconds\":5500,\"line\":\"How are you?\"}," +
                   "{\"milliseconds\":9500,\"line\":\"I am fine.\"}]";

        var subtitle = importer.AutoGuessImport([line], "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.True(subtitle.Paragraphs[0].EndTime.TotalMilliseconds > subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void AutoGuessImportJsonReadsTextArray()
    {
        var importer = new UnknownFormatImporter();
        var line = "[" +
                   "{\"text\": [ \"Hello there.\" ], \"index\":1,\"start\": 1500, \"end\": 4300 }," +
                   "{\"text\": [ \"How are\", \"you?\" ], \"index\":2,\"start\": 5500, \"end\": 8300 }," +
                   "{\"text\": [ \"I am fine.\" ], \"index\":3,\"start\": 9500, \"end\": 12300 }]";

        var subtitle = importer.AutoGuessImport([line], "test.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are" + Environment.NewLine + "you?", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void AutoGuessImportIsFastOnDigitHeavyBinaryInput()
    {
        // Regression for issue #12683: a raw PGS .sup decoded as text reached the
        // no-line-break importers, whose per-digit Substring(i) + greedy \d+ retry
        // at every position of a digit run made megabytes of digit-heavy binary
        // quadratic - the UI froze for hours. Linear behavior finishes in well
        // under a second; the generous bound only guards against the quadratic case
        // (2M digits was on the order of 10^12 operations before the fix).
        var importer = new UnknownFormatImporter();
        var text = new string('0', 2_000_000) + " --> ";

        var stopwatch = Stopwatch.StartNew();
        var subtitle = importer.AutoGuessImport([text], "test.sup");
        stopwatch.Stop();

        Assert.Empty(subtitle.Paragraphs);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20),
            $"AutoGuessImport took {stopwatch.Elapsed} on digit-heavy input - quadratic behavior is back?");
    }
}
