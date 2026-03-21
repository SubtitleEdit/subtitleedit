using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Static settings for the ASSA Draw feature.
/// </summary>
public static class DrawSettings
{
    public static Color BackgroundColor { get; set; } = Color.FromRgb(40, 40, 40);
    public static Color ScreenSizeColor { get; set; } = Color.FromRgb(100, 100, 255);
    public static Color PointColor { get; set; } = Color.FromRgb(255, 100, 100);
    public static Color PointHelperColor { get; set; } = Color.FromRgb(100, 255, 100);
    public static Color ActiveShapeLineColor { get; set; } = Color.FromRgb(255, 200, 50);
    public static Color ShapeLineColor { get; set; } = Color.FromRgb(200, 200, 200);
    public static Color GridColor { get; set; } = Color.FromRgb(80, 80, 80);
    public static bool ShowGrid { get; set; } = true;
    public static int GridSize { get; set; } = 20;
    public static bool SnapToGrid { get; set; } = false;
}
