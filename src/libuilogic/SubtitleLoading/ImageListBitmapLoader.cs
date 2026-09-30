using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.SubtitleLoading;

/// <summary>
/// Loads the image a cue of an image-list subtitle names (see <see cref="ImageListSubtitleLoader"/>),
/// resolving the file like the GUI's BDN OCR import: the cue's Extra (BDN "file://" path) first,
/// otherwise one image per text line relative to the subtitle file, stacked top to bottom.
/// </summary>
public static class ImageListBitmapLoader
{
    private const int StackGap = 7;

    /// <returns>A bitmap the caller owns, or null when no named image could be read.</returns>
    public static SKBitmap? Load(Paragraph paragraph, string subtitleFileName)
    {
        var folder = Path.GetDirectoryName(subtitleFileName) ?? string.Empty;
        if (!string.IsNullOrEmpty(paragraph.Extra))
        {
            var fromExtra = Path.Combine(folder, paragraph.Extra.Replace("file://", string.Empty));
            if (File.Exists(fromExtra))
            {
                return Decode(fromExtra);
            }
        }

        var parts = new List<SKBitmap>();
        try
        {
            foreach (var name in paragraph.Text.SplitToLines())
            {
                var path = ResolvePath(folder, name.Trim());
                var part = path is null ? null : Decode(path);
                if (part != null)
                {
                    parts.Add(part);
                }
            }

            if (parts.Count == 0)
            {
                return null;
            }

            if (parts.Count == 1)
            {
                var only = parts[0];
                parts.Clear();
                return only;
            }

            return Stack(parts);
        }
        finally
        {
            foreach (var part in parts)
            {
                part.Dispose();
            }
        }
    }

    private static string? ResolvePath(string folder, string name)
    {
        if (name.Length == 0)
        {
            return null;
        }

        var path = Path.Combine(folder, name);
        if (File.Exists(path))
        {
            return path;
        }

        // AVISubDetector lines: "... i=<something> <file name>"
        var idxOfIEquals = name.IndexOf("i=", StringComparison.OrdinalIgnoreCase);
        if (idxOfIEquals >= 0)
        {
            var idxOfSpace = name.IndexOf(' ', idxOfIEquals);
            if (idxOfSpace > 0)
            {
                path = Path.Combine(folder, name.Remove(0, idxOfSpace).Trim());
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return null;
    }

    private static SKBitmap? Decode(string path)
    {
        try
        {
            return SKBitmap.Decode(path);
        }
        catch
        {
            return null;
        }
    }

    private static SKBitmap Stack(List<SKBitmap> parts)
    {
        var width = parts.Max(p => p.Width);
        var height = parts.Sum(p => p.Height) + StackGap * parts.Count;
        var merged = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888));
        using var canvas = new SKCanvas(merged);
        canvas.Clear(SKColors.Transparent);
        var y = 0;
        foreach (var part in parts)
        {
            canvas.DrawBitmap(part, 0, y);
            y += part.Height + StackGap;
        }

        return merged;
    }
}
