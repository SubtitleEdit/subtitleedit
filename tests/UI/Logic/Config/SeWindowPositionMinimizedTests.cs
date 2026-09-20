using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

/// <summary>
/// Windows parks a minimized window at (-32000, -32000) and reports that as its position.
/// Quitting with a minimized window - common for the undocked windows, whose close button
/// minimizes - must save where the window was before, not the parking spot (#15106).
/// </summary>
public class SeWindowPositionMinimizedTests
{
    [Theory]
    [InlineData(-32000, -32000, true)]
    [InlineData(-32000, 100, true)]
    [InlineData(100, -32000, true)]
    [InlineData(-3840, -1080, false)] // a monitor left of / above the primary one
    [InlineData(0, 0, false)]
    public void IsMinimizedPosition_OnlyMatchesTheParkingSpot(int x, int y, bool expected)
    {
        Assert.Equal(expected, SeWindowPosition.IsMinimizedPosition(x, y));
    }

    [Fact]
    public void TryApplyStateBeforeMinimize_UsesTheRememberedPositionStateAndScreen()
    {
        var state = new SeWindowPosition("W", false, false, -32000, -32000, 800, 300);

        var applied = state.TryApplyStateBeforeMinimize(
            new PixelPoint(1950, 40), WindowState.Maximized, new PixelRect(1920, 0, 2560, 1440));

        Assert.True(applied);
        Assert.Equal(1950, state.X);
        Assert.Equal(40, state.Y);
        Assert.True(state.IsMaximized);
        Assert.False(state.IsFullScreen);
        Assert.Equal(800, state.Width);
        Assert.Equal(300, state.Height);
        Assert.Equal(1920, state.ScreenX);
        Assert.Equal(2560, state.ScreenWidth);
        Assert.Equal(1440, state.ScreenHeight);
    }

    [Fact]
    public void TryApplyStateBeforeMinimize_WithoutAUsablePosition_ChangesNothing()
    {
        var state = new SeWindowPosition("W", false, false, -32000, -32000, 800, 300);

        Assert.False(state.TryApplyStateBeforeMinimize(null, null, null));
        Assert.False(state.TryApplyStateBeforeMinimize(new PixelPoint(-32000, -32000), WindowState.Normal, null));
        Assert.Equal(-32000, state.X);
        Assert.Equal(-32000, state.Y);
    }

    [AvaloniaFact]
    public void SaveWindowPosition_ForAMinimizedWindow_SavesWhereItWasBefore()
    {
        var rememberBefore = Se.Settings.General.RememberPositionAndSize;
        var positionsBefore = Se.Settings.General.WindowPositions.ToList();
        var window = new Window { Name = "MinimizedPositionTestWindow", Width = 640, Height = 240 };
        try
        {
            Se.Settings.General.RememberPositionAndSize = true;
            Se.Settings.General.WindowPositions.RemoveAll(p => p.WindowName == window.Name);

            window.Show();
            UiUtil.RestoreWindowPosition(window); // what every position-saving window does when loaded
            window.Position = new PixelPoint(120, 80);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            window.WindowState = WindowState.Minimized;
            window.Position = new PixelPoint(-32000, -32000); // what Windows reports from here on
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            UiUtil.SaveWindowPosition(window);

            var saved = Assert.Single(Se.Settings.General.WindowPositions, p => p.WindowName == window.Name);
            Assert.Equal(120, saved.X);
            Assert.Equal(80, saved.Y);
            Assert.False(saved.IsMaximized);
        }
        finally
        {
            window.Close();
            Se.Settings.General.RememberPositionAndSize = rememberBefore;
            Se.Settings.General.WindowPositions.Clear();
            Se.Settings.General.WindowPositions.AddRange(positionsBefore);
        }
    }

    [AvaloniaFact]
    public void SaveWindowPosition_ForAMinimizedWindowWithNothingRemembered_KeepsTheEarlierEntry()
    {
        var rememberBefore = Se.Settings.General.RememberPositionAndSize;
        var positionsBefore = Se.Settings.General.WindowPositions.ToList();
        var window = new Window { Name = "MinimizedPositionUntrackedTestWindow", Width = 640, Height = 240 };
        try
        {
            Se.Settings.General.RememberPositionAndSize = true;
            Se.Settings.General.WindowPositions.RemoveAll(p => p.WindowName == window.Name);
            Se.Settings.General.WindowPositions.Add(new SeWindowPosition(window.Name, false, false, 300, 200, 640, 240));

            window.Show();
            window.WindowState = WindowState.Minimized; // never went through RestoreWindowPosition
            window.Position = new PixelPoint(-32000, -32000);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            UiUtil.SaveWindowPosition(window);

            var saved = Assert.Single(Se.Settings.General.WindowPositions, p => p.WindowName == window.Name);
            Assert.Equal(300, saved.X);
            Assert.Equal(200, saved.Y);
        }
        finally
        {
            window.Close();
            Se.Settings.General.RememberPositionAndSize = rememberBefore;
            Se.Settings.General.WindowPositions.Clear();
            Se.Settings.General.WindowPositions.AddRange(positionsBefore);
        }
    }
}
