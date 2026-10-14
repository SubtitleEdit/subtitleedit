using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// The ASSA lines ASSA draw writes for text and outlines.
/// <para>
/// Text goes on its own line, on the layer of its shape and right after the drawing, so it shows
/// on top of the bubble: <c>{\an5\pos(x,y)\q2\fnArial\fs48\c&amp;H000000&amp;\bord0\shad0}{assadraw-text box=l,t,r,b fit=48}Hi\nthere</c>.
/// The comment block keeps the text area (and the largest size for shrink to fit) so the dialog
/// can open the text again. Wrapping is done here (\q2): \n is a wrap, \N a line break the user typed.
/// </para>
/// </summary>
public static class AssaDrawTextLine
{
    public const string StyleName = "AssaDrawText";

    private static readonly Regex MarkerRegex = new(
        @"\{assadraw-text box=(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+)(?: fit=([\d.]+))?\}",
        RegexOptions.Compiled);
    private static readonly Regex TagBlockRegex = new(@"\{[^}]*\}", RegexOptions.Compiled);
    private static readonly Regex PosRegex = new(@"\\pos\(\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*\)", RegexOptions.Compiled);
    private static readonly Regex FontNameRegex = new(@"\\fn([^\\}]+)", RegexOptions.Compiled);
    private static readonly Regex RotationRegex = new(@"\\frz?(-?[\d.]+)", RegexOptions.Compiled);
    private static readonly Regex FontSizeRegex = new(@"\\fs([\d.]+)", RegexOptions.Compiled);
    private static readonly Regex BoldRegex = new(@"\\b(\d+)", RegexOptions.Compiled);
    private static readonly Regex PrimaryColorRegex = new(@"\\1?c&H([0-9a-fA-F]+)&?", RegexOptions.Compiled);
    private static readonly Regex PrimaryAlphaRegex = new(@"\\1a&H([0-9a-fA-F]+)&?", RegexOptions.Compiled);
    private static readonly Regex OutlineWidthRegex = new(@"\\bord([\d.]+)", RegexOptions.Compiled);
    private static readonly Regex OutlineColorRegex = new(@"\\3c&H([0-9a-fA-F]+)&?", RegexOptions.Compiled);
    private static readonly Regex OutlineAlphaRegex = new(@"\\3a&H([0-9a-fA-F]+)&?", RegexOptions.Compiled);

    public sealed class Parsed
    {
        public string Text { get; set; } = string.Empty;
        public (float Left, float Top, float Right, float Bottom) Box { get; set; }
        public string FontName { get; set; } = "Arial";
        public float FontSize { get; set; } = 48;

        /// <summary>
        /// Degrees clockwise on screen (\frz is counter-clockwise).
        /// </summary>
        public float Angle { get; set; }
        public bool Bold { get; set; }
        public bool ShrinkToFit { get; set; }
        public Color? Color { get; set; }
        public float OutlineWidth { get; set; }
        public Color OutlineColor { get; set; } = Colors.Black;
    }

    /// <summary>
    /// A line written by ASSA draw for text (it carries the text area comment).
    /// </summary>
    public static bool IsTextLine(string text) => !string.IsNullOrEmpty(text) && MarkerRegex.IsMatch(text);

    /// <summary>
    /// The text line for a shape with text; outline only for a text box (a bubble's outline is the bubble's).
    /// </summary>
    public static string Format(DrawShape shape)
    {
        var (left, top, right, bottom) = shape.GetTextBounds();
        var angle = shape.GetTextFrame().Angle;
        var layout = AssaTextLayout.Layout(shape.Text, shape.FontName, shape.FontSize, shape.FontBold, right - left, bottom - top, shape.ShrinkToFit);

        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"{{\\an5\\pos({N((left + right) / 2)},{N((top + bottom) / 2)})\\q2\\fn{shape.FontName}\\fs{N(layout.FontSize)}");
        if (Math.Abs(angle) >= 0.01f)
        {
            // Turns around \pos (the center) - the text area is stored unrotated
            sb.Append(CultureInfo.InvariantCulture, $"\\frz{N(-angle)}");
        }

        if (shape.FontBold)
        {
            sb.Append("\\b1");
        }

        sb.Append("\\c").Append(ToAssaColor(shape.TextColor));
        if (shape.TextColor.A < 255)
        {
            sb.Append("\\1a").Append(ToAssaAlpha(shape.TextColor));
        }

        AppendOutline(sb, shape.IsTextBox ? shape.OutlineWidth : 0, shape.OutlineColor);
        sb.Append("\\shad0}");

        sb.Append(CultureInfo.InvariantCulture, $"{{assadraw-text box={N(left)},{N(top)},{N(right)},{N(bottom)}");
        if (shape.ShrinkToFit)
        {
            sb.Append(CultureInfo.InvariantCulture, $" fit={N(shape.FontSize)}");
        }

        sb.Append('}');

