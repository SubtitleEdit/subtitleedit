using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
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

    [AvaloniaFact]
    public void PreviousAndNext_StepThroughTheSelectedLines()
    {
        var vm = new AssSetBackgroundViewModel();
        var lines = new[] { "One", "{\\i1}Two{\\i0}", "Three" }
            .Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, i * 2000, i * 2000 + 1500), new AdvancedSubStationAlpha()))
            .ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080, null);

        Assert.StartsWith("1 / 3", vm.PreviewLineInfo);
        Assert.False(vm.PreviousLineCommand.CanExecute(null));

        vm.NextLineCommand.Execute(null);
        Assert.Equal(1, vm.PreviewIndex);
        Assert.Equal("2 / 3   Two", vm.PreviewLineInfo);

        vm.NextLineCommand.Execute(null);
        Assert.False(vm.NextLineCommand.CanExecute(null));
        Assert.True(vm.PreviousLineCommand.CanExecute(null));
    }
}
