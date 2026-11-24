using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
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

    [Fact]
    public void TimedText_RepairedXml_StripsInvalidControlCharacters()
    {
        const string xml = "<?xml version=\"1.0\"?>" +
            "<tt xmlns=\"http://www.w3.org/ns/ttml\"><body><div>" +
            "<p begin=\"00:00:01.000\" end=\"00:00:03.\u001C000\">Tom & Jerry</p>" +
            "</div></body></tt>";
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ttml");
        File.WriteAllText(fileName, xml);
        try
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out _, null);

            Assert.IsType<TimedText10>(format);
            var paragraph = Assert.Single(subtitle.Paragraphs);
            Assert.Equal("Tom & Jerry", paragraph.Text);
            Assert.Equal(1000, paragraph.StartTime.TotalMilliseconds);
            Assert.Equal(3000, paragraph.EndTime.TotalMilliseconds);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CsvNuendo_SkipsContinuationOfMalformedRow(bool includePreviousCue)
    {
        const string header = "\"Character\",\"Timecode In\",\"Timecode Out\",\"Dialogue\"\n";
        const string previousCue = ",00:00:00:00,00:00:01:00,\"Previous cue\"\n";
        const string malformedCue = ",00:00:01:00,00:00:03:05,\"Hello\"00:04:00,00:00:06:00,\"Second line\nThird line\"\n";
        const string validCue = ",00:00:07:00,00:00:10:00,\"Keep me\nAnd this line\"";
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        File.WriteAllText(fileName, header + (includePreviousCue ? previousCue : string.Empty) + malformedCue + validCue);
        try
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out _, null);

            Assert.IsType<CsvNuendo>(format);
            var expected = new List<string>();
            if (includePreviousCue)
            {
                expected.Add("Previous cue");
            }

            expected.Add("Keep me" + Environment.NewLine + "And this line");
            Assert.Equal(expected, subtitle.Paragraphs.Select(p => p.Text));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SubRip_LoadsValidCuesAroundMalformedNumberedCue(bool malformedFirst)
    {
        const string malformed = "44\nNaN --> NaN\n123\n\n";
        const string firstCue = "1\n00:00:01,234 --> 00:00:03,456\nFirst\n\n";
        const string lastCue = "2\n00:00:04,567 --> 00:00:06,789\nLast\n";
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".srt");
        File.WriteAllText(fileName, malformedFirst ? malformed + firstCue + lastCue : firstCue + malformed + lastCue);
        try
        {
            var subtitle = new Subtitle();
            Assert.IsType<SubRip>(subtitle.LoadSubtitle(fileName, out _, null));
            Assert.Equal(new[] { "First", "Last" }, subtitle.Paragraphs.Select(p => p.Text));
            Assert.Equal(1234, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(6789, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("999999999:59:59,999")]
    [InlineData("1e309")]
    public void GenericCsv_InvalidTimeDoesNotAffectValidRows(string invalidTime)
    {
        var lines = new List<string>
        {
            "start,end,text",
            $"\"{invalidTime}\",\"{invalidTime}\",Bad row",
            "1.234,3.456,First",
            "4.567,6.789,Second",
            "7.890,9.999,Last",
        };

        var subtitle = new UnknownFormatImporter().AutoGuessImport(lines, "test.csv");

        Assert.Equal(new[] { "First", "Second", "Last" }, subtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 1234, 4567, 7890 }, subtitle.Paragraphs.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new double[] { 3456, 6789, 9999 }, subtitle.Paragraphs.Select(p => p.EndTime.TotalMilliseconds));
    }

    private static async Task<T> RunWithTimeout<T>(Func<T> action)
    {
        var task = Task.Run(action, TestContext.Current.CancellationToken);
        var finished = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(20), TestContext.Current.CancellationToken));
        Assert.True(finished == task, "did not return within 20 seconds");
        return await task;
    }

    [Fact]
    public async Task JsonReadArray_NoClosingBracket_Returns()
    {
        // When the scan passed the last ']', IndexOf returned -1 and "nextEnd + 1" restarted it
        // at 0 with a deeper tagLevel every pass - JSON Type 5 detection never returned.
        const string json = "{\"text_tees\":[[1,[2,[3],[4,[5}";

        var list = await RunWithTimeout(() => Json.ReadArray(json, "text_tees"));
        var isMine = await RunWithTimeout(() => new JsonType5().IsMine(json.SplitToLines(), "test.json"));

        Assert.NotNull(list);
        Assert.False(isMine);
    }

    [Fact]
    public async Task Mp4Trun_HugeSampleCountWithoutPerSampleFields_IsCapped()
    {
        // No per-sample fields: the loop read nothing and ran sample_count (4 billion) times.
        var data = new byte[64];
        data[4] = 0xFF;
        data[5] = 0xFF;
        data[6] = 0xFF;
        data[7] = 0xFF;

        var trun = await RunWithTimeout(() => new Trun(new MemoryStream(data), (ulong)data.Length));

        Assert.InRange(trun.Samples.Count, 1, data.Length);
    }

    [Fact]
    public void Mp4Trun_SamplesWithoutPerSampleFields_AreKept()
    {
        // Legal: durations/sizes come from the tfhd/trex defaults, so the samples must stay.
        var data = new byte[64];
        data[7] = 3;

        var trun = new Trun(new MemoryStream(data), (ulong)data.Length);

        Assert.Equal(3, trun.Samples.Count);
    }

    [Fact]
    public void Mp4Elst_HugeBoxSize_DoesNotAllocateIt()
    {
        // The payload buffer was sized from the box size alone: 4 GB for a 32-byte file.
        var data = new byte[32];

        var elst = new Elst(new MemoryStream(data), 0xFFFFFFF0);

        Assert.Empty(elst.Entries);
    }

    [Fact]
    public void Mp4Mdia_HugeHdlrBoxSize_DoesNotAllocateIt()
    {
        var data = new byte[48];
        data[0] = 0x7F;
        data[1] = 0xFF;
        data[2] = 0xFF;
        data[3] = 0xF0;
        "hdlr"u8.CopyTo(data.AsSpan(4));
        "text"u8.CopyTo(data.AsSpan(16));

        var mdia = new Mdia(new MemoryStream(data), (ulong)data.Length);

        Assert.Equal("text", mdia.HandlerType);
    }

    [Fact]
    public void GetCcData_HugeSampleSize_DoesNotAllocateIt()
    {
        // A corrupt sample size near int.MaxValue made ArrayPool.Rent ask for more than the
        // largest possible array when reading CEA-608 from fragmented MP4 video samples.
        var data = new byte[64];

        var ccData = GetCcDataHelper.GetCcData(new MemoryStream(data), 0, int.MaxValue - 1, false, 4);

        Assert.Empty(ccData);
    }
}
