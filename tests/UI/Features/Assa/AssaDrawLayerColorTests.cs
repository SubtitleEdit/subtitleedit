using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// The layer color field follows the selection, but only a user change of it recolors the layer -
/// lines imported with different styles can share a layer.
/// </summary>
public class AssaDrawLayerColorTests
{
    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) OpenMixedColorsOnOneLayer()
    {
        var header = AdvancedSubStationAlpha.DefaultHeader;
        header = AdvancedSubStationAlpha.AddSsaStyle(new SsaStyle { Name = "Red", Primary = SkiaSharp.SKColors.Red }, header);
        header = AdvancedSubStationAlpha.AddSsaStyle(new SsaStyle { Name = "White", Primary = SkiaSharp.SKColors.White }, header);
        var format = new AdvancedSubStationAlpha();
        var lines = new List<SubtitleLineViewModel>
        {
            new(new Paragraph("{\\p1}m 100 100 l 600 100 600 600 100 600{\\p0}", 0, 2000) { Extra = "Red", Layer = 0 }, format),
            new(new Paragraph("{\\p1}m 1200 200 l 1600 200 1600 600 1200 600{\\p0}", 0, 2000) { Extra = "White", Layer = 0 }, format),
        };

        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        vm.Initialize(new Subtitle { Header = header }, lines, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    [AvaloniaFact]
    public void SelectingShape_DoesNotRecolorItsLayer_OrPushUndo()
    {
        var (window, vm) = OpenMixedColorsOnOneLayer();
        try
        {
            var red = vm.Shapes[0].ForeColor;
            var white = vm.Shapes[1].ForeColor;
            Assert.NotEqual(red, white);
            Assert.Equal(vm.Shapes[0].Layer, vm.Shapes[1].Layer);

            vm.SelectShape(vm.Shapes[0]);
            Assert.Equal(red, vm.LayerColor);
            vm.SelectShape(vm.Shapes[1]);
            Assert.Equal(white, vm.LayerColor);

            Assert.Equal(red, vm.Shapes[0].ForeColor);
            Assert.Equal(white, vm.Shapes[1].ForeColor);
            Assert.False(vm.CanUndo);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ChangingLayerColor_StillRecolorsTheLayer_Undoably()
    {
        var (window, vm) = OpenMixedColorsOnOneLayer();
        try
        {
            var red = vm.Shapes[0].ForeColor;
            var white = vm.Shapes[1].ForeColor;
            vm.SelectShape(vm.Shapes[1]);

            vm.LayerColor = Colors.Cyan;

            Assert.Equal(Colors.Cyan, vm.Shapes[0].ForeColor);
            Assert.Equal(Colors.Cyan, vm.Shapes[1].ForeColor);
            Assert.True(vm.CanUndo);

            // Undo restores the selection of the mixed-color layer - that must not recolor it again or drop redo
            vm.UndoCommand.Execute(null);
            Assert.Equal(red, vm.Shapes[0].ForeColor);
            Assert.Equal(white, vm.Shapes[1].ForeColor);
            Assert.True(vm.CanRedo);

            vm.RedoCommand.Execute(null);
            Assert.Equal(Colors.Cyan, vm.Shapes[0].ForeColor);
            Assert.Equal(Colors.Cyan, vm.Shapes[1].ForeColor);
        }
        finally
        {
            window.Close();
        }
    }
}
