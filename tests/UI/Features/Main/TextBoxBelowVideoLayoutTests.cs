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
/// Layouts 12 and 13 put the edit box directly under the video player instead of under the
/// subtitle grid (issue #14812). Layout 15 is the old layout 12 (grid and text box only).
/// </summary>
public class TextBoxBelowVideoLayoutTests
{
    [AvaloniaTheory]
    [InlineData(12, 0, 2)]
    [InlineData(13, 2, 0)]
    public void Layout12And13_PutTheEditBoxUnderTheVideoPlayer(int layoutNumber, int gridColumn, int videoColumn)
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            Assert.Equal(layoutNumber, InitLayout.MakeLayout(vm.MainView!, vm, layoutNumber));
            Dispatcher.UIThread.RunJobs();

            var contentGrid = Assert.IsType<Grid>(vm.ContentGrid.Children[0]);
            var topContent = contentGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == 0);
            var bottomContent = contentGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == 2);
            var editGrid = FindEditGrid(window);

            // The waveform is alone across the bottom.
            Assert.NotNull(vm.AudioVisualizer);
            Assert.Contains(bottomContent, vm.AudioVisualizer!.GetLogicalAncestors());
            Assert.DoesNotContain(bottomContent, editGrid.GetLogicalAncestors());

            // The subtitle grid and the video player sit in opposite columns of the top section.
            Assert.Contains(topContent, editGrid.GetLogicalAncestors());
            var gridBorder = ColumnBorder(vm.SubtitleGrid!);
            var videoBorder = ColumnBorder(vm.VideoPlayerControl!);
            Assert.Equal(gridColumn, Grid.GetColumn(gridBorder));
            Assert.Equal(videoColumn, Grid.GetColumn(videoBorder));

            // The edit box shares the video player's column, not the grid's.
            Assert.Contains(videoBorder, editGrid.GetLogicalAncestors());
            Assert.DoesNotContain(gridBorder, editGrid.GetLogicalAncestors());

            // The video/edit box boundary is resizable, and the min-height floors keep both
            // sides recoverable after a drag.
            var editSection = Assert.IsType<Grid>(editGrid.GetLogicalParent());
            var videoGrid = Assert.IsType<Grid>(editSection.GetLogicalParent());
            Assert.Contains(videoGrid.Children, c => c is GridSplitter);
            Assert.True(videoGrid.RowDefinitions[0].MinHeight > 0);
            Assert.True(videoGrid.RowDefinitions[1].MinHeight > 0);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Layout15_IsTheNoVideoLayout()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            Assert.Equal(15, InitLayout.LayoutWithoutVideo);
            Assert.True(InitLayout.LayoutHasNoVideo(15));
            Assert.False(InitLayout.LayoutHasNoVideo(12));
            Assert.False(InitLayout.LayoutHasNoVideo(13));
            Assert.False(InitLayout.LayoutHasNoVideo(14));

            Assert.Equal(15, InitLayout.MakeLayout(vm.MainView!, vm, 15));
            Dispatcher.UIThread.RunJobs();

            Assert.Null(vm.AudioVisualizer?.GetLogicalParent());
            var editGrid = FindEditGrid(window);
            Assert.Contains(vm.ContentGrid, editGrid.GetLogicalAncestors());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [Fact]
    public void MigrateLayoutNumber_MovesOldNoVideoLayoutToTheEnd_Once()
    {
        // From before any migration: 12 was "no video", which is 15 today (via 14).
        var general = new SeGeneral { LayoutNumber = 12 };
        Se.MigrateLayoutNumber(general);
        Assert.Equal(15, general.LayoutNumber);
        Assert.Equal(Se.CurrentLayoutMigrationVersion, general.LayoutMigrationVersion);

        // A 12 chosen after the migration is the new text-box-below-video layout and stays.
        general.LayoutNumber = 12;
        Se.MigrateLayoutNumber(general);
        Assert.Equal(12, general.LayoutNumber);
    }

    [Fact]
    public void MigrateLayoutNumber_MovesNoVideoLayout14To15_Once()
    {
        // Version 1 settings: 14 was "no video" until the editor-style layout took the number.
        var general = new SeGeneral { LayoutNumber = 14, LayoutMigrationVersion = 1 };
        Se.MigrateLayoutNumber(general);
        Assert.Equal(15, general.LayoutNumber);
        Assert.Equal(Se.CurrentLayoutMigrationVersion, general.LayoutMigrationVersion);

        // A 14 chosen after the migration is the editor-style layout and stays.
        general.LayoutNumber = 14;
        Se.MigrateLayoutNumber(general);
        Assert.Equal(14, general.LayoutNumber);

        // The text-box-below-video layouts of version 1 are not the old "no video" 12.
        general = new SeGeneral { LayoutNumber = 12, LayoutMigrationVersion = 1 };
        Se.MigrateLayoutNumber(general);
        Assert.Equal(12, general.LayoutNumber);
    }

    [Fact]
    public void MigrateLayoutNumber_LeavesOtherLayoutsAlone()
    {
        var general = new SeGeneral { LayoutNumber = 3 };
        Se.MigrateLayoutNumber(general);
        Assert.Equal(3, general.LayoutNumber);
    }

    private static Border ColumnBorder(Control control)
    {
        return control.GetLogicalAncestors().OfType<Border>()
            .First(b => b.GetLogicalParent() is Grid g && g.ColumnDefinitions.Count == 3);
    }

    private static Grid FindEditGrid(Window window)
    {
        return Assert.Single(window.GetLogicalDescendants().OfType<Grid>(), g => g.Name == "SubtitleTextEditGrid");
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
