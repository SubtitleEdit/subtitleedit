using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Sync;
using System.Linq;

namespace UITests.Features.Sync;

/// <summary>
/// The video-over-waveform panel the sync dialogs share (issue #14414): a drag handle between the
/// player and the waveform, rows that collapse when either side is hidden, and a remembered height.
/// </summary>
public class VideoWaveformSplitGridTests
{
    private static VideoWaveformSplitGrid Make(double savedHeight = 80)
        => new(new Border(), new AudioVisualizer(), savedHeight);

    private static GridSplitter HandleOf(Grid grid) => grid.Children.OfType<GridSplitter>().Single();
    private static RowDefinition VideoRow(Grid grid) => grid.RowDefinitions[0];
    private static RowDefinition WaveformRow(Grid grid) => grid.RowDefinitions[2];

    /// <summary>What a GridSplitter drag leaves behind: the row resized and the drag events raised.</summary>
    private static void SimulateDrag(VideoWaveformSplitGrid grid, double newHeight)
    {
        WaveformRow(grid).Height = new GridLength(newHeight, GridUnitType.Pixel);
        HandleOf(grid).RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragDeltaEvent });
        HandleOf(grid).RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
    }

    [AvaloniaFact]
    public void VideoAndWaveform_ShowsTheHandleOverAPixelSizedWaveformRow()
    {
        var grid = Make(120);
        grid.IsWaveformVisible = true;

        Assert.True(HandleOf(grid).IsVisible);
        Assert.True(VideoRow(grid).Height.IsStar);
        Assert.True(WaveformRow(grid).Height.IsAbsolute);
        Assert.Equal(120, WaveformRow(grid).Height.Value);
        Assert.Equal(120, grid.WaveformHeight);
    }

    [AvaloniaFact]
    public void WithoutWaveform_CollapsesTheRowAndHidesTheHandle()
    {
        // A pixel row keeps its height when its child is merely hidden, so the row itself has to go.
        var grid = Make();

        Assert.False(HandleOf(grid).IsVisible);
        Assert.True(WaveformRow(grid).Height.IsAbsolute);
        Assert.Equal(0, WaveformRow(grid).Height.Value);
        Assert.Equal(0, WaveformRow(grid).MinHeight);
    }

    [AvaloniaFact]
    public void WithoutVideo_TheWaveformTakesTheSpaceAndThereIsNoHandle()
    {
        var grid = Make();
        grid.IsVideoVisible = false;
        grid.IsWaveformVisible = true;

        Assert.False(HandleOf(grid).IsVisible);
        Assert.Equal(0, VideoRow(grid).Height.Value);
        Assert.Equal(0, VideoRow(grid).MinHeight); // a MinHeight would prop the collapsed row open
        Assert.True(WaveformRow(grid).Height.IsStar);
    }

    [AvaloniaFact]
    public void Drag_RemembersTheNewHeightAndTellsListeners()
    {
        var grid = Make(80);
        grid.IsWaveformVisible = true;
        double? seen = null;
        grid.WaveformHeightChanged += h => seen = h;

        SimulateDrag(grid, 150);

        Assert.Equal(150, grid.WaveformHeight);
        Assert.Equal(150, seen);
    }

    [AvaloniaFact]
    public void HidingAndShowingTheWaveform_KeepsTheDraggedHeight()
    {
        var grid = Make(80);
        grid.IsWaveformVisible = true;
        SimulateDrag(grid, 150);

        grid.IsWaveformVisible = false;
        Assert.Equal(0, WaveformRow(grid).Height.Value);
        Assert.Equal(150, grid.WaveformHeight); // the collapsed row is not what gets saved

        grid.IsWaveformVisible = true;
        Assert.Equal(150, WaveformRow(grid).Height.Value);
    }

    [AvaloniaFact]
    public void SetWaveformHeight_FollowsAnotherPaneWithoutEchoingBack()
    {
        // Visual sync has two panes side by side; a drag on one moves the other, and following
        // must not raise the event again or the two would ping-pong.
        var left = Make(80);
        var right = Make(80);
        left.IsWaveformVisible = true;
        right.IsWaveformVisible = true;
        var rightRaised = 0;
        left.WaveformHeightChanged += right.SetWaveformHeight;
        right.WaveformHeightChanged += left.SetWaveformHeight;
        right.WaveformHeightChanged += _ => rightRaised++;

        SimulateDrag(left, 130);

        Assert.Equal(130, WaveformRow(right).Height.Value);
        Assert.Equal(130, right.WaveformHeight);
        Assert.Equal(0, rightRaised);
    }

    [AvaloniaTheory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1)]
    [InlineData(5)]
    public void BadSavedHeight_FallsBackToTheDefault(double saved)
    {
        var grid = Make(saved);
        grid.IsWaveformVisible = true;

        Assert.Equal(VideoWaveformSplitGrid.DefaultWaveformHeight, grid.WaveformHeight);
        Assert.Equal(VideoWaveformSplitGrid.DefaultWaveformHeight, WaveformRow(grid).Height.Value);
    }
}
