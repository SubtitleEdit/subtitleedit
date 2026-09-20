using Avalonia;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for issue #14440: the "Set position" preview rendered the line's own \frz into
/// the overlay bitmap and then rotated that bitmap again by the spinner value (around its center
/// instead of the \pos anchor), so \frz25 previewed at 50° in a different place than libass drew it.
/// </summary>
public class AssaSetPositionRotationTests
{
    private static SubtitleLineViewModel MakeLine(string text)
    {
        var subtitle = new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader };
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000) { Extra = "Default" });
        return new SubtitleLineViewModel(subtitle.Paragraphs[0], new AdvancedSubStationAlpha());
    }

    private static string PreviewText(string text)
    {
        var subtitle = new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader };
        var preview = AssaSetPositionViewModel.MakePreviewSubtitle(subtitle, MakeLine(text));
        return Assert.Single(preview.Paragraphs).Text;
    }

    [Fact]
    public void PreviewSubtitle_RendersTheTextUnrotatedButKeepsPos()
    {
        Assert.Equal(@"{\frz0}{\pos(627,136)}J envió un mensaje", PreviewText(@"{\pos(627,136)\frz25}J envió un mensaje"));
    }

    [Theory]
    [InlineData(@"{\frz-12.5}Hello", @"{\frz0}Hello")]
    [InlineData(@"{\frz(25)}Hello", @"{\frz0}Hello")]
    [InlineData(@"{\an5\frz25\pos(1,2)}Hello", @"{\frz0}{\an5\pos(1,2)}Hello")]
    public void PreviewSubtitle_StripsEveryFrzForm(string text, string expected)
    {
        Assert.Equal(expected, PreviewText(text));
    }

    [Fact]
    public void PreviewSubtitle_PinsRotationSoTheStyleAngleIsNotRenderedEither()
    {
        Assert.Equal(@"{\frz0}Hello world", PreviewText("Hello world"));
    }

    [Theory]
    [InlineData("2", null, "2")]
    [InlineData("8", "Hello", "8")]
    [InlineData("2", @"{\an5}Hello", "5")] // inline \an overrides the style
    [InlineData("2", @"{\pos(1,2)\an7\frz3}Hello", "7")]
    [InlineData(null, "Hello", "2")] // no style: ASS default
    [InlineData("", "Hello", "2")]
    [InlineData("11", "Hello", "2")] // garbage in the style
    public void ResolveAlignment_PrefersInlineTagThenStyleThenDefault(string? styleAlignment, string? text, string expected)
    {
        Assert.Equal(expected, AssaSetPositionViewModel.ResolveAlignment(styleAlignment, text));
    }

    [Theory]
    [InlineData("1", 0.0, 1.0)]
    [InlineData("2", 0.5, 1.0)]
    [InlineData("3", 1.0, 1.0)]
    [InlineData("4", 0.0, 0.5)]
    [InlineData("5", 0.5, 0.5)]
    [InlineData("6", 1.0, 0.5)]
    [InlineData("7", 0.0, 0.0)]
    [InlineData("8", 0.5, 0.0)]
    [InlineData("9", 1.0, 0.0)]
    public void RotationOrigin_IsTheAlignmentAnchorNotTheCenter(string alignment, double x, double y)
    {
        var origin = AssaSetPositionViewModel.GetRotationOrigin(alignment);

        Assert.Equal(RelativeUnit.Relative, origin.Unit);
        Assert.Equal(new Point(x, y), origin.Point);
    }

    [Theory]
    [InlineData(0, 0, @"\pos(627,136)")]
    [InlineData(25, 0, @"\pos(627,136)\frz25")]
    [InlineData(-1, 0, @"\pos(627,136)\frz-1")]
    [InlineData(12.5, 0, @"\pos(627,136)\frz12.5")]
    [InlineData(0, 10, @"\pos(627,136)\frz0")] // spinner at 0 must override a rotated style
    [InlineData(30, 10, @"\pos(627,136)\frz30")]
    public void BuildPositionTags_WritesFrzWhenRotatedOrWhenTheStyleIs(decimal rotation, decimal styleAngle, string expected)
    {
        Assert.Equal(expected, AssaSetPositionViewModel.BuildPositionTags(627, 136, rotation, styleAngle));
    }

    [Fact]
    public void BuildPositionTags_UsesInvariantCultureWithoutTrailingZeros()
    {
        Assert.Equal(@"\pos(1,2)\frz25", AssaSetPositionViewModel.BuildPositionTags(1, 2, 25.00m, 0));
    }

    [AvaloniaFact]
    public void ResultTags_CombineTheScriptSpaceAnchorAndTheRotation()
    {
        var vm = new AssaSetPositionViewModel
        {
            SourceWidth = 1920,
            SourceHeight = 1080,
            ScreenshotX = 300,
            ScreenshotY = 600,
            Rotation = 25,
        };

        // 1x1 overlay, bottom-center anchor: X = 300 + 0.5 -> 301, Y = 600 + 1 -> 601.
        Assert.Equal(@"\pos(301,601)\frz25", vm.ResultTags);
    }
}
