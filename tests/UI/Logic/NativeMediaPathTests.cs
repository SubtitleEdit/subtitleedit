using Nikse.SubtitleEdit.Logic.VideoPlayers;

namespace UITests.Logic;

/// <summary>
/// Paths handed to libmpv/libvlc on Windows must survive MAX_PATH (#14407): a plain path of
/// 260+ chars fails silently in both players. mpv gets the "\\?\" extended-length prefix,
/// VLC (which cannot take that prefix) gets an 8.3 short name. Short paths, URLs and other
/// platforms pass through untouched.
/// </summary>
public class NativeMediaPathTests
{
    private static string LongWindowsPath(int length)
    {
        const string root = @"C:\Users\someone\Downloads\";
        var name = new string('a', length - root.Length - 4) + ".mkv";
        var path = root + name;
        Assert.Equal(length, path.Length);
        return path;
    }

    [Fact]
    public void AddLongPathPrefix_DriveLetterPath_GetsExtendedPrefix()
    {
        var result = NativeMediaPath.AddLongPathPrefix(@"C:\dir\file.mkv");
        Assert.Equal(@"\\?\C:\dir\file.mkv", result);
    }

    [Fact]
    public void AddLongPathPrefix_UncPath_GetsUncForm()
    {
        var result = NativeMediaPath.AddLongPathPrefix(@"\\server\share\dir\file.mkv");
        Assert.Equal(@"\\?\UNC\server\share\dir\file.mkv", result);
    }

    [Fact]
    public void AddLongPathPrefix_AlreadyPrefixed_IsUnchanged()
    {
        var result = NativeMediaPath.AddLongPathPrefix(@"\\?\C:\dir\file.mkv");
        Assert.Equal(@"\\?\C:\dir\file.mkv", result);
    }

    [Theory]
    [InlineData(@"\\?\C:\dir\file.mkv", @"C:\dir\file.mkv")]
    [InlineData(@"\\?\UNC\server\share\file.mkv", @"\\server\share\file.mkv")]
    [InlineData(@"C:\dir\file.mkv", @"C:\dir\file.mkv")]
    public void RemoveLongPathPrefix_RoundTrips(string input, string expected)
    {
        Assert.Equal(expected, NativeMediaPath.RemoveLongPathPrefix(input));
    }

    [Theory]
    [InlineData(@"C:\Videos\short.mkv")]
    [InlineData("https://example.com/stream.m3u8")]
    [InlineData("/home/user/video.mkv")]
    public void ForMpv_ShortPathsAndUrls_PassThrough(string path)
    {
        Assert.Equal(path, NativeMediaPath.ForMpv(path));
        Assert.Equal(path, NativeMediaPath.ForVlc(path));
    }

    [Fact]
    public void ForMpv_LongWindowsPath_IsPrefixedOnWindowsOnly()
    {
        var path = LongWindowsPath(300);
        var result = NativeMediaPath.ForMpv(path);

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(@"\\?\" + path, result);
        }
        else
        {
            Assert.Equal(path, result);
        }
    }

    [Fact]
    public void ForMpv_PathJustUnderMaxPath_IsUnchanged()
    {
        // 259 chars + NUL fits in MAX_PATH, so no prefix is needed - and none is added.
        var path = LongWindowsPath(NativeMediaPath.WindowsMaxPath - 1);
        Assert.Equal(path, NativeMediaPath.ForMpv(path));
    }

    [Fact]
    public void ForMpv_LongUrl_IsUnchanged()
    {
        var url = "https://example.com/" + new string('x', 300) + ".mp4";
        Assert.Equal(url, NativeMediaPath.ForMpv(url));
        Assert.Equal(url, NativeMediaPath.ForVlc(url));
    }

    [Fact]
    public void ForVlc_LongPathToMissingFile_FallsBackToOriginal()
    {
        // No 8.3 name can be produced for a file that does not exist; VLC then gets the
        // original path rather than a mangled one.
        var path = LongWindowsPath(300);
        Assert.Equal(path, NativeMediaPath.ForVlc(path));
    }
}
