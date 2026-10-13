using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Wraps (and with shrink to fit, sizes) text for a text area the way libass will show it, so the
/// canvas and the generated \q2 line agree. libass scales a font so that ascent + descent equals
/// \fs, and each line of text is \fs high.
/// </summary>
public static class AssaTextLayout
{
    public const float MinFontSize = 6;
    private const float MeasureSize = 100;

    private static readonly object CacheLock = new();
    private static readonly Dictionary<(string Font, bool Bold, string Word), float> WidthCache = new();
    private static readonly Dictionary<(string Font, bool Bold), float> HeightCache = new();
    private static readonly Dictionary<string, string> SkiaNameCache = new();

    public sealed class Result
    {
        public List<string> Lines { get; }

        /// <summary>
        /// The \fs used (smaller than asked for when shrunk to fit).
        /// </summary>
        public float FontSize { get; }

        /// <summary>
        /// Font em size that gives a line height of <see cref="FontSize"/>, for drawing the text.
        /// </summary>
        public float EmSize { get; }

        public Result(List<string> lines, float fontSize, float emSize)
        {
            Lines = lines;
            FontSize = fontSize;
            EmSize = emSize;
        }
    }

    public static Result Layout(string text, string fontName, float fontSize, bool bold, float width, float height, bool shrinkToFit)
    {
        var size = MathF.Max(MinFontSize, MathF.Round(fontSize));
        var heightAt100 = MeasureHeight(fontName, bold);
        while (true)
        {
            var scale = size / heightAt100;
            var lines = Wrap(text, fontName, bold, width / scale, out var widest);
            var fits = lines.Count * size <= height + 0.5f && widest * scale <= width + 0.5f;
            if (!shrinkToFit || fits || size <= MinFontSize)
            {
                return new Result(lines, size, MeasureSize * scale);
            }

            size = MathF.Max(MinFontSize, size - (size > 40 ? 2 : 1));
        }
    }

    /// <summary>
    /// Greedy word wrap; <paramref name="maxWidth"/> and <paramref name="widest"/> are widths at the measuring size.
    /// User line breaks (\n) are kept. A word longer than the line gets a line of its own.
    /// </summary>
    private static List<string> Wrap(string text, string fontName, bool bold, float maxWidth, out float widest)
    {
        var lines = new List<string>();
        widest = 0;
        var spaceWidth = MeasureWord(" ", fontName, bold);
        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                lines.Add(string.Empty);
                continue;
            }

            var line = words[0];
            var lineWidth = MeasureWord(words[0], fontName, bold);
            for (var i = 1; i < words.Length; i++)
            {
                var wordWidth = MeasureWord(words[i], fontName, bold);
                if (lineWidth + spaceWidth + wordWidth > maxWidth)
                {
                    lines.Add(line);
                    widest = MathF.Max(widest, lineWidth);
                    line = words[i];
                    lineWidth = wordWidth;
                }
                else
                {
                    line += " " + words[i];
                    lineWidth += spaceWidth + wordWidth;
                }
            }

            lines.Add(line);
            widest = MathF.Max(widest, lineWidth);
        }

        return lines;
    }

    private static float MeasureWord(string word, string fontName, bool bold)
    {
        lock (CacheLock)
        {
            var key = (fontName, bold, word);
            if (!WidthCache.TryGetValue(key, out var width))
            {
                width = TextMeasurer.MeasureString(word, GetSkiaFontName(fontName), MeasureSize, bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal).Width;
                if (WidthCache.Count > 20_000)
                {
                    WidthCache.Clear();
                }

                WidthCache[key] = width;
            }

            return width;
        }
    }

    private static float MeasureHeight(string fontName, bool bold)
    {
        lock (CacheLock)
        {
            if (!HeightCache.TryGetValue((fontName, bold), out var height))
            {
                height = TextMeasurer.MeasureString("Xg", GetSkiaFontName(fontName), MeasureSize, bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal).Height;
                if (height < 1)
                {
                    height = MeasureSize * 1.15f;
                }

                HeightCache[(fontName, bold)] = height;
            }

            return height;
        }
    }

    /// <summary>
    /// The family name Skia/Avalonia know for a libass (GDI) font name.
    /// </summary>
    public static string GetSkiaFontName(string fontName)
    {
        lock (CacheLock)
        {
            if (!SkiaNameCache.TryGetValue(fontName, out var name))
            {
                try
                {
                    name = FontHelper.GetSkiaFontNameFromLibAssaFontName(fontName);
                }
                catch
                {
                    name = fontName;
                }

                SkiaNameCache[fontName] = string.IsNullOrEmpty(name) ? fontName : name;
            }

            return SkiaNameCache[fontName];
        }
    }
}