        // Wrapped lines of one paragraph are joined with the soft \n, paragraphs with \N
        var paragraphs = shape.Text.Replace("\r\n", "\n").Split('\n');
        var lineIndex = 0;
        for (var p = 0; p < paragraphs.Length; p++)
        {
            if (p > 0)
            {
                sb.Append("\\N");
            }

            var wordCount = paragraphs[p].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var first = true;
            var words = 0;
            while (lineIndex < layout.Lines.Count && (first || words < wordCount))
            {
                var line = layout.Lines[lineIndex++];
                if (!first)
                {
                    sb.Append("\\n");
                }

                sb.Append(Escape(line));
                words += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                first = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// "\bord2\3c&amp;H000000&amp;" (and \3a when translucent), or "\bord0".
    /// </summary>
    public static void AppendOutline(StringBuilder sb, float width, Color color)
    {
        sb.Append(CultureInfo.InvariantCulture, $"\\bord{N(Math.Max(0, width))}");
        if (width > 0)
        {
            sb.Append("\\3c").Append(ToAssaColor(color));
            if (color.A < 255)
            {
                sb.Append("\\3a").Append(ToAssaAlpha(color));
            }
        }
    }

    public static bool TryParse(string text, out Parsed result)
    {
        result = new Parsed();
        var marker = MarkerRegex.Match(text ?? string.Empty);
        if (!marker.Success)
        {
            return false;
        }

        var box = (F(marker.Groups[1].Value), F(marker.Groups[2].Value), F(marker.Groups[3].Value), F(marker.Groups[4].Value));
        var tags = string.Concat(TagBlockRegex.Matches(text!).Select(m => m.Value));

        // \pos wins over the comment, so a line moved in the grid (or resampled) keeps its new place
        var pos = PosRegex.Match(tags);
        if (pos.Success)
        {
            var dx = F(pos.Groups[1].Value) - (box.Item1 + box.Item3) / 2;
            var dy = F(pos.Groups[2].Value) - (box.Item2 + box.Item4) / 2;
            box = (box.Item1 + dx, box.Item2 + dy, box.Item3 + dx, box.Item4 + dy);
        }

        result.Box = box;
        if (FontNameRegex.Match(tags) is { Success: true } fontName)
        {
            result.FontName = fontName.Groups[1].Value.Trim();
        }

        if (marker.Groups[5].Success)
        {
            result.ShrinkToFit = true;
            result.FontSize = F(marker.Groups[5].Value);
        }
        else if (FontSizeRegex.Match(tags) is { Success: true } fontSize)
        {
            result.FontSize = F(fontSize.Groups[1].Value);
        }

        if (RotationRegex.Match(tags) is { Success: true } rotation)
        {
            result.Angle = -F(rotation.Groups[1].Value);
        }

        result.Bold = BoldRegex.Match(tags) is { Success: true } bold && bold.Groups[1].Value != "0";

        if (PrimaryColorRegex.Match(tags) is { Success: true } color)
        {
            var alpha = PrimaryAlphaRegex.Match(tags);
            result.Color = FromAssaColor(color.Groups[1].Value, alpha.Success ? alpha.Groups[1].Value : null);
        }

        ParseOutline(tags, out var outlineWidth, out var outlineColor);
        result.OutlineWidth = outlineWidth;
        result.OutlineColor = outlineColor;

        var body = TagBlockRegex.Replace(text!, string.Empty);
        result.Text = body.Replace("\\N", "\n").Replace("\\n", " ").Replace("\\h", " ").Replace('\u29F5', '\\');
        return true;
    }

    /// <summary>
    /// \bord and \3c (\3a) from a line's tags; width 0 when there are none.
    /// </summary>
    public static void ParseOutline(string text, out float width, out Color color)
    {
        width = 0;
        color = Colors.Black;
        var tags = string.Concat(TagBlockRegex.Matches(text).Select(m => m.Value));
        if (OutlineWidthRegex.Match(tags) is { Success: true } bord)
        {
            width = F(bord.Groups[1].Value);
        }

        if (OutlineColorRegex.Match(tags) is { Success: true } outline)
        {
            var alpha = OutlineAlphaRegex.Match(tags);
            color = FromAssaColor(outline.Groups[1].Value, alpha.Success ? alpha.Groups[1].Value : null);
        }
    }

    public static string ToAssaColor(Color color) => $"&H{color.B:X2}{color.G:X2}{color.R:X2}&";

    public static string ToAssaAlpha(Color color) => $"&H{255 - color.A:X2}&";

    private static Color FromAssaColor(string hex, string? alphaHex)
    {
        var value = uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        var alpha = alphaHex != null && byte.TryParse(alphaHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a)
            ? (byte)(255 - a)
            : (byte)255;
        return Color.FromArgb(alpha, (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF));
    }

    /// <summary>
    /// Braces would start an override block, a backslash a tag.
    /// </summary>
    private static string Escape(string text) => text.Replace('{', '(').Replace('}', ')').Replace("\\", "\u29F5");

    private static string N(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static float F(string value) => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0;
}
