using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UITests.Logic;

/// <summary>
/// Issue #14330: mpv stops the audio device when playback pauses and resets it on every seek.
/// Over HDMI to an A/V receiver the link goes idle and restarting it costs a re-handshake, heard
/// as a second or two of missing audio on resume. mpv's "audio-stream-silence" keeps the device
/// running and writes silence instead; SE exposes it as an off-by-default setting.
/// </summary>
public class MpvAudioOptionTests
{
    [Fact]
    public void AudioStreamSilence_IsOffByDefault()
    {
        // mpv's manual calls the option "strongly discouraged" - it changes A/V-sync and underrun
        // handling - and it only helps the HDMI-receiver case, so SE leaves mpv's behavior alone
        // unless the setting asks for it.
        Assert.False(new SeVideo().MpvAudioStreamSilence);
    }

    /// <summary>
    /// Issue #14523: SE set mpv's audio-buffer to 0.05 s (mpv default 0.2 s). Any hiccup on mpv's
    /// audio thread then emptied the device, mpv stopped the output until the buffer refilled and
    /// its clock stood still meanwhile - the waveform cursor and time display froze for up to a
    /// second or two, worst around pause/resume. Zero means "leave mpv's default alone".
    /// </summary>
    [Fact]
    public void AudioBuffer_LeavesMpvsDefaultAlone()
    {
        Assert.Equal(0, new SeVideo().MpvAudioBufferSeconds);
    }

    /// <summary>
    /// The 0.05 s value is persisted in every settings file written between 5.2.0 beta 20 and rc2,
    /// so the default change alone would only reach fresh installs.
    /// </summary>
    [Theory]
    [InlineData(0.05, 0)]
    [InlineData(0.0500001, 0)]
    [InlineData(0, 0)]
    [InlineData(0.1, 0.1)]
    [InlineData(0.2, 0.2)]
    [InlineData(0.04, 0.04)]
    public void AudioBufferMigration_ResetsOnlyTheShippedValue(double persisted, double expected)
    {
        var video = new SeVideo { MpvAudioBufferSeconds = persisted };

        Se.MigrateMpvAudioBuffer(video);

        Assert.Equal(expected, video.MpvAudioBufferSeconds);
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
        var source = File.ReadAllText(PlayerSourcePath);

        var inits = Regex.Matches(source, @"_mpvInitialize\(_mpv\)").Count;
        var optionCalls = Regex.Matches(source, @"^\s*SetPreInitAudioOptions\(\);", RegexOptions.Multiline).Count;

        Assert.True(inits > 0, "no mpv_initialize call found - did the player move?");
        Assert.True(
            optionCalls == inits,
            $"{inits} mpv_initialize call(s) but {optionCalls} SetPreInitAudioOptions() call(s). " +
            "Every init path must set the pre-init audio options, or that path silently falls back " +
            "to mpv's defaults (#14330).");
    }

    /// <summary>
    /// mpv answers an unknown option name with an error code that SE only logs, so a misspelled or
    /// invented option is a silent no-op - #14330 first shipped a fix built on "audio-keep-open",
    /// which mpv has never had, and the setting did nothing in either position. Every option name
    /// SE passes to mpv therefore has to appear below, having been checked against mpv's manual
    /// (https://mpv.io/manual/master/ - or "mpv --list-options"). Adding a name here is the step
    /// that forces that check.
    /// </summary>
    [Fact]
    public void EveryMpvOptionNameSet_IsARealMpvOption()
    {
        var names = new[] { PlayerSourcePath, NativeControlSourcePath }
            .SelectMany(p => Regex.Matches(File.ReadAllText(p), @"SetOptionString\(""([a-z0-9-]+)"""))
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(names);

        var unverified = names.Where(n => !VerifiedMpvOptionNames.Contains(n)).ToList();
        Assert.True(
            unverified.Count == 0,
            $"mpv option name(s) not on the verified list: {string.Join(", ", unverified)}. " +
            "Check each against mpv's manual and add it to VerifiedMpvOptionNames - mpv silently " +
            "ignores names it does not know, so a wrong one is a setting that does nothing (#14330).");
    }

    /// <summary>
    /// mpv option names SE sets, each verified to exist in mpv's manual.
    /// </summary>
    private static readonly HashSet<string> VerifiedMpvOptionNames = new(StringComparer.Ordinal)
    {
        "audio-buffer",
        "audio-stream-silence",
        "background-color",
        "brightness",
        "contrast",
        "end",
        "force-window",
        "gpu-api",
        "hr-seek",
        "idle",
        "keep-open",
        "lavfi-complex",
        "pause",
        "rebase-start-time",
        "script-opts",
        "sid",
        "start",
        "sub-ass-force-margins",
        "sub-ass-justify",
        "sub-justify",
        "sub-use-margins",
        "vo",
        "wid",
    };

    private static string PlayerSourcePath => Path.Combine(
        FindRepoRoot(), "src", "ui", "Logic", "VideoPlayers", "LibMpvDynamic", "LibMpvDynamicPlayer.cs");

    private static string NativeControlSourcePath => Path.Combine(
        FindRepoRoot(), "src", "ui", "Logic", "VideoPlayers", "LibMpvDynamic", "LibMpvDynamicNativeControl.cs");

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
