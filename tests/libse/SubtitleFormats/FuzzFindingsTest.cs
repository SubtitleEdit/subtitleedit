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
}
