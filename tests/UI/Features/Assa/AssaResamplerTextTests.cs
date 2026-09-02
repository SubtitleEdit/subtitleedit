using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

namespace UITests.Features.Assa;

/// <summary>
/// <see cref="AssaResamplerHelper.ResampleText"/> is what the main window runs over the grid rows
/// when a video with another picture size is opened (#14367): the rows are the working text, so
/// scaling only <c>_subtitle.Paragraphs</c> left every positioned line at its old coordinates
/// while the header already named the new resolution.
/// </summary>
public class AssaResamplerTextTests
{
    [Fact]
    public void ResampleText_ScalesPositionMoveAndOrigin()
    {
        var text = @"{\pos(640,360)\move(100,200,300,400)\org(64,36)}Hello";

        var result = AssaResamplerHelper.ResampleText(text, 1280, 720, 1920, 1080);

        Assert.Equal(@"{\pos(960,540)\move(150,300,450,600)\org(96,54)}Hello", result);
    }

    [Fact]
    public void ResampleText_ScalesFontSizeOnlyWhenAsked()
    {
        var text = @"{\fs40\pos(640,360)}Hello";

        var positionsOnly = AssaResamplerHelper.ResampleText(text, 1280, 720, 1920, 1080, changeFontSize: false);
        var fontOnly = AssaResamplerHelper.ResampleText(text, 1280, 720, 1920, 1080, changePositions: false);

        Assert.Equal(@"{\fs40\pos(960,540)}Hello", positionsOnly);
        Assert.Equal(@"{\fs60\pos(640,360)}Hello", fontOnly);
    }

    [Fact]
    public void ResampleText_LeavesPlainTextAlone()
    {
        Assert.Equal("Hello", AssaResamplerHelper.ResampleText("Hello", 1280, 720, 1920, 1080));
        Assert.Equal(string.Empty, AssaResamplerHelper.ResampleText(string.Empty, 1280, 720, 1920, 1080));
    }

    [Fact]
    public void ApplyResampling_ScalesParagraphsAndHeader()
    {
        var subtitle = new Subtitle
        {
            Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: 720",
                "[Script Info]", AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: 1280",
                    "[Script Info]", AdvancedSubStationAlpha.DefaultHeader)),
        };
        subtitle.Paragraphs.Add(new Paragraph(@"{\pos(640,360)}Hello", 0, 1000));

        AssaResamplerHelper.ApplyResampling(subtitle, 1280, 720, 1920, 1080);

        Assert.Equal(@"{\pos(960,540)}Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal("1920", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", subtitle.Header));
        Assert.Equal("1080", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", subtitle.Header));
    }
}
