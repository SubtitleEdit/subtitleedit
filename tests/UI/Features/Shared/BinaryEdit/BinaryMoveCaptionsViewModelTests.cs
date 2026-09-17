using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryMoveCaptions;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System.Collections.Generic;

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

        BinaryMoveCaptionsViewModel.Apply(new List<BinarySubtitleItem> { top, bottom }, 1080, 138, 0, MoveCaptionsMode.IntoBars);

        Assert.Equal(0, top.Y);
        Assert.Equal(1040, bottom.Y);
    }
}
