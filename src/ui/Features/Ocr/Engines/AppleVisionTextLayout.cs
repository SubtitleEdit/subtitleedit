using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// One recognized piece of text and where Vision found it.
/// </summary>
/// <remarks>
/// The coordinates are Vision's own: normalized to 0-1 of the image, with the origin at the
/// <em>bottom</em> left, so a larger Y is higher up the image. Kept in Vision's system rather
/// than flipped at the interop boundary, so the numbers here can be compared against what the
/// framework prints while debugging.
/// </remarks>
public readonly struct AppleVisionObservation
{
    public string Text { get; }
    public double Left { get; }
    public double Right { get; }
    public double Top { get; }
    public double Bottom { get; }

    public AppleVisionObservation(string text, double left, double right, double top, double bottom)
    {
        Text = text;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    internal double CenterY => (Top + Bottom) / 2;
    internal double Height => Math.Abs(Top - Bottom);
}

/// <summary>
/// Turns Vision's observations into subtitle text.
///
/// Vision returns a flat, unordered set of observations, and it splits one visual line into
/// several of them wherever the gap between words is wide - which is exactly what a two-speaker
/// dialogue line ("- Yes.        - No.") looks like. Reading the list in the order it arrives
/// would scramble such a line, and a naive "one observation per line" rule would break it in
/// two. So the observations are grouped back into visual lines by their vertical position, then
/// read left to right within each line.
/// </summary>
public static class AppleVisionTextLayout
{
    /// <summary>
    /// Two observations belong to the same visual line when their vertical centres are closer
    /// than this fraction of the shorter one's height. Half a line height is comfortably below
    /// the spacing of even tightly set subtitles, and comfortably above the wobble Vision leaves
    /// on parts of the same line.
    /// </summary>
    private const double SameLineToleranceOfHeight = 0.5;

    public static string Compose(IEnumerable<AppleVisionObservation> observations)
    {
        if (observations == null)
        {
            return string.Empty;
        }

        var ordered = observations
            .Where(o => !string.IsNullOrWhiteSpace(o.Text))
            .OrderByDescending(o => o.CenterY) // Vision's Y grows upwards, so this is top-first.
            .ToList();

        if (ordered.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var line = new List<AppleVisionObservation>();

        foreach (var observation in ordered)
        {
            if (line.Count > 0 && !BelongsToLine(line, observation))
            {
                AppendLine(sb, line);
                line.Clear();
            }

            line.Add(observation);
        }

        AppendLine(sb, line);

        return sb.ToString();
    }

    /// <summary>
    /// Compared against the line's lowest member rather than its first: on a line Vision split
    /// into several observations the pieces drift slightly, and measuring from the piece nearest
    /// the candidate keeps a long line from splitting halfway along.
    /// </summary>
    private static bool BelongsToLine(List<AppleVisionObservation> line, AppleVisionObservation candidate)
    {
        var nearest = line.MinBy(o => Math.Abs(o.CenterY - candidate.CenterY));
        var height = Math.Min(nearest.Height, candidate.Height);
        if (height <= 0)
        {
            return false;
        }

        return Math.Abs(nearest.CenterY - candidate.CenterY) < height * SameLineToleranceOfHeight;
    }

    private static void AppendLine(StringBuilder sb, List<AppleVisionObservation> line)
    {
        if (line.Count == 0)
        {
            return;
        }

        if (sb.Length > 0)
        {
            sb.Append(Environment.NewLine);
        }

        sb.Append(string.Join(" ", line.OrderBy(o => o.Left).Select(o => o.Text.Trim())));
    }
}
