using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for issue #13101: the ASSA style editor lost information when switching
/// between styles. The editor panel binds two-way through "CurrentStyle.X", and two of those
/// bindings destroyed data on the previously (or newly) shown style:
/// - the nine alignment radios share one group, and the group-uncheck of the old radio wrote
///   false into a style that still had that alignment set;
/// - the font combo cleared SelectedItem (and thereby the style's FontName) when the new
///   current style's font was not in the combo's item list.
/// These tests drive the real controls headless, built exactly like the production windows.
/// </summary>
public class AssaStyleEditorDataLossTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }
    private static AssaStylesViewModel MakeVm()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<AssaStylesViewModel>();
    }

    private static StackPanel MakeAlignmentRadios(AssaStylesViewModel vm)
    {
        // Same construction as AssaStylesWindow/SsaStylesWindow/AssaSingleStyleWindow.
        var panel = new StackPanel();
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn7), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn8), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn9), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn4), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn5), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn6), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn1), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn2), "align"));
        panel.Children.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn3), "align"));
        return panel;
    }

    [AvaloniaFact]
    public void SwitchBetweenStylesWithDifferentAlignment_DoesNotLoseAlignment()
    {
        var vm = MakeVm();
        var a = new StyleDisplay(new SsaStyle { Name = "A", Alignment = "2" });
        var b = new StyleDisplay(new SsaStyle { Name = "B", Alignment = "5" });

        var window = new Window { Content = MakeAlignmentRadios(vm) };
        _windows.Add(window);
        window.Show();

        vm.CurrentStyle = a;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        vm.CurrentStyle = b;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(b.AlignmentAn5, "style B lost An5 after direct switch");
        Assert.True(a.AlignmentAn2, "style A lost An2 after direct switch");

        vm.CurrentStyle = a;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(a.AlignmentAn2, "style A lost An2 after switching back");
        Assert.True(b.AlignmentAn5, "style B lost An5 after switching back");
    }

    [AvaloniaFact]
    public void SwitchBetweenStylesWithSameAlignment_ThroughNull_DoesNotLoseAlignment()
    {
        var vm = MakeVm();
        var fileStyle = new StyleDisplay(new SsaStyle { Name = "FileDefault", Alignment = "2" });
        var storageStyle = new StyleDisplay(new SsaStyle { Name = "StorageDefault", Alignment = "2" });

        var window = new Window { Content = MakeAlignmentRadios(vm) };
        _windows.Add(window);
        window.Show();

        vm.CurrentStyle = fileStyle;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // The file and storage grids both switch the current style on GotFocus and on
        // SelectionChanged, so a grid-to-grid click briefly passes through a stale selection.
        vm.CurrentStyle = null;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        vm.CurrentStyle = storageStyle;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(storageStyle.AlignmentAn2, "storage style lost An2 after switching through null");
        Assert.True(fileStyle.AlignmentAn2, "file style lost An2 after switching through null");
    }

    [AvaloniaFact]
    public void CheckingAnotherAlignmentRadio_MovesTheAlignment()
    {
        var vm = MakeVm();
        var style = new StyleDisplay(new SsaStyle { Name = "A", Alignment = "2" });

        var panel = MakeAlignmentRadios(vm);
        var window = new Window { Content = panel };
        _windows.Add(window);
        window.Show();

        vm.CurrentStyle = style;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // Check the An8 radio (top center) like a user click would.
        var an8Radio = (RadioButton)panel.Children[1];
        an8Radio.IsChecked = true;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.True(style.AlignmentAn8);
        Assert.False(style.AlignmentAn2);
        Assert.Equal("8", style.GetAlignment());
    }

    [AvaloniaFact]
    public void SwitchToStyleWithFontNotInFontsList_DoesNotLoseFontName()
    {
        var vm = MakeVm();
        vm.Fonts.Clear();
        vm.Fonts.Add("Arial");

        var a = new StyleDisplay(new SsaStyle { Name = "A", FontName = "Arial" });
        var b = new StyleDisplay(new SsaStyle { Name = "B", FontName = "SomeUninstalledFont" });

        var combo = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontName));
        var window = new Window { Content = combo };
        _windows.Add(window);
        window.Show();

        vm.CurrentStyle = a;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        vm.CurrentStyle = b;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal("SomeUninstalledFont", b.FontName);
        Assert.Equal("Arial", a.FontName);
    }
}
