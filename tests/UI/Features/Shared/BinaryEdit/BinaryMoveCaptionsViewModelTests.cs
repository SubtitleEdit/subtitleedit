using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryMoveCaptions;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace UITests.Features.Shared.BinaryEdit;

public class BinaryMoveCaptionsViewModelTests
{
    private static BinarySubtitleItem MakeItem(int y, int height)
    {
        using var skBitmap = new SKBitmap(100, height);
        return new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1))
        {
            X = 0,
            Y = y,
            Bitmap = skBitmap.ToAvaloniaBitmap(),
        };
    }

    [AvaloniaFact]
    public void IntoBars_BottomCaptionSitsAgainstBottomEdge()
    {
        var item = MakeItem(y: 900, height: 60);

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 10, MoveCaptionsMode.IntoBars);

        Assert.Equal(1080 - 60 - 10, y);
    }

    [AvaloniaFact]
    public void IntoBars_TopCaptionSitsAgainstTopEdge()
    {
        var item = MakeItem(y: 200, height: 60);

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 10, MoveCaptionsMode.IntoBars);

        Assert.Equal(10, y);
    }

    [AvaloniaFact]
    public void IntoPicture_BottomCaptionSitsAboveBottomBar()
    {
        var item = MakeItem(y: 1010, height: 60);

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 10, MoveCaptionsMode.IntoPicture);

        Assert.Equal(1080 - 138 - 60 - 10, y);
    }

    [AvaloniaFact]
    public void IntoPicture_TopCaptionSitsBelowTopBar()
    {
        var item = MakeItem(y: 0, height: 60);

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 10, MoveCaptionsMode.IntoPicture);

        Assert.Equal(138 + 10, y);
    }

    [AvaloniaFact]
    public void GetNewY_ClampsSoImageStaysOnScreen()
    {
        var item = MakeItem(y: 500, height: 60);

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 5000, MoveCaptionsMode.IntoBars);

        Assert.Equal(1080 - 60, y);
    }

    [Fact]
    public void GetNewY_NoBitmapIsUntouched()
    {
        var item = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { Y = 42 };

        var y = BinaryMoveCaptionsViewModel.GetNewY(item, 1080, 138, 10, MoveCaptionsMode.IntoBars);

        Assert.Equal(42, y);
    }

    /// <summary>
    /// The constructor pre-selects the 2.39:1 item as a plain field write, so selecting that
    /// same item in Initialize is a SetProperty no-op and never triggered the bar height
    /// calculation - the dialog opened with a 0 px bar for 2.39:1 while every other preset worked.
    /// </summary>
    [AvaloniaFact]
    public void Initialize_ComputesBarHeightForTheDefault239Preset()
    {
        var vm = new BinaryMoveCaptionsViewModel();

        vm.Initialize(new List<BinarySubtitleItem>(), 1920, 1080, "2.39", 0);

        var expected = PositionMonitorLogic.CalculateBarHeight(1920, 1080, 2.39);
        Assert.True(expected > 0);
        Assert.Equal(expected, vm.BarHeight);
        Assert.False(vm.IsBarHeightEditable);
    }

    [AvaloniaFact]
    public void Initialize_ComputesBarHeightForAnotherPreset()
    {
        var vm = new BinaryMoveCaptionsViewModel();

        vm.Initialize(new List<BinarySubtitleItem>(), 1920, 1080, "1.85", 0);

        Assert.Equal(PositionMonitorLogic.CalculateBarHeight(1920, 1080, 1.85), vm.BarHeight);
    }

    [AvaloniaFact]
    public void Initialize_CustomPresetKeepsTheCurrentBarHeight()
    {
        var vm = new BinaryMoveCaptionsViewModel();

        vm.Initialize(new List<BinarySubtitleItem>(), 1920, 1080, "custom", 77);

        Assert.Equal(77, vm.BarHeight);
        Assert.True(vm.IsBarHeightEditable);
    }

    [AvaloniaFact]
    public void Apply_MovesEveryItem()
    {
        var top = MakeItem(y: 100, height: 40);
        var bottom = MakeItem(y: 900, height: 40);

        BinaryMoveCaptionsViewModel.Apply(new List<BinarySubtitleItem> { top, bottom }, 1080, 138, 0, MoveCaptionsMode.IntoBars, MoveCaptionsAlignment.Baseline, null);

        Assert.Equal(0, top.Y);
        Assert.Equal(1040, bottom.Y);
    }

    [AvaloniaFact]
    public void KeepRelative_CaptionsThatSharedAYKeepSharingIt()
    {
        // #16360: same Y, different bitmap heights (one has descenders). 2.39:1 on 1080p, offset 10.
        var withDescenders = MakeItem(y: 876, height: 149);
        var withoutDescenders = MakeItem(y: 876, height: 137);

        BinaryMoveCaptionsViewModel.Apply(new List<BinarySubtitleItem> { withDescenders, withoutDescenders }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.KeepRelativePositions, null);

        Assert.Equal(783, withDescenders.Y);
        Assert.Equal(783, withoutDescenders.Y);
    }

    [AvaloniaFact]
    public void KeepRelative_TopCaptionsKeepTheirRelativePositions()
    {
        var higher = MakeItem(y: 20, height: 60);
        var lower = MakeItem(y: 30, height: 80);

        BinaryMoveCaptionsViewModel.Apply(new List<BinarySubtitleItem> { higher, lower }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.KeepRelativePositions, null);

        Assert.Equal(148, higher.Y);
        Assert.Equal(158, lower.Y);
    }

    [AvaloniaFact]
    public void KeepRelative_IntoBars_BottomOffsetComesFromTheLowestBottomEdge()
    {
        var a = MakeItem(y: 800, height: 100); // bottom edge 900
        var b = MakeItem(y: 820, height: 60);  // bottom edge 880

        BinaryMoveCaptionsViewModel.Apply(new List<BinarySubtitleItem> { a, b }, 1080, 138, 10, MoveCaptionsMode.IntoBars, MoveCaptionsAlignment.KeepRelativePositions, null);

        Assert.Equal(1080 - 10 - 100, a.Y);
        Assert.Equal(a.Y + 20, b.Y);
    }

    [AvaloniaFact]
    public void KeepRelative_TopAndBottomMoveIndependently()
    {
        var top = MakeItem(y: 0, height: 60);
        var bottom = MakeItem(y: 1010, height: 60);
        var noBitmap = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { Y = 42 };

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { top, bottom, noBitmap }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.KeepRelativePositions, null);

        Assert.Equal(new[] { 148, 1080 - 138 - 60 - 10, 42 }, newYs);
    }

    [AvaloniaFact]
    public void Baseline_CaptionsFromTheIssueGetTheSameBaseline()
    {
        // #16360: (1) has descenders (12 px), (3) has none. Their baselines both end up at 932.
        var withDescenders = MakeItem(y: 876, height: 149);
        var withoutDescenders = MakeItem(y: 876, height: 137);

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { withDescenders, withoutDescenders }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.Baseline, new[] { 12, 0 });

        Assert.Equal(932 - 149 + 12, newYs[0]);
        Assert.Equal(932 - 137, newYs[1]);
        Assert.Equal(newYs[0] + 149 - 12, newYs[1] + 137);
    }

    [AvaloniaFact]
    public void Baseline_EachCaptionIsPlacedOnItsOwn()
    {
        // A caption already in the bar must not push one that is already in place further up.
        var inBar = MakeItem(y: 1000, height: 60);
        var inPlace = MakeItem(y: 872, height: 60);

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { inBar, inPlace }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.Baseline, new[] { 0, 0 });

        Assert.Equal(new[] { 872, 872 }, newYs);
    }

    [AvaloniaFact]
    public void Baseline_TopCaptionIgnoresTheDescent()
    {
        var top = MakeItem(y: 0, height: 60);

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { top }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.Baseline, new[] { 12 });

        Assert.Equal(148, newYs[0]);
    }

    [AvaloniaFact]
    public void Baseline_IntoBars_StaysOnScreen()
    {
        var bottom = MakeItem(y: 900, height: 60);

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { bottom }, 1080, 138, 5, MoveCaptionsMode.IntoBars, MoveCaptionsAlignment.Baseline, new[] { 12 });

        Assert.Equal(1080 - 60, newYs[0]);
    }

    [AvaloniaFact]
    public void Baseline_WithoutDescentsPlacesByImageEdge()
    {
        var bottom = MakeItem(y: 1010, height: 60);
        var noBitmap = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { Y = 42 };

        var newYs = BinaryMoveCaptionsViewModel.GetNewYs(new List<BinarySubtitleItem> { bottom, noBitmap }, 1080, 138, 10, MoveCaptionsMode.IntoPicture, MoveCaptionsAlignment.Baseline, null);

        Assert.Equal(new[] { 1080 - 138 - 60 - 10, 42 }, newYs);
    }

    [AvaloniaFact]
    public void GetDescents_FindsTheDescenderOfTheLastLine()
    {
        var withDescender = MakeTextItem("abc jy");
        var withoutDescender = MakeTextItem("abc");
        var noBitmap = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1));

        var descents = BinaryMoveCaptionsViewModel.GetDescents(new List<BinarySubtitleItem> { withDescender, withoutDescender, noBitmap }, 1080);

        Assert.True(descents[0] > 5, $"descent was {descents[0]}");
        Assert.Equal(0, descents[1]);
        Assert.Equal(0, descents[2]);
    }

    private static BinarySubtitleItem MakeTextItem(string text)
    {
        using var font = new SKFont(SKTypeface.Default, 64);
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = false };
        font.MeasureText(text, out var bounds, paint);
        var width = (int)Math.Ceiling(bounds.Width) + 4;
        var height = (int)Math.Ceiling(bounds.Height) + 4;
        using var skBitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(skBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(text, 2 - bounds.Left, 2 - bounds.Top, font, paint);
        }

        return new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { Y = 900, Bitmap = skBitmap.ToAvaloniaBitmap() };
    }
}
