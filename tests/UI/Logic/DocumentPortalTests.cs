using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class DocumentPortalTests
{
    [Theory]
    [InlineData("/run/user/1000/doc/a1b2c3/FOO", "/run/user/1000", true)]
    [InlineData("/run/user/1000/doc/a1b2c3/FOO.srt", "/run/user/1000", true)]
    [InlineData("/run/user/1000/doc/a1b2c3/FOO", "/run/user/1000/", true)] // trailing slash in XDG_RUNTIME_DIR
    [InlineData("/home/user/Videos/FOO.srt", "/run/user/1000", false)]
    [InlineData("/run/user/1000/gvfs/smb-share/FOO.srt", "/run/user/1000", false)]
    [InlineData("/run/user/1000/doc", "/run/user/1000", false)] // the mount root itself is not a granted file
    public void IsPortalPath_WithRuntimeDir(string path, string runtimeDir, bool expected)
    {
        Assert.Equal(expected, DocumentPortal.IsPortalPath(path, runtimeDir));
    }

    [Theory]
    [InlineData("/run/user/1000/doc/a1b2c3/FOO.srt", true)]
    [InlineData("/home/user/Videos/FOO.srt", false)]
    [InlineData("/run/user/1000/gvfs/smb-share/FOO.srt", false)]
    public void IsPortalPath_WithoutRuntimeDir_FallsBackToPatternMatch(string path, bool expected)
    {
        Assert.Equal(expected, DocumentPortal.IsPortalPath(path, null));
    }
}
