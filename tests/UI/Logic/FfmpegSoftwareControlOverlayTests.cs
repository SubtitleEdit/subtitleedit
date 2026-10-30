using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using System.Reflection;

namespace UITests.Logic;

/// <summary>
/// After the video was closed with a subtitle on screen, the overlay tick kept comparing the
/// (now empty) active lines with the lines last drawn - which Render never cleared on its "no
/// file" early return - so it repainted at 10 Hz forever.
/// </summary>
public class FfmpegSoftwareControlOverlayTests
{
    private static readonly FieldInfo OverlayLinesField =
        typeof(FfmpegSoftwareControl).GetField("_overlayLines", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [AvaloniaFact]
    public void Render_WithoutFile_ClearsLastDrawnLines()
    {
        var player = new FfmpegPlayer();
        var control = new FfmpegSoftwareControl(player);
        var window = new Window { Width = 320, Height = 180, Content = control };
        window.Show();
        try
        {
            OverlayLinesField.SetValue(control, new List<FfmpegPreviewSubtitle.Line>
            {
                new(0, 2, "Still on screen", [], false),
            });

            control.InvalidateVisual();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            using var frame = window.CaptureRenderedFrame();

            Assert.Empty((List<FfmpegPreviewSubtitle.Line>)OverlayLinesField.GetValue(control)!);
        }
        finally
        {
            window.Close();
        }
    }

    private static readonly Rect VideoRect = new(0, 100, 1280, 720);
    private static readonly Rect Window = new(0, 0, 1280, 920);

    private static FfmpegPreviewSubtitle.TextStyle Style(int alignment = 2, int borderStyle = 1, string justify = "auto") =>
        new("Arial", 20, false, 10, Colors.White, Colors.Black, Colors.Gray, 2, 1, alignment, borderStyle, justify);

    private static FfmpegPreviewSubtitle.Line Line(string text, bool secondary = false, int? alignment = null, FfmpegPreviewSubtitle.StyleRun[]? runs = null) =>
        new(0, 2, text, runs ?? [], secondary, alignment);

    [AvaloniaFact]
    public void BuildOverlay_LineWithOwnAlignment_GetsItsOwnBlock()
    {
        var blocks = FfmpegSoftwareControl.BuildOverlay([Line("bottom"), Line("top", alignment: 8)], VideoRect, VideoRect, Style(), null).Blocks;

        Assert.Equal(2, blocks.Count);
        Assert.True(blocks[1].Origin.Y < blocks[0].Origin.Y);
    }

    [AvaloniaFact]
    public void BuildOverlay_SameSpot_Stacks()
    {
        var secondary = Style() with { FontSize = 40 };

        var blocks = FfmpegSoftwareControl.BuildOverlay([Line("second", secondary: true), Line("main")], VideoRect, VideoRect, Style(), secondary).Blocks;

        Assert.True(blocks[0].Paint.FontSize < blocks[1].Paint.FontSize); // the primary comes first
        Assert.Equal(blocks[0].Origin.Y - blocks[1].Text.Height, blocks[1].Origin.Y, 3);
    }

    [AvaloniaFact]
    public void BuildOverlay_SecondaryStyle_SetsItsPlaceAndSize()
    {
        var secondary = Style(alignment: 7) with { FontSize = 40 };

        var blocks = FfmpegSoftwareControl.BuildOverlay([Line("main"), Line("second", secondary: true)], VideoRect, VideoRect, Style(), secondary).Blocks;

        Assert.True(blocks[1].Origin.Y < blocks[0].Origin.Y);
        Assert.Equal(blocks[0].Paint.FontSize * 2, blocks[1].Paint.FontSize, 3);
    }

    [AvaloniaFact]
    public void BuildOverlay_SecondaryWithoutStyle_GoesOppositeThePrimary()
    {
        var blocks = FfmpegSoftwareControl.BuildOverlay([Line("main"), Line("second", secondary: true)], VideoRect, VideoRect, Style(), null).Blocks;

        Assert.True(blocks[1].Origin.Y < blocks[0].Origin.Y);
    }

    [AvaloniaFact]
    public void BuildOverlay_MarginInBars_MeasuresFromTheWindow()
    {
        var inside = FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, VideoRect, Style(), null).Blocks[0];
        var bars = FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, Window, Style(), null).Blocks[0];

        Assert.Equal(inside.Origin.Y + 100, bars.Origin.Y, 3);
    }

    [AvaloniaFact]
    public void BuildOverlay_Justify_KeepsBlockAtAlignment_AndMovesShortLine()
    {
        var text = "a much longer first line" + Environment.NewLine + "short";
        var block = FfmpegSoftwareControl.BuildOverlay([Line(text)], VideoRect, VideoRect, Style(justify: "left"), null).Blocks[0];

        var width = block.Text.MaxTextWidth;
        Assert.Equal(TextAlignment.Left, block.Text.TextAlignment);
        Assert.Equal((VideoRect.Width - width) / 2, block.Origin.X, 3);
    }

    [AvaloniaFact]
    public void BuildOverlay_Box_ReplacesOutline()
    {
        var perLine = FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, VideoRect, Style(borderStyle: 3), null).Blocks[0];
        var oneBox = FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, VideoRect, Style(borderStyle: 4), null).Blocks[0];
        var outline = FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, VideoRect, Style(), null).Blocks[0];

        Assert.NotNull(perLine.Box);
        Assert.Null(perLine.Paint.OutlinePen);
        Assert.NotNull(oneBox.Box);
        Assert.Null(outline.Box);
        Assert.NotNull(outline.Paint.OutlinePen);
    }

    [AvaloniaFact]
    public void BuildOverlay_RightToLeftText_FlowsRightToLeft()
    {
        var block = FfmpegSoftwareControl.BuildOverlay([Line("مرحبا!")], VideoRect, VideoRect, Style(), null).Blocks[0];

        Assert.Equal(FlowDirection.RightToLeft, block.Text.FlowDirection);
    }

    [AvaloniaFact]
    public void BuildOverlay_ColorRun_IsDrawnAsText()
    {
        var run = new FfmpegPreviewSubtitle.StyleRun(0, 2, false, false, Colors.Red);

        Assert.True(FfmpegSoftwareControl.BuildOverlay([Line("main", runs: [run])], VideoRect, VideoRect, Style(), null).Blocks[0].HasColors);
        Assert.False(FfmpegSoftwareControl.BuildOverlay([Line("main")], VideoRect, VideoRect, Style(), null).Blocks[0].HasColors);
    }
}
