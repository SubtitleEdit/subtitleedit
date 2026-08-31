using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UITests.Logic;

/// <summary>
/// Issue #14330: mpv closes the audio device whenever playback pauses or stops (its
/// "audio-keep-open" default is "no"). Over HDMI to a receiver that drops the audio link on
/// every pause and costs a re-handshake on resume, heard as missing audio at the start of
/// playback. SE now holds the device open by default, with a setting to give it back.
/// </summary>
public class MpvAudioOptionTests
{
    [Fact]
    public void KeepAudioDeviceOpen_IsOnByDefault()
    {
        Assert.True(new SeVideo().MpvAudioKeepOpen);
    }

    /// <summary>
    /// The option only takes effect when it is set before mpv_initialize, and the player has four
    /// init paths (software, OpenGL, Metal, and the preview core). Missing the call on one of them
    /// would leave that path silently on mpv's default, which is exactly the reported bug - so
    /// every init has to be preceded by the one call that sets the pre-init audio options.
    /// </summary>
    [Fact]
    public void EveryMpvInitPath_SetsThePreInitAudioOptions()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(), "src", "ui", "Logic", "VideoPlayers", "LibMpvDynamic", "LibMpvDynamicPlayer.cs"));

        var inits = Regex.Matches(source, @"_mpvInitialize\(_mpv\)").Count;
        var optionCalls = Regex.Matches(source, @"^\s*SetPreInitAudioOptions\(\);", RegexOptions.Multiline).Count;

        Assert.True(inits > 0, "no mpv_initialize call found - did the player move?");
        Assert.True(
            optionCalls == inits,
            $"{inits} mpv_initialize call(s) but {optionCalls} SetPreInitAudioOptions() call(s). " +
            "Every init path must set the pre-init audio options, or that path silently falls back " +
            "to mpv's defaults (#14330).");
    }

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
