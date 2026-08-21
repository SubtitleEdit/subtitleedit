using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.IO;

namespace LibUiLogicTests.Media;

/// <summary>
/// Reading media info must not take the caller down with it when ffmpeg cannot be launched - a
/// moved install, a stale configured path, no permission. Speech to text died exactly there: the
/// Win32 exception escaped Parse, so adding a file added no row and Transcribe stopped dead, with
/// nothing on screen to explain it (issue #13820).
/// </summary>
public class FfmpegMediaInfoParseTests : IDisposable
{
    private readonly string _previousLocation;
    private readonly string _folder;

    public FfmpegMediaInfoParseTests()
    {
        _previousLocation = Configuration.Settings.General.FFmpegLocation;
        _folder = Path.Combine(Path.GetTempPath(), "se-ffmpeg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Configuration.Settings.General.FFmpegLocation = _previousLocation;
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    [Fact]
    public void Parse_FfmpegPathDoesNotExist_ReturnsEmptyInfoInsteadOfThrowing()
    {
        Configuration.Settings.General.FFmpegLocation = Path.Combine(_folder, "gone", "ffmpeg.exe");

        var info = FfmpegMediaInfo.Parse(Path.Combine(_folder, "video.mkv"));

        Assert.NotNull(info);
        Assert.Empty(info.Tracks);
    }

    /// <summary>
    /// The reported case: a path that passes File.Exists but cannot be started as a process. The
    /// file here is not an executable, which is the same shape of failure on every platform.
    /// </summary>
    [Fact]
    public void Parse_FfmpegPathIsNotRunnable_ReturnsEmptyInfoInsteadOfThrowing()
    {
        var notAnExecutable = Path.Combine(_folder, "ffmpeg.exe");
        File.WriteAllText(notAnExecutable, "this is not a program");
        Configuration.Settings.General.FFmpegLocation = notAnExecutable;

        var info = FfmpegMediaInfo.Parse(Path.Combine(_folder, "video.mkv"));

        Assert.NotNull(info);
        Assert.Empty(info.Tracks);
    }
}
