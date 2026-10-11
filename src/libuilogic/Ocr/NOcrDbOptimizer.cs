using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public sealed class NOcrDbOptimizerSettings
{
    /// <summary>Fonts to render the validation glyphs in; missing fonts are skipped.</summary>
    public List<string> FontNames { get; set; } = new(DefaultFontNames);

    public List<float> FontSizes { get; set; } = new() { 24, 30, 40 };

    /// <summary>
    /// Entries keep at least this many lines. The wide and deep-seek match passes skip entries
    /// with fewer than 41 / 51 lines, so going below 52 trades wrong letters for asterisks.
    /// </summary>
    public int MinLines { get; set; } = 52;

    /// <summary>
    /// The loosest wrong-pixel budget a lookalike must stay above: the last-ditch pass (batch
    /// convert, seconv, binary OCR fallback) doubles the deep-seek default of 25.
    /// </summary>
    public int MaxWrongPixels { get; set; } = 50;

    public static readonly string[] DefaultFontNames =
    {
        "Arial", "Helvetica", "Verdana", "Tahoma", "Trebuchet MS", "Arial Narrow", "Helvetica Neue", "Times New Roman",
    };
}

public sealed class NOcrDbOptimizeResult
{
    public int Entries { get; init; }
    public int LinesBefore { get; init; }
    public int LinesAfter { get; init; }
    public int ValidationGlyphs { get; init; }
    public List<string> FontsUsed { get; init; } = new();
}

/// <summary>
/// Removes the lines of each nOCR entry that don't tell it apart from its lookalikes. Most
/// trained lines are random and also hold for similar characters ("i" and "l" share the stem),
/// so they carry no evidence, and they dilute the share of wrong points the matcher ranks
/// candidates by. Every entry and its lookalikes are rendered in common subtitle fonts; the
/// entry then keeps the fewest lines for which each lookalike:
/// - still has at least min(original wrong points, budget + 1) wrong points, so it is still
///   rejected by every pass (and by the exact-size check, which needs one wrong point), and
/// - has a share of wrong points no lower than before,
/// while the entry's own rendered glyphs keep their share of wrong points. A/B on real
/// Blu-ray/DVD images: 43% fewer lines with a lower character error rate and fewer wrong letters.
/// Multi-part (expanded) entries are left as they are.
/// </summary>
public static class NOcrDbOptimizer
{
    private const int TwoColorThreshold = 200;
    private const int PixelsAreSpace = 10;
    private const int MinLineHeight = 25;
    private const double PositiveShareMargin = 0.005;

    private sealed record Glyph(string Text, bool[] Ink, int Width, int Height);

    /// <summary>
    /// A backup file name for <paramref name="fileName"/> that doesn't exist yet
    /// (Latin.nocr.bak, then Latin.nocr.2.bak, ...), so optimizing again never overwrites the
    /// backup of the unoptimized database.
    /// </summary>
    public static string GetFreeBackupFileName(string fileName)
    {
        var backupFileName = fileName + ".bak";
        for (var i = 2; File.Exists(backupFileName); i++)
        {
            backupFileName = fileName + "." + i + ".bak";
        }

        return backupFileName;
    }

    /// <summary>
    /// Optimizes the single-part entries of <paramref name="db"/> in place (call
    /// <see cref="NOcrDb.Save"/> afterwards). Call from a background thread. Returns null, with
    /// the database unchanged, when <paramref name="abortRequested"/> returns true.
    /// </summary>
    public static NOcrDbOptimizeResult? Optimize(NOcrDb db, NOcrDbOptimizerSettings settings, Func<bool>? abortRequested = null, Action<int, int>? progress = null)
    {
        var fonts = GetInstalledFonts(settings.FontNames);
        var glyphs = RenderGlyphs(db, settings, fonts);
        var entries = db.OcrCharacters.ToList();
        var newLines = new (List<NOcrLine> Foreground, List<NOcrLine> Background)?[entries.Count];
        var done = 0;
        var aborted = false;

        Parallel.For(0, entries.Count, (index, state) =>
        {
            if (abortRequested?.Invoke() == true)
            {
                aborted = true;
                state.Stop();
                return;
            }

            newLines[index] = Prune(entries[index], glyphs, settings);
            progress?.Invoke(Interlocked.Increment(ref done), entries.Count);
        });

        if (aborted)
        {
            return null;
        }

        var before = 0;
        var after = 0;
        for (var i = 0; i < entries.Count; i++)
        {
            var oc = entries[i];
            before += oc.LinesForeground.Count + oc.LinesBackground.Count;
            if (newLines[i] is { } lines)
            {
                oc.LinesForeground.Clear();
                oc.LinesForeground.AddRange(lines.Foreground);
                oc.LinesBackground.Clear();
                oc.LinesBackground.AddRange(lines.Background);
            }

            after += oc.LinesForeground.Count + oc.LinesBackground.Count;
        }

        return new NOcrDbOptimizeResult
        {
            Entries = entries.Count,
            LinesBefore = before,
            LinesAfter = after,
            ValidationGlyphs = glyphs.Count,
            FontsUsed = fonts,
        };
    }

