using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// With an audio file loaded as the video the media info is there, but its dimension is 0x0 - and
/// a struct, so "?? 1080" never kicked in. "Remember these settings" then divided by zero (the
/// dialog never closed), and a remembered second subtitle got PlayResY 0 with font size 1.
/// </summary>
public class SecondarySubtitleStylerAudioOnlyTests
{
    private static FfmpegMediaInfo2 MakeAudioOnlyMediaInfo() =>
        (FfmpegMediaInfo2)Activator.CreateInstance(typeof(FfmpegMediaInfo2), nonPublic: true)!;

    [Fact]
    public void GetVideoSize_AudioOnlyOrNoMediaInfo_Is1080p()
    {
        Assert.Equal((1920, 1080), SecondarySubtitleStyler.GetVideoSize(MakeAudioOnlyMediaInfo()));
        Assert.Equal((1920, 1080), SecondarySubtitleStyler.GetVideoSize(null));
    }

    [Fact]
    public void BuildFromSettings_AudioOnly_UsesA1080pScript()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 2000));

        var result = SecondarySubtitleStyler.BuildFromSettings(subtitle, MakeAudioOnlyMediaInfo());

        Assert.Contains("PlayResY: 1080", result.Header);
    }
}
