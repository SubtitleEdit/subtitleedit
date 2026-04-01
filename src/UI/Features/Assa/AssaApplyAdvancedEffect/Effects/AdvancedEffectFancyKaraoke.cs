using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Fancy karaoke effect for subtitles with a pre-marked active word (via {\u1} underline
/// or a highlight \1c color). Dims inactive words and pops the active word with a glow.
///
/// Set AutoDetectActiveWord = true to auto-sequence whole words (word-by-word karaoke).
/// Set ActiveWordColor to choose the color used for the active word.
/// Set ApplyGlow to enable/disable the glow effect for the active word.
/// </summary>
public class AdvancedEffectFancyKaraoke : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFancyKaraoke;
    public string Description => Se.Language.Assa.AdvancedEffectFancyKaraokeDescription;
    public bool UsesAudio => false;

    /// <summary>
    /// When true, ignore explicit active-word markup and auto-sequence whole words
    /// (one subtitle per word, timing = duration / wordCount).
    /// </summary>
    public bool AutoDetectActiveWord { get; set; } = false;

    /// <summary>
    /// Alpha value used for inactive words (ASS \alpha uses hex).
    /// </summary>
    public int InactiveAlpha { get; set; } = 100;

    /// <summary>
    /// Glow color used for the active word's 3c (ASS BGR).
    /// </summary>
    public Color GlowColor { get; set; } = Color.FromRgb(0xFF, 0xDD, 0x00); // &H00DDFF& in ASS BGR

    /// <summary>
    /// Color used for the active word's primary color (\1c). Default white.
    /// </summary>
    public Color ActiveWordColor { get; set; } = Color.FromRgb(0xFF, 0xFF, 0xFF);

    /// <summary>
    /// Color used for inactive words' primary color (\1c). Default dim gray.
    /// This ensures color is explicitly reset after the active word so subsequent words are not colored.
    /// </summary>
    public Color InactiveWordColor { get; set; } = Color.FromRgb(0x80, 0x80, 0x80);

    /// <summary>
    /// When true, apply the glow (3c, blur, scale transform) to the active word.
    /// When false, the active word will be colored but without the glow/pop styling.
    /// </summary>
    public bool ApplyGlow { get; set; } = true;

    public override string ToString() => Name;

    private static string ToAssColor(Color color) => $"&H{color.B:X2}{color.G:X2}{color.R:X2}&";

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var sub in subtitles)
        {
            // If auto mode is enabled, attempt auto-sequencing by words.
            if (AutoDetectActiveWord)
            {
                var auto = TryAutoSequenceByWords(sub);
                if (auto != null)
                {
                    result.AddRange(auto);
                    continue;
                }
                // If auto failed (e.g., no words), fall through to normal behavior.
            }

            // Normal behavior: parse active word from explicit markup and render single line.
            var parsed = ParseActiveWord(sub.Text);
            var newSub = new SubtitleLineViewModel(sub, generateNewId: true);
            string posTags = ExtractPositionalTags(sub.Text);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(posTags)) sb.Append(posTags);

            // Inactive tags now explicitly set \1c to InactiveWordColor so color resets after active word.
            string inactiveTags = $"{{\\alpha&H{InactiveAlpha:X2}&\\1c{ToAssColor(InactiveWordColor)}\\bord0\\shad0\\blur0\\fscx100\\fscy100}}";

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

                // Build active tag depending on ApplyGlow
                string activeTag;
                if (ApplyGlow)
                {
                    activeTag = "{\\alpha&H00&\\1c" + ToAssColor(ActiveWordColor) +
                                "\\bord2\\shad0\\blur4\\3c" + ToAssColor(GlowColor) +
                                "\\fscx115\\fscy115\\t(0,250,\\fscx100\\fscy100)}";
                }
                else
                {
                    // No glow: only set alpha and primary color; keep border/shadow minimal
                    activeTag = "{\\alpha&H00&\\1c" + ToAssColor(ActiveWordColor) +
                                "\\bord0\\shad0\\blur0\\fscx100\\fscy100}";
                }

                sb.Append(activeTag).Append(active);

                if (!string.IsNullOrEmpty(after))
                    sb.Append(inactiveTags).Append(after);
            }

            newSub.Text = sb.ToString();
            result.Add(newSub);
        }
        return result;
    }

    /// <summary>
    /// Attempt to auto-sequence the subtitle by whole words.
    /// Returns a list of generated SubtitleLineViewModel if successful, otherwise null.
    /// </summary>
    private List<SubtitleLineViewModel>? TryAutoSequenceByWords(SubtitleLineViewModel sub)
    {
        // Remove SSA tags for tokenization and normalize newlines.
        var cleanText = Utilities.RemoveSsaTags(sub.Text);
        if (string.IsNullOrWhiteSpace(cleanText)) return null;
        cleanText = cleanText.Replace("\r\n", "\n").Replace("\r", "\n");

        // Tokenize into runs of non-whitespace (words/punctuation) and whitespace (spaces/newlines)
        var matches = Regex.Matches(cleanText, @"\S+|\s+");
        var tokens = new List<string>(matches.Count);
        foreach (Match m in matches) tokens.Add(m.Value);

        // Count words (non-whitespace tokens)
        var wordCount = tokens.Count(t => !string.IsNullOrWhiteSpace(t));
        if (wordCount == 0) return null;

        var totalMs = sub.Duration.TotalMilliseconds;
        var msPerWord = totalMs / wordCount;

        string posTags = ExtractPositionalTags(sub.Text);
        // Inactive tags explicitly include InactiveWordColor so color is reset for non-active words and spaces.
        string inactiveTags = $"{{\\alpha&H{InactiveAlpha:X2}&\\1c{ToAssColor(InactiveWordColor)}\\bord0\\shad0\\blur0\\fscx100\\fscy100}}";

        // Build active tag depending on ApplyGlow
        string activeTags;
        if (ApplyGlow)
        {
            activeTags = "{\\alpha&H00&\\1c" + ToAssColor(ActiveWordColor) +
                         "\\bord2\\shad0\\blur4\\3c" + ToAssColor(GlowColor) +
                         "\\fscx115\\fscy115\\t(0,250,\\fscx100\\fscy100)}";
        }
        else
        {
            activeTags = "{\\alpha&H00&\\1c" + ToAssColor(ActiveWordColor) +
                         "\\bord0\\shad0\\blur0\\fscx100\\fscy100}";
        }

        var result = new List<SubtitleLineViewModel>();

        for (int w = 0; w < wordCount; w++)
        {
            var line = new SubtitleLineViewModel(sub, generateNewId: true);
            var start = sub.StartTime.Add(TimeSpan.FromMilliseconds(w * msPerWord));
            var end = w == wordCount - 1
                ? sub.EndTime
                : sub.StartTime.Add(TimeSpan.FromMilliseconds((w + 1) * msPerWord));
            line.StartTime = start;
            line.EndTime = end;

            // Build text for this word index
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(posTags)) sb.Append(posTags);

            string? currentColor = null;
            int localWordCounter = 0;

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    // whitespace may contain newlines; convert '\n' to SSA line break and ensure whitespace is dimmed
                    if (token.Contains("\n"))
                    {
                        var parts = token.Split('\n');
                        for (var p = 0; p < parts.Length; p++)
                        {
                            if (parts[p].Length > 0)
                            {
                                // Ensure spaces before newline are dimmed
                                if (currentColor != inactiveTags)
                                {
                                    sb.Append(inactiveTags);
                                    currentColor = inactiveTags;
                                }
                                sb.Append(parts[p]);
                            }
                            if (p < parts.Length - 1)
                            {
                                sb.Append(@"\N");
                                currentColor = null; // reset color after line break
                            }
                        }
                    }
                    else
                    {
                        // regular spaces: ensure they are dimmed (inactive)
                        if (currentColor != inactiveTags)
                        {
                            sb.Append(inactiveTags);
                            currentColor = inactiveTags;
                        }
                        sb.Append(token);
                    }
                    continue;
                }

                // Non-whitespace token = a word/punctuation group
                // Only the current word is active
                var colorTag = localWordCounter == w ? activeTags : inactiveTags;
                if (colorTag != currentColor)
                {
                    sb.Append(colorTag);
                    currentColor = colorTag;
                }
                sb.Append(token);
                localWordCounter++;
            }

            line.Text = sb.ToString();
            result.Add(line);
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
