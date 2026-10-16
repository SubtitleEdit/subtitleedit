using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The link toggle beside the edit box's Duration field picks what changing Show does: linked
/// moves the whole line (StartTimeKeepDuration), unlinked moves only the start (StartTimeOnly).
/// Until the user clicks it, the old rule applies - linked while the end-time editor is hidden,
/// unlinked while it is shown. Toggling persists the choice and rebinds the Show box at once.
/// </summary>
public class EditBoxStartDurationLinkTests
{
    [AvaloniaTheory]
    [InlineData(false, true)]  // default: no end-time editor -> moving Show keeps the duration
    [InlineData(true, false)]  // end-time editor shown -> moving Show keeps the end
    public void Default_FollowsTheEndTimeEditorSetting(bool showEndTime, bool expectLinked)
    {
        var appearance = Se.Settings.Appearance;
        var oldEnd = appearance.ShowUpDownEndTime;
        var oldLink = appearance.EditBoxStartKeepsDuration;
        appearance.ShowUpDownEndTime = showEndTime;
        appearance.EditBoxStartKeepsDuration = null;

        var (window, vm) = CreateMainViewModel();
        try
        {
            var toggle = FindToggle(window);
            Assert.Equal(expectLinked, toggle.IsChecked);

            var line = vm.Subtitles[1];
            vm.EditBoxStartTimeUpDown!.Value = line.StartTime + TimeSpan.FromMilliseconds(500);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(3500, line.StartTime.TotalMilliseconds);
            Assert.Equal(expectLinked ? 5500 : 5000, line.EndTime.TotalMilliseconds);
            Assert.Null(appearance.EditBoxStartKeepsDuration); // nothing persisted until clicked
        }
        finally
        {
            CloseWindow(window, vm);
            appearance.ShowUpDownEndTime = oldEnd;
            appearance.EditBoxStartKeepsDuration = oldLink;
        }
    }

    [AvaloniaFact]
    public void Toggle_RebindsShowBox_AndPersistsTheChoice()
    {
        var appearance = Se.Settings.Appearance;
        var oldEnd = appearance.ShowUpDownEndTime;
        var oldLink = appearance.EditBoxStartKeepsDuration;
        appearance.ShowUpDownEndTime = false;
        appearance.EditBoxStartKeepsDuration = null;

        var (window, vm) = CreateMainViewModel();
        try
        {
            var toggle = FindToggle(window);
            Assert.True(toggle.IsChecked);
            var line = vm.Subtitles[1];

            // Unlink: Show moves only the start, the end stays.
            toggle.IsChecked = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(appearance.EditBoxStartKeepsDuration);
            Assert.Equal(TimeSpan.FromMilliseconds(3000), vm.EditBoxStartTimeUpDown!.Value); // rebinding keeps the shown value
            Assert.Equal(5000, line.EndTime.TotalMilliseconds);

            vm.EditBoxStartTimeUpDown.Value = TimeSpan.FromMilliseconds(3400);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3400, line.StartTime.TotalMilliseconds);
            Assert.Equal(5000, line.EndTime.TotalMilliseconds);

            // Link again: Show drags the whole line.
            toggle.IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(appearance.EditBoxStartKeepsDuration);

            vm.EditBoxStartTimeUpDown.Value = TimeSpan.FromMilliseconds(3000);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3000, line.StartTime.TotalMilliseconds);
            Assert.Equal(4600, line.EndTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
            appearance.ShowUpDownEndTime = oldEnd;
            appearance.EditBoxStartKeepsDuration = oldLink;
        }
    }

    [AvaloniaFact]
    public void Nudge_StepsShowByTheUpDownIncrement_ThroughTheBinding()
    {
        var appearance = Se.Settings.Appearance;
        var general = Se.Settings.General;
        var oldEnd = appearance.ShowUpDownEndTime;
        var oldLink = appearance.EditBoxStartKeepsDuration;
        var oldStep = general.TimeCodeUpDownStepMs;
        var oldFrameMode = general.UseFrameMode;
        appearance.ShowUpDownEndTime = true;
        appearance.EditBoxStartKeepsDuration = null; // unlinked, as the end editor is shown
        general.TimeCodeUpDownStepMs = 40;
        general.UseFrameMode = false;

        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = vm.Subtitles[1];
            InitListViewAndEditBox.NudgeTimeCode(vm.EditBoxStartTimeUpDown!, +1);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3040, line.StartTime.TotalMilliseconds);
            Assert.Equal(5000, line.EndTime.TotalMilliseconds);

            InitListViewAndEditBox.NudgeTimeCode(vm.EditBoxEndTimeUpDown!, -1);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(4960, line.EndTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
            appearance.ShowUpDownEndTime = oldEnd;
            appearance.EditBoxStartKeepsDuration = oldLink;
            general.TimeCodeUpDownStepMs = oldStep;
            general.UseFrameMode = oldFrameMode;
        }
    }

    private static ToggleButton FindToggle(Window window)
    {
        return Assert.Single(window.GetLogicalDescendants().OfType<ToggleButton>(),
            t => t.Name == InitListViewAndEditBox.StartDurationLinkToggleName);
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

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Subtitles.Clear();
        for (var i = 0; i < 3; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Line " + i, i * 3000, i * 3000 + 2000), null!) { Number = i + 1 });
        }

        Dispatcher.UIThread.RunJobs();
        vm.SelectedSubtitleIndex = 1;
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[1];
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
        return (window, vm);
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
