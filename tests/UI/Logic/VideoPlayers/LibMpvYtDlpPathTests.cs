using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers;

/// <summary>
/// mpv's ytdl_hook only searches mpv's config directory and PATH, so the copy of yt-dlp that
/// Subtitle Edit downloads into the data folder has to be named explicitly - otherwise streaming
/// a URL fails in the installed Windows build, where the data folder is %AppData%\Subtitle Edit
/// (issue #13067).
/// </summary>
public class LibMpvYtDlpPathTests
{
    [Fact]
    public void GetYtDlpPathList_ListsTheDownloadedBinaryFirst()
    {
        var paths = LibMpvDynamicPlayer.GetYtDlpPathList().Split(Path.PathSeparator);

        Assert.Equal(YtDlpDownloadService.GetFullFileName(), paths[0]);
    }

    [Fact]
    public void GetYtDlpPathList_KeepsMpvsOwnDefaultAsLastResort()
    {
        var paths = LibMpvDynamicPlayer.GetYtDlpPathList().Split(Path.PathSeparator);

        Assert.Equal("yt-dlp", paths[^1]);
    }

    [Fact]
    public void GetYtDlpPathList_UsesFullPathsSoLookupDoesNotDependOnPath()
    {
        var paths = LibMpvDynamicPlayer.GetYtDlpPathList().Split(Path.PathSeparator);

        // Everything but mpv's trailing "yt-dlp" default must be a full path - a bare name would
        // send ytdl_hook back to a PATH lookup, which is exactly what fails for an installed build.
        Assert.All(paths[..^1], p => Assert.True(Path.IsPathRooted(p), p));
    }

    [Fact]
    public void JoinYtDlpPaths_KeepsOrderAndDropsRepeats()
    {
        // A portable install has the data folder and the executable folder at the same place.
        var joined = LibMpvDynamicPlayer.JoinYtDlpPaths(["/a/yt-dlp", "/b/yt-dlp", "/a/yt-dlp", "yt-dlp"]);

        Assert.Equal(string.Join(Path.PathSeparator, "/a/yt-dlp", "/b/yt-dlp", "yt-dlp"), joined);
    }

    [Fact]
    public void JoinYtDlpPaths_DropsPathsThatWouldMakeMpvRejectTheWholeOption()
    {
        // script-opts is a comma-separated key=value list with no escaping; libmpv answers
        // MPV_ERROR_OPTION_ERROR when a value holds a comma, losing every path at once.
        var joined = LibMpvDynamicPlayer.JoinYtDlpPaths(["/home/doe, john/yt-dlp", "/b/yt-dlp", "yt-dlp"]);

        Assert.Equal(string.Join(Path.PathSeparator, "/b/yt-dlp", "yt-dlp"), joined);
    }
}
