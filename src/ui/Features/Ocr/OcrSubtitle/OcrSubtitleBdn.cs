using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleBdn : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly Subtitle _bdnXmlSubtitle;
    private readonly string _bdnFileName;
    private readonly bool _isSon;
    private readonly bool _makeTransparent;
    private readonly bool _autoTransparentBackground;

    public OcrSubtitleBdn(Subtitle subtitle, string bdnFileName, bool isSon, bool makeTransparent = false, bool autoTransparentBackground = false)
    {
        _bdnXmlSubtitle = subtitle;
        _bdnFileName = bdnFileName;
        _isSon = isSon;
        _makeTransparent = makeTransparent;
        _autoTransparentBackground = autoTransparentBackground;

        Count = subtitle.Paragraphs.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        // Initialize output colors
        var background = SKColors.Transparent;
        var pattern = SKColors.White;
        var emphasis1 = SKColors.Black;
        var emphasis2 = SKColors.Red;

        var returnBmp = new SKBitmap(1, 1, true);

        if (index >= 0 && index < _bdnXmlSubtitle.Paragraphs.Count)
        {
            var fileNames = _bdnXmlSubtitle.Paragraphs[index].Text.SplitToLines();
            var bitmaps = new List<SKBitmap>();
            int maxWidth = 0;
            int totalHeight = 0;

            string fullFileName = string.Empty;
            if (!string.IsNullOrEmpty(_bdnXmlSubtitle.Paragraphs[index].Extra))
            {
                fullFileName = Path.Combine(
                    Path.GetDirectoryName(_bdnFileName) ?? string.Empty,
                    _bdnXmlSubtitle.Paragraphs[index].Extra.Replace("file://", string.Empty));
            }

            if (File.Exists(fullFileName))
            {
                try
                {
                    returnBmp = SKBitmap.Decode(fullFileName);
                    if (_makeTransparent && _autoTransparentBackground)
                    {
                        returnBmp = MakeTransparent(returnBmp);
                    }
                }
                catch
                {
                    // ignore
                }
            }
            else
            {
                foreach (string fn in fileNames)
                {
                    fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName) ?? string.Empty, fn);

                    //TODO
                    //// Check if we need to load the original VSF image
                    //if (checkBoxCloudVisionSendOriginalImages.Visible && checkBoxCloudVisionSendOriginalImages.Checked)
                    //{
                    //    var originalFileName = GetVSFOriginalImageFileName(fullFileName);
                    //    if (originalFileName != fullFileName && File.Exists(originalFileName))
                    //    {
                    //        fullFileName = originalFileName;
                    //    }
                    //}

                    if (!File.Exists(fullFileName))
                    {
                        // fix AVISubDetector lines
                        int idxOfIEquals = fn.IndexOf("i=", StringComparison.OrdinalIgnoreCase);
                        if (idxOfIEquals >= 0)
                        {
                            int idxOfSpace = fn.IndexOf(' ', idxOfIEquals);
                            if (idxOfSpace > 0)
                            {
                                fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName) ?? string.Empty,
                                    fn.Remove(0, idxOfSpace).Trim());
                            }
                        }
                    }

                    if (File.Exists(fullFileName))
                    {
                        try
                        {
                            var temp = SKBitmap.Decode(fullFileName);
                            if (temp.Width > maxWidth)
                            {
                                maxWidth = temp.Width;
                            }

                            totalHeight += temp.Height;
                            bitmaps.Add(temp);
                        }
                        catch
                        {
                            return new SKBitmap(1, 1, true);
                        }
                    }
                }

                SKBitmap b = new SKBitmap(1, 1, true);
                if (bitmaps.Count > 1)
                {
                    var imageInfo = new SKImageInfo(maxWidth, totalHeight + 7 * bitmaps.Count, SKColorType.Rgba8888);
                    var merged = new SKBitmap(imageInfo);

                    using (var canvas = new SKCanvas(merged))
                    {
                        canvas.Clear(SKColors.Transparent);

                        int y = 0;
                        for (int k = 0; k < bitmaps.Count; k++)
                        {
                            SKBitmap part = bitmaps[k];
                            if (_makeTransparent && _autoTransparentBackground)
                            {
                                part = MakeTransparent(part);
                            }

                            canvas.DrawBitmap(part, 0, y);
                            y += part.Height + 7;

                            // Dispose the temporary bitmap if it was made transparent
                            if (_makeTransparent && _autoTransparentBackground)
                            {
                                part.Dispose();
                            }
                        }
                    }

                    b = merged;

                    // Dispose original bitmaps
                    foreach (var bitmap in bitmaps)
                    {
                        if (!_makeTransparent || !_autoTransparentBackground)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
                else if (bitmaps.Count == 1)
                {
                    b = bitmaps[0];
                }

                if (b != null)
                {
                    //TODO:
                    //if (_isSon && checkBoxCustomFourColors.Checked)
                    //{
                    //    GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);
                    //    b = ApplyCustomColors(b, background, pattern, emphasis1, emphasis2);
                    //}

                    if (_makeTransparent && _autoTransparentBackground)
                    {
                        b = MakeTransparent(b);
                    }

                    returnBmp = b;
                }
            }
        }

        return returnBmp ?? new SKBitmap(1, 1, true);
    }

    // Helper method to make bitmap transparent (replaces WinForms MakeTransparent)
    private SKBitmap MakeTransparent(SKBitmap original)
    {
        if (original == null)
        {
            return new SKBitmap(1, 1, true);
        }

        var imageInfo = new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888);
        var transparent = new SKBitmap(imageInfo);

        using (var canvas = new SKCanvas(transparent))
        {
            canvas.Clear(SKColors.Transparent);

            // Get the background color (typically the color at 0,0)
            var bgColor = original.GetPixel(0, 0);

            using (var paint = new SKPaint())
            {
                paint.BlendMode = SKBlendMode.Src;

                for (int x = 0; x < original.Width; x++)
                {
                    for (int y = 0; y < original.Height; y++)
                    {
                        var pixel = original.GetPixel(x, y);

                        // Make background color transparent
                        if (ColorsAreEqual(pixel, bgColor))
                        {
                            transparent.SetPixel(x, y, SKColors.Transparent);
                        }
                        else
                        {
                            transparent.SetPixel(x, y, pixel);
                        }
                    }
                }
            }
        }

        return transparent;
    }

    // Helper method to apply custom colors (replaces the FastBitmap logic)
    private SKBitmap ApplyCustomColors(SKBitmap original, SKColor background, SKColor pattern, SKColor emphasis1, SKColor emphasis2)
    {
        var imageInfo = new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888);
        var result = new SKBitmap(imageInfo);

        for (int x = 0; x < original.Width; x++)
        {
            for (int y = 0; y < original.Height; y++)
            {
                var c = original.GetPixel(x, y);

                if (ColorsAreEqual(c, SKColors.Red)) // normally anti-alias
                {
                    result.SetPixel(x, y, emphasis2);
                }
                else if (ColorsAreEqual(c, SKColors.Blue)) // normally text?
                {
                    result.SetPixel(x, y, pattern);
                }
                else if (ColorsAreEqual(c, SKColors.White)) // normally background
                {
                    result.SetPixel(x, y, background);
                }
                else if (ColorsAreEqual(c, SKColors.Black)) // outline/border
                {
                    result.SetPixel(x, y, emphasis1);
                }
                else
                {
                    result.SetPixel(x, y, c);
                }
            }
        }

        return result;
    }

    private bool ColorsAreEqual(SKColor c1, SKColor c2)
    {
        return c1.Red == c2.Red && c1.Green == c2.Green && c1.Blue == c2.Blue;
    }


    public TimeSpan GetStartTime(int index)
    {
        return _bdnXmlSubtitle.Paragraphs[index].StartTime.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _bdnXmlSubtitle.Paragraphs[index].EndTime.TimeSpan;
    }

    public List<OcrSubtitleItem> MakeOcrSubtitleItems()
    {
        var ocrSubtitleItems = new List<OcrSubtitleItem>(Count);
        for (var i = 0; i < Count; i++)
        {
            ocrSubtitleItems.Add(new OcrSubtitleItem(this, i));
        }

        return ocrSubtitleItems;
    }

    public SKPointI GetPosition(int index)
    {
        return new SKPointI(-1, -1);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(-1, -1);
    }
}