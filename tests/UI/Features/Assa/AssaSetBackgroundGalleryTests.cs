using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

namespace UITests.Features.Assa;

/// <summary>
/// Set background's style gallery and the options it shows.
/// </summary>
public class AssaSetBackgroundGalleryTests
{
    [AvaloniaFact]
    public void EveryBoxStyle_HasAThumbnail()
    {
        var vm = new AssSetBackgroundViewModel();
        Assert.Equal(24, vm.BoxStyles.Count);
        for (var i = 0; i < vm.BoxStyles.Count; i++)
        {
            var bounds = AssSetBackgroundWindow.MakeThumbnail(i).Bounds;
            Assert.True(bounds.Width > 4 && bounds.Height > 4, vm.BoxStyles[i]);
        }
    }

    [AvaloniaFact]
    public void Radius_IsOnlyForRoundedCorners_AndTheStyleNameFollowsTheSelection()
    {
        var vm = new AssSetBackgroundViewModel();
        vm.BoxStyleIndex = 1;
        Assert.True(vm.IsRadiusEnabled);
        Assert.Equal(vm.BoxStyles[1], vm.BoxStyleName);

        vm.BoxStyleIndex = 3;
        Assert.False(vm.IsRadiusEnabled);
        Assert.Equal(vm.BoxStyles[3], vm.BoxStyleName);
    }
}
