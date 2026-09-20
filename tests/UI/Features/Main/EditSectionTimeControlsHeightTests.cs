using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The edit section row is a fixed Pixel row since #14834, sized by a tracker that used to
/// look at the text column only. The up/down column to its left (Start/End/Duration/Layer)
/// is taller than the text column as soon as three or four of them are shown, and then ran
/// past the row and drew over whatever sits below the edit box.
/// </summary>
public class EditSectionTimeControlsHeightTests
{
    [AvaloniaTheory]
    [InlineData(true, false)]  // Start + End + Duration
    [InlineData(true, true)]   // Start + End + Duration + Layer (ASSA)
    [InlineData(false, false)] // default: Start + Duration
    public void EditSectionRow_IsTallEnoughForTheUpDownColumn(bool showEndTime, bool showLayer)
    {
        var appearance = Se.Settings.Appearance;
        var oldStart = appearance.ShowUpDownStartTime;
        var oldEnd = appearance.ShowUpDownEndTime;
        var oldDuration = appearance.ShowUpDownDuration;
        var oldLabels = appearance.ShowUpDownLabels;
        var oldLayer = appearance.ShowLayer;
        appearance.ShowUpDownStartTime = true;
        appearance.ShowUpDownEndTime = showEndTime;
        appearance.ShowUpDownDuration = true;
        appearance.ShowUpDownLabels = true;
        appearance.ShowLayer = showLayer;

        var (window, vm) = CreateMainViewModel();
        try
        {
            if (showLayer)
            {
                vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
                vm.ShowLayer = true;
            }

            Dispatcher.UIThread.RunJobs();

            var textEditGrid = Assert.Single(window.GetLogicalDescendants().OfType<Grid>(), g => g.Name == "SubtitleTextEditGrid");
            var editGrid = Assert.IsType<Grid>(textEditGrid.GetLogicalParent());
            var timeControls = Assert.Single(editGrid.Children.OfType<StackPanel>(), p => p.Name == InitListViewAndEditBox.TimeControlsPanelName);

            var visible = timeControls.Children.Where(c => c.IsVisible).ToList();
            Assert.Equal(showEndTime ? (showLayer ? 4 : 3) : 2, visible.Count);

            var lowestBottom = visible.Max(c => c.Bounds.Bottom) + timeControls.Bounds.Top;
            Assert.True(lowestBottom <= editGrid.Bounds.Height + 0.5,
                $"up/down column runs to {lowestBottom} px in a {editGrid.Bounds.Height} px edit section");
        }
        finally
        {
            CloseWindow(window, vm);
            appearance.ShowUpDownStartTime = oldStart;
            appearance.ShowUpDownEndTime = oldEnd;
            appearance.ShowUpDownDuration = oldDuration;
            appearance.ShowUpDownLabels = oldLabels;
            appearance.ShowLayer = oldLayer;
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
