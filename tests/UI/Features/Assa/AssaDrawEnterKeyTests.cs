using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Enter in a property field (layer, position, size...) commits the value - it must not press the
/// OK button and close the editor. OK isn't the default button at all, so Enter elsewhere (the layer
/// tree, a check box) doesn't close the editor either.
/// </summary>
public class AssaDrawEnterKeyTests
{
    private const string Square = "{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}";

    [AvaloniaFact]
    public void Enter_InPropertyNumberField_DoesNotPressOk()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = new List<SubtitleLineViewModel> { new(new Paragraph(Square, 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha()) };
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1400, Height = 900 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            vm.SelectShape(vm.Shapes[0]);
            Dispatcher.UIThread.RunJobs();

            var fields = window.GetVisualDescendants().OfType<NumericUpDown>().Where(n => n.IsEffectivelyVisible).ToList();
            Assert.NotEmpty(fields);
            foreach (var field in fields)
            {
                var textBox = field.GetVisualDescendants().OfType<TextBox>().First();
                textBox.Focus();
                Dispatcher.UIThread.RunJobs();

                window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                Dispatcher.UIThread.RunJobs();

                Assert.False(vm.OkPressed);
                Assert.True(window.IsVisible);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Enter_InLayerTree_DoesNotPressOk()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = new List<SubtitleLineViewModel> { new(new Paragraph(Square, 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha()) };
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1400, Height = 900 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var tree = window.GetVisualDescendants().OfType<TreeView>().First(t => t.IsEffectivelyVisible);
            tree.Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(vm.OkPressed);
            Assert.True(window.IsVisible);
            Assert.DoesNotContain(window.GetVisualDescendants().OfType<Button>(), b => b.IsDefault);
        }
        finally
        {
            window.Close();
        }
    }
}
