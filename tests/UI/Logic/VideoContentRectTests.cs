using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;

namespace UITests.Logic;

/// <summary>
/// Issue #14328, "the preview stretches the original subtitle image" / "it should scale based on
/// the window size, retaining its original aspect ratio".
///
/// The binary (Blu-ray sup) edit window placed its green content border - and the subtitle
/// overlay scaled against it - by subtracting a hard-coded 55 px from the player control's height
/// for the controls row. That row is Auto-sized, so its real height moves with the UI scale and
/// the platform, and the overlay was sized and positioned against a rectangle that did not match
/// where the picture actually is. The burn-in logo preview already did this correctly off the
/// measured surface; both now share one calculation.
/// </summary>
public class VideoContentRectTests
{
    private const double Tolerance = 1.0; // the result is rounded to whole pixels

    // Same aspect: the picture fills the surface, no bars.
    [Fact]
    public void MatchingAspect_FillsTheSurface()
    {
        var rect = VideoContentRect.Calculate(1600, 900, 1920, 1080);

        Assert.NotNull(rect);
        Assert.Equal(0, rect!.Value.X);
        Assert.Equal(0, rect.Value.Y);
        Assert.Equal(1600, rect.Value.Width, Tolerance);
        Assert.Equal(900, rect.Value.Height, Tolerance);
    }

    // Surface wider than the picture: pillarboxed, centred horizontally, full height.
    [Fact]
    public void WideSurface_PillarboxesAndCentres()
    {
        var rect = VideoContentRect.Calculate(2000, 900, 1920, 1080);

        Assert.NotNull(rect);
        Assert.Equal(900, rect!.Value.Height, Tolerance);
        Assert.Equal(1600, rect.Value.Width, Tolerance);
        Assert.Equal(200, rect.Value.X, Tolerance);   // (2000 - 1600) / 2
        Assert.Equal(0, rect.Value.Y);
    }

    // Surface taller than the picture: letterboxed, centred vertically, full width.
    [Fact]
    public void TallSurface_LetterboxesAndCentres()
    {
        var rect = VideoContentRect.Calculate(1600, 1200, 1920, 1080);

        Assert.NotNull(rect);
        Assert.Equal(1600, rect!.Value.Width, Tolerance);
        Assert.Equal(900, rect.Value.Height, Tolerance);
        Assert.Equal(0, rect.Value.X);
        Assert.Equal(150, rect.Value.Y, Tolerance);   // (1200 - 900) / 2
    }

    // The reporter's actual ask: whatever the window size, the rectangle keeps the video's
    // aspect ratio, so a subtitle scaled against it is scaled equally in x and y.
    [Theory]
    [InlineData(1600, 900)]
    [InlineData(2000, 900)]
    [InlineData(1600, 1200)]
    [InlineData(640, 480)]
    [InlineData(3840, 1000)]
    [InlineData(300, 2000)]
    public void AnySurfaceSize_KeepsTheVideoAspectRatio(double surfaceWidth, double surfaceHeight)
    {
        const int videoWidth = 1920;
        const int videoHeight = 1080;

        var rect = VideoContentRect.Calculate(surfaceWidth, surfaceHeight, videoWidth, videoHeight);
        Assert.NotNull(rect);

        // Equal scale factors in both axes is exactly "retains its original aspect ratio" -
        // an overlay uses rect/screen as its x and y scale.
        var scaleX = rect!.Value.Width / videoWidth;
        var scaleY = rect.Value.Height / videoHeight;
        Assert.Equal(scaleX, scaleY, 2);

        // And it always fits inside the surface.
        Assert.True(rect.Value.Width <= surfaceWidth + Tolerance);
        Assert.True(rect.Value.Height <= surfaceHeight + Tolerance);
    }

    [Theory]
    [InlineData(0, 900, 1920, 1080)]
    [InlineData(1600, 0, 1920, 1080)]
    [InlineData(1600, 900, 0, 1080)]
    [InlineData(1600, 900, 1920, 0)]
    [InlineData(-10, 900, 1920, 1080)]
    public void NoSizeYet_ReturnsNull(double surfaceWidth, double surfaceHeight, double videoWidth, double videoHeight)
    {
        Assert.Null(VideoContentRect.Calculate(surfaceWidth, surfaceHeight, videoWidth, videoHeight));
    }

    // The bug was reading the control's own height and guessing what the controls row took.
    // Both callers must measure the surface instead.
    [Theory]
    [InlineData("src/ui/Features/Shared/BinaryEdit/BinaryEditViewModel.cs")]
    [InlineData("src/ui/Features/Video/BurnIn/BurnInLogoViewModel.cs")]
    public void BothOverlayCallers_MeasureTheSurface(string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

        Assert.Contains("VideoContentRect.Calculate(", source, StringComparison.Ordinal);
        Assert.Contains("VideoPlayerControl.ContentWidth", source, StringComparison.Ordinal);
        Assert.Contains("VideoPlayerControl.ContentHeight", source, StringComparison.Ordinal);
        Assert.DoesNotContain("controlsHeight", source, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }
}
