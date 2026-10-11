using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>One glyph as nOCR read it: the database entry that matched (null for a space, a line break or an unknown glyph).</summary>
public sealed class NOcrGlyphRead
{
    public NOcrChar? Entry { get; init; }
    public string Text { get; init; } = string.Empty;
    public NikseBitmap2? Bitmap { get; init; }
}

public sealed class NOcrDbErrorSample
{
    public int LineIndex { get; init; }
    public string Expected { get; init; } = string.Empty;
    public NikseBitmap2? Bitmap { get; init; }
}

public sealed class NOcrDbSuspectEntry
{
    public NOcrChar Entry { get; }
    public int Uses { get; internal set; }
    public int Errors { get; internal set; }
    public Dictionary<string, int> ExpectedCounts { get; } = new();
    public List<NOcrDbErrorSample> Samples { get; } = new();

    public NOcrDbSuspectEntry(NOcrChar entry)
    {
        Entry = entry;
    }

    public double ErrorRate => Uses == 0 ? 0 : (double)Errors / Uses;

    /// <summary>What the other engines read most often where this entry matched (empty = no character there).</summary>
    public string MostCommonExpected => ExpectedCounts.OrderByDescending(p => p.Value).Select(p => p.Key).FirstOrDefault() ?? string.Empty;

    /// <summary>
    /// The text the entry should have, when the other engines read the same thing in most places
    /// it matched; null when the entry is mostly right (it then also matches some other glyph).
    /// </summary>
    public string? SuggestedText
    {
        get
        {
            var best = ExpectedCounts.OrderByDescending(p => p.Value).FirstOrDefault();
            return best.Key is { Length: > 0 } && best.Value * 2 > Uses ? best.Key : null;
        }
    }
}

/// <summary>
/// Finds probably wrong entries in an nOCR database by checking real subtitle images against
/// other OCR engines ("controls"). Every character nOCR reads is traced back to the database
/// entry that matched it; where the controls read another character at that place, the entry
/// gets an error. Entries are scored only on lines where the controls give one reading (one
/// control, or several that agree), and only by character - spaces, italics, quote styles and
/// the case of size-only twins (o/O, s/S...) and I/l are not the entry's fault.
/// </summary>
public sealed class NOcrDbErrorFinder
{
    private const string CaseTwinLetters = "cCoOsSuUvVwWxXzZøØ";
    private const string BarLetters = "Il|";
    private const int MaxSamples = 8;

    private readonly Dictionary<NOcrChar, NOcrDbSuspectEntry> _entries = new(ReferenceEqualityComparer.Instance);

    public int LinesChecked { get; private set; }

    /// <summary>Lines where the controls gave one reading, so the entries could be scored.</summary>
    public int LinesCompared { get; private set; }

    public void AddLine(int lineIndex, IReadOnlyList<NOcrGlyphRead> glyphs, IReadOnlyList<string?> controlTexts)
    {
        LinesChecked++;

        var controls = controlTexts
            .Where(p => p != null)
            .Select(p => RemoveWhiteSpace(Normalize(p)))
            .ToList();
        if (controls.Count == 0 || controls.Any(p => p != controls[0]) || controls[0].Length == 0)
        {
            return;
        }

        var reference = controls[0];

        // nOCR's characters, each pointing back at the glyph that produced it
        var chars = new List<char>();
        var owners = new List<int>();
        for (var g = 0; g < glyphs.Count; g++)
        {
            foreach (var ch in RemoveWhiteSpace(Normalize(glyphs[g].Text)))
            {
                chars.Add(ch);
                owners.Add(g);
            }
        }

        if (chars.Count == 0)
        {
            return;
        }

        LinesCompared++;

        var wrong = new bool[glyphs.Count];
        var expected = new string[glyphs.Count];
        for (var g = 0; g < glyphs.Count; g++)
        {
            expected[g] = string.Empty;
        }

        var lastGlyph = -1;
        foreach (var (a, b) in Align(chars, reference))
        {
            if (a >= 0)
            {
                var g = owners[a];
                lastGlyph = g;
                if (b >= 0)
                {
                    expected[g] += reference[b];
                    if (!SameChar(chars[a], reference[b]))
                    {
                        wrong[g] = true;
                    }
                }
                else
                {
                    wrong[g] = true; // nOCR has a character the controls don't
                }
            }
            else if (b >= 0)
            {
                // A character only the controls read belongs to the glyph before it ("m" read where
                // the controls see "rn") - but only blames a glyph that was already wrong; after a
                // correct glyph it is more likely a glyph nOCR could not match at all.
                if (lastGlyph >= 0 && wrong[lastGlyph])
                {
                    expected[lastGlyph] += reference[b];
                }
            }
        }

        for (var g = 0; g < glyphs.Count; g++)
        {
            var entry = glyphs[g].Entry;
            if (entry == null)
            {
                continue;
            }

            if (!_entries.TryGetValue(entry, out var suspect))
            {
                suspect = new NOcrDbSuspectEntry(entry);
                _entries.Add(entry, suspect);
            }

            suspect.Uses++;
            if (!wrong[g])
            {
                continue;
            }

            suspect.Errors++;
            suspect.ExpectedCounts[expected[g]] = suspect.ExpectedCounts.GetValueOrDefault(expected[g]) + 1;
            if (suspect.Samples.Count < MaxSamples)
            {
                suspect.Samples.Add(new NOcrDbErrorSample { LineIndex = lineIndex, Expected = expected[g], Bitmap = glyphs[g].Bitmap });
            }
        }
    }

