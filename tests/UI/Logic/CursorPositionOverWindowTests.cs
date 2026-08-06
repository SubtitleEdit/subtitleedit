using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The video windows poll the desktop cursor position (the mpv native child window swallows
/// Avalonia pointer events), so activity must be hit-tested against the window - otherwise
/// mouse movement in any other app or on another monitor shows the video controls (issue #13207).
/// </summary>
public class CursorPositionOverWindowTests : IDisposable
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

    private Window ShowWindow()
    {
        var window = new Window { Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        return window;
    }

    [AvaloniaFact]
    public void CursorInsideWindowCounts()
    {
        var window = ShowWindow();

        var inside = window.PointToScreen(new Point(window.ClientSize.Width / 2, window.ClientSize.Height / 2));

        Assert.True(CursorPositionHelper.IsCursorOverWindow(window, (inside.X, inside.Y)));
    }

    [AvaloniaTheory]
    [InlineData(-50, 0.5)] // left of the window
    [InlineData(0.5, -50)] // above the window
    [InlineData(50, 0.5)] // right of the window
    [InlineData(0.5, 50)] // below the window
    public void CursorOutsideWindowDoesNotCount(double x, double y)
    {
        var window = ShowWindow();

        // Fractions are a point inside that axis, whole numbers an offset outside the client area.
        var clientX = x < 1 && x > 0 ? window.ClientSize.Width * x : x < 0 ? x : window.ClientSize.Width + x;
        var clientY = y < 1 && y > 0 ? window.ClientSize.Height * y : y < 0 ? y : window.ClientSize.Height + y;
        var outside = window.PointToScreen(new Point(clientX, clientY));

        Assert.False(CursorPositionHelper.IsCursorOverWindow(window, (outside.X, outside.Y)));
    }

    [AvaloniaFact]
    public void MinimizedWindowNeverCounts()
    {
        var window = ShowWindow();
        var inside = window.PointToScreen(new Point(window.ClientSize.Width / 2, window.ClientSize.Height / 2));

        window.WindowState = WindowState.Minimized;

        Assert.False(CursorPositionHelper.IsCursorOverWindow(window, (inside.X, inside.Y)));
    }

    [AvaloniaFact]
    public void NoWindowNeverCounts()
    {
        Assert.False(CursorPositionHelper.IsCursorOverWindow(null, (0, 0)));
    }
}
