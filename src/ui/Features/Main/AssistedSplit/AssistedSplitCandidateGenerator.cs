using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.AssistedSplit;

/// <summary>
/// Builds the ranked list of split suggestions shown in the "Assisted split" window.
/// Each raw candidate is a character index into the subtitle's text; the final text and
/// timing of both halves are produced by running the real <see cref="ISplitManager"/> on a
/// throw-away copy, so the previews match exactly what applying the split will do.
/// </summary>
public static class AssistedSplitCandidateGenerator
{
    private const int MaxCandidates = 5;
    private const char Lf = (char)10;
    private const char Cr = (char)13;

    public static List<AssistedSplitCandidate> Generate(SubtitleLineViewModel subtitle, string languageCode, ISplitManager splitManager)
    {
        var text = subtitle.Text ?? string.Empty;
        var result = new List<AssistedSplitCandidate>();
        if (text.Trim().Length < 4)
        {
            return result;
        }

        var inTag = MakeTagMask(text);
        var raw = new List<(int Index, string Title)>();

        // 1) Dialog dash / line breaks: split where the line is already broken.
        foreach (var nl in GetNewLineIndices(text))
        {
            var nextLine = text.Substring(SkipLineBreak(text, nl)).TrimStart();
            var isDialog = nextLine.StartsWith('-') || nextLine.StartsWith('–');
            raw.Add((nl, isDialog ? Se.Language.General.SplitAtDialogDash : Se.Language.General.SplitAtLineBreak));
        }

        // 2) Sentence ends.
        foreach (var idx in GetSentenceEndIndices(text, inTag))
        {
            raw.Add((idx, Se.Language.General.SplitAtSentenceEnd));
        }

        // 3) Comma nearest the middle.
        var commaIdx = GetBestCommaIndex(text, inTag);
        if (commaIdx > 0)
        {
            raw.Add((commaIdx, Se.Language.General.SplitAtComma));
        }

        // 4) Whitespace nearest the middle (always available as a fallback).
        var middleIdx = GetMiddleWhitespaceIndex(text, inTag);
        if (middleIdx > 0)
        {
            raw.Add((middleIdx, Se.Language.General.SplitNearMiddle));
        }

        // Rank: dialog first, then sentence ends, line breaks, comma, and even split last.
        var order = new List<string>
        {
            Se.Language.General.SplitAtDialogDash,
            Se.Language.General.SplitAtSentenceEnd,
            Se.Language.General.SplitAtLineBreak,
            Se.Language.General.SplitAtComma,
            Se.Language.General.SplitNearMiddle,
        };
        raw = raw.OrderBy(r => order.IndexOf(r.Title)).ThenBy(r => r.Index).ToList();

        var seen = new HashSet<string>();
        foreach (var (index, title) in raw)
        {
            if (result.Count >= MaxCandidates)
            {
                break;
            }

            var candidate = Simulate(subtitle, index, title, languageCode, splitManager);
            if (candidate == null)
            {
                continue;
            }

            var key = candidate.FirstText + "|" + candidate.SecondText;
            if (!seen.Add(key))
            {
                continue;
            }

            candidate.Number = result.Count + 1;
            result.Add(candidate);
        }

        return result;
    }

    /// <summary>
    /// Runs the real split on a copy so the preview (text, continuation style, tag fix-up,
    /// timing) is exactly what the user will get.
    /// </summary>
    private static AssistedSplitCandidate? Simulate(SubtitleLineViewModel subtitle, int textIndex, string title, string languageCode, ISplitManager splitManager)
    {
        var copy = new SubtitleLineViewModel(subtitle, true);
        var temp = new ObservableCollection<SubtitleLineViewModel> { copy };
        splitManager.Split(temp, copy, textIndex, languageCode);
        if (temp.Count != 2 ||
            string.IsNullOrWhiteSpace(temp[0].Text) ||
            string.IsNullOrWhiteSpace(temp[1].Text))
        {
            return null;
        }

        // Splitting in the middle of a dialog line can leave a half with an orphan
        // dash-only line (e.g. splitting "- A?/- B, c." at the comma) - not a useful option.
        if (HasDashOnlyLine(temp[0].Text) || HasDashOnlyLine(temp[1].Text))
        {
            return null;
        }

        return new AssistedSplitCandidate
        {
            Title = title,
            TextIndex = textIndex,
            FirstText = temp[0].Text,
            SecondText = temp[1].Text,
            FirstInfo = MakeInfo(temp[0]),
            SecondInfo = MakeInfo(temp[1]),
        };
    }