    /// <summary>
    /// Entries with at least one error, the ones most often wrong first. A high rate means the
    /// entry's text is wrong; a low one means the entry also matches some other glyph.
    /// </summary>
    public List<NOcrDbSuspectEntry> GetSuspects() =>
        _entries.Values
            .Where(p => p.Errors > 0)
            .OrderByDescending(p => p.ErrorRate)
            .ThenByDescending(p => p.Errors)
            .ToList();

    /// <summary>Stops tracking an entry that was changed or deleted.</summary>
    public void Forget(NOcrChar entry) => _entries.Remove(entry);

    /// <summary>
    /// Edit-distance alignment of nOCR's characters and the reference: pairs (a, b) in order,
    /// a = -1 for a reference-only character, b = -1 for an nOCR-only character.
    /// </summary>
    private static List<(int A, int B)> Align(List<char> chars, string reference)
    {
        var n = chars.Count;
        var m = reference.Length;
        var cost = new int[n + 1, m + 1];
        for (var i = 0; i <= n; i++)
        {
            cost[i, 0] = i;
        }

        for (var j = 0; j <= m; j++)
        {
            cost[0, j] = j;
        }

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var substitution = cost[i - 1, j - 1] + (SameChar(chars[i - 1], reference[j - 1]) ? 0 : 1);
                cost[i, j] = Math.Min(substitution, Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1));
            }
        }

        var pairs = new List<(int A, int B)>();
        int x = n, y = m;
        // Walking back, a reference-only character is taken before an equally cheap substitution,
        // so it ends up after the substituted character: "m" vs "rn" is m->r plus n, not r plus m->n.
        while (x > 0 || y > 0)
        {
            if (y > 0 && cost[x, y] == cost[x, y - 1] + 1 && (x == 0 || !SameChar(chars[x - 1], reference[y - 1])))
            {
                pairs.Add((-1, y - 1));
                y--;
            }
            else if (x > 0 && y > 0 && cost[x, y] == cost[x - 1, y - 1] + (SameChar(chars[x - 1], reference[y - 1]) ? 0 : 1))
            {
                pairs.Add((x - 1, y - 1));
                x--;
                y--;
            }
            else
            {
                pairs.Add((x - 1, -1));
                x--;
            }
        }

        pairs.Reverse();
        return pairs;
    }

    /// <summary>
    /// Removes tags and evens out the characters engines write differently (curly quotes,
    /// dash styles, the ellipsis character). White space is dropped by the caller.
    /// </summary>
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var s = HtmlUtil.RemoveHtmlTags(text, true);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            switch (ch)
            {
                case '‘':
                case '’':
                case '‚':
                case '′':
                case '`':
                case '´':
                    sb.Append('\'');
                    break;
                case '“':
                case '”':
                case '„':
                case '″':
                    sb.Append('"');
                    break;
                case '‐':
                case '‑':
                case '‒':
                case '–':
                case '—':
                case '―':
                case '−':
                    sb.Append('-');
                    break;
                case '…':
                    sb.Append("...");
                    break;
                case '\r':
                case '\n':
                case '\t':
                case ' ':
                    sb.Append(' ');
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Characters an entry cannot be blamed for mixing up: case twins that differ only in size,
    /// and I/l/| which are the same bar in most subtitle fonts - the OCR fix engine picks
    /// between those from the words around them.
    /// </summary>
    private static bool SameChar(char a, char b) =>
        a == b ||
        (char.ToLowerInvariant(a) == char.ToLowerInvariant(b) && CaseTwinLetters.Contains(a)) ||
        (BarLetters.Contains(a) && BarLetters.Contains(b));

    private static string RemoveWhiteSpace(string s) => string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
}
