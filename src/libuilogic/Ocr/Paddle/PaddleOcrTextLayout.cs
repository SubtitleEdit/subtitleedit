using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

/// <summary>
/// Turns the loose text boxes of one OCR result into the subtitle text for that line.
/// PaddleOCR reports boxes in detection order, not reading order, so the geometry has to
/// decide which boxes share a line and how the lines stack - without this a two-line cue
/// comes out in whatever order the detector happened to find it.
/// </summary>
public static class PaddleOcrTextLayout
{
    /// <summary>
    /// Joins the regions into the text of one subtitle line and reports the average
    /// confidence of what survived.
    /// </summary>
    /// <param name="minConfidencePercent">
    /// Regions below this recognition confidence are dropped (0 = keep everything). Video OCR
    /// turns it on, where low-confidence boxes are nearly always background clutter; subtitle
    /// bitmaps keep everything.
    /// </param>
    /// <param name="rightToLeft">Arabic-script languages, where the first word is the rightmost box.</param>
    public static string BuildText(
        IReadOnlyList<PaddleOcrTextRegion> regions, int minConfidencePercent, bool rightToLeft, out double confidence)
    {
        var kept = regions;
        if (minConfidencePercent > 0)
        {
            // A confidence of 0 means "not reported" (older output formats) - never drop those.
            var filtered = regions
                .Where(p => p.Confidence <= 0 || p.Confidence * 100.0 >= minConfidencePercent)
                .ToList();

            // The cut removes low-confidence clutter *next to* confident text. When nothing
            // clears the bar there is no confident text to prefer - dropping everything would
            // erase a short real subtitle ("Wait.") that the engine merely hesitated on, so
            // keep the frame's regions and let the low average confidence weigh the vote down.
            kept = filtered.Count > 0 ? filtered : regions;
        }

        if (kept.Count == 0)
        {
            confidence = 0;
            return string.Empty;
        }

        confidence = kept.Average(p => p.Confidence <= 0 ? 1.0 : p.Confidence);

        var sb = new StringBuilder();
        foreach (var line in MakeLines(kept, rightToLeft))
        {
            sb.AppendLine(string.Join(' ', line.Select(p => p.Text)));
        }

        return sb.ToString().Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
    }

    /// <summary>
    /// Groups the detected text boxes into visual lines by vertical overlap (two boxes
    /// share a line when either box's vertical midpoint falls inside the other), then
    /// orders lines top-to-bottom and the words within a line left-to-right - or
    /// right-to-left for Arabic-script languages, where the first word of the sentence
    /// is the rightmost box.
    /// </summary>
    public static List<List<PaddleOcrTextRegion>> MakeLines(
        IReadOnlyList<PaddleOcrTextRegion> input, bool rightToLeft)
    {
        var lines = new List<List<PaddleOcrTextRegion>>();
        foreach (var element in input)
        {
            List<PaddleOcrTextRegion>? home = null;
            foreach (var line in lines)
            {
                if (IsOnSameLine(line[0].BoundingBox, element.BoundingBox))
                {
                    home = line;
                    break;
                }
            }

            if (home == null)
            {
                lines.Add(new List<PaddleOcrTextRegion> { element });
            }
            else
            {
                home.Add(element);
            }
        }

        foreach (var line in lines)
        {
            line.Sort((a, b) => rightToLeft
                ? b.BoundingBox.TopLeft.X.CompareTo(a.BoundingBox.TopLeft.X)
                : a.BoundingBox.TopLeft.X.CompareTo(b.BoundingBox.TopLeft.X));
        }

        lines.Sort((a, b) => MinY(a).CompareTo(MinY(b)));
        return lines;
    }

    private static double MinY(List<PaddleOcrTextRegion> line)
    {
        var min = double.MaxValue;
        foreach (var element in line)
        {
            if (element.BoundingBox.MinY < min)
            {
                min = element.BoundingBox.MinY;
            }
        }

        return min;
    }

    private static bool IsOnSameLine(PaddleOcrBoundingBox a, PaddleOcrBoundingBox b)
    {
        var aMid = (a.MinY + a.MaxY) / 2.0;
        var bMid = (b.MinY + b.MaxY) / 2.0;
        return (a.MinY < bMid && bMid < a.MaxY) || (b.MinY < aMid && aMid < b.MaxY);
    }
}
