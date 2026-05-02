using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Headless image renderer for image-based subtitle output formats. Extracted from
/// SE's ExportImageBasedViewModel so it's callable from seconv (no Avalonia / no MVVM).
/// All UI consumers should call this and then convert the SKBitmap if needed.
/// </summary>
public static class ImageRenderer
{
    private static SKPointI CalculatePosition(ImageParameter ip, double width, double height)
    {
        var x = 0;
        var y = 0;

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.MiddleLeft ||
            ip.Alignment == ExportAlignment.BottomLeft)
        {
            x = (int)ip.LeftRightMargin;
        }
        else if (ip.Alignment == ExportAlignment.TopCenter ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.BottomCenter)
        {
            x = (int)((ip.ScreenWidth - width) / 2);
        }
        else if (ip.Alignment == ExportAlignment.TopRight ||
                 ip.Alignment == ExportAlignment.MiddleRight ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            x = (int)(ip.ScreenWidth - width - ip.LeftRightMargin);
        }

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.TopCenter ||
            ip.Alignment == ExportAlignment.TopRight)
        {
            y = (int)ip.BottomTopMargin;
        }
        else if (ip.Alignment == ExportAlignment.MiddleLeft ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.MiddleRight)
        {
            y = (int)((ip.ScreenHeight - height) / 2);
        }
        else if (ip.Alignment == ExportAlignment.BottomLeft ||
                 ip.Alignment == ExportAlignment.BottomCenter ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            y = (int)(ip.ScreenHeight - height - ip.BottomTopMargin);
        }

        return new SKPointI(x, y);
    }

    public static SKBitmap GenerateBitmap(ImageParameter ip)
    {
        var fontName = ip.FontName;
        var fontSize = ip.FontSize;
        var fontColor = ip.FontColor;

        var outlineColor = ip.OutlineColor;
        var outlineWidth = ip.OutlineWidth;

        var shadowColor = ip.ShadowColor;
        var shadowWidth = ip.ShadowWidth;

        // Parse text and create text segments with styling
        var text = ReverseNumberAndLatinOnly(ip.Text, ip.IsRightToLeft);

        var segments = ParseTextWithStyling(text, fontColor);

        // Create fonts
        using var regularTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Normal);
        using var boldTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Bold);
        using var italicTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.Italic);
        using var boldItalicTypeface = SKTypeface.FromFamilyName(fontName, SKFontStyle.BoldItalic);

        using var regularFont = new SKFont(ip.IsBold ? boldTypeface : regularTypeface, fontSize);
        using var boldFont = new SKFont(boldTypeface, fontSize);
        using var italicFont = new SKFont(ip.IsBold ? boldItalicTypeface : italicTypeface, fontSize);
        using var boldItalicFont = new SKFont(boldItalicTypeface, fontSize);

        // Split segments into lines
        var lines = SplitIntoSegments(segments);
        var fontMetrics = regularFont.Metrics;

        // Calculate line spacing
        var baseLineHeight = Math.Abs(fontMetrics.Ascent) + Math.Abs(fontMetrics.Descent);
        var lineSpacing = (float)(baseLineHeight * ip.LineSpacingPercent / 100.0);

        // Create oversized temporary bitmap for rendering (with fixed margin for effects)
        var tempWidth = Math.Max(4000, ip.ScreenWidth);
        var tempHeight = Math.Max(2000, ip.ScreenHeight);
        using var tempBitmap = new SKBitmap(tempWidth, tempHeight);
        using var tempCanvas = new SKCanvas(tempBitmap);
        tempCanvas.Clear(SKColors.Transparent);

        // Render text to temporary bitmap to measure actual bounds
        RenderTextToCanvas(tempCanvas, lines, ip, regularFont, boldFont, italicFont, boldItalicFont,
            100, 100, baseLineHeight, lineSpacing,
            outlineColor, shadowColor, outlineWidth, shadowWidth);

        //System.IO.File.WriteAllBytes(@"C:\temp\debug_raw.png", tempBitmap.ToPngArray());

        var bitmapNoPadding = tempBitmap.TrimTransparentPixels();
        //System.IO.File.WriteAllBytes(@"C:\temp\debug_no_padding.png", bitmapNoPadding.TrimmedBitmap.ToPngArray());
        if (ip.PaddingTopBottom == 0 && ip.PaddingLeftRight == 0)
        {
            return bitmapNoPadding.TrimmedBitmap;
        }

        var bitmapWithMargins = bitmapNoPadding.TrimmedBitmap.AddTransparentMargins(ip.PaddingLeftRight, ip.PaddingTopBottom, ip.PaddingLeftRight, ip.PaddingTopBottom);
        //System.IO.File.WriteAllBytes(@"C:\temp\debug_with_margins.png", bitmapWithMargins.ToPngArray());

        return bitmapWithMargins;
    }

    private static void RenderTextToCanvas(
        SKCanvas canvas,
        List<List<TextSegment>> lines,
        ImageParameter ip,
        SKFont regularFont,
        SKFont boldFont,
        SKFont italicFont,
        SKFont boldItalicFont,
        float textStartX,
        float textStartY,
        float baseLineHeight,
        float lineSpacing,
        SKColor outlineColor,
        SKColor shadowColor,
        double outlineWidth,
        double shadowWidth)
    {
        // Pre-calculate all line widths so alignment is relative to the widest line,
        // not the canvas width (which would push text far right or off-canvas).
        var lineWidths = new float[lines.Count];
        for (var li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            for (var j = 0; j < line.Count; j++)
            {
                var seg = line[j];
                var font = GetFont(seg, regularFont, boldFont, italicFont, boldItalicFont);
                lineWidths[li] += MeasureTextWithShaping(seg.Text, font);
                if ((seg.IsItalic || seg.IsBold) && j < line.Count - 1)
                    lineWidths[li] += regularFont.Size * 0.17f;
            }
        }
        var maxLineWidth = lineWidths.Length > 0 ? lineWidths.Max() : 0f;

        var currentY = 0f;
        var lineIndex = 0;

        foreach (var line in lines)
        {
            // Reverse segments for RTL languages
            var segmentsToRender = ip.IsRightToLeft ? line.AsEnumerable().Reverse().ToList() : line;

            var lineWidth = lineWidths[lineIndex++];

            float currentX;

            // Calculate X position based on content alignment, relative to the widest line.
            // This keeps all lines within [textStartX, textStartX + maxLineWidth].
            if (ip.IsRightToLeft)
            {
                if (ip.ContentAlignment == ExportContentAlignment.Center)
                {
                    currentX = textStartX + (maxLineWidth - lineWidth) / 2;
                }
                else if (ip.ContentAlignment == ExportContentAlignment.Left)
                {
                    currentX = textStartX + maxLineWidth - lineWidth;
                }
                else // Right (natural/default for RTL)
                {
                    currentX = textStartX;
                }
            }
            else
            {
                if (ip.ContentAlignment == ExportContentAlignment.Center)
                {
                    currentX = textStartX + (maxLineWidth - lineWidth) / 2;
                }
                else if (ip.ContentAlignment == ExportContentAlignment.Right)
                {
                    currentX = textStartX + maxLineWidth - lineWidth;
                }
                else // Left (default)
                {
                    currentX = textStartX;
                }
            }

            for (var i = 0; i < segmentsToRender.Count; i++)
            {
                var segment = segmentsToRender[i];
                var currentFont = GetFont(segment, regularFont, boldFont, italicFont, boldItalicFont);

                // Draw shadow first (if shadow width > 0)
                if (shadowWidth > 0)
                {
                    var shadowOffsetX = currentX + (float)shadowWidth;
                    var shadowOffsetY = textStartY + currentY + (float)shadowWidth;

                    if (outlineWidth > 0)
                    {
                        using var shadowOutlinePaint = new SKPaint
                        {
                            Color = shadowColor,
                            IsAntialias = true,
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = (float)outlineWidth,
                            StrokeJoin = SKStrokeJoin.Round,
                            StrokeCap = SKStrokeCap.Round,
                        };

                        DrawShapedText(canvas, segment.Text, shadowOffsetX, shadowOffsetY, currentFont, shadowOutlinePaint);
                    }

                    using var shadowTextPaint = new SKPaint
                    {
                        Color = shadowColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                    };

                    DrawShapedText(canvas, segment.Text, shadowOffsetX, shadowOffsetY, currentFont, shadowTextPaint);
                }

                // Draw outline second (if outline width > 0)
                if (outlineWidth > 0)
                {
                    using var outlinePaint = new SKPaint
                    {
                        Color = outlineColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = (float)outlineWidth,
                        StrokeJoin = SKStrokeJoin.Round,
                        StrokeCap = SKStrokeCap.Round,
                    };

                    DrawShapedText(canvas, segment.Text, currentX, textStartY + currentY, currentFont, outlinePaint);
                }

                // Draw the main text on top
                using var textPaint = new SKPaint
                {
                    Color = segment.Color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };

                DrawShapedText(canvas, segment.Text, currentX, textStartY + currentY, currentFont, textPaint);

                // Update X position for next segment
                currentX += MeasureTextWithShaping(segment.Text, currentFont);

                // Add small spacing after styled segments to prevent crowding
                if ((segment.IsItalic || segment.IsBold) && i < segmentsToRender.Count - 1)
                {
                    currentX += regularFont.Size * 0.17f;
                }
            }

            // Move to next line
            currentY += baseLineHeight + lineSpacing;
        }
    }

    // Helper method to measure text with HarfBuzz shaping via SKShaper
    private static float MeasureTextWithShaping(string text, SKFont font)
    {
        using var shaper = new SKShaper(font.Typeface);
        var result = shaper.Shape(text, font);

        // Measure visual bounds to account for glyph overhang (important for italic text)
        var bounds = new SKRect();
        font.MeasureText(text, out bounds);

        // Use the maximum of advance width and visual right edge
        return Math.Max(result.Width, bounds.Right);
    }

    // Helper method to draw shaped text using SKShaper
    private static void DrawShapedText(SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint paint)
    {
        using var shaper = new SKShaper(font.Typeface);
        canvas.DrawShapedText(shaper, text, x, y, SKTextAlign.Left, font, paint);
    }

    private static SKFont GetFont(TextSegment segment, SKFont regular, SKFont bold, SKFont italic, SKFont boldItalic)
    {
        if (segment.IsBold && segment.IsItalic)
        {
            return boldItalic;
        }

        if (segment.IsBold)
        {
            return bold;
        }

        if (segment.IsItalic)
        {
            return italic;
        }

        return regular;
    }

    private static List<List<TextSegment>> SplitIntoSegments(List<TextSegment> segments)
    {
        var lines = new List<List<TextSegment>>();
        var currentLine = new List<TextSegment>();

        foreach (var segment in segments)
        {
            var text = segment.Text;
            var parts = text.SplitToLines();

            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];

                if (!string.IsNullOrEmpty(part))
                {
                    currentLine.Add(new TextSegment(part, segment.IsItalic, segment.IsBold, segment.Color));
                }

                // Add line break (except for the last part)
                if (i < parts.Count - 1)
                {
                    if (currentLine.Count > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<TextSegment>();
                    }
                    else
                    {
                        // Empty line
                        lines.Add(new List<TextSegment>());
                    }
                }
            }
        }

        // Add the last line if it has content
        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        // Ensure we have at least one line
        if (lines.Count == 0)
        {
            lines.Add(new List<TextSegment>());
        }

        return lines;
    }

    private static List<TextSegment> ParseTextWithStyling(string text, SKColor defaultFontColor)
    {
        var segments = new List<TextSegment>();
        var currentPos = 0;
        var styleStack = new Stack<TextStyle>();
        var currentStyle = new TextStyle { Color = defaultFontColor };

        while (currentPos < text.Length)
        {
            var nextTagPos = FindNextTag(text, currentPos);

            if (nextTagPos == -1)
            {
                // No more tags, add remaining text
                if (currentPos < text.Length)
                {
                    var remainingText = text.Substring(currentPos);
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        segments.Add(new TextSegment(remainingText, currentStyle.IsItalic, currentStyle.IsBold,
                            currentStyle.Color));
                    }
                }

                break;
            }

            // Add text before the tag
            if (nextTagPos > currentPos)
            {
                var beforeTag = text.Substring(currentPos, nextTagPos - currentPos);
                if (!string.IsNullOrEmpty(beforeTag))
                {
                    segments.Add(new TextSegment(beforeTag, currentStyle.IsItalic, currentStyle.IsBold,
                        currentStyle.Color));
                }
            }

            // Process the tag
            var tagInfo = ParseTag(text, nextTagPos, defaultFontColor);
            if (tagInfo != null)
            {
                switch (tagInfo.TagType)
                {
                    case TagType.ItalicOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { IsItalic = true };
                        break;
                    case TagType.ItalicClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { IsItalic = false };
                        }

                        break;
                    case TagType.BoldOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { IsBold = true };
                        break;
                    case TagType.BoldClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { IsBold = false };
                        }

                        break;
                    case TagType.FontOpen:
                        styleStack.Push(currentStyle);
                        currentStyle = currentStyle with { Color = tagInfo.Color ?? currentStyle.Color };
                        break;
                    case TagType.FontClose:
                        if (styleStack.Count > 0)
                        {
                            currentStyle = styleStack.Pop();
                        }
                        else
                        {
                            currentStyle = currentStyle with { Color = defaultFontColor };
                        }

                        break;
                }

                currentPos = tagInfo.EndPosition;
            }
            else
            {
                // Invalid tag, treat as regular text
                currentPos++;
            }
        }

        // Filter out empty segments
        return segments.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
    }

    private static int FindNextTag(string text, int startPos)
    {
        var openBracket = text.IndexOf('<', startPos);
        return openBracket;
    }

    private static TagInfo? ParseTag(string text, int startPos, SKColor defaultFontColor)
    {
        if (startPos >= text.Length || text[startPos] != '<')
        {
            return null;
        }

        var endBracket = text.IndexOf('>', startPos);
        if (endBracket == -1)
        {
            return null;
        }

        var tagContent = text.Substring(startPos + 1, endBracket - startPos - 1);
        var endPosition = endBracket + 1;

        // Check for specific tags
        if (tagContent.Equals("i", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.ItalicOpen, endPosition);
        }

        if (tagContent.Equals("/i", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.ItalicClose, endPosition);
        }

        if (tagContent.Equals("b", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.BoldOpen, endPosition);
        }

        if (tagContent.Equals("/b", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.BoldClose, endPosition);
        }

        if (tagContent.Equals("/font", StringComparison.OrdinalIgnoreCase))
        {
            return new TagInfo(TagType.FontClose, endPosition);
        }

        // Check for font tag with color attribute
        if (tagContent.StartsWith("font", StringComparison.OrdinalIgnoreCase))
        {
            var color = ParseColorFromFontTag(tagContent, defaultFontColor);
            return new TagInfo(TagType.FontOpen, endPosition, color);
        }

        return null;
    }

    private static SKColor ParseColorFromFontTag(string tagContent, SKColor defaultFontColor)
    {
        // Look for color="#ffffff" pattern
        var colorMatch = System.Text.RegularExpressions.Regex.Match(
            tagContent,
            @"color\s*=\s*[""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (colorMatch.Success)
        {
            var colorValue = colorMatch.Groups[1].Value;

            // Handle hex colors
            if (colorValue.StartsWith("#") && colorValue.Length == 7)
            {
                try
                {
                    var hex = colorValue.Substring(1);
                    var r = Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return new SKColor(r, g, b);
                }
                catch
                {
                    return defaultFontColor;
                }
            }

            // Handle named colors (basic set)
            return colorValue.ToLowerInvariant() switch
            {
                "red" => SKColors.Red,
                "green" => SKColors.Green,
                "blue" => SKColors.Blue,
                "white" => SKColors.White,
                "black" => SKColors.Black,
                "yellow" => SKColors.Yellow,
                "orange" => SKColors.Orange,
                "purple" => SKColors.Purple,
                "pink" => SKColors.Pink,
                "gray" or "grey" => SKColors.Gray,
                _ => defaultFontColor
            };
        }

        return defaultFontColor;
    }

    // Pre-handler: reverse only Latin letters and ASCII digits in RTL mode, leave tags/entities untouched
    private static string ReverseNumberAndLatinOnly(string input, bool isRightToLeft)
    {
        if (!isRightToLeft || string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new System.Text.StringBuilder(input.Length);
        int i = 0;
        while (i < input.Length)
        {
            var c = input[i];

            // Preserve HTML-like tags: <...>
            if (c == '<')
            {
                int end = input.IndexOf('>', i);
                if (end == -1)
                {
                    sb.Append(input.AsSpan(i));
                    break;
                }
                sb.Append(input.AsSpan(i, end - i + 1));
                i = end + 1;
                continue;
            }

            // Preserve ASS/SSA override blocks: {\...}
            if (c == '{')
            {
                int end = input.IndexOf('}', i);
                if (end == -1)
                {
                    sb.Append(input.AsSpan(i));
                    break;
                }
                sb.Append(input.AsSpan(i, end - i + 1));
                i = end + 1;
                continue;
            }

            // Preserve HTML entities: &...;
            if (c == '&')
            {
                int end = input.IndexOf(';', i);
                if (end > i)
                {
                    sb.Append(input.AsSpan(i, end - i + 1));
                    i = end + 1;
                    continue;
                }
            }

            if (IsLatinLetterOrAsciiDigit(c))
            {
                int start = i;
                int j = i;
                while (j < input.Length && IsLatinLetterOrAsciiDigit(input[j]))
                {
                    j++;
                }
                // reverse slice [start, j)
                for (int k = j - 1; k >= start; k--)
                {
                    sb.Append(input[k]);
                }
                i = j;
                continue;
            }

            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    private static bool IsLatinLetterOrAsciiDigit(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return true; // ASCII digits only
        }

        if (!char.IsLetter(c))
        {
            return false;
        }

        // Basic Latin and Latin-1 Supplement and Latin Extended-A/B
        var code = (int)c;
        return (code >= 0x0041 && code <= 0x005A) || // A-Z
               (code >= 0x0061 && code <= 0x007A) || // a-z
               (code >= 0x00C0 && code <= 0x00FF) || // Latin-1 letters
               (code >= 0x0100 && code <= 0x024F);   // Latin Extended-A/B
    }

    record TextSegment(string Text, bool IsItalic, bool IsBold, SKColor Color);

    record TextStyle(bool IsItalic = false, bool IsBold = false, SKColor Color = default)
    {
        public SKColor Color { get; init; } = Color == default ? SKColors.Black : Color;
    }

    record TagInfo(TagType TagType, int EndPosition, SKColor? Color = null);

    enum TagType
    {
        ItalicOpen,
        ItalicClose,
        BoldOpen,
        BoldClose,
        FontOpen,
        FontClose
    }

}
