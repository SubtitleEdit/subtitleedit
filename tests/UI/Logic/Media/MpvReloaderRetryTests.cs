using System.Reflection;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.Media;

/// <summary>
/// Going fullscreen, undocking and every layout rebuild create a brand new mpv player, and the
/// subtitle has to be pushed into it as an external file. That push used to be counted as done
/// even when mpv refused it - which it does until its core is initialized (lazily, by the
/// rendering surface) and the video is actually playing. Nothing pushes again during plain
/// playback, so one mistimed push left the video without subtitles for the rest of the
/// fullscreen session (issue #13407).
///
/// A default-constructed player has no core, which is exactly the state a just-created player
/// is in, so it stands in for "mpv is not ready yet" here.
/// </summary>
public class MpvReloaderRetryTests
{
    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));
        return subtitle;
    }

    private static T GetPrivateField<T>(MpvReloader reloader, string name)
    {
        var field = typeof(MpvReloader).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (T)field!.GetValue(reloader)!;
    }

    [Fact]
    public async Task RefreshMpv_ReportsFailure_WhenTheCoreIsNotUpYet()
    {
        var reloader = new MpvReloader();
        using var player = new LibMpvDynamicPlayer();

        var applied = await reloader.RefreshMpv(player, MakeSubtitle(), null, new SubRip());

        Assert.False(applied);

        reloader.Reset(); // drops the temp file the attempt created
    }

    [Fact]
    public async Task RefreshMpv_DoesNotRememberSubtitleTextMpvRejected()
    {
        var reloader = new MpvReloader();
        using var player = new LibMpvDynamicPlayer();

        await reloader.RefreshMpv(player, MakeSubtitle(), null, new SubRip());

        // Remembering the text as "what mpv is showing" is what made the miss permanent: the
        // next push would compare equal and skip the sub-add that never happened.
        Assert.Empty(GetPrivateField<string>(reloader, "_mpvTextOld"));

        reloader.Reset();
    }

    [Fact]
    public async Task RefreshMpv_DoesNotSpendARetry_WhenMpvRejectedThePush()
    {
        var reloader = new MpvReloader();
        using var player = new LibMpvDynamicPlayer();
        var before = GetPrivateField<int>(reloader, "_retryCount");

        await reloader.RefreshMpv(player, MakeSubtitle(), null, new SubRip());
        await reloader.RefreshMpv(player, MakeSubtitle(), null, new SubRip());

        // The retry budget covers pushes mpv accepted; burning it on pushes that never reached
        // a live core would leave nothing for the ones that can actually land.
        Assert.Equal(before, GetPrivateField<int>(reloader, "_retryCount"));

        reloader.Reset();
    }

    [Fact]
    public void SubtitleCommands_ReportNotInitialized_WhenThereIsNoCore()
    {
        using var player = new LibMpvDynamicPlayer();

        // Without this the commands answered mpv's success code (0) for a core that does not
        // exist, so callers could not tell an applied subtitle from a dropped one.
        Assert.True(player.SubAdd("some.ass") < 0);
        Assert.True(player.SubReload() < 0);
        Assert.True(player.SubRemove() < 0);
    }
}
