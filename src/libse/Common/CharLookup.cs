using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Buffers;
#endif

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A character set prepared once per call site.
    /// <para>
    /// Several checks in the code base asked "is there anything but these characters here?" by
    /// building the stripped string with <c>RemoveChar</c> and handing it to
    /// <c>string.IsNullOrWhiteSpace</c> - an allocation per call for an answer
    /// that never needed the string. <see cref="StringExtensions.IsOnlyChars"/> and
    /// <see cref="StringExtensions.IsOnlyCharsOrWhiteSpace"/> answer it directly, and this type
    /// holds the search structure they scan with, so it is built once instead of per call.
    /// </para>
    /// </summary>
    public sealed class CharLookup
    {
        private static readonly char[] WhiteSpaceChars = BuildWhiteSpaceChars();

        private readonly bool[] _ascii;
        private readonly bool[] _asciiOrWhiteSpace;
        private readonly char[] _nonAscii;
        private readonly char[] _nonAsciiOrWhiteSpace;
#if NET8_0_OR_GREATER
        private readonly SearchValues<char> _chars;
        private readonly SearchValues<char> _charsOrWhiteSpace;
#endif

        private CharLookup(char[] chars)
        {
            var withWhiteSpace = new List<char>(chars.Length + WhiteSpaceChars.Length);
            withWhiteSpace.AddRange(chars);
            foreach (var ch in WhiteSpaceChars)
            {
                if (!withWhiteSpace.Contains(ch))
                {
                    withWhiteSpace.Add(ch);
                }
            }

            _ascii = BuildAsciiTable(chars, out _nonAscii);
            _asciiOrWhiteSpace = BuildAsciiTable(withWhiteSpace.ToArray(), out _nonAsciiOrWhiteSpace);
#if NET8_0_OR_GREATER
            _chars = SearchValues.Create(chars);
            _charsOrWhiteSpace = SearchValues.Create(withWhiteSpace.ToArray());
#endif
        }

        public static CharLookup Create(params char[] chars) => new CharLookup(chars);

        /// <summary>
        /// True when every character of <paramref name="value"/> belongs to this set. An empty
        /// or null string is "only these characters", matching the
        /// <c>string.IsNullOrEmpty(value.RemoveChar(...))</c> shape this replaces.
        /// </summary>
        internal bool IsOnly(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

#if NET8_0_OR_GREATER
            return value.AsSpan().IndexOfAnyExcept(_chars) < 0;
#else
            return IsOnlyScalar(value, _ascii, _nonAscii);
#endif
        }

        /// <summary>
        /// True when every character of <paramref name="value"/> is either in this set or white
        /// space - the same answer as <c>string.IsNullOrWhiteSpace(value.RemoveChar(...))</c>.
        /// </summary>
        internal bool IsOnlyOrWhiteSpace(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

#if NET8_0_OR_GREATER
            return value.AsSpan().IndexOfAnyExcept(_charsOrWhiteSpace) < 0;
#else
            return IsOnlyScalar(value, _asciiOrWhiteSpace, _nonAsciiOrWhiteSpace);
#endif
        }

        private static bool IsOnlyScalar(string value, bool[] ascii, char[] nonAscii)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch < 128)
                {
                    if (!ascii[ch])
                    {
                        return false;
                    }

                    continue;
                }

                var found = false;
                for (var j = 0; j < nonAscii.Length; j++)
                {
                    if (nonAscii[j] == ch)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool[] BuildAsciiTable(char[] chars, out char[] nonAscii)
        {
            var table = new bool[128];
            var rest = new List<char>();
            foreach (var ch in chars)
            {
                if (ch < 128)
                {
                    table[ch] = true;
                }
                else
                {
                    rest.Add(ch);
                }
            }

            nonAscii = rest.ToArray();
            return table;
        }

        private static char[] BuildWhiteSpaceChars()
        {
            var list = new List<char>(32);
            for (var i = 0; i <= char.MaxValue; i++)
            {
                var ch = (char)i;
                if (char.IsWhiteSpace(ch))
                {
                    list.Add(ch);
                }
            }

            return list.ToArray();
        }
    }
}
