using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// Display-only bidi workaround for ASSA override tags ("{\an8}", "{\pos(320,50)}", ...) in
/// right-to-left paragraphs.
///
/// The backslash and other neutral characters inside a tag have no strong direction, so the
/// Unicode bidi algorithm resolves them to the paragraph (RTL) level and displays them on the
/// wrong side of the letter/digit runs: "{\an8}" renders as "{an8\}". There is no public
/// Avalonia API to override bidi levels per run, so instead the tag content is pre-permuted in
/// the string given to the text layout such that the bidi reordering puts every character back
/// in its true visual spot. The stored text is never touched - only what the presenter renders.
///
/// How the permutation works: within a tag, bidi keeps letter/digit runs intact (left-to-right)
/// and reverses the display order of the top-level segments (a segment is a letter/digit run or
/// a single neutral character); neutral brackets are additionally mirrored. So the layout string
/// holds the segments in reverse order (with neutral brackets swapped for their mirror pair),
/// and the renderer's reversal restores the original order. Which characters join a letter/digit
/// run is decided by a small UAX#9 subset (W4/W7 for numbers, N1 for neutrals between strong
/// characters) - verified against Avalonia's actual renderer for the ASSA tag grammar.
/// </summary>
public static class AssaTagRtlLayout
{
    /// <summary>
    /// Permutes the content of every "{\...}" override block for RTL display. Returns false when
    /// the text contains no override blocks (the common case - no allocation happens then).
    /// <paramref name="toSource"/> maps each index of <paramref name="layoutText"/> back to its
    /// index in <paramref name="text"/>, for remapping style spans; it is the identity outside
    /// override blocks.
    /// </summary>
    public static bool TryPermuteForRtlDisplay(string text, out string layoutText, out int[] toSource)
    {
        layoutText = text;
        toSource = Array.Empty<int>();

        var firstBlock = FindBlockStart(text, 0);
        if (firstBlock < 0)
        {
            return false;
        }

        var chars = text.ToCharArray();
        var map = new int[text.Length];
        for (var i = 0; i < map.Length; i++)
        {
            map[i] = i;
        }

        var changed = false;
        var blockStart = firstBlock;
        while (blockStart >= 0)
        {
            // A run of directly adjacent override blocks ("{\an8}{\i1}") reorders as one bidi
            // region, so it must be permuted as one region - braces included.
            var regionEnd = text.IndexOf('}', blockStart + 1);
            if (regionEnd < 0)
            {
                break;
            }

            while (regionEnd + 1 < text.Length && text[regionEnd + 1] == '{')
            {
                var nextEnd = text.IndexOf('}', regionEnd + 2);
                var nextBackslash = text.IndexOf('\\', regionEnd + 2);
                if (nextEnd < 0 || nextBackslash < 0 || nextBackslash > nextEnd)
                {
                    break;
                }

                regionEnd = nextEnd;
            }

            changed |= PermuteRegion(text, blockStart, regionEnd + 1, chars, map);
            blockStart = FindBlockStart(text, regionEnd + 1);
        }

        if (!changed)
        {
            return false;
        }

        layoutText = new string(chars);
        toSource = map;
        return true;
    }

    /// <summary>
    /// An override block is "{" with a "\" before the next "}" - a plain "{...}" without
    /// override tags is left alone.
    /// </summary>
    private static int FindBlockStart(string text, int from)
    {
        var start = text.IndexOf('{', from);
        while (start >= 0)
        {
            var end = text.IndexOf('}', start + 1);
            if (end < 0)
            {
                return -1;
            }

            var backslash = text.IndexOf('\\', start + 1);
            if (backslash > start && backslash < end)
            {
                return start;
            }

            start = text.IndexOf('{', end + 1);
        }

        return -1;
    }

    private enum CharClass
    {
        Strong, // renders as an intact left-to-right run (letters, and digits attached to them)
        Number, // digit run without a preceding letter - still left-to-right but neutral-hostile
        Neutral, // takes the paragraph (RTL) level and gets displaced
    }

