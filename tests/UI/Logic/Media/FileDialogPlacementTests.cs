using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Issue #13515: on Windows the native file picker opens anchored to its owner at its remembered
/// size, so a wide picker opened from a dialog away from the left edge spilled onto the next
/// monitor. <see cref="FileDialogPlacement.Fit"/> computes where to move it.
/// </summary>
public class FileDialogPlacementTests
{
    private static FileDialogPlacement.Rect R(int left, int top, int right, int bottom) =>
        new() { Left = left, Top = top, Right = right, Bottom = bottom };

    private static readonly FileDialogPlacement.Rect PrimaryWorkArea = R(0, 0, 1920, 1032);

    [Fact]
    public void Fit_AlreadyInside_ReturnsNull()
    {
        var rect = R(100, 100, 900, 700);

        Assert.Null(FileDialogPlacement.Fit(rect, rect, PrimaryWorkArea));
    }

    [Fact]
    public void Fit_SpillsOverRightEdge_MovesLeftKeepingSize()
    {
        // The reporter's case: ~1875 px wide picker anchored at x=270 on a 1920 px monitor.
        var rect = R(270, 100, 2145, 700);

        var fitted = FileDialogPlacement.Fit(rect, rect, PrimaryWorkArea);

        Assert.NotNull(fitted);
        Assert.Equal(R(45, 100, 1920, 700), fitted.Value);
    }

    [Fact]
    public void Fit_BelowTaskbar_MovesUp()
    {
        var rect = R(100, 800, 900, 1200);

        var fitted = FileDialogPlacement.Fit(rect, rect, PrimaryWorkArea);

        Assert.Equal(R(100, 632, 900, 1032), fitted!.Value);
    }

    [Fact]
    public void Fit_LargerThanWorkArea_ShrinksToWorkArea()
    {
        var rect = R(-50, -20, 2100, 1200);

        var fitted = FileDialogPlacement.Fit(rect, rect, PrimaryWorkArea);

        Assert.Equal(PrimaryWorkArea, fitted!.Value);
    }

    [Fact]
    public void Fit_InvisibleBorders_VisibleFrameLandsFlushWithEdge()
    {
        // Windows 10+: the window rect includes 7 px invisible borders left/right/bottom.
        var frame = R(270, 100, 2145, 700);
        var window = R(263, 100, 2152, 707);

        var fitted = FileDialogPlacement.Fit(window, frame, PrimaryWorkArea);

        Assert.Equal(R(38, 100, 1927, 707), fitted!.Value);
    }

    [Fact]
    public void Fit_SecondaryMonitorToTheRight_UsesItsWorkArea()
    {
        var secondary = R(1920, 0, 3840, 1080);
        var rect = R(3000, 100, 4000, 600);

        var fitted = FileDialogPlacement.Fit(rect, rect, secondary);

        Assert.Equal(R(2840, 100, 3840, 600), fitted!.Value);
    }
}