    private static bool HasDashOnlyLine(string text)
    {
        return HtmlUtil.RemoveHtmlTags(text, true)
            .SplitToLines()
            .Any(line => line.Trim() is "-" or "–");
    }

    private static string MakeInfo(SubtitleLineViewModel s)
    {
        var start = new TimeCode(s.StartTime).ToShortDisplayString();
        var end = new TimeCode(s.EndTime).ToShortDisplayString();
        var chars = CountVisibleCharacters(s.Text);
        var seconds = s.Duration.TotalSeconds;
        var cps = seconds > 0.001 ? chars / seconds : 0;
        return $"{start} → {end}      {chars} chars, {cps:0.#} CPS";
    }

    private static int CountVisibleCharacters(string text)
    {
        // Line breaks (CR/LF) are the only control characters left after tag stripping.
        var stripped = HtmlUtil.RemoveHtmlTags(text, true);
        return stripped.Count(c => !char.IsControl(c));
    }

    // True at positions inside "<...>" or "{...}" blocks, so split points never land inside a tag.
    private static bool[] MakeTagMask(string text)
    {
        var mask = new bool[text.Length];
        var inAngle = false;
        var inCurly = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '<')
            {
                inAngle = true;
            }
            else if (c == '{')
            {
                inCurly = true;
            }

            mask[i] = inAngle || inCurly;

            if (c == '>')
            {
                inAngle = false;
            }
            else if (c == '}')
            {
                inCurly = false;
            }
        }

        return mask;
    }

    private static List<int> GetNewLineIndices(string text)
    {
        var result = new List<int>();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == Lf || text[i] == Cr)
            {
                result.Add(i);
                i = SkipLineBreak(text, i) - 1;
            }
        }

        return result;
    }

    private static int SkipLineBreak(string text, int index)
    {
        if (index < text.Length && text[index] == Cr)
        {
            index++;
        }

        if (index < text.Length && text[index] == Lf)
        {
            index++;
        }

        return index;
    }

    private static IEnumerable<int> GetSentenceEndIndices(string text, bool[] inTag)
    {
        const string sentenceEnd = ".!?…";
        const string closers = "\"'”’»)]";
        for (var i = 0; i < text.Length; i++)
        {
            if (inTag[i] || !sentenceEnd.Contains(text[i]))
            {
                continue;
            }

            // Skip decimal numbers like "1.5" and web addresses like "nikse.dk".
            if (text[i] == '.' &&
                i > 0 && i + 1 < text.Length &&
                !char.IsWhiteSpace(text[i + 1]))
            {
                continue;
            }

            // Consume a run of end punctuation ("...", "?!") and trailing closing quotes.
            var end = i;
            while (end + 1 < text.Length && sentenceEnd.Contains(text[end + 1]))
            {
                end++;
            }

            while (end + 1 < text.Length && closers.Contains(text[end + 1]))
            {
                end++;
            }

            var splitIndex = end + 1;
            i = end;

            // Only a real sentence boundary: whitespace after, and more text following.
            if (splitIndex >= text.Length ||
                !char.IsWhiteSpace(text[splitIndex]) ||
                string.IsNullOrWhiteSpace(text.Substring(splitIndex)))
            {
                continue;
            }

            yield return splitIndex;
        }
    }

    private static int GetBestCommaIndex(string text, bool[] inTag)
    {
        var middle = text.Length / 2;
        var best = -1;
        for (var i = 1; i < text.Length - 1; i++)
        {
            if (inTag[i] || (text[i] != ',' && text[i] != '，') || !char.IsWhiteSpace(text[i + 1]))
            {
                continue;
            }

            if (best < 0 || Math.Abs(i - middle) < Math.Abs(best - middle))
            {
                best = i;
            }
        }

        return best < 0 ? -1 : best + 1;
    }

    private static int GetMiddleWhitespaceIndex(string text, bool[] inTag)
    {
        var middle = text.Length / 2;
        var best = -1;
        for (var i = 1; i < text.Length - 1; i++)
        {
            if (inTag[i] || !char.IsWhiteSpace(text[i]))
            {
                continue;
            }

            if (best < 0 || Math.Abs(i - middle) < Math.Abs(best - middle))
            {
                best = i;
            }
        }

        return best;
    }
}
