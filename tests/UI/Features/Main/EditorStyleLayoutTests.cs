using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// Layout 14 is the editor-style layout: subtitle grid (edit box docked under it) and video
/// player side by side, and a timeline across the bottom where a video row and a subtitle row
/// (<see cref="TimelineTracks"/>) sit on top of the waveform and follow its time axis.
/// </summary>
public class EditorStyleLayoutTests
{
    [AvaloniaFact]
    public void Layout14_PutsGridWithEditBoxAndVideoSideBySide_AboveTheTimeline()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            Assert.Equal(14, InitLayout.MakeLayout(vm.MainView!, vm, 14));
            Dispatcher.UIThread.RunJobs();

            var contentGrid = Assert.IsType<Grid>(vm.ContentGrid.Children[0]);
            var topContent = contentGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == 0);
            var bottomContent = contentGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == 2);
            var columns = Assert.IsType<Grid>(topContent.Child);
            Assert.Equal(3, columns.ColumnDefinitions.Count);
            Assert.Single(columns.Children.OfType<GridSplitter>());

            // RightToLeftHelper locates the edit section by this name too: exactly one must exist.
            var editGrid = Assert.Single(window.GetLogicalDescendants().OfType<Grid>(), g => g.Name == "SubtitleTextEditGrid");
            var videoPlayer = Assert.Single(window.GetLogicalDescendants().OfType<VideoPlayerControl>());

            // The edit box is docked under the grid, in the same column.
            Assert.Equal(0, Grid.GetColumn(ColumnBorder(vm.SubtitleGrid!, columns)));
            Assert.Equal(0, Grid.GetColumn(ColumnBorder(editGrid, columns)));
            Assert.Equal(2, Grid.GetColumn(ColumnBorder(videoPlayer, columns)));

            // The timeline: the track rows directly above the waveform, in one grid.
            var tracks = Assert.Single(window.GetLogicalDescendants().OfType<TimelineTracks>());
            Assert.Contains(bottomContent, tracks.GetLogicalAncestors());
            Assert.Same(vm.AudioVisualizer, tracks.Source);
            var timelineGrid = Assert.IsType<Grid>(tracks.GetLogicalParent());
            Assert.Same(timelineGrid, vm.AudioVisualizer!.GetLogicalParent());
            Assert.Equal(0, Grid.GetRow(tracks));
            Assert.Equal(1, Grid.GetRow(vm.AudioVisualizer));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void OtherLayouts_HaveNoTimelineTracks_AndTheWaveformSurvivesTheSwitch()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 14);
            Dispatcher.UIThread.RunJobs();
            var visualizer = vm.AudioVisualizer;

            // The subtitle row carries the text in layout 14, so the waveform does not repeat it.
            Assert.False(visualizer!.ShowParagraphText);

            InitLayout.MakeLayout(vm.MainView!, vm, 1);
            Dispatcher.UIThread.RunJobs();

            Assert.Empty(window.GetLogicalDescendants().OfType<TimelineTracks>());
            Assert.Same(visualizer, vm.AudioVisualizer);
            Assert.True(visualizer.ShowParagraphText);
            Assert.NotNull(vm.AudioVisualizer!.GetLogicalParent());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void TimelineTracks_HitTestsSubtitleBlocksOnTheWaveformsTimeAxis()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) });
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 2, Text = "Two", StartTime = TimeSpan.FromSeconds(3), EndTime = TimeSpan.FromSeconds(4) });

            InitLayout.MakeLayout(vm.MainView!, vm, 14);
            Dispatcher.UIThread.RunJobs();

            const int sampleRate = 100;
            var visualizer = vm.AudioVisualizer!;
            visualizer.WavePeaks = new WavePeakData2(sampleRate, new WavePeak2[sampleRate * 120]); // longer than the window is wide, so it can scroll
            visualizer.ZoomFactor = 1.0;
            visualizer.SetPosition(0, vm.Subtitles, 0, 0, new List<SubtitleLineViewModel>());
            Dispatcher.UIThread.RunJobs();

            var tracks = window.GetLogicalDescendants().OfType<TimelineTracks>().Single();
            var subtitleRowY = TimelineTracks.VideoRowHeight + TimelineTracks.RowGap + 5;

            // 1 second = 100 pixels at zoom 1
            Assert.Same(vm.Subtitles[0], tracks.HitTestParagraph(new Point(150, subtitleRowY)));
            Assert.Same(vm.Subtitles[1], tracks.HitTestParagraph(new Point(350, subtitleRowY)));
            Assert.Null(tracks.HitTestParagraph(new Point(250, subtitleRowY)));

            // The video row never selects a subtitle.
            Assert.Null(tracks.HitTestParagraph(new Point(150, 5)));

            // Scrolled two seconds in, the same pixel is a different time.
            visualizer.StartPositionSeconds = 2;
            Assert.Same(vm.Subtitles[1], tracks.HitTestParagraph(new Point(150, subtitleRowY)));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MouseWheelOverTheTracks_ScrollsTheWaveform()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 14);
            Dispatcher.UIThread.RunJobs();

            const int sampleRate = 100;
            var visualizer = vm.AudioVisualizer!;
            visualizer.WavePeaks = new WavePeakData2(sampleRate, new WavePeak2[sampleRate * 600]);
            visualizer.StartPositionSeconds = 100;
            Dispatcher.UIThread.RunJobs();

            // The rows sit in the middle of the timeline; without handing the wheel on to the
            // waveform they would be a strip where scrolling silently does nothing.
            var tracks = window.GetLogicalDescendants().OfType<TimelineTracks>().Single();
            var point = tracks.TranslatePoint(new Point(200, 10), window)!.Value;
            window.MouseWheel(point, new Vector(0, -1));
            Dispatcher.UIThread.RunJobs();

            Assert.NotEqual(100, visualizer.StartPositionSeconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingABlockInTheSubtitleRow_MovesTheSubtitle_LikeADragOnTheWaveform()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(4) });
            var tracks = ShowTimeline(window, vm);
            var rowY = TimelineTracks.VideoRowHeight + TimelineTracks.RowGap + 10;

            // 100 pixels per second: the block spans x 200-400, so 300 is its middle.
            Drag(window, tracks, new Point(300, rowY), new Point(450, rowY));

            Assert.Equal(3.5, vm.Subtitles[0].StartTime.TotalSeconds, 1);
            Assert.Equal(5.5, vm.Subtitles[0].EndTime.TotalSeconds, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingABlocksEdgeInTheSubtitleRow_ResizesTheSubtitle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(4) });
            var tracks = ShowTimeline(window, vm);
            var rowY = TimelineTracks.VideoRowHeight + TimelineTracks.RowGap + 10;

            Drag(window, tracks, new Point(399, rowY), new Point(500, rowY));

            Assert.Equal(2.0, vm.Subtitles[0].StartTime.TotalSeconds, 1);
            Assert.Equal(5.0, vm.Subtitles[0].EndTime.TotalSeconds, 1);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void GroupedByActor_EachActorGetsARow_AndADragPicksTheSubtitleOfItsRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            // Two speakers talking over each other: the same stretch of time, different rows.
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "Anna", Actor = "Anna", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(4) });
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 2, Text = "Bob", Actor = "Bob", StartTime = TimeSpan.FromSeconds(2.5), EndTime = TimeSpan.FromSeconds(4.5) });
            var tracks = ShowTimeline(window, vm);

            tracks.Grouping = TimelineTrackGrouping.Actor;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, tracks.TrackCount);
            Assert.Equal("Anna", tracks.GetTrackLabel(0));
            Assert.Equal("Bob", tracks.GetTrackLabel(1));
            Assert.Equal(TimelineTracks.GetTotalHeight(2), tracks.Height);

            var annaY = TimelineTracks.VideoRowHeight + TimelineTracks.RowGap + 10;
            var bobY = annaY + TimelineTracks.SubtitleRowHeight + TimelineTracks.RowGap;
            Assert.Same(vm.Subtitles[0], tracks.HitTestParagraph(new Point(320, annaY)));
            Assert.Same(vm.Subtitles[1], tracks.HitTestParagraph(new Point(320, bobY)));

            // x 320 is inside both blocks; the row decides.
            Drag(window, tracks, new Point(320, bobY), new Point(420, bobY));

            Assert.Equal(2.0, vm.Subtitles[0].StartTime.TotalSeconds, 1);
            Assert.Equal(3.5, vm.Subtitles[1].StartTime.TotalSeconds, 1);

            // Leaving the rows hands hit testing back to the waveform unfiltered.
            window.MouseMove(new Point(2, 2));
            Dispatcher.UIThread.RunJobs();
            Assert.Null(vm.AudioVisualizer!.HitTestFilter);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ALoadedOriginal_GetsARowOfItsOwn()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "Hej", OriginalText = "Hello", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(4) });
            var tracks = ShowTimeline(window, vm);
            Assert.Equal(1, tracks.TrackCount);

            vm.ShowColumnOriginalText = true;
            tracks.UpdateTracks(); // what the timer does twice a second
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, tracks.TrackCount);
            Assert.Equal(Se.Language.General.Text, tracks.GetTrackLabel(0));
            Assert.Equal(Se.Language.General.OriginalText, tracks.GetTrackLabel(1));

            // The original row holds a block for every subtitle, and dragging it moves the line.
            var originalY = TimelineTracks.VideoRowHeight + TimelineTracks.RowGap + TimelineTracks.SubtitleRowHeight + TimelineTracks.RowGap + 10;
            Assert.Same(vm.Subtitles[0], tracks.HitTestParagraph(new Point(300, originalY)));
            Drag(window, tracks, new Point(300, originalY), new Point(400, originalY));
            Assert.Equal(3.0, vm.Subtitles[0].StartTime.TotalSeconds, 1);

            vm.ShowColumnOriginalText = false;
            tracks.UpdateTracks();
            Assert.Equal(1, tracks.TrackCount);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void RowGrouping_IsOfferedOnTheToolbarAndInTheContextMenu_AndTheTwoStayInStep()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var tracks = ShowTimeline(window, vm);
            var title = Se.Language.Waveform.TimelineGroupTracksBy;

            // A translucent icon on top of the filmstrip was as good as invisible; the choice
            // lives on the waveform toolbar, and in the context menu for a hidden toolbar.
            var button = Assert.Single(window.GetLogicalDescendants().OfType<Button>(),
                b => Avalonia.Automation.AutomationProperties.GetName(b) == title);
            var toolbarItems = Assert.IsType<MenuFlyout>(button.Flyout).Items.OfType<MenuItem>().ToList();
            var contextMenu = Assert.Single(vm.AudioVisualizer!.MenuFlyout.Items.OfType<MenuItem>(), m => Equals(m.Header, title));
            var contextItems = contextMenu.Items.OfType<MenuItem>().ToList();
            Assert.Equal(4, toolbarItems.Count);
            Assert.Equal(4, contextItems.Count);
            Assert.True(toolbarItems[0].IsChecked); // None

            var actor = toolbarItems.Single(i => Equals(i.Header, Se.Language.General.Actor));
            actor.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            Assert.Equal(TimelineTrackGrouping.Actor, tracks.Grouping);
            Assert.Equal("Actor", Se.Settings.Waveform.TimelineTrackGrouping);
            Assert.True(contextItems.Single(i => Equals(i.Header, Se.Language.General.Actor)).IsChecked);
            Assert.False(contextItems[0].IsChecked);

            // Other layouts share the waveform and its menu, but have no rows to group.
            InitLayout.MakeLayout(vm.MainView!, vm, 1);
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(vm.AudioVisualizer.MenuFlyout.Items.OfType<MenuItem>(), m => Equals(m.Header, title));
            Assert.DoesNotContain(window.GetLogicalDescendants().OfType<Button>(),
                b => Avalonia.Automation.AutomationProperties.GetName(b) == title);
        }
        finally
        {
            Se.Settings.Waveform.TimelineTrackGrouping = "None";
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void TheGroupingButton_IsAConfigurableToolbarItem_TheContextMenuStaysWhenItIsHidden()
    {
        var (window, vm) = CreateMainViewModel();
        var item = Se.Settings.Waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.TimelineTrackGrouping);
        try
        {
            item.IsVisible = false;
            ShowTimeline(window, vm);
            var title = Se.Language.Waveform.TimelineGroupTracksBy;

            Assert.DoesNotContain(window.GetLogicalDescendants().OfType<Button>(),
                b => Avalonia.Automation.AutomationProperties.GetName(b) == title);
            Assert.Contains(vm.AudioVisualizer!.MenuFlyout.Items.OfType<MenuItem>(), m => Equals(m.Header, title));
        }
        finally
        {
            item.IsVisible = true;
            CloseWindow(window, vm);
        }
    }

    [Fact]
    public void EnsureAllToolbarItems_AddsTheGroupingButtonVisible_ToOlderSettings()
    {
        // A settings file from before the item existed. It only renders in layout 14, so like
        // the audio-track picker it may default to visible without changing anyone's toolbar.
        var waveform = new SeWaveform();
        waveform.ToolbarItems.RemoveAll(p => p.Type == SeWaveformToolbarItemType.TimelineTrackGrouping);

        waveform.EnsureAllToolbarItems();

        var added = Assert.Single(waveform.ToolbarItems, p => p.Type == SeWaveformToolbarItemType.TimelineTrackGrouping);
        Assert.True(added.IsVisible);
        Assert.Equal(155, added.SortOrder);
    }

    [AvaloniaFact]
    public void TheThumbnailRow_IsSwitchedFromTheContextMenu_AndTheSubtitleRowsMoveUp()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(4) });
            var tracks = ShowTimeline(window, vm);
            var contentGrid = Assert.IsType<Grid>(vm.ContentGrid.Children[0]);
            var timelineHeight = contentGrid.RowDefinitions[2].Height.Value;
            Assert.True(tracks.ShowVideoRow);

            var toggle = Assert.Single(vm.AudioVisualizer!.MenuFlyout.Items.OfType<MenuItem>(),
                m => Equals(m.Header, Se.Language.Waveform.TimelineShowThumbnails));
            Assert.True(toggle.IsChecked);
            toggle.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.False(tracks.ShowVideoRow);
            Assert.False(toggle.IsChecked);
            Assert.False(Se.Settings.Waveform.TimelineShowThumbnails);
            Assert.Equal(TimelineTracks.GetTotalHeight(1, showVideoRow: false), tracks.Height);

            // The timeline gives the row's height back; the waveform keeps its own.
            Assert.Equal(timelineHeight - (TimelineTracks.VideoRowHeight + TimelineTracks.RowGap), contentGrid.RowDefinitions[2].Height.Value);

            // The subtitle row is now at the very top, and still draggable.
            Assert.Same(vm.Subtitles[0], tracks.HitTestParagraph(new Point(300, 10)));
            Drag(window, tracks, new Point(300, 10), new Point(400, 10));
            Assert.Equal(3.0, vm.Subtitles[0].StartTime.TotalSeconds, 1);
        }
        finally
        {
            Se.Settings.Waveform.TimelineShowThumbnails = true;
            CloseWindow(window, vm);
        }
    }

    [Fact]
    public void ManyRows_AreCompact_SoTheTimelineStaysATimeline()
    {
        Assert.Equal(TimelineTracks.TotalHeight, TimelineTracks.GetTotalHeight(1));
        Assert.Equal(TimelineTracks.VideoRowHeight + 2 + 2 * 32, TimelineTracks.GetTotalHeight(2));
        Assert.Equal(TimelineTracks.VideoRowHeight + 2 + 4 * 26, TimelineTracks.GetTotalHeight(4));
    }

    private static TimelineTracks ShowTimeline(Window window, MainViewModel vm)
    {
        InitLayout.MakeLayout(vm.MainView!, vm, 14);
        Dispatcher.UIThread.RunJobs();

        const int sampleRate = 100;
        var visualizer = vm.AudioVisualizer!;
        visualizer.WavePeaks = new WavePeakData2(sampleRate, new WavePeak2[sampleRate * 120]);
        visualizer.ZoomFactor = 1.0;
        visualizer.SetPosition(0, vm.Subtitles, 0, -1, new List<SubtitleLineViewModel>());
        Dispatcher.UIThread.RunJobs();

        return window.GetLogicalDescendants().OfType<TimelineTracks>().Single();
    }

    private static void Drag(Window window, TimelineTracks tracks, Point from, Point to)
    {
        var start = tracks.TranslatePoint(from, window)!.Value;
        var end = tracks.TranslatePoint(to, window)!.Value;
        window.MouseMove(start);
        window.MouseDown(start, Avalonia.Input.MouseButton.Left);
        for (var i = 1; i <= 5; i++)
        {
            window.MouseMove(new Point(start.X + (end.X - start.X) * i / 5, start.Y));
            Dispatcher.UIThread.RunJobs();
        }

        window.MouseUp(end, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [Theory]
    [InlineData(126, 1)]      // default zoom: a 96 px frame needs 0.76 s, the next step up is 1 s
    [InlineData(1000, 0.1)]   // zoomed far in
    [InlineData(96, 1)]       // a frame fits exactly
    [InlineData(10, 10)]      // 9.6 s needed
    [InlineData(0.001, 3600)] // beyond the last step: stay on it
    [InlineData(0, 3600)]
    public void FilmstripStep_IsTheSmallestFixedStepThatFitsAFrame(double pixelsPerSecond, double expectedStep)
    {
        Assert.Equal(expectedStep, TimelineTracks.GetFilmstripStepSeconds(pixelsPerSecond));
    }

    [Fact]
    public void FilmstripFrameTime_IsTheTileStart_ExceptForTheOftenBlackVeryFirstFrame()
    {
        Assert.Equal(500, TimelineTracks.GetFilmstripFrameMilliseconds(0, 1));
        Assert.Equal(1000, TimelineTracks.GetFilmstripFrameMilliseconds(0, 60));
        Assert.Equal(50, TimelineTracks.GetFilmstripFrameMilliseconds(0, 0.1));
        Assert.Equal(3000, TimelineTracks.GetFilmstripFrameMilliseconds(3, 1));
        Assert.Equal(750, TimelineTracks.GetFilmstripFrameMilliseconds(3, 0.25));
    }

    private static Border ColumnBorder(Control control, Grid columns)
    {
        return control.GetLogicalAncestors().OfType<Border>().First(b => ReferenceEquals(b.GetLogicalParent(), columns));
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
