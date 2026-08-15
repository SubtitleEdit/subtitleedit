namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    internal static class TextElements
    {
        /// <summary>
        /// True when every char of <paramref name="s"/> is a text element of its own, so the
        /// calculator can count chars directly instead of walking
        /// StringInfo.GetTextElementEnumerator - which allocates an enumerator plus one string
        /// per element and runs per grid-row repaint, per keystroke and per waveform frame.
        /// Everything that can make a text element longer than one char (combining marks, ZWJ,
        /// prepend characters, surrogates) and everything the calculators skip explicitly
        /// (zero-width and BiDi controls) is >= U+0300. U+2010-U+2027 (typographic dashes,
        /// curly quotes, ellipsis - common in professional subtitles) are plain punctuation,
        /// never part of a longer element, so they stay on the fast path; any other char
        /// >= U+0300 fails the probe. "\r\n" is the one multi-char element that can survive
        /// it, and the calculators score that one differently, so it is counted separately.
        /// </summary>
        internal static bool AreAllSingleChar(string s, out int crLfCount)
        {
            crLfCount = 0;
            if (s == null)
            {
                return false;
            }

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= '\u0300' && (c < '\u2010' || c > '\u2027'))
                {
                    return false;
                }

                if (c == '\r' && i + 1 < s.Length && s[i + 1] == '\n')
                {
                    crLfCount++;
                }
            }

            return true;
        }
    }
}
