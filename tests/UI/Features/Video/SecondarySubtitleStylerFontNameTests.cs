using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video;

/// <summary>
/// The second subtitle's own font name: stored empty while it is the preview font, so it keeps
/// following that font (#13492), and used as saved otherwise.
/// </summary>
public class SecondarySubtitleStylerFontNameTests
{
    [Fact]
    public void FontName_SavedAndUsedWhenRemembered()
    {
        var video = Se.Settings.Video;
        var oldFontName = video.SecondarySubtitleFontName;
        var oldPreviewFontName = video.MpvPreviewFontName;
        try
        {
            video.MpvPreviewFontName = "Arial";

            SecondarySubtitleStyler.SaveToSettings("Arial", 40, 1080, true, Colors.White, FontBoxType.None, "8", "auto");
            Assert.Equal(string.Empty, video.SecondarySubtitleFontName);

            SecondarySubtitleStyler.SaveToSettings("Georgia", 40, 1080, true, Colors.White, FontBoxType.None, "8", "auto");
            Assert.Equal("Georgia", video.SecondarySubtitleFontName);

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 2000));
            var result = SecondarySubtitleStyler.BuildFromSettings(subtitle, null);
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(result.Header);
            Assert.Equal("Georgia", Assert.Single(styles).FontName);
        }
        finally
        {
            video.SecondarySubtitleFontName = oldFontName;
            video.MpvPreviewFontName = oldPreviewFontName;
        }
    }

    [Fact]
    public void MakeStyle_EmptyFontName_FollowsPreviewFont()
    {
        var style = SecondarySubtitleStyler.MakeStyle("s", string.Empty, 40, false, Colors.White, FontBoxType.None, "8");
        Assert.Equal(Se.Settings.Video.MpvPreviewFontName, style.FontName);
    }
}
