using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Trains an nOCR database by rendering characters with real fonts (white fill, black outline,
/// like typical image subtitles) and generating line segments for each glyph the database does
/// not already recognize. Port of SE4's "Train nOCR" (VobSubNOcrTrain).
/// </summary>
public sealed class NOcrTrainerSettings
{
    public List<string> FontNames { get; set; } = new();
    public float FontSize { get; set; } = 30;
    public bool IncludeBold { get; set; }
    public bool IncludeItalic { get; set; }
    public int NumberOfLineSegments { get; set; } = 60;

    /// <summary>All characters to train, as a plain string (whitespace is ignored).</summary>
    public string CharactersToTrain { get; set; } = NOcrTrainer.DefaultTrainingCharacters;

    /// <summary>Space separated multi-character combinations (2-3 chars, e.g. ligature-prone pairs like "fi ff rn").</summary>
    public string MergedLetterCombinations { get; set; } = string.Empty;

    public NOcrLineAlgorithm LineSegmentAlgorithm { get; set; } = NOcrLineAlgorithm.Random;
}

public sealed class NOcrTrainerProgress
{
    public int CharactersLearned { get; init; }
    public int CharactersSkipped { get; init; }
    public string CurrentFontName { get; init; } = string.Empty;
    public string CurrentText { get; init; } = string.Empty;
}

public sealed class NOcrTrainer
{
    public const string DefaultTrainingCharacters =
        "abcdefghijklmnopqrstuvwxyz" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "0123456789" +
        ".,!?\"'()[]:;-+/%&$#@*=" +
        "æøåäöüßéèêëáàâíìîóòôúùûñç" +
        "ÆØÅÄÖÜÉÈÊËÁÀÂÍÌÎÓÒÔÚÙÛÑÇ";

    // Matches the runtime OCR pipeline (OcrViewModel/seconv): two-color threshold and the
    // wrong-pixel budget used when checking whether a glyph is already recognized.
    private const int TwoColorThreshold = 200;
    private const int MaxWrongPixels = 25;
    private const int PixelsAreSpace = 10;
    private const int MinLineHeight = 25;
    private const float OutlineWidth = 2.0f;

    /// <summary>
    /// Trains <paramref name="db"/> in place. Call from a background thread; use
    /// <paramref name="abortRequested"/> to cancel between characters.
    /// </summary>
    /// <returns>The number of characters learned (added to the database).</returns>
    public int Train(NOcrTrainerSettings settings, NOcrDb db, Func<bool>? abortRequested = null, Action<NOcrTrainerProgress>? progress = null)
    {
        var learned = 0;
        var skipped = 0;

        var singleCharacters = new List<string>();
        foreach (var ch in settings.CharactersToTrain)
        {
            var s = ch.ToString();
            if (!char.IsWhiteSpace(ch) && !singleCharacters.Contains(s))
            {
                singleCharacters.Add(s);
            }
        }

        var mergedCombinations = settings.MergedLetterCombinations
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 1 && s.Length <= 3)
            .Distinct()
            .ToList();

        foreach (var fontName in settings.FontNames)
        {
            foreach (var text in singleCharacters.Concat(mergedCombinations))
            {
                if (abortRequested?.Invoke() == true)
                {
                    return learned;
                }

                var isMerged = text.Length > 1;
                foreach (var (bold, italic) in GetStyles(settings))
                {
                    if (TrainCharacter(settings, db, text, fontName, bold, italic, isMerged))
                    {
                        learned++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                progress?.Invoke(new NOcrTrainerProgress
                {
                    CharactersLearned = learned,
                    CharactersSkipped = skipped,
                    CurrentFontName = fontName,
                    CurrentText = text,
                });
            }
        }

        return learned;
    }

    private static IEnumerable<(bool Bold, bool Italic)> GetStyles(NOcrTrainerSettings settings)
    {
        yield return (false, false);

        if (settings.IncludeBold)
        {
            yield return (true, false);
        }

        if (settings.IncludeItalic)
        {
            yield return (false, true);
        }
    }

    private static bool TrainCharacter(NOcrTrainerSettings settings, NOcrDb db, string text, string fontName, bool bold, bool italic, bool isMerged)
    {
        // "H" establishes the line's cap height so the target glyph gets a realistic top
        // margin; the wide gap makes the splitter separate it from the target.
        using var bitmap = RenderCharacterImage("H   " + text, fontName, settings.FontSize, bold, italic);
        if (bitmap == null)
        {
            return false;
        }

        var nikseBitmap = new NikseBitmap2(bitmap);
        nikseBitmap.MakeTwoColor(TwoColorThreshold);
        nikseBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
        var list = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nikseBitmap, PixelsAreSpace, false, false, MinLineHeight, false);

        // Expected: [H][space][target...]
        if (list.Count == 3 && list[2].NikseBitmap != null)
        {
            var item = list[2];
            var match = db.GetMatchSingle(item.NikseBitmap!, item.Top, false, MaxWrongPixels);
            if (match != null && match.Text == text)
            {
                return false; // already recognized
            }

            var nOcrChar = new NOcrChar(text)
            {
                Width = item.NikseBitmap!.Width,
                Height = item.NikseBitmap.Height,
                MarginTop = item.Top,
                Italic = italic,
            };
            var segments = settings.NumberOfLineSegments + (isMerged ? 20 : 0);
            NOcrChar.GenerateLineSegments(segments, false, nOcrChar, item.NikseBitmap, settings.LineSegmentAlgorithm);
            db.Add(nOcrChar);
            return true;
        }

        // Multi-part glyphs (e.g. ", %, ?) - train as an expanded character spanning all parts.
        // Merged letter combinations are only trained when they render as one connected glyph
        // (same as SE4); separate glyphs are already covered by the single characters.
        if (!isMerged && list.Count is 4 or 5)
        {
            var group = ExpandedOcrGroup.Create(nikseBitmap, list, 2, list.Count - 2);
            if (group == null)
            {
                return false;
            }

            var existing = db.GetMatchExpanded(nikseBitmap, list[2], 2, list);
            if (existing != null && existing.Text == text)
            {
                return false;
            }

            var nOcrChar = group.CreateNOcrChar();
            nOcrChar.Text = text;
            nOcrChar.Italic = italic;
            var segments = settings.NumberOfLineSegments + (list.Count == 5 ? 10 : 5);
            NOcrChar.GenerateLineSegments(segments, false, nOcrChar, group.PreviewBitmap, settings.LineSegmentAlgorithm);
            db.Add(nOcrChar);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Renders text like a typical image subtitle: white fill with a black outline on a
    /// transparent background.
    /// </summary>
    public static SKBitmap? RenderCharacterImage(string text, string fontName, float fontSize, bool bold, bool italic)
    {
        using var typeface = SKTypeface.FromFamilyName(
            fontName,
            bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
        if (typeface == null)
        {
            return null;
        }

        using var font = new SKFont(typeface, fontSize);
        if (italic && !typeface.IsItalic)
        {
            font.SkewX = -0.25f; // synthetic slant when the font has no italic face
        }

        using var paint = new SKPaint { IsAntialias = true };
        font.MeasureText(text, out var textBounds, paint);
        font.GetFontMetrics(out var metrics);

        const int padding = 10;
        var width = (int)Math.Ceiling(textBounds.Width + padding * 2 + OutlineWidth * 2);
        var height = (int)Math.Ceiling(-metrics.Ascent + metrics.Descent + padding * 2 + OutlineWidth * 2);
        if (width < 1 || height < 1)
        {
            return null;
        }

        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        var x = padding + OutlineWidth;
        var baseline = padding + OutlineWidth - metrics.Ascent;
        using var textPath = font.GetTextPath(text, new SKPoint(x, baseline));

        using var outlinePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = OutlineWidth,
            Color = SKColors.Black,
        };
        canvas.DrawPath(textPath, outlinePaint);

        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = SKColors.White,
        };
        canvas.DrawPath(textPath, fillPaint);

        return bitmap;
    }
}