    private static List<string> GetInstalledFonts(List<string> fontNames)
    {
        var installed = new HashSet<string>(SKFontManager.Default.FontFamilies, StringComparer.OrdinalIgnoreCase);
        var fonts = fontNames.Where(installed.Contains).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (fonts.Count == 0)
        {
            fonts.Add(SKTypeface.Default.FamilyName);
        }

        return fonts;
    }

    private static List<Glyph> RenderGlyphs(NOcrDb db, NOcrDbOptimizerSettings settings, List<string> fonts)
    {
        // Every single-character text in the database is a possible lookalike, not just the
        // default training set (e.g. "č" or "♪" taught during OCR).
        var texts = new List<string>();
        foreach (var text in NOcrTrainer.DefaultTrainingCharacters.Select(c => c.ToString())
                     .Concat(db.OcrCharacters.Select(oc => oc.Text)))
        {
            if (text.Length == 1 && !char.IsWhiteSpace(text[0]) && !texts.Contains(text))
            {
                texts.Add(text);
            }
        }

        var jobs = (from font in fonts
                    from size in settings.FontSizes
                    from style in new[] { (Bold: false, Italic: false), (Bold: true, Italic: false), (Bold: false, Italic: true) }
                    select (font, size, style)).ToList();
        var perJob = new List<Glyph>[jobs.Count];
        Parallel.For(0, jobs.Count, i =>
        {
            var (font, size, style) = jobs[i];
            var list = new List<Glyph>();
            foreach (var text in texts)
            {
                var glyph = RenderGlyph(text, font, size, style.Bold, style.Italic);
                if (glyph != null)
                {
                    list.Add(glyph);
                }
            }

            perJob[i] = list;
        });

        return perJob.SelectMany(p => p).ToList();
    }

    private static Glyph? RenderGlyph(string text, string font, float size, bool bold, bool italic)
    {
        // Same rendering and splitting as NOcrTrainer: "H" sets the cap height, and only
        // characters that come out as one connected glyph are used.
        using var bitmap = NOcrTrainer.RenderCharacterImage("H   " + text, font, size, bold, italic);
        if (bitmap == null)
        {
            return null;
        }

        var nikseBitmap = new NikseBitmap2(bitmap);
        nikseBitmap.MakeTwoColor(TwoColorThreshold);
        nikseBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
        var list = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nikseBitmap, PixelsAreSpace, false, false, MinLineHeight, false);
        if (list.Count != 3 || list[2].NikseBitmap == null)
        {
            return null;
        }

