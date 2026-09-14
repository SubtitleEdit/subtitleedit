using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// ApplyLetterboxRibbon is opt-in per player instance (#14872 review: a burn-in/transparent-
/// subtitles/TTS/binary-edit dialog's own throwaway preview must never show bars that will not be
/// in that dialog's actual output). These tests only cover the gating and the no-throw contract -
/// a bare <see cref="LibMpvDynamicPlayer"/> has no real mpv handle loaded, so SetOptionString
/// safely no-ops internally either way; the filter-string content itself is covered by
/// <see cref="global::UITests.Logic.VideoPlayers.LibMpvDynamic.LetterboxFilterBuilderTests"/>.
/// </summary>
public class LibMpvDynamicPlayerLetterboxTests
{
    [Fact]
    public void IsMainPreviewPlayer_DefaultsToFalse()
    {
        var player = new LibMpvDynamicPlayer();

        Assert.False(player.IsMainPreviewPlayer);
    }

    [Fact]
    public void ApplyLetterboxRibbon_WhenNotMainPreview_DoesNotThrowEvenWithRibbonEnabled()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = true;
        Se.Settings.Video.Letterbox.TopHeightPercent = 15;

        var player = new LibMpvDynamicPlayer { IsMainPreviewPlayer = false };

        var exception = Record.Exception(() => player.ApplyLetterboxRibbon());

        Assert.Null(exception);
    }

    [Fact]
    public void ApplyLetterboxRibbon_WhenMainPreview_DoesNotThrow()
    {
        using var _ = new SettingsScope(
            "Video.Letterbox.Enabled",
            "Video.Letterbox.TopHeightPercent",
            "Video.Letterbox.BottomHeightPercent");

        Se.Settings.Video.Letterbox.Enabled = true;
        Se.Settings.Video.Letterbox.TopHeightPercent = 15;

        var player = new LibMpvDynamicPlayer { IsMainPreviewPlayer = true };

        var exception = Record.Exception(() => player.ApplyLetterboxRibbon());

        Assert.Null(exception);
    }
}
