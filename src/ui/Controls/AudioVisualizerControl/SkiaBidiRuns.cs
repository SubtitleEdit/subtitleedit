using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// Splits one line of text into directional runs in visual (left to right) order, so the Skia
/// waveform renderer can shape each run with an explicit HarfBuzz direction. HarfBuzz itself does
/// no bidi reordering: shaping "שלום 12 abc" as one right to left buffer draws "abc" as "cba" and
/// "12" as "21". This is a deliberately small subset of the Unicode bidi algorithm (UAX #9): one
/// embedding level, strong types L/R, numbers as weak left to right runs that count as R when
/// resolving neutrals (N1), W7 for numbers after Latin text, and neutrals taking their neighbours'
/// direction when both agree, else the paragraph direction. Enough for subtitle lines - no
/// explicit embedding controls or brackets.
/// </summary>
internal static class SkiaBidiRuns
{
    internal readonly record struct Run(string Text, bool RightToLeft);

    private enum Class : byte
    {
        Neutral,
        Left,
        Right,
        Number,
    }

    /// <summary>
    /// True when the text has a letter from a right to left script, i.e. a single left to right
    /// shaping pass would not draw it in the right order.
    /// </summary>
    public static bool HasStrongRightToLeft(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            if (Rune.IsLetter(rune) && IsRightToLeft(rune.Value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The directional runs of <paramref name="text"/> ordered left to right as they should be
    /// drawn. A left to right paragraph without right to left letters comes back as one run.
    /// </summary>
    public static List<Run> Split(string text, bool rightToLeftParagraph)
    {
        var runs = new List<Run>();
        if (text.Length == 0)
        {
            return runs;
        }

        if (!rightToLeftParagraph && !HasStrongRightToLeft(text))
        {
            runs.Add(new Run(text, false));
            return runs;
        }

        // Classify per UTF-16 code unit so run boundaries map straight back to substrings; the
        // second half of a surrogate pair and combining marks inherit the class before them.
        var classes = new Class[text.Length];
        var previous = Class.Neutral;
        var index = 0;
        while (index < text.Length)
        {
            var rune = Rune.GetRuneAt(text, index);
            var category = Rune.GetUnicodeCategory(rune);
            Class current;
            if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark or UnicodeCategory.Format)
            {
                current = previous;
            }
            else if (category == UnicodeCategory.DecimalDigitNumber)
            {
                current = Class.Number;
            }
            else if (Rune.IsLetter(rune) && IsRightToLeft(rune.Value))
            {
                current = Class.Right;
            }
            else if (Rune.IsLetter(rune) || category is UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber)
            {
                current = Class.Left;
            }
            else
            {
                current = Class.Neutral;
            }

            for (var k = 0; k < rune.Utf16SequenceLength; k++)
            {
                classes[index + k] = current;
            }

            previous = current;
            index += rune.Utf16SequenceLength;
        }

        // W4: a single separator between two digits is part of the number ("12:30", "1.5").
        for (var i = 1; i < classes.Length - 1; i++)
        {
            if (classes[i] == Class.Neutral && classes[i - 1] == Class.Number && classes[i + 1] == Class.Number && IsNumberSeparator(text[i]))
            {
                classes[i] = Class.Number;
            }
        }

        // W7: a number whose nearest preceding strong type is L (or the start of a left to right
        // paragraph) reads as part of the Latin text.
        var lastStrong = rightToLeftParagraph ? Class.Right : Class.Left;
        for (var i = 0; i < classes.Length; i++)
        {
            if (classes[i] == Class.Left || classes[i] == Class.Right)
            {
                lastStrong = classes[i];
            }
            else if (classes[i] == Class.Number && lastStrong == Class.Left)
            {
                classes[i] = Class.Left;
            }
        }

        // N1/N2: neutrals take the direction of their neighbours when both agree (numbers count as
        // R here), otherwise the paragraph direction. The ends of the line count as the paragraph
        // direction.
        var paragraph = rightToLeftParagraph ? Class.Right : Class.Left;
        var directions = new bool[classes.Length]; // true = right to left
        var i0 = 0;
        while (i0 < classes.Length)
        {
            if (classes[i0] != Class.Neutral)
            {
                directions[i0] = classes[i0] == Class.Right;
                i0++;
                continue;
            }

            var end = i0;
            while (end < classes.Length && classes[end] == Class.Neutral)
            {
                end++;
            }

            var before = i0 == 0 ? paragraph : ForNeutrals(classes[i0 - 1]);
            var after = end == classes.Length ? paragraph : ForNeutrals(classes[end]);
            var resolved = before == after ? before : paragraph;
            for (var i = i0; i < end; i++)
            {
                directions[i] = resolved == Class.Right;
            }

            i0 = end;
        }

        // Group into runs in logical order, then reverse the order for a right to left paragraph:
        // with one embedding level that is exactly the UAX #9 L2 reordering.
        var start = 0;
        for (var i = 1; i <= directions.Length; i++)
        {
            if (i == directions.Length || directions[i] != directions[start])
            {
                runs.Add(new Run(text.Substring(start, i - start), directions[start]));
                start = i;
            }
        }

        if (rightToLeftParagraph)
        {
            runs.Reverse();
        }

        return runs;
    }

    private static Class ForNeutrals(Class c)
    {
        return c == Class.Number ? Class.Right : c;
    }

    private static bool IsNumberSeparator(char c)
    {
        return c is '.' or ',' or ':' or '/' or ' ';
    }

    /// <summary>
    /// Letters of the right to left scripts: Hebrew, Arabic, Syriac, Thaana, N'Ko, Samaritan,
    /// Mandaic, the Arabic extended blocks and the Arabic presentation forms.
    /// </summary>
    private static bool IsRightToLeft(int cp)
    {
        return (cp >= 0x0590 && cp <= 0x08FF) ||
               (cp >= 0xFB1D && cp <= 0xFDFF) ||
               (cp >= 0xFE70 && cp <= 0xFEFF) ||
               (cp >= 0x10800 && cp <= 0x10FFF) ||
               (cp >= 0x1E800 && cp <= 0x1EFFF);
    }
}
