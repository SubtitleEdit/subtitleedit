using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Where the video picture actually sits inside a <c>VideoPlayerControl</c>'s surface.
///
/// The player letterboxes or pillarboxes the picture to preserve its aspect ratio, so anything
/// drawn on top of the video - the green content border, a burn-in logo, a subtitle overlay -
/// has to be placed against this rectangle rather than against the control's own bounds.
///
/// Shared because the binary (Blu-ray sup) edit window and the burn-in logo preview had grown
/// their own copies of the same arithmetic, and only one of them measured the surface. The
/// binary edit copy subtracted a hard-coded 55 px for the player's controls row, but that row is
/// Auto-sized - its real height moves with the UI scale and the platform - so its overlay was
/// scaled and positioned against the wrong rectangle (#14328).
/// </summary>
public readonly record struct VideoContentRect(double X, double Y, double Width, double Height)
{
    /// <summary>
    /// The picture's rectangle within a surface of <paramref name="surfaceWidth"/> x
    /// <paramref name="surfaceHeight"/>, for a video of <paramref name="videoWidth"/> x
    /// <paramref name="videoHeight"/>. Null when any input is non-positive - there is nothing
    /// meaningful to place against a surface or a video that has no size yet.
    /// </summary>
    public static VideoContentRect? Calculate(double surfaceWidth, double surfaceHeight, double videoWidth, double videoHeight)
    {
        if (surfaceWidth <= 0 || surfaceHeight <= 0 || videoWidth <= 0 || videoHeight <= 0)
        {
            return null;
        }

        var videoAspect = videoWidth / videoHeight;
        var surfaceAspect = surfaceWidth / surfaceHeight;

        double width, height, x, y;
        if (surfaceAspect > videoAspect)
        {
            // Surface is wider than the picture - pillarboxed, full height.
            height = surfaceHeight;
            width = height * videoAspect;
            x = (surfaceWidth - width) / 2;
            y = 0;
        }
        else
        {
            // Surface is taller than the picture - letterboxed, full width.
            width = surfaceWidth;
            height = width / videoAspect;
            x = 0;
            y = (surfaceHeight - height) / 2;
        }

        // Rounded to avoid sub-pixel rendering seams between the border and the picture.
        return new VideoContentRect(Math.Round(x), Math.Round(y), Math.Round(width), Math.Round(height));
    }
}
