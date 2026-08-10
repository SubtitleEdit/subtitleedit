﻿using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public static class TextDiffHighlighter
{
    private static IBrush GetForegroundDifferenceColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Bright red for dark theme - easier to read
            return new SolidColorBrush(Color.FromRgb(255, 100, 100));
        }
        // Dark red for light theme
        return new SolidColorBrush(Color.FromRgb(183, 28, 28));
    }

    private static IBrush GetBackDifferenceColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Darker red background for dark theme
            return new SolidColorBrush(Color.FromRgb(80, 30, 30));
        }
        // Light pink background for light theme
        return new SolidColorBrush(Color.FromRgb(255, 235, 238));
    }

    private static IBrush GetForegroundAddedColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            return new SolidColorBrush(Color.FromRgb(100, 255, 100));
        }
        return new SolidColorBrush(Color.FromRgb(27, 94, 32));
    }

    private static IBrush GetBackAddedColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            return new SolidColorBrush(Color.FromRgb(30, 80, 30));
        }
        return new SolidColorBrush(Color.FromRgb(200, 250, 205));
    }

    private static IBrush GetDiffBackgroundColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Darker green background for dark theme
            return new SolidColorBrush(Color.FromRgb(30, 60, 30));
        }
        // Light green background for light theme
        return new SolidColorBrush(Color.FromRgb(230, 255, 237));
    }

    // Diff and render with line-feed line breaks only. Subtitle text can carry \r\n (libse joins
    // lines with Environment.NewLine, so CRLF on Windows) while regex-replaced text comes back
    // \n-normalized (RegexUtils.ReplaceNewLineSafe). Diffing the raw strings then isolates the \r
    // as a one-character "difference" run - and while \r\n inside a single Run renders as one
    // line break, a \r/\n pair split across two runs renders as two, showing a phantom empty
    // line (plus a red mark for the invisible \r) in the Multiple Replace preview (#12622).
    private static string NormalizeNewLines(string text)
    {
        if (string.IsNullOrEmpty(text) || (text.IndexOf('\r') < 0 && text.IndexOf('\u2028') < 0))
        {
            return text;
        }

        return string.Join("\n", text.SplitToLines());
    }

    // A text block laid out left to right places its runs left to right, so the chunks of a
    // right to left line come out in reverse reading order - the line reads backwards even
    // though each chunk on its own is fine. Each side follows its own content, so an Arabic
    // line still compares against a Latin one (#13435).
    private static TextBlock MakeTextBlock(string text)
    {
        return new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            FlowDirection = TextToFlowDirectionConverter.GetFlowDirection(text),
        };
    }

    public static (TextBlock left, TextBlock right) Compare(string text1, string text2)
    {
        text1 = NormalizeNewLines(text1);
        text2 = NormalizeNewLines(text2);

        var left = MakeTextBlock(text1);
        var right = MakeTextBlock(text2);

        if (left.Inlines == null || right.Inlines == null)
        {
            return (left, right);
        }

        if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
        {
            return (left, right);
        }

        if (string.IsNullOrEmpty(text1))
        {
            right.Inlines.Add(new Run(text2)
            {
                Foreground = GetForegroundDifferenceColor(),
                Background = GetBackDifferenceColor()
            });
            return (left, right);
        }

        if (string.IsNullOrEmpty(text2))
        {
            left.Inlines.Add(new Run(text1)
            {
                Foreground = GetForegroundDifferenceColor(),
                Background = GetBackDifferenceColor()
            });
            return (left, right);
        }

        // Use longest common substring to find the best match
        var (commonStart, commonEnd, middleCommon1, middleCommon2) = FindCommonParts(text1, text2);

        bool hasDifferences = commonStart != text1.Length || commonEnd != 0 || middleCommon1.Count > 0;

        if (!hasDifferences)
        {
            // Texts are identical - don't set Foreground to allow theme color inheritance
            left.Inlines.Add(new Run(text1));
            right.Inlines.Add(new Run(text2));
            return (left, right);
        }

        // Build the visual representation
        var redFg = GetForegroundDifferenceColor();
        var redBg = GetBackDifferenceColor();
        BuildDiffRuns(left, text1, commonStart, commonEnd, middleCommon1, hasDifferences, redFg, redBg);
        BuildDiffRuns(right, text2, commonStart, commonEnd, middleCommon2, hasDifferences, redFg, redBg);

        return (left, right);
    }

    public static (TextBlock before, TextBlock after) CompareReplacement(string text1, string text2)
    {
        text1 = NormalizeNewLines(text1);
        text2 = NormalizeNewLines(text2);

        var before = MakeTextBlock(text1);
        var after = MakeTextBlock(text2);

        if (before.Inlines == null || after.Inlines == null)
        {
            return (before, after);
        }

        if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
        {
            return (before, after);
        }

        if (string.IsNullOrEmpty(text1))
        {
            after.Inlines.Add(new Run(text2) { Foreground = GetForegroundAddedColor(), Background = GetBackAddedColor() });
            return (before, after);
        }

        if (string.IsNullOrEmpty(text2))
        {
            before.Inlines.Add(new Run(text1) { Foreground = GetForegroundDifferenceColor(), Background = GetBackDifferenceColor() });
            return (before, after);
        }

        var (commonStart, commonEnd, middleCommon1, middleCommon2) = FindCommonParts(text1, text2);
        var hasDifferences = commonStart != text1.Length || commonEnd != 0 || middleCommon1.Count > 0;

        if (!hasDifferences)
        {
            before.Inlines.Add(new Run(text1));
            after.Inlines.Add(new Run(text2));
            return (before, after);
        }

        BuildDiffRuns(before, text1, commonStart, commonEnd, middleCommon1, hasDifferences, GetForegroundDifferenceColor(), GetBackDifferenceColor());
        BuildDiffRuns(after, text2, commonStart, commonEnd, middleCommon2, hasDifferences, GetForegroundAddedColor(), GetBackAddedColor());

        return (before, after);
    }

    private static void BuildDiffRuns(TextBlock textBlock, string text, int commonStart, int commonEnd,
        List<(int start, int length)> middleCommon, bool hasDifferences, IBrush diffForeground, IBrush diffBackground)
    {
        // Mark which characters differ first, then emit one run per stretch. Going through a
        // mask instead of straight to runs is what lets the boundaries be moved (see
        // SnapToWordBoundaries) before anything is rendered.
        var isDiff = BuildDiffMask(text, commonStart, commonEnd, middleCommon);

        if (LanguageAutoDetect.ContainsRightToLeftLetter(text))
        {
            SnapToWordBoundaries(text, isDiff);
        }

        var commonBackground = hasDifferences ? GetDiffBackgroundColor() : null;

        var pos = 0;
        while (pos < text.Length)
        {
            var diff = isDiff[pos];
            var end = pos + 1;
            while (end < text.Length && isDiff[end] == diff)
            {
                end++;
            }

            textBlock.Inlines!.Add(new Run(text.Substring(pos, end - pos))
            {
                Foreground = diff ? diffForeground : null,
                Background = diff ? diffBackground : commonBackground,
            });

            pos = end;
        }
    }

    private static bool[] BuildDiffMask(string text, int commonStart, int commonEnd, List<(int start, int length)> middleCommon)
    {
        var isDiff = new bool[text.Length];
        var currentPos = Math.Clamp(commonStart, 0, text.Length);
        var middleEnd = Math.Clamp(text.Length - commonEnd, 0, text.Length);

        foreach (var (start, length) in middleCommon)
        {
            var commonPartStart = Math.Clamp(start, 0, text.Length);
            for (var i = currentPos; i < commonPartStart; i++)
            {
                isDiff[i] = true;
            }

            currentPos = Math.Clamp(commonPartStart + length, 0, text.Length);
        }

        for (var i = currentPos; i < middleEnd; i++)
        {
            isDiff[i] = true;
        }

        return isDiff;
    }

    /// <summary>
    /// Grows every difference to cover whole words. Arabic script letters join up and Avalonia
    /// shapes each run on its own, so a boundary inside a word forces the two halves into
    /// isolated letter forms - a Persian word comes out as loose, unconnected letters (#13435).
    /// Only applied to text holding right to left letters, so left to right diffs keep their
    /// finer character level granularity.
    /// </summary>
    private static void SnapToWordBoundaries(string text, bool[] isDiff)
    {
        var i = 0;
        while (i < text.Length)
        {
            if (!IsWordChar(text[i]))
            {
                i++;
                continue;
            }

            var wordStart = i;
            while (i < text.Length && IsWordChar(text[i]))
            {
                i++;
            }

            var wordHasDifference = false;
            for (var j = wordStart; j < i; j++)
            {
                if (isDiff[j])
                {
                    wordHasDifference = true;
                    break;
                }
            }

            if (!wordHasDifference)
            {
                continue;
            }

            for (var j = wordStart; j < i; j++)
            {
                isDiff[j] = true;
            }
        }
    }

    private static bool IsWordChar(char c)
    {
        // Zero width non-joiner and joiner sit inside Persian words (می‌رود), and Arabic
        // diacritics are non spacing marks - both belong to the word they are written in.
        return char.IsLetterOrDigit(c)
               || c == '\u200c'
               || c == '\u200d'
               || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark;
    }

    private static (int commonStart, int commonEnd, List<(int start, int length)> middleCommon1, List<(int start, int length)> middleCommon2) FindCommonParts(string text1, string text2)
    {
        int commonStart = 0;
        int minLength = Math.Min(text1.Length, text2.Length);

        // Find common prefix
        while (commonStart < minLength && text1[commonStart] == text2[commonStart])
        {
            commonStart++;
        }

        // If everything matches
        if (commonStart == text1.Length && commonStart == text2.Length)
        {
            return (commonStart, 0, new List<(int, int)>(), new List<(int, int)>());
        }

        // Find common suffix
        int commonEnd = 0;
        while (commonEnd < minLength - commonStart &&
               text1[text1.Length - 1 - commonEnd] == text2[text2.Length - 1 - commonEnd])
        {
            commonEnd++;
        }

        // Find common parts in the middle using longest common substring
        var middleCommon1 = new List<(int start, int length)>();
        var middleCommon2 = new List<(int start, int length)>();

        if (commonStart < text1.Length && commonStart < text2.Length)
        {
            string middle1 = text1.Substring(commonStart, text1.Length - commonStart - commonEnd);
            string middle2 = text2.Substring(commonStart, text2.Length - commonStart - commonEnd);

            if (middle1.Length > 0 && middle2.Length > 0)
            {
                // Find longest common substrings in the middle
                var commonSubstrings = FindLongestCommonSubstrings(middle1, middle2, 3); // Minimum length of 3

                // Convert positions back to original text positions and store both
                foreach (var (pos1, pos2, length) in commonSubstrings)
                {
                    middleCommon1.Add((commonStart + pos1, length));
                    middleCommon2.Add((commonStart + pos2, length));
                }

                // middleCommon1 is sorted by pos1 (text1 positions); sort middleCommon2 by its own positions
                // so BuildDiffRuns can process text2 in forward order
                middleCommon2.Sort((a, b) => a.start.CompareTo(b.start));
            }
        }

        return (commonStart, commonEnd, middleCommon1, middleCommon2);
    }

    private static List<(int pos1, int pos2, int length)> FindLongestCommonSubstrings(string s1, string s2, int minLength)
    {
        var result = new List<(int pos1, int pos2, int length)>();
        var used1 = new bool[s1.Length];
        var used2 = new bool[s2.Length];

        // Both selection routines pick the exact same cell (longest unused run; ties keep the
        // smallest (i, j) in row-major order, like the original scan did). The per-cell walk
        // has the lower constant on short texts but re-walks whole runs, which goes cubic on
        // long repetitive text (a freeze in the Compare window / Multiple Replace preview);
        // the anti-diagonal scan is O(n*m) per round, so it takes over above the threshold.
        var useDiagonalScan = (long)s1.Length * s2.Length >= 10_000;

        // Find multiple common substrings
        while (true)
        {
            var (maxLen, maxPos1, maxPos2) = useDiagonalScan
                ? FindLongestUnusedRunByDiagonals(s1, s2, used1, used2)
                : FindLongestUnusedRunByWalk(s1, s2, used1, used2);

            // If no common substring found or too short, stop
            if (maxLen < minLength)
            {
                break;
            }

            // Mark as used
            for (int k = 0; k < maxLen; k++)
            {
                used1[maxPos1 + k] = true;
                used2[maxPos2 + k] = true;
            }

            result.Add((maxPos1, maxPos2, maxLen));
        }

        // Sort by position in first string
        result.Sort((a, b) => a.pos1.CompareTo(b.pos1));

        return result;
    }

    private static (int maxLen, int maxPos1, int maxPos2) FindLongestUnusedRunByWalk(string s1, string s2, bool[] used1, bool[] used2)
    {
        int maxLen = 0;
        int maxPos1 = 0;
        int maxPos2 = 0;

        for (int i = 0; i < s1.Length; i++)
        {
            if (used1[i])
            {
                continue;
            }

            for (int j = 0; j < s2.Length; j++)
            {
                if (used2[j])
                {
                    continue;
                }

                int len = 0;
                while (i + len < s1.Length && j + len < s2.Length &&
                       !used1[i + len] && !used2[j + len] &&
                       s1[i + len] == s2[j + len])
                {
                    len++;
                }

                if (len > maxLen)
                {
                    maxLen = len;
                    maxPos1 = i;
                    maxPos2 = j;
                }
            }
        }

        return (maxLen, maxPos1, maxPos2);
    }

    private static (int maxLen, int maxPos1, int maxPos2) FindLongestUnusedRunByDiagonals(string s1, string s2, bool[] used1, bool[] used2)
    {
        int maxLen = 0;
        int maxPos1 = 0;
        int maxPos2 = 0;

        // Walking each anti-diagonal backward, the run length starting at (i, j) is just the
        // run below-right plus one - no per-cell re-walk, no table. The explicit tie-break
        // compensates for the diagonal visiting order.
        void WalkDiagonal(int startI, int startJ)
        {
            var steps = Math.Min(s1.Length - startI, s2.Length - startJ);
            var below = 0; // run length starting at (i + 1, j + 1)
            for (var t = steps - 1; t >= 0; t--)
            {
                int i = startI + t;
                int j = startJ + t;
                var run = !used1[i] && !used2[j] && s1[i] == s2[j] ? below + 1 : 0;
                below = run;

                if (run > maxLen ||
                    (run == maxLen && run > 0 && (i < maxPos1 || (i == maxPos1 && j < maxPos2))))
                {
                    maxLen = run;
                    maxPos1 = i;
                    maxPos2 = j;
                }
            }
        }

        for (int startI = 0; startI < s1.Length; startI++)
        {
            WalkDiagonal(startI, 0);
        }

        for (int startJ = 1; startJ < s2.Length; startJ++)
        {
            WalkDiagonal(0, startJ);
        }

        return (maxLen, maxPos1, maxPos2);
    }
}
