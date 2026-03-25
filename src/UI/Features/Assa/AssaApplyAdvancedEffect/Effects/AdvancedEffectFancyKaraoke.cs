using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Fancy karaoke effect for subtitles with a pre-marked active word (via {\u1} underline
/// or a highlight \1c color). Dims inactive words and pops the active word with a glow.
/// </summary>
public class AdvancedEffectFancyKaraoke : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFancyKaraoke;
    public string Description => Se.Language.Assa.AdvancedEffectFancyKaraokeDescription;
    public bool UsesAudio => false;
    public int InactiveAlpha { get; set; } = 100;
    public Color GlowColor { get; set; } = Color.FromRgb(0xFF, 0xDD, 0x00); // &H00DDFF& in ASS BGR

    public override string ToString() => Name;

    private static string ToAssColor(Color color) => $"&H{color.B:X2}{color.G:X2}{color.R:X2}&";

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var sub in subtitles)
        {
            var parsed = ParseActiveWord(sub.Text);
            var newSub = new SubtitleLineViewModel(sub, generateNewId: true);
            string posTags = ExtractPositionalTags(sub.Text);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(posTags)) sb.Append(posTags);

            string inactiveTags = $"{{\\alpha&H{InactiveAlpha:X2}&\\bord0\\shad0\\blur0\\fscx100\\fscy100}}";

            if (parsed == null || string.IsNullOrEmpty(parsed.Value.Active))
            {
                // No active word detected: dim the whole line to avoid a brightness blink
                // against adjacent subtitles that have partially dimmed text.
                string cleanText = Utilities.RemoveSsaTags(sub.Text)
                    .Replace("\r\n", "\\N").Replace("\r", "\\N").Replace("\n", "\\N");
                sb.Append(inactiveTags).Append(cleanText);
            }
            else
            {
                var (before, active, after) = parsed.Value;

                if (!string.IsNullOrEmpty(before))
                    sb.Append(inactiveTags).Append(before);

                sb.Append("{\\alpha&H00&\\1c&HFFFFFF&\\bord2\\shad0\\blur4\\3c" + ToAssColor(GlowColor) +
                          "\\fscx115\\fscy115\\t(0,250,\\fscx100\\fscy100)}").Append(active);

                if (!string.IsNullOrEmpty(after))
                    sb.Append(inactiveTags).Append(after);
            }

            newSub.Text = sb.ToString();
            result.Add(newSub);
        }
        return result;
    }

    private static string ExtractPositionalTags(string text)
    {
        var firstBlock = System.Text.RegularExpressions.Regex.Match(text, @"^\{([^}]*)\}");
        if (!firstBlock.Success) return string.Empty;
        string inner = firstBlock.Groups[1].Value;
        var sb = new StringBuilder("{");
        var anM = System.Text.RegularExpressions.Regex.Match(inner, @"\\an\d");
        if (anM.Success) sb.Append(anM.Value);
        var posM = System.Text.RegularExpressions.Regex.Match(inner, @"\\pos\([^)]+\)");
        if (posM.Success) sb.Append(posM.Value);
        var moveM = System.Text.RegularExpressions.Regex.Match(inner, @"\\move\([^)]+\)");
        if (moveM.Success) sb.Append(moveM.Value);
        sb.Append("}");
        return sb.Length > 2 ? sb.ToString() : string.Empty;
    }

    private static (string Before, string Active, string After)? ParseActiveWord(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var segments = BuildSegments(text);
        if (segments.Count == 0) return null;

        // Strategy 1: explicit underline {\u1}
        for (int i = 0; i < segments.Count; i++)
            if (System.Text.RegularExpressions.Regex.IsMatch(segments[i].Tags, @"\\u1")
                && !string.IsNullOrWhiteSpace(segments[i].Text))
                return BuildResult(segments, i);

        // Strategy 2: least-common \1c color is the highlight
        var colorCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (tags, _) in segments)
        {
            var m = System.Text.RegularExpressions.Regex.Match(tags, @"\\1?c(&H[0-9A-Fa-f]{6}&)");
            if (m.Success)
            {
                string c = m.Groups[1].Value.ToUpperInvariant();
                if (!colorCounts.TryGetValue(c, out int cnt)) cnt = 0;
                colorCounts[c] = cnt + 1;
            }
        }
        if (colorCounts.Count == 0) return null;

        string highlightColor;
        if (colorCounts.Count == 1)
        {
            // Only one color tag in the line — treat it as the active word only if it is
            // distinctly colorful (e.g. \c&H00FFFF& cyan). A plain white or grey tag is
            // just a style reset and should not be mistaken for a highlight.
            var singleColor = colorCounts.Keys.First();
            if (!IsColorfulAssColor(singleColor)) return null;
            highlightColor = singleColor;
        }
        else
        {
            highlightColor = colorCounts
                .OrderBy(p => p.Value)
                .ThenByDescending(p => IsColorfulAssColor(p.Key))
                .First().Key;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            var m = System.Text.RegularExpressions.Regex.Match(segments[i].Tags, @"\\1?c(&H[0-9A-Fa-f]{6}&)");
            if (m.Success
                && m.Groups[1].Value.Equals(highlightColor, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(segments[i].Text))
                return BuildResult(segments, i);
        }
        return null;
    }

    private static (string Before, string Active, string After) BuildResult(
        List<(string Tags, string Text)> segments, int activeIdx)
    {
        string before = string.Concat(segments.Take(activeIdx).Select(s => s.Text));
        string rawActive = segments[activeIdx].Text;
        string after = string.Concat(segments.Skip(activeIdx + 1).Select(s => s.Text));
        string active = rawActive.Trim();
        string leadingSpaces = rawActive[..(rawActive.Length - rawActive.TrimStart().Length)];
        string trailingSpaces = rawActive[rawActive.TrimEnd().Length..];
        return (before + leadingSpaces, active, trailingSpaces + after);
    }

    private static List<(string Tags, string Text)> BuildSegments(string text)
    {
        var result = new List<(string Tags, string Text)>();
        int pos = 0;
        while (pos < text.Length)
        {
            var tags = new StringBuilder();
            while (pos < text.Length && text[pos] == '{')
            {
                int end = text.IndexOf('}', pos);
                if (end < 0) break;
                tags.Append(text, pos, end - pos + 1);
                pos = end + 1;
            }
            int textStart = pos;
            while (pos < text.Length && text[pos] != '{') pos++;
            string plainText = text[textStart..pos];
            if (tags.Length > 0 || !string.IsNullOrEmpty(plainText))
                result.Add((tags.ToString(), plainText));
        }
        return result;
    }

    private static bool IsColorfulAssColor(string assColor)
    {
        string hex = assColor.Replace("&", "").Replace("H", "").ToUpperInvariant();
        if (hex.Length != 6) return false;
        int b = Convert.ToInt32(hex[0..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int r = Convert.ToInt32(hex[4..6], 16);
        return Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b)) > 40;
    }
}

