using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// The ffmpeg command line of "Write chapters to video". Both points below were checked by
/// running it: three rewrites of one .mp4 left three chapter tracks and no title tag.
/// </summary>
public class WriteChaptersParametersTests
{
    private static readonly string Parameters = FfmpegGenerator.GetWriteChaptersParameters("in.mp4", "chapters.ffmeta", "out.mp4");

    /// <summary>
    /// The ffmetadata input holds chapters only, so taking the global metadata from it wiped the
    /// video's title, comment and every other tag.
    /// </summary>
    [Fact]
    public void GlobalMetadata_StaysWithTheVideo()
    {
        Assert.Contains("-map_metadata 0", Parameters);
        Assert.DoesNotContain("-map_metadata 1", Parameters);
        Assert.Contains("-map_chapters 1", Parameters);
    }

    /// <summary>
    /// The chapter track an mp4 already has is read as a data stream, and "-map 0" copied it
    /// next to the new one - one more stale track per edit.
    /// </summary>
    [Fact]
    public void ExistingChapterTrack_IsNotCopied()
    {
        Assert.Contains("-map 0 -map -0:d:m:handler_name:SubtitleHandler -c copy", Parameters);
    }
}
