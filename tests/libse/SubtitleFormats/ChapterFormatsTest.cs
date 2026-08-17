using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class ChapterFormatsTest
{
    private static List<Chapter> MakeChapters() => new()
    {
        new Chapter(0, "Opening"),
        new Chapter(10_000, "The Middle Bit"),
        new Chapter(3_725_500, "Finale & Credits"),
    };

    private static List<Chapter> LoadChapters(SubtitleFormat format, string text)
    {
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);
        return ChapterHelper.FromSubtitle(subtitle);
    }

    [Fact]
    public void MatroskaChaptersXml_RoundTrip()
    {
        var text = MatroskaChaptersXml.ToXml(MakeChapters(), "eng");
        var chapters = LoadChapters(new MatroskaChaptersXml(), text);

        Assert.Equal(3, chapters.Count);
        Assert.Equal(0, chapters[0].StartMilliseconds);
        Assert.Equal("Opening", chapters[0].Title);
        Assert.Equal(10_000, chapters[1].StartMilliseconds);
        Assert.Equal(3_725_500, chapters[2].StartMilliseconds);
        Assert.Equal("Finale & Credits", chapters[2].Title);
    }

    [Fact]
    public void MatroskaChaptersXml_WritesNanosecondTimeCodes()
    {
        var text = MatroskaChaptersXml.ToXml(new List<Chapter> { new Chapter(3_725_500, "x") }, "eng");

        Assert.Contains("<ChapterTimeStart>01:02:05.500000000</ChapterTimeStart>", text);
    }

    [Theory]
    [InlineData("00:00:05.500000000", 5500)]
    [InlineData("00:00:05.500", 5500)]
    [InlineData("00:00:05", 5000)]
    [InlineData("01:02:05.250000000", 3_725_250)]
    public void MatroskaChaptersXml_ReadsBothNanosecondAndMillisecondTimeCodes(string time, double expected)
    {
        Assert.Equal(expected, MatroskaChaptersXml.DecodeTimeCode(time));
    }

    [Fact]
    public void MatroskaChaptersXml_EscapesTitles()
    {
        var text = MatroskaChaptersXml.ToXml(new List<Chapter> { new Chapter(0, "A & B <c>") }, "eng");
        var chapters = LoadChapters(new MatroskaChaptersXml(), text);

        Assert.Equal("A & B <c>", chapters[0].Title);
    }

    [Fact]
    public void FfmpegMetadataChapters_RoundTrip()
    {
        var text = FfmpegMetadataChapters.ToFfmpegMetadata(MakeChapters());
        var chapters = LoadChapters(new FfmpegMetadataChapters(), text);

        Assert.Equal(3, chapters.Count);
        Assert.Equal(0, chapters[0].StartMilliseconds);
        Assert.Equal(10_000, chapters[1].StartMilliseconds);
        Assert.Equal(3_725_500, chapters[2].StartMilliseconds);
        Assert.Equal("Finale & Credits", chapters[2].Title);
    }

    [Fact]
    public void FfmpegMetadataChapters_EndOfChapterIsStartOfTheNext()
    {
        var text = FfmpegMetadataChapters.ToFfmpegMetadata(MakeChapters());

        Assert.Contains("START=0", text);
        Assert.Contains("END=10000", text);
    }

    [Fact]
    public void FfmpegMetadataChapters_EscapesReservedCharacters()
    {
        // "=", ";" and "#" all mean something to the ffmetadata reader.
        var text = FfmpegMetadataChapters.ToFfmpegMetadata(new List<Chapter> { new Chapter(0, "a=b; c #2 \\ d") });
        var chapters = LoadChapters(new FfmpegMetadataChapters(), text);

        Assert.Equal("a=b; c #2 \\ d", chapters[0].Title);
    }

    [Fact]
    public void FfmpegMetadataChapters_HonoursTimeBase()
    {
        const string text = @";FFMETADATA1

[CHAPTER]
TIMEBASE=1/1000000000
START=12500000000
END=25000000000
title=Nanoseconds";

        var chapters = LoadChapters(new FfmpegMetadataChapters(), text);

        Assert.Single(chapters);
        Assert.Equal(12_500, chapters[0].StartMilliseconds);
    }

    [Fact]
    public void FfmpegMetadataChapters_RejectsFileWithoutMagicHeader()
    {
        var format = new FfmpegMetadataChapters();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, new List<string> { "[CHAPTER]", "START=0", "title=x" }, null);

        Assert.Empty(subtitle.Paragraphs);
    }

    [Fact]
    public void YouTubeChapters_RoundTrip()
    {
        var text = YouTubeChapters.ToDescriptionText(MakeChapters());
        var chapters = LoadChapters(new YouTubeChapters(), text);

        Assert.Equal(3, chapters.Count);
        Assert.Equal(0, chapters[0].StartMilliseconds);
        Assert.Equal(10_000, chapters[1].StartMilliseconds);

        // Seconds resolution only - YouTube ignores anything finer.
        Assert.Equal(3_725_000, chapters[2].StartMilliseconds);
    }

    [Fact]
    public void YouTubeChapters_WritesHoursOnlyWhenNeeded()
    {
        var text = YouTubeChapters.ToDescriptionText(MakeChapters());

        Assert.Contains("0:00 Opening", text);
        Assert.Contains("1:02:05 Finale & Credits", text);
    }

    [Theory]
    [InlineData("0:00 - Intro")]
    [InlineData("0:00 Intro")]
    [InlineData("00:00:00 Intro")]
    public void YouTubeChapters_AcceptsCommonSeparatorsAndTimeForms(string firstLine)
    {
        var chapters = LoadChapters(new YouTubeChapters(), firstLine + Environment.NewLine + "1:30 Second");

        Assert.Equal(2, chapters.Count);
        Assert.Equal("Intro", chapters[0].Title);
        Assert.Equal(90_000, chapters[1].StartMilliseconds);
    }

    [Fact]
    public void YouTubeChapters_RejectsListNotStartingAtZero()
    {
        var format = new YouTubeChapters();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, new List<string> { "1:00 One", "2:00 Two" }, null);

        Assert.Empty(subtitle.Paragraphs);
    }

    [Fact]
    public void YouTubeChapters_DoesNotClaimFrameBasedTimeCodes()
    {
        var format = new YouTubeChapters();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, new List<string> { "00:00:00:00 Hello", "00:00:04:00 There" }, null);

        Assert.Empty(subtitle.Paragraphs);
    }

    [Fact]
    public void ChapterHelper_StretchesEachChapterToTheNext()
    {
        var subtitle = ChapterHelper.ToSubtitle(MakeChapters());

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(10_000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(3_725_500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(
            3_725_500 + ChapterHelper.LastChapterDurationMilliseconds,
            subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ChapterHelper_TitleLosesTagsAndLineBreaks()
    {
        Assert.Equal("Two lines", ChapterHelper.ToTitle("<i>Two</i>\r\nlines"));
    }

    [Theory]
    [InlineData(typeof(MatroskaChaptersXml))]
    [InlineData(typeof(FfmpegMetadataChapters))]
    [InlineData(typeof(YouTubeChapters))]
    public void ChapterFormats_AreAutoDetected(Type formatType)
    {
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;
        var text = format.ToText(ChapterHelper.ToSubtitle(MakeChapters()), "test");
        var lines = new List<string>(text.SplitToLines());

        var detected = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(f => f.IsMine(lines, null));

        Assert.NotNull(detected);
        Assert.Equal(format.Name, detected!.Name);
    }
}
