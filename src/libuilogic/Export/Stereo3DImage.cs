using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Subtitles for frame-packed 3D video - SE4's "3D" export option. A half side-by-side or half
/// top/bottom frame holds one squeezed view per eye, so a flat subtitle would show up in one
/// eye only, cut in half. This draws the subtitle once in each view, squeezed the same way, and
/// moves the two copies apart by the depth so the subtitle stands out of (or behind) the screen.
/// </summary>
public static class Stereo3DImage
{
    public const int MinDepth = -100;
    public const int MaxDepth = 100;

    /// <summary>
    /// D-Cinema has no packed frame to draw into - stereoscopic cinema places a subtitle with its
    /// Z-position, which those handlers write from <see cref="ImageParameter.Depth3D"/>.
    /// </summary>
    public static bool IsModeSupported(ExportImageType exportImageType)
    {
        return exportImageType != ExportImageType.DCinemaPng;
    }

    /// <summary>
    /// Replaces <see cref="ImageParameter.Bitmap"/> with the 3D image for
    /// <see cref="ImageParameter.Mode3D"/> and points <see cref="ImageParameter.OverridePosition"/>
    /// at its top left corner, so every handler puts it where it belongs. Call it after
    /// <see cref="ExportTextTags.ApplyPositionTag"/>: each eye's copy goes where the flat subtitle
    /// would be - alignment, margins or "{\pos(x,y)}" - squeezed into that eye's half, so a
    /// bottom margin of 10 is still 10 pixels once the player stretches the view back out.
    /// Does nothing for <see cref="Export3DMode.None"/>.
    /// </summary>
    /// <param name="ip">The subtitle, with its flat bitmap rendered and positioned.</param>
    /// <param name="disposeSource">Dispose the flat bitmap once replaced - off when the caller does not own it.</param>
    public static void Apply(ImageParameter ip, bool disposeSource = true)
    {
        var source = ip.Bitmap;
        if (ip.Mode3D == Export3DMode.None || source == null || source.Width <= 0 || source.Height <= 0 ||
            ip.ScreenWidth < 2 || ip.ScreenHeight < 2)
        {
            return;
        }

        var position = FullFrameImage.GetPosition(ip, source.Width, source.Height);
        var depth = GetDepth(ip);

        // Same direction as SE4: the left eye's copy moves right and the right eye's copy moves
        // left, so a positive depth crosses the eyes and the subtitle comes towards the viewer.
        SKRectI leftEye;
        SKRectI rightEye;
        SKRectI leftView;
        SKRectI rightView;
        if (ip.Mode3D == Export3DMode.HalfTopBottom)
        {
            var half = ip.ScreenHeight / 2;
            var height = Math.Max(1, (source.Height + 1) / 2);
            var y = (int)Math.Floor(position.Y / 2.0);
            leftEye = SKRectI.Create(position.X + depth, y, source.Width, height);
            rightEye = SKRectI.Create(position.X - depth, half + y, source.Width, height);
            leftView = new SKRectI(0, 0, ip.ScreenWidth, half);
            rightView = new SKRectI(0, half, ip.ScreenWidth, half * 2);
        }
        else
        {
            var half = ip.ScreenWidth / 2;
            var width = Math.Max(1, (source.Width + 1) / 2);
            var x = (int)Math.Floor(position.X / 2.0);
            leftEye = SKRectI.Create(x + depth, position.Y, width, source.Height);
            rightEye = SKRectI.Create(half + x - depth, position.Y, width, source.Height);
            leftView = new SKRectI(0, 0, half, ip.ScreenHeight);
            rightView = new SKRectI(half, 0, half * 2, ip.ScreenHeight);
        }

        var leftVisible = SKRectI.Intersect(leftEye, leftView);
        var rightVisible = SKRectI.Intersect(rightEye, rightView);
        SKRectI bounds;
        if (leftVisible.IsEmpty && rightVisible.IsEmpty)
        {
            return;
        }
        else if (leftVisible.IsEmpty)
        {
            bounds = rightVisible;
        }
        else if (rightVisible.IsEmpty)
        {
            bounds = leftVisible;
        }
        else
        {
            bounds = SKRectI.Union(leftVisible, rightVisible);
        }

        var image3D = new SKBitmap(bounds.Width, bounds.Height, false);
        using (var canvas = new SKCanvas(image3D))
        using (var image = SKImage.FromBitmap(source))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.Translate(-bounds.Left, -bounds.Top);
            DrawEye(canvas, image, leftEye, leftView);
            DrawEye(canvas, image, rightEye, rightView);
        }

        if (disposeSource)
        {
            source.Dispose();
        }

        ip.Bitmap = image3D;
        ip.OverridePosition = new SKPointI(bounds.Left, bounds.Top);
    }

    /// <summary>
    /// The depth <see cref="Apply"/> moves the two copies apart by: from the 3D-Plane when it has
    /// one for the subtitle's frames, else <see cref="ImageParameter.Depth3D"/>.
    /// </summary>
    public static int GetDepth(ImageParameter ip)
    {
        var offset = ip.Plane3D?.GetOffset(ip.StartTime, ip.EndTime);
        return offset.HasValue ? DepthFromPlaneOffset(offset.Value, ip.Mode3D, ip.ScreenWidth) : ip.Depth3D;
    }

    /// <summary>
    /// A 3D-Plane offset is how far the Blu-ray player shifts each eye's view of the subtitle, in
    /// pixels of the 1920 wide frame - the same thing as the depth, once scaled to the export's
    /// width. A half side-by-side view is squeezed to half that width, so the shift is too.
    /// </summary>
    public static int DepthFromPlaneOffset(int offset, Export3DMode mode, int screenWidth)
    {
        var depth = offset * (double)screenWidth / Stereo3DPlane.SourceWidth;
        if (mode == Export3DMode.HalfSideBySide)
        {
            depth /= 2;
        }

        return (int)Math.Round(depth, MidpointRounding.AwayFromZero);
    }

    private static void DrawEye(SKCanvas canvas, SKImage image, SKRectI eye, SKRectI view)
    {
        // Clipped to its own view: a large depth must not push one eye's copy over the middle of
        // the frame, where the other eye would see it.
        canvas.Save();
        canvas.ClipRect(view);

        // Linear without mipmaps: halving one axis then averages each pair of source pixels.
        canvas.DrawImage(image, eye, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        canvas.Restore();
    }
}
