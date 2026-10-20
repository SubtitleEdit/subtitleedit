using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

namespace UITests.Features.Assa;

/// <summary>
/// Set position: anchor grid, snapping, the X/Y fields and reset.
/// </summary>
public class AssaSetPositionAnchorTests
{
    [Fact]
    public void Snap_StartCenterOrEnd_StickToTheNearestGuide()
    {
        // 1920 wide: guides at 96 (safe left), 960 (center), 1824 (safe right); threshold 19.2
        Assert.Equal(96, AssaSetPositionViewModel.Snap(110, 200, 1920));          // start to safe left
        Assert.Equal(860, AssaSetPositionViewModel.Snap(850, 200, 1920));         // center 950 -> 960
        Assert.Equal(1624, AssaSetPositionViewModel.Snap(1610, 200, 1920));       // end 1810 -> 1824
        Assert.Equal(500, AssaSetPositionViewModel.Snap(500, 200, 1920));         // nothing near
    }

    [AvaloniaFact]
    public void Anchors_PlaceTheBoxInsideTheSafeArea()
    {
        var vm = new AssaSetPositionViewModel { IsPreviewReady = true }; // 1x1 text box on a 1920x1080 render canvas

        vm.SetAnchorCommand.Execute(0);
        Assert.Equal(96, vm.ScreenshotX);
        Assert.Equal(54, vm.ScreenshotY);
        Assert.Equal(0, vm.AnchorIndex);

        vm.SetAnchorCommand.Execute(8);
        Assert.Equal(1920 - 96 - 1, vm.ScreenshotX);
        Assert.Equal(1080 - 54 - 1, vm.ScreenshotY);
        Assert.Equal(vm.AnchorName, AssaSetPositionViewModel.AnchorNames[8]);

        vm.Nudge(10, 0);
        Assert.Equal(-1, vm.AnchorIndex);
    }

    [AvaloniaFact]
    public void PositionFields_MoveTheAnchor_AndFollowTheBox()
    {
        var vm = new AssaSetPositionViewModel { IsPreviewReady = true };
        vm.ScreenshotX = 100;
        Assert.Equal(vm.ResultX, vm.PositionX);

        vm.PositionX = 700;
        vm.PositionY = 300;
        Assert.Equal(700, vm.ResultX);
        Assert.Equal(300, vm.ResultY);
        Assert.Contains("\\pos(700,300)", vm.TagsPreview);
    }

    [AvaloniaFact]
    public void BeforeTheTextIsRendered_MovesAndOkWait()
    {
        // The dialog opens before ffmpeg has rendered the text (#16289); its place is unknown then
        var vm = new AssaSetPositionViewModel();

        vm.SetAnchorCommand.Execute(0);
        vm.Nudge(10, 10);
        vm.DragTo(500, 500);
        Assert.Equal(0, vm.ScreenshotX);
        Assert.Equal(0, vm.ScreenshotY);
        Assert.False(vm.OkCommand.CanExecute(null));
        Assert.Equal(string.Empty, vm.TagsPreview);

        vm.IsPreviewReady = true;
        Assert.True(vm.OkCommand.CanExecute(null));
    }
}
