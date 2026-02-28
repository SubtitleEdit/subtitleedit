using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public static class WindowExtensions
{
    public static bool IsClosing(this Window? window)
    {
        return window == null || !window.IsVisible || window.PlatformImpl == null;
    }
}