        var glyph = list[2].NikseBitmap!;
        var width = glyph.Width;
        var height = glyph.Height;
        var pixels = glyph.GetPixelData();
        var ink = new bool[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                ink[y * width + x] = pixels[x * 4 + y * width * 4 + 3] > 150;
            }
        }

        return new Glyph(text, ink, width, height);
    }

    private static (List<NOcrLine> Foreground, List<NOcrLine> Background)? Prune(NOcrChar oc, List<Glyph> glyphs, NOcrDbOptimizerSettings settings)
    {
        var lines = oc.LinesForeground.Select(l => (Line: l, Foreground: true))
            .Concat(oc.LinesBackground.Select(l => (Line: l, Foreground: false)))
            .ToList();
        var lineCount = lines.Count;
        if (lineCount <= settings.MinLines || oc.Width <= 0 || oc.Height <= 0)
        {
            return null;
        }

        var positives = new List<Glyph>();
        var negatives = new List<Glyph>();
        foreach (var glyph in glyphs)
        {
            if (glyph.Text == oc.Text)
            {
                positives.Add(glyph);
            }
            else if (!AreSameShape(oc.Text, glyph.Text) && IsNear(oc, glyph))
            {
                negatives.Add(glyph);
            }
        }

        var negErrors = new int[lineCount, negatives.Count];
        var negPoints = new int[lineCount, negatives.Count];
        var posErrors = new int[lineCount, positives.Count];
        var posPoints = new int[lineCount, positives.Count];
        for (var i = 0; i < lineCount; i++)
        {
            for (var j = 0; j < negatives.Count; j++)
            {
                Count(oc, lines[i].Line, lines[i].Foreground, negatives[j], out negErrors[i, j], out negPoints[i, j]);
            }

            for (var j = 0; j < positives.Count; j++)
            {
                Count(oc, lines[i].Line, lines[i].Foreground, positives[j], out posErrors[i, j], out posPoints[i, j]);
            }
        }

        var negTotalErrors = new int[negatives.Count];
        var negTotalPoints = new int[negatives.Count];
        var need = new int[negatives.Count];
        for (var j = 0; j < negatives.Count; j++)
        {
            for (var i = 0; i < lineCount; i++)
            {
                negTotalErrors[j] += negErrors[i, j];
                negTotalPoints[j] += negPoints[i, j];
            }

            // Mirrors the matcher's budget: scaled up for bigger glyphs, capped by glyph area.
            var glyph = negatives[j];
            var scale = glyph.Height / (double)oc.Height;
            var budget = Math.Min((int)Math.Round(settings.MaxWrongPixels * Math.Max(1, scale)), Math.Max(8, glyph.Width * glyph.Height / 8));
            need[j] = Math.Min(negTotalErrors[j], budget + 1);
        }

        var posTotalErrors = new int[positives.Count];
        var posTotalPoints = new int[positives.Count];
        var posErrorsPerLine = new int[lineCount];
        var negErrorsPerLine = new int[lineCount];
        for (var i = 0; i < lineCount; i++)
        {
            for (var j = 0; j < positives.Count; j++)
            {
                posTotalErrors[j] += posErrors[i, j];
                posTotalPoints[j] += posPoints[i, j];
                posErrorsPerLine[i] += posErrors[i, j];
            }

            for (var j = 0; j < negatives.Count; j++)
            {
                negErrorsPerLine[i] += negErrors[i, j];
            }
        }

        var selected = new bool[lineCount];
        var selectedCount = 0;

        // Greedy set multicover: the line that adds the most still-needed wrong points to the
        // lookalikes, preferring lines the entry's own glyphs agree with.
        var remaining = (int[])need.Clone();
        while (true)
        {
            var best = -1;
            var bestScore = 0.0;
            for (var i = 0; i < lineCount; i++)
            {
                if (selected[i])
                {
                    continue;
                }

                var gain = 0;
                for (var j = 0; j < negatives.Count; j++)
                {
                    if (remaining[j] > 0)
                    {
                        gain += Math.Min(negErrors[i, j], remaining[j]);
                    }
                }

                var score = gain - 0.1 * posErrorsPerLine[i];
                if (gain > 0 && (best < 0 || score > bestScore))
                {
                    best = i;
                    bestScore = score;
                }
            }

            if (best < 0)
            {
                break;
            }

            selected[best] = true;
            selectedCount++;
            for (var j = 0; j < negatives.Count; j++)
            {
                remaining[j] = Math.Max(0, remaining[j] - negErrors[best, j]);
            }
        }

        // Fill up to the minimum with the most telling lines.
        foreach (var i in Enumerable.Range(0, lineCount).OrderByDescending(i => negErrorsPerLine[i] - 10 * posErrorsPerLine[i]))
        {
            if (selectedCount >= settings.MinLines)
            {
                break;
            }

            if (!selected[i])
            {
                selected[i] = true;
                selectedCount++;
            }
        }

        // Shares: a lookalike within the budget competes on its share of wrong points, so it
        // must not look better than before; the entry's own glyphs must not look worse.
        for (var round = 0; round < lineCount; round++)
        {
            var added = AddLineForNegativeShare(selected, negErrors, negPoints, negTotalErrors, negTotalPoints, need) ??
                        AddLineForPositiveShare(selected, posErrors, posPoints, posTotalErrors, posTotalPoints);
            if (added == null)
            {
                break;
            }

            selected[added.Value] = true;
        }

        var foreground = new List<NOcrLine>();
        var background = new List<NOcrLine>();
        for (var i = 0; i < lineCount; i++)
        {
            if (selected[i])
            {
                (lines[i].Foreground ? foreground : background).Add(lines[i].Line);
            }
        }

        return (foreground, background);
    }

    private static int? AddLineForNegativeShare(bool[] selected, int[,] errors, int[,] points, int[] totalErrors, int[] totalPoints, int[] need)
    {
        for (var j = 0; j < totalErrors.Length; j++)
        {
            // Never matched, or rejected by the budget whatever its share.
            if (totalErrors[j] == 0 || totalErrors[j] > need[j])
            {
                continue;
            }

            long e = 0;
            long p = 0;
            for (var i = 0; i < selected.Length; i++)
            {
                if (selected[i])
                {
                    e += errors[i, j];
                    p += points[i, j];
                }
            }

            if (p > 0 && e * totalPoints[j] >= (long)totalErrors[j] * p)
            {
                continue;
            }

            int? best = null;
            var bestShare = -1.0;
            for (var i = 0; i < selected.Length; i++)
            {
                if (!selected[i] && points[i, j] > 0 && errors[i, j] / (double)points[i, j] > bestShare)
                {
                    best = i;
                    bestShare = errors[i, j] / (double)points[i, j];
                }
            }

            if (best != null)
            {
                return best;
            }
        }

        return null;
    }

    private static int? AddLineForPositiveShare(bool[] selected, int[,] errors, int[,] points, int[] totalErrors, int[] totalPoints)
    {
        for (var j = 0; j < totalErrors.Length; j++)
        {
            long e = 0;
            long p = 0;
            for (var i = 0; i < selected.Length; i++)
            {
                if (selected[i])
                {
                    e += errors[i, j];
                    p += points[i, j];
                }
            }

            var oldShare = totalPoints[j] == 0 ? 0 : totalErrors[j] / (double)totalPoints[j];
            if (p > 0 && e / (double)p <= oldShare + PositiveShareMargin)
            {
                continue;
            }

            int? best = null;
            var bestPoints = 0;
            for (var i = 0; i < selected.Length; i++)
            {
                if (!selected[i] && errors[i, j] == 0 && points[i, j] > bestPoints)
                {
                    best = i;
                    bestPoints = points[i, j];
                }
            }

            if (best != null)
            {
                return best;
            }
        }

        return null;
    }

    /// <summary>Case pairs (o/O) share the shape, and I/l/| are the same bar in sans-serif fonts.</summary>
    private static bool AreSameShape(string a, string b)
    {
        return a.Equals(b, StringComparison.OrdinalIgnoreCase) ||
               a is "I" or "l" or "|" && b is "I" or "l" or "|";
    }

    /// <summary>The matcher's loosest size and aspect gates, widened.</summary>
    private static bool IsNear(NOcrChar oc, Glyph glyph)
    {
        var area = glyph.Width * glyph.Height;
        var ocArea = oc.Width * oc.Height;
        var smallArea = Math.Min(area, ocArea);
        if (smallArea < 150 && Math.Max(area, ocArea) > smallArea * 5)
        {
            return false;
        }

        var aspect = glyph.Height * 100.0 / glyph.Width;
        var ocAspect = oc.HeightToWidthPercent;
        if (area < 150)
        {
            return Math.Max(aspect, ocAspect) / Math.Min(aspect, ocAspect) <= 2.0;
        }

        return Math.Abs(aspect - ocAspect) * 100.0 / Math.Min(aspect, ocAspect) < 100;
    }

    private static void Count(NOcrChar oc, NOcrLine line, bool foreground, Glyph glyph, out int errors, out int points)
    {
        errors = 0;
        points = 0;
        foreach (var p in line.ScaledWalkPoints(oc, glyph.Width, glyph.Height))
        {
            if ((uint)p.X < (uint)glyph.Width && (uint)p.Y < (uint)glyph.Height)
            {
                points++;
                if (glyph.Ink[p.Y * glyph.Width + p.X] != foreground)
                {
                    errors++;
                }
            }
        }
    }
}
