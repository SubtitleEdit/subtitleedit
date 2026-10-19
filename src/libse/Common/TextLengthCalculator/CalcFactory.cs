using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public static class CalcFactory
    {
        public static List<ICalcLength> Calculators = new List<ICalcLength>
        {
            new CalcAll(),
            new CalcNoSpaceCpsOnly(),
            new CalcNoSpace(),
            new CalcCjk(),
            new CalcCjkNoSpace(),
            new CalcIncludeCompositionCharacters(),
            new CalcIncludeCompositionCharactersNotSpace(),
            new CalcNoSpaceOrPunctuation(),
            new CalcNoSpaceOrPunctuationCpsOnly(),
            new CalcIgnoreArabicDiacritics(),
            new CalcIgnoreArabicDiacriticsNoSpace(),
        };

        // Memo of the last strategy resolution. A single immutable pair swapped by reference
        // keeps the lookup thread-safe without locking; the strategy is a settings value that
        // practically never changes at runtime, so this hits on every call but the first.
        private sealed class ResolvedCalculator
        {
            public readonly string Strategy;
            public readonly ICalcLength Calculator;

            public ResolvedCalculator(string strategy, ICalcLength calculator)
            {
                Strategy = strategy;
                Calculator = calculator;
            }
        }

        private static volatile ResolvedCalculator _lastResolved;

        /// <summary>
        /// Do not count the text of ASSA comment blocks like {comment} - set while the subtitle
        /// is ASSA/SSA, where renderers never draw a brace block (#15584). In other formats a
        /// brace is real text, so this is off by default.
        /// </summary>
        public static bool IgnoreAssaCommentBlocks { get; set; }

        /// <summary>
        /// The text the character counters measure: HTML and ASSA tags removed, and ASSA comment
        /// blocks too when <see cref="IgnoreAssaCommentBlocks"/> is set.
        /// </summary>
        public static string RemoveTags(string text)
        {
            var s = HtmlUtil.RemoveHtmlTags(text, true, IgnoreAssaCommentBlocks);
            return RemoveIgnoredTags(s);
        }

        private static volatile Regex _ignoreTagsRegex;

        /// <summary>
        /// A user pattern for markup SE does not know - a client platform's own tags like
        /// &lt;=title&gt; or &lt;@&gt; inside SRT/DFXP (#16163). Matches are not text, so the
        /// counters (and the grid's "Hide tags" mode) drop them. Empty or invalid clears it.
        /// </summary>
        public static void SetIgnoreTagsPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                _ignoreTagsRegex = null;
                return;
            }

            try
            {
                _ignoreTagsRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
            }
            catch (ArgumentException)
            {
                _ignoreTagsRegex = null;
            }
        }

        public static bool HasIgnoreTagsPattern => _ignoreTagsRegex != null;

        /// <summary>
        /// Strips the matches of <see cref="SetIgnoreTagsPattern"/>; a no-op without a pattern.
        /// </summary>
        public static string RemoveIgnoredTags(string text)
        {
            var regex = _ignoreTagsRegex;
            if (regex == null || string.IsNullOrEmpty(text))
            {
                return text;
            }

            try
            {
                return regex.Replace(text, string.Empty);
            }
            catch (RegexMatchTimeoutException)
            {
                return text;
            }
        }

        public static ICalcLength MakeCalculator(string strategy)
        {
            // Called for every CountCharacters (grid CPS/length columns on repaint, waveform
            // footer, per-keystroke text info), so resolving the strategy must be cheap: the
            // memo avoids re-comparing reflection type names on every call.
            var last = _lastResolved;
            if (last != null && last.Strategy == strategy)
            {
                return last.Calculator;
            }

            // Avoid the LINQ closure and the fallback allocation; the list entries are shared anyway.
            var calculators = Calculators;
            var result = calculators[0]; // CalcAll
            for (var i = 0; i < calculators.Count; i++)
            {
                if (calculators[i].GetType().Name == strategy)
                {
                    result = calculators[i];
                    break;
                }
            }

            _lastResolved = new ResolvedCalculator(strategy, result);
            return result;
        }
    }
}
