namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// How Avalonia puts SE's windows on the screen on Windows (Win32PlatformOptions), read once at
/// startup. Default keeps Avalonia's own choice (WinUI composition over ANGLE/Direct3D 11).
/// </summary>
public enum WindowsRenderingModeType
{
    Default,
    RedirectionSurface,
    LowLatencySwapChain,
    Software,
}
