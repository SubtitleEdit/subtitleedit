using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWindowPosition
{
    public string WindowName { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsMaximized { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int ScreenX { get; set; }
    public int ScreenY { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }

    public SeWindowPosition()
    {
        WindowName = string.Empty;
        IsFullScreen = false;
        IsMaximized = false;
    }

    public SeWindowPosition(string windowName, bool isFullScreen, bool isMaximized, int x, int y, int width, int height)
    {
        WindowName = windowName;
        IsFullScreen = isFullScreen;
        IsMaximized = isMaximized;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static SeWindowPosition SaveState(Window? window)
    {
        try
        {
            if (window == null || window.Name == null)
            {
                return new SeWindowPosition();
            }

            var screen = window.Screens.ScreenFromWindow(window);

            var state = new SeWindowPosition
            {
                WindowName = window.Name,
                IsFullScreen = window.WindowState == WindowState.FullScreen,
                IsMaximized = window.WindowState == WindowState.Maximized,
                X = window.Position.X,
                Y = window.Position.Y,
                Width = (int)window.Width,
                Height = (int)window.Height,
                ScreenX = screen?.Bounds.X ?? 0,
                ScreenY = screen?.Bounds.Y ?? 0,
                ScreenWidth = screen?.Bounds.Width ?? 0,
                ScreenHeight = screen?.Bounds.Height ?? 0,
            };

            return state;
        }
        catch
        {
            return new SeWindowPosition();
        }
    }
}