    private static bool PermuteRegion(string text, int regionStart, int regionEnd, char[] chars, int[] map)
    {
        var length = regionEnd - regionStart;
        if (length <= 1)
        {
            return false;
        }

        // 1) Initial classes for the region characters (ASCII tag grammar). Braces always
        // resolve to the RTL level in a right-to-left paragraph (paired-bracket rule N0: they
        // enclose left-to-right tag content in a right-to-left context), so they are never
        // upgraded below and act as barriers for the N1 upgrade.
        var classes = new CharClass[length];
        var isBrace = new bool[length];
        for (var i = 0; i < length; i++)
        {
            var ch = text[regionStart + i];
            isBrace[i] = ch is '{' or '}';
            classes[i] = char.IsLetter(ch) ? CharClass.Strong
                : char.IsDigit(ch) ? CharClass.Number
                : CharClass.Neutral;
        }

        // 2) W4: a single '.' ',' ':' between two digits joins the number.
        for (var i = 1; i < length - 1; i++)
        {
            var ch = text[regionStart + i];
            if ((ch == '.' || ch == ',' || ch == ':') &&
                classes[i - 1] is CharClass.Number or CharClass.Strong && char.IsDigit(text[regionStart + i - 1]) &&
                classes[i + 1] == CharClass.Number && char.IsDigit(text[regionStart + i + 1]))
            {
                classes[i] = classes[i - 1];
            }
        }

        // 3) W7: numbers become part of the letter run when the last strong character before
        // them is a letter (the context before the region is RTL text/start-of-line, so numbers
        // at the very front stay Number).
        var lastStrongIsLetter = false;
        for (var i = 0; i < length; i++)
        {
            if (classes[i] == CharClass.Strong)
            {
                lastStrongIsLetter = true;
            }
            else if (classes[i] == CharClass.Number && lastStrongIsLetter)
            {
                classes[i] = CharClass.Strong;
            }
        }

        // 4) N1: a run of neutrals with Strong on both sides joins the strong run - unless the
        // run contains a brace (see N0 note above). Number does not count as Strong here
        // (UAX#9 treats EN as R for this rule).
        var i2 = 0;
        while (i2 < length)
        {
            if (classes[i2] != CharClass.Neutral)
            {
                i2++;
                continue;
            }

            var runStart = i2;
            var containsBrace = false;
            while (i2 < length && classes[i2] == CharClass.Neutral)
            {
                containsBrace |= isBrace[i2];
                i2++;
            }

            var strongBefore = runStart > 0 && classes[runStart - 1] == CharClass.Strong;
            var strongAfter = i2 < length && classes[i2] == CharClass.Strong;
            if (strongBefore && strongAfter && !containsBrace)
            {
                for (var j = runStart; j < i2; j++)
                {
                    classes[j] = CharClass.Strong;
                }
            }
        }

        // 5) Split into display segments: maximal left-to-right runs (Strong/Number) stay
        // intact; every remaining neutral is its own segment.
        var segments = new List<(int Start, int Length, bool IsNeutral)>();
        var pos = 0;
        while (pos < length)
        {
            if (classes[pos] == CharClass.Neutral)
            {
                segments.Add((pos, 1, true));
                pos++;
                continue;
            }

            var runStart = pos;
            while (pos < length && classes[pos] != CharClass.Neutral)
            {
                pos++;
            }

            segments.Add((runStart, pos - runStart, false));
        }

        if (segments.Count <= 1)
        {
            return false; // a single run renders correctly as-is
        }

        // 6) Write the segments in reverse order; neutral brackets are swapped for their mirror
        // pair because the renderer will mirror them at the RTL level.
        var write = regionStart;
        var changed = false;
        for (var s = segments.Count - 1; s >= 0; s--)
        {
            var (segStart, segLength, isNeutral) = segments[s];
            for (var j = 0; j < segLength; j++)
            {
                var sourceIndex = regionStart + segStart + j;
                var ch = text[sourceIndex];
                if (isNeutral)
                {
                    ch = MirrorPair(ch);
                }

                if (chars[write] != ch || map[write] != sourceIndex)
                {
                    changed = true;
                }

                chars[write] = ch;
                map[write] = sourceIndex;
                write++;
            }
        }

        return changed;
    }

    private static char MirrorPair(char ch) => ch switch
    {
        '(' => ')',
        ')' => '(',
        '<' => '>',
        '>' => '<',
        '[' => ']',
        ']' => '[',
        '{' => '}',
        '}' => '{',
        _ => ch,
    };
}
