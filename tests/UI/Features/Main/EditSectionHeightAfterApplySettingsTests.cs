using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Settings OK/Apply rebuilds the whole main layout, which creates a new edit section row.
/// A text box the user had dragged taller snapped back to its minimum size (#15318).
/// </summary>
public class EditSectionHeightAfterApplySettingsTests
{
    [AvaloniaFact]
    public void ApplySettings_KeepsDraggedEditSectionHeight()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var oldRow = Assert.IsType<RowDefinition>(vm.EditSectionRow);
            var dragged = oldRow.MinHeight + 150;
            oldRow.Height = new GridLength(dragged, GridUnitType.Pixel);
            Dispatcher.UIThread.RunJobs();

            vm.ApplySettings();
            Dispatcher.UIThread.RunJobs();

            var newRow = Assert.IsType<RowDefinition>(vm.EditSectionRow);
            Assert.NotSame(oldRow, newRow);
            Assert.Equal(dragged, newRow.Height.Value, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ApplySettings_UntouchedEditSectionStaysAtItsFloor()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var oldRow = Assert.IsType<RowDefinition>(vm.EditSectionRow);
            var oldHeight = oldRow.Height.Value;

            vm.ApplySettings();
            Dispatcher.UIThread.RunJobs();

            var newRow = Assert.IsType<RowDefinition>(vm.EditSectionRow);
            Assert.Equal(oldHeight, newRow.Height.Value, 1);
            Assert.Equal(newRow.MinHeight, newRow.Height.Value, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.SuppressSaveChangesPromptOnClose(vm);
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
