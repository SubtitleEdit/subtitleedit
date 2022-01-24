using System;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public static class TextLengthHelper
    {
        public static decimal CountCharacters(string text)
        {
            return CalcFactory.MakeCalculator(Configuration.Settings.General.CpsLineLengthStrategy).CountCharacters(text);
        }

        public static bool IsKnownHtmlTag(string input, int index)
        {
            var s = input.Remove(0, index + 1).ToLowerInvariant();
            return s.StartsWith('/') ||
                   s.StartsWith("i>", StringComparison.Ordinal) ||
                   s.StartsWith("b>", StringComparison.Ordinal) ||
                   s.StartsWith("u>", StringComparison.Ordinal) ||
                   s.StartsWith("font ", StringComparison.Ordinal) ||
                   s.StartsWith("ruby", StringComparison.Ordinal) ||
                   s.StartsWith("span>", StringComparison.Ordinal) ||
                   s.StartsWith("span ", StringComparison.Ordinal) ||
                   s.StartsWith("p>", StringComparison.Ordinal) ||
                   s.StartsWith("br>", StringComparison.Ordinal) ||
                   s.StartsWith("box>", StringComparison.Ordinal) ||
                   s.StartsWith("div>", StringComparison.Ordinal) ||
                   s.StartsWith("div ", StringComparison.Ordinal);
        }
    }
}