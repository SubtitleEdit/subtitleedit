using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.IO;

namespace LibSETests.ContainerFormats;

/// <summary>
/// The three fixtures cover the three ways a container carries chapters: Matroska chapter
/// elements, the Nero "chpl" box in MP4, and a QuickTime chapter track referenced by "tref"/"chap"
/// (written with ffmpeg's "-movflags disable_chpl", which leaves only the track).
/// </summary>
public class VideoChapterReaderTest
{
    private static string PathTo(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);

    [Theory]
    [InlineData("sample_MKV_chapters.mkv")]
    [InlineData("sample_MP4_chapters.mp4")]
    [InlineData("sample_MP4_chapter_track.mp4")]
    public void ReadsChaptersFromContainer(string fileName)
    {
        var chapters = VideoChapterReader.GetChapters(PathTo(fileName));

        Assert.Equal(3, chapters.Count);
        Assert.Equal(0, chapters[0].StartMilliseconds);
        Assert.Equal("Intro", chapters[0].Title);
        Assert.Equal(2000, chapters[1].StartMilliseconds);
        Assert.Equal("Middle & End", chapters[1].Title);
        Assert.Equal(4000, chapters[2].StartMilliseconds);
        Assert.Equal("Outro", chapters[2].Title);
    }

    [Fact]
    public void ChapterTrackIsNotListedAsASubtitleTrack()
    {
        var parser = new MP4Parser(PathTo("sample_MP4_chapter_track.mp4"));

        Assert.NotEmpty(parser.GetChapters());
        Assert.Empty(parser.GetSubtitleTracks());
    }

    [Fact]
    public void ReturnsNothingForAFileWithoutChapters()
    {
        Assert.Empty(VideoChapterReader.GetChapters(PathTo("sample_MP4.mp4")));
    }

    [Fact]
    public void ReturnsNothingForAMissingFile()
    {
        Assert.Empty(VideoChapterReader.GetChapters(PathTo("does_not_exist.mkv")));
    }

    [Theory]
    [InlineData("movie.mkv", true)]
    [InlineData("movie.mp4", true)]
    [InlineData("movie.mov", true)]
    [InlineData("movie.avi", false)]
    [InlineData("movie.ts", false)]
    [InlineData("", false)]
    public void KnowsWhichContainersCanCarryChapters(string fileName, bool expected)
    {
        Assert.Equal(expected, VideoChapterReader.IsSupportedContainer(fileName));
    }
}
