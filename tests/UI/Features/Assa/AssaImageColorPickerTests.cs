using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;
using SkiaSharp;

namespace UITests.Features.Assa;

/// <summary>
/// Image color picker: the picked color in hex and ASSA form, the history, the magnifier.
/// </summary>
public class AssaImageColorPickerTests
{
    [AvaloniaFact]
    public void Pick_GivesHexAndAssaCode_AndKeepsAHistory()
    {
        var vm = new AssaImageColorPickerViewModel();
        vm.Pick(Color.FromRgb(0x6B, 0x8F, 0xB8));

        Assert.True(vm.HasPickedColor);
        Assert.Equal("#6B8FB8", vm.ClickedColorRgbHex);
        Assert.Equal("&HB88F6B&", vm.ClickedColorAssa);

        vm.Pick(Colors.White);
        vm.Pick(Color.FromRgb(0x6B, 0x8F, 0xB8)); // again: moves to the front, no duplicate
        Assert.Equal(2, vm.History.Count);
        Assert.Equal("#6B8FB8", vm.History[0].Hex);

        for (var i = 0; i < 20; i++)
        {
            vm.Pick(Color.FromRgb((byte)i, 0, 0));
        }

        Assert.Equal(14, vm.History.Count);

        vm.PickFromHistoryCommand.Execute(vm.History[5]);
        Assert.Equal(vm.History[0].Hex, vm.ClickedColorRgbHex);
    }

    [AvaloniaFact]
    public void ColorSelection_WithoutVideo_OpensOnPlaceholder_AndOnlyConfirmsAPickedColor()
    {
        var vm = new AssaImageColorPickerViewModel();
        vm.InitializeForColorSelection(null, 640, 360, 0);

        Assert.True(vm.IsColorSelection);
        Assert.False(vm.IsLoadingFrame);
        Assert.Equal(640, vm.Screenshot.PixelSize.Width);
        Assert.Null(vm.SelectedColor);

        vm.OkCommand.Execute(null);
        Assert.False(vm.OkPressed);

        vm.Pick(Color.FromRgb(0x10, 0x20, 0x30));
        vm.OkCommand.Execute(null);
        Assert.True(vm.OkPressed);
        Assert.Equal(Color.FromRgb(0x10, 0x20, 0x30), vm.SelectedColor);
    }

    [Fact]
    public void Magnifier_IsElevenPixelsEightTimesLarger_CenteredOnThePointer()
    {
        using var source = new SKBitmap(40, 40);
        source.Erase(SKColors.Black);
        source.SetPixel(20, 20, SKColors.Red);
        source.SetPixel(21, 20, SKColors.Lime);

        using var magnifier = AssaImageColorPickerViewModel.MakeMagnifier(source, 20, 20);
        Assert.Equal(88, magnifier.Width);
        Assert.Equal(SKColors.Red, magnifier.GetPixel(5 * 8 + 4, 5 * 8 + 4));   // the pointer pixel, in the middle
        Assert.Equal(SKColors.Lime, magnifier.GetPixel(6 * 8 + 4, 5 * 8 + 4));  // its right neighbor
        Assert.Equal(SKColors.Black, magnifier.GetPixel(2, 2));
    }
}
