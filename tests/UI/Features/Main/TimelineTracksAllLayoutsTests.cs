using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Layout 14's track rows above the waveform as an option for every layout (#16354): the rows
/// appear on request, and the waveform's fixed-height row grows by their height so the waveform
/// itself keeps its size.
/// </summary>
public class TimelineTracksAllLayoutsTests
{
    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(12)]
    public void SettingOn_AddsTheTrackRowsAndGrowsTheWaveformRow(int layout)
    {
        using var _ = new SettingsScope("General.LayoutNumber", "Waveform.ShowTimelineTracks");
        Se.Settings.General.LayoutNumber = layout;
        Se.Settings.Waveform.ShowTimelineTracks = true;
        var (window, vm) = CreateMainViewModel();
        try
        {
            var tracks = window.GetVisualDescendants().OfType<TimelineTracks>().SingleOrDefault();
            Assert.NotNull(tracks);
            Assert.False(vm.AudioVisualizer!.ShowParagraphText);

            var row = FindWaveformRow(vm);
            if (row != null)
            {
                Assert.Equal(InitLayout.WaveFormHeight + tracks.Height, row.Height.Value, 1);
            }
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void SettingOff_NoTrackRows()
    {
        using var _ = new SettingsScope("General.LayoutNumber", "Waveform.ShowTimelineTracks");
        Se.Settings.General.LayoutNumber = 1;
        Se.Settings.Waveform.ShowTimelineTracks = false;
        var (window, vm) = CreateMainViewModel();
        try
        {
            Assert.Empty(window.GetVisualDescendants().OfType<TimelineTracks>());
            Assert.True(vm.AudioVisualizer!.ShowParagraphText);
            Assert.Equal(InitLayout.WaveFormHeight, FindWaveformRow(vm)!.Height.Value, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Toggle_RebuildsTheLayoutWithTheRows()
    {
        using var _ = new SettingsScope("General.LayoutNumber", "Waveform.ShowTimelineTracks");
        Se.Settings.General.LayoutNumber = 1;
        Se.Settings.Waveform.ShowTimelineTracks = false;
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.ToggleWaveformTimelineTracksCommand.Execute(null);
            Settle(window);

            Assert.True(Se.Settings.Waveform.ShowTimelineTracks);
            Assert.Single(window.GetVisualDescendants().OfType<TimelineTracks>());

            vm.ToggleWaveformTimelineTracksCommand.Execute(null);
            Settle(window);

            Assert.False(Se.Settings.Waveform.ShowTimelineTracks);
            Assert.Empty(window.GetVisualDescendants().OfType<TimelineTracks>());
            Assert.Equal(InitLayout.WaveFormHeight, FindWaveformRow(vm)!.Height.Value, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ThumbnailsOff_ShrinksTheRowWithTheTracks()
    {
        using var _ = new SettingsScope("General.LayoutNumber", "Waveform.ShowTimelineTracks", "Waveform.TimelineShowThumbnails");
        Se.Settings.General.LayoutNumber = 1;
        Se.Settings.Waveform.ShowTimelineTracks = true;
        Se.Settings.Waveform.TimelineShowThumbnails = true;
        var (window, vm) = CreateMainViewModel();
        try
        {
            var tracks = window.GetVisualDescendants().OfType<TimelineTracks>().Single();
            var row = FindWaveformRow(vm)!;
            var before = row.Height.Value;

            tracks.ShowVideoRow = false;
            Settle(window);

            Assert.Equal(InitLayout.WaveFormHeight + tracks.Height, row.Height.Value, 1);
            Assert.True(row.Height.Value < before);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>The fixed-height row holding the waveform, if the layout has one.</summary>
    private static RowDefinition? FindWaveformRow(MainViewModel vm)
    {
        Control child = vm.AudioVisualizer!;
        while (child.Parent is Control parent && child != vm.ContentGrid)
        {
            if (parent is Grid grid && grid.RowDefinitions.Count > 0)
            {
                var row = grid.RowDefinitions[Math.Clamp(Grid.GetRow(child), 0, grid.RowDefinitions.Count - 1)];
                if (row.Height.IsAbsolute)
                {
                    return row;
                }
            }

            child = parent;
        }

        return null;
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Settle(window);

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
