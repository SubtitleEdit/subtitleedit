using System;
using System.IO;
using System.Text.RegularExpressions;

namespace UITests.Logic;

/// <summary>
/// Issue #14929: mpv starts its clipboard thread at init whether or not anything uses the
/// clipboard, and in mpv 0.40.0 the Wayland backend can leave a writer-less pipe in its poll set
/// forever - the thread then spins at 100% CPU for the life of the process. SE never touches
/// mpv's clipboard, so it clears "clipboard-backends" before mpv_initialize and the thread is
/// never created. 0.41.0 fixed the upstream bug, but Linux can load a system libmpv and many
/// distributions still ship 0.40.x.
/// </summary>
public class MpvClipboardOptionTests
{
    /// <summary>
    /// The option only takes effect when it is set before mpv_initialize, and the player has four
    /// init paths (software, OpenGL, Metal, and the preview core). A path that misses the call
    /// still gets a clipboard thread, which is the whole failure mode.
    /// </summary>
    [Fact]
    public void EveryMpvInitPath_ClearsTheClipboardBackends()
    {
        var source = File.ReadAllText(PlayerSourcePath);

        var inits = Regex.Matches(source, @"_mpvInitialize\(_mpv\)").Count;
        var optionCalls = Regex.Matches(source, @"^\s*SetClipboardBackendsOption\(\);", RegexOptions.Multiline).Count;

        Assert.True(inits > 0, "no mpv_initialize call found - did the player move?");
        Assert.True(
            optionCalls == inits,
            $"{inits} mpv_initialize call(s) but {optionCalls} SetClipboardBackendsOption() call(s). " +
            "Every init path must clear clipboard-backends, or that path still starts mpv's " +
            "clipboard thread and can hang the player on libmpv 0.40.x (#14929).");
    }

    /// <summary>
    /// The option is only in mpv from 0.40 - the version that added clipboard backends at all - so
    /// an older libmpv answers MPV_ERROR_OPTION_NOT_FOUND. That is expected, not a failure, and
    /// must not reach the error log on every player construction.
    /// </summary>
    [Fact]
    public void ClearingTheClipboardBackends_DoesNotLogOnOlderLibmpv()
    {
        var source = File.ReadAllText(PlayerSourcePath);

        var body = Regex.Match(
            source,
            @"private void SetClipboardBackendsOption\(\)\s*\{(?<body>.*?)\n    \}",
            RegexOptions.Singleline);

        Assert.True(body.Success, "SetClipboardBackendsOption() not found - did it get renamed?");
        Assert.Contains("MpvErrorOptionNotFound", body.Groups["body"].Value, StringComparison.Ordinal);
    }

    private static string PlayerSourcePath => Path.Combine(
        FindRepoRoot(), "src", "ui", "Logic", "VideoPlayers", "LibMpvDynamic", "LibMpvDynamicPlayer.cs");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }
}
