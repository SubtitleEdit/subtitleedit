using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Counts, in a single pass over a text, how many times the words of each of a fixed set of
    /// word lists occur in it.
    ///
    /// The result is the same as running <c>new Regex("\\b(" + string.Join("|", words) + ")\\b",
    /// RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)</c> over the text once per list
    /// and taking the match count - which is what <see cref="LanguageAutoDetect"/> used to do,
    /// costing one full scan of the text per list (40+ per detection) plus a compiled regex
    /// per list. Here the text is tokenized once into maximal runs of word characters, every
    /// token is looked up in one case-insensitive hash table shared by all lists, and the few
    /// list entries that are not plain words (a phrase with an apostrophe or space, an
    /// alternation such as <c>quest[ao]</c>, a prefix such as <c>[Pp]řed.*</c>) are expanded or
    /// handled by a dedicated rule that reproduces what the regex would have matched.
    ///
    /// A word character is what .NET's <c>\b</c> treats as one: the Unicode categories
    /// L*, Mn, Nd and Pc, plus U+200C/U+200D (zero-width non-joiner/joiner).
    /// </summary>
    internal sealed class WordListIndex
    {
        private const char ZeroWidthNonJoiner = '\u200C';
        private const char ZeroWidthJoiner = '\u200D';

        private static readonly ulong[] WordCharBits = BuildWordCharBits();

        private readonly Dictionary<string[], int> _listIds = new Dictionary<string[], int>(ArrayReferenceComparer.Instance);
        private readonly int[] _buckets;
        private readonly Slot[] _slots;
        private readonly Candidate[] _candidates;
        private readonly PrefixEntry[] _prefixes;
        private readonly LeadingEntry[] _leading;
        private readonly LineContainsEntry[] _lineContains;
        private readonly ulong _lengthMask;
        private readonly bool _requiresLines;

        public int ListCount => _listIds.Count;

        /// <summary>
        /// Each list entry is a (very small) regex fragment. It is expanded into literal
        /// alternatives up front; the only syntax accepted is what the word lists actually
        /// use: <c>[abc]</c> character sets, <c>(a|b)</c> groups, an optional <c>?</c> after a
        /// character or group, a whole-entry <c>.*[chars].*</c> and a trailing <c>.*</c>.
        /// Anything else throws so that a new entry with unsupported syntax fails the tests
        /// instead of silently not matching.
        /// </summary>
        public WordListIndex(IReadOnlyList<string[]> lists)
        {
            var slotsByKey = new Dictionary<string, List<Candidate>>(StringComparer.OrdinalIgnoreCase);
            var prefixes = new List<PrefixEntry>();
            var leading = new List<LeadingEntry>();
            var lineContains = new List<LineContainsEntry>();

            for (var listId = 0; listId < lists.Count; listId++)
            {
                var list = lists[listId];
                if (_listIds.ContainsKey(list))
                {
                    throw new ArgumentException("The same word list was registered twice", nameof(lists));
                }

                _listIds.Add(list, listId);
                var lineContainsInList = 0;

                for (var entryIndex = 0; entryIndex < list.Length; entryIndex++)
                {
                    var entry = list[entryIndex];
                    if (entry.StartsWith(".*", StringComparison.Ordinal))
                    {
                        // ".*[Řř].*": matches from the first word boundary of a line to its
                        // end when the line contains one of the characters. The regex tried
                        // the alternatives in order, so this only reproduces the original
                        // count when these entries precede every other entry of the list.
                        if (entryIndex != lineContainsInList)
                        {
                            throw new ArgumentException($"Line pattern '{entry}' must be at the start of its word list", nameof(lists));
                        }

                        lineContainsInList++;
                        lineContains.Add(new LineContainsEntry(listId, ParseLineContainsChars(entry)));
                        continue;
                    }

                    var isPrefix = entry.EndsWith(".*", StringComparison.Ordinal);
                    var literals = ExpandPattern(isPrefix ? entry.Substring(0, entry.Length - 2) : entry);
                    foreach (var literal in literals)
                    {
                        if (literal.Length == 0)
                        {
                            throw new ArgumentException($"Word list entry '{entry}' expands to an empty word", nameof(lists));
                        }

                        var head = LeadingWordCharCount(literal);
                        if (isPrefix)
                        {
                            if (head != literal.Length)
                            {
                                throw new ArgumentException($"Prefix entry '{entry}' must consist of word characters", nameof(lists));
                            }

                            prefixes.Add(new PrefixEntry(listId, entryIndex, literal));
                        }
                        else if (head == 0)
                        {
                            leading.Add(new LeadingEntry(listId, literal));
                        }
                        else
                        {
                            var key = head == literal.Length ? literal : literal.Substring(0, head);
                            var tail = head == literal.Length ? null : literal.Substring(head);
                            if (!slotsByKey.TryGetValue(key, out var candidates))
                            {
                                candidates = new List<Candidate>();
                                slotsByKey.Add(key, candidates);
                            }

                            candidates.Add(new Candidate(listId, entryIndex, tail));
                        }
                    }
                }
            }

            // A prefix entry competes with the plain entries of the same list that come
            // after it in the regex alternation; the counting loop always lets a whole-word
            // match win, so reject lists where that would change the count.
            foreach (var prefix in prefixes)
            {
                foreach (var pair in slotsByKey)
                {
                    if (!pair.Key.StartsWith(prefix.Head, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var candidate in pair.Value)
                    {
                        if (candidate.ListId == prefix.ListId && candidate.EntryIndex > prefix.EntryIndex)
                        {
                            throw new ArgumentException($"Prefix entry '{prefix.Head}.*' must come after '{pair.Key}' in its word list", nameof(lists));
                        }
                    }
                }
            }

            _prefixes = prefixes.ToArray();
            _leading = leading.ToArray();
            _lineContains = lineContains.ToArray();
            _requiresLines = _prefixes.Length > 0 || _lineContains.Length > 0;

            // Hash table: chained buckets, keys compared ordinal-ignore-case, candidates of a
            // key ordered like the regex alternation would try them (list, then entry index).
            var slotCount = slotsByKey.Count;
            var bucketCount = 1;
            while (bucketCount < slotCount * 2)
            {
                bucketCount <<= 1;
            }

            _buckets = new int[bucketCount];
            _slots = new Slot[slotCount];
            var allCandidates = new List<Candidate>();
            var slotIndex = 0;
            foreach (var pair in slotsByKey)
            {
                pair.Value.Sort((a, b) => a.ListId != b.ListId ? a.ListId.CompareTo(b.ListId) : a.EntryIndex.CompareTo(b.EntryIndex));
                var hash = HashIgnoreCase(pair.Key.AsSpan());
                var bucket = hash & (bucketCount - 1);
                _slots[slotIndex] = new Slot(pair.Key, hash, _buckets[bucket] - 1, allCandidates.Count, pair.Value.Count);
                _buckets[bucket] = slotIndex + 1;
                allCandidates.AddRange(pair.Value);
                if (pair.Key.Length < 64)
                {
                    _lengthMask |= 1UL << pair.Key.Length;
                }

                slotIndex++;
            }

            _candidates = allCandidates.ToArray();
        }

        public int IdOf(string[] list)
        {
            if (_listIds.TryGetValue(list, out var id))
            {
                return id;
            }

            throw new ArgumentException("Word list is not registered in the index", nameof(list));
        }

        /// <summary>
        /// Returns, per registered list (indexed by <see cref="IdOf"/>), how many times a word of
        /// that list occurs in <paramref name="text"/>.
        /// </summary>
        public int[] Count(string text)
        {
            var listCount = _listIds.Count;
            var counts = new int[listCount];
            // Per list: the regex consumed text up to here (a phrase, a prefix or a whole
            // line), so tokens starting before it cannot match again for that list.
            var skipUntil = new int[listCount];
            // Per list: start of the token it last matched, so a token counts at most once
            // per list even when several alternatives match it.
            var countedAt = new int[listCount];
            for (var i = 0; i < countedAt.Length; i++)
            {
                countedAt[i] = -1;
            }

            var span = text.AsSpan();
            var length = span.Length;
            var lineEnd = -1;
            var position = 0;
            while (position < length)
            {
                if (!IsWordChar(span[position]))
                {
                    position++;
                    continue;
                }

                var start = position;
                position++;
                while (position < length && IsWordChar(span[position]))
                {
                    position++;
                }

                var end = position;
                if (_requiresLines && start >= lineEnd)
                {
                    lineEnd = span.Slice(start).IndexOf('\n');
                    lineEnd = lineEnd < 0 ? length : start + lineEnd;
                    for (var i = 0; i < _lineContains.Length; i++)
                    {
                        ref readonly var entry = ref _lineContains[i];
                        if (start >= skipUntil[entry.ListId] && span.Slice(start, lineEnd - start).IndexOfAny(entry.Chars.AsSpan()) >= 0)
                        {
                            counts[entry.ListId]++;
                            skipUntil[entry.ListId] = lineEnd;
                        }
                    }
                }

                var tokenLength = end - start;
                if (tokenLength < 64 && (_lengthMask & (1UL << tokenLength)) != 0)
                {
                    var token = span.Slice(start, tokenLength);
                    var slot = FindSlot(token);
                    if (slot >= 0)
                    {
                        var first = _slots[slot].FirstCandidate;
                        var last = first + _slots[slot].CandidateCount;
                        for (var i = first; i < last; i++)
                        {
                            ref readonly var candidate = ref _candidates[i];
                            var listId = candidate.ListId;
                            if (start < skipUntil[listId] || countedAt[listId] == start)
                            {
                                continue;
                            }

                            if (candidate.Tail == null)
                            {
                                counts[listId]++;
                                countedAt[listId] = start;
                            }
                            else if (ContinuesWith(span, end, candidate.Tail))
                            {
                                counts[listId]++;
                                countedAt[listId] = start;
                                skipUntil[listId] = end + candidate.Tail.Length;
                            }
                        }
                    }
                }

                for (var i = 0; i < _prefixes.Length; i++)
                {
                    ref readonly var prefix = ref _prefixes[i];
                    var listId = prefix.ListId;
                    if (tokenLength >= prefix.Head.Length && start >= skipUntil[listId] && countedAt[listId] != start &&
                        span.Slice(start, prefix.Head.Length).Equals(prefix.Head.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    {
                        counts[listId]++;
                        countedAt[listId] = start;
                        skipUntil[listId] = lineEnd;
                    }
                }

                for (var i = 0; i < _leading.Length; i++)
                {
                    ref readonly var entry = ref _leading[i];
                    var listId = entry.ListId;
                    if (end >= skipUntil[listId] && ContinuesWith(span, end, entry.Text))
                    {
                        counts[listId]++;
                        skipUntil[listId] = end + entry.Text.Length;
                    }
                }
            }

            return counts;
        }

        private int FindSlot(ReadOnlySpan<char> token)
        {
            var hash = HashIgnoreCase(token);
            var slot = _buckets[hash & (_buckets.Length - 1)] - 1;
            while (slot >= 0)
            {
                ref readonly var s = ref _slots[slot];
                if (s.Hash == hash && EqualsIgnoreCase(token, s.Key))
                {
                    return slot;
                }

                slot = s.Next;
            }

            return -1;
        }

        // Keys are hashed and compared through the same per-character invariant upper-casing,
        // so equal-ignoring-case tokens always land in the same bucket (netstandard2.1 has no
        // span overload of string.GetHashCode(StringComparison)).
        private static int HashIgnoreCase(ReadOnlySpan<char> s)
        {
            var hash = unchecked((int)2166136261);
            foreach (var c in s)
            {
                hash = unchecked((hash ^ ToUpperInvariantFast(c)) * 16777619);
            }

            return hash;
        }

        private static bool EqualsIgnoreCase(ReadOnlySpan<char> a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (var i = 0; i < b.Length; i++)
            {
                if (a[i] != b[i] && ToUpperInvariantFast(a[i]) != ToUpperInvariantFast(b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static char ToUpperInvariantFast(char c)
        {
            if (c < 128)
            {
                return c >= 'a' && c <= 'z' ? (char)(c - 32) : c;
            }

            return char.ToUpperInvariant(c);
        }

        /// <summary>
        /// True when the text at <paramref name="position"/> continues with <paramref name="rest"/>
        /// (case-insensitively) and a <c>\b</c> holds after it.
        /// </summary>
        private static bool ContinuesWith(ReadOnlySpan<char> text, int position, string rest)
        {
            var endOfMatch = position + rest.Length;
            if (endOfMatch > text.Length || !text.Slice(position, rest.Length).Equals(rest.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var lastIsWord = IsWordChar(text[endOfMatch - 1]);
            var nextIsWord = endOfMatch < text.Length && IsWordChar(text[endOfMatch]);
            return lastIsWord != nextIsWord;
        }

        private static int LeadingWordCharCount(string s)
        {
            var count = 0;
            while (count < s.Length && IsWordChar(s[count]))
            {
                count++;
            }

            return count;
        }

        private static bool IsWordChar(char c)
        {
            return (WordCharBits[c >> 6] & (1UL << (c & 63))) != 0;
        }

        private static ulong[] BuildWordCharBits()
        {
            var bits = new ulong[65536 / 64];
            for (var i = 0; i < 65536; i++)
            {
                var c = (char)i;
                bool isWord;
                if (c == ZeroWidthNonJoiner || c == ZeroWidthJoiner)
                {
                    isWord = true;
                }
                else
                {
                    switch (CharUnicodeInfo.GetUnicodeCategory(c))
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.ModifierLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.NonSpacingMark:
                        case UnicodeCategory.DecimalDigitNumber:
                        case UnicodeCategory.ConnectorPunctuation:
                            isWord = true;
                            break;
                        default:
                            isWord = false;
                            break;
                    }
                }

                if (isWord)
                {
                    bits[i >> 6] |= 1UL << (i & 63);
                }
            }

            return bits;
        }

        private static char[] ParseLineContainsChars(string entry)
        {
            // Exactly ".*[...].*"
            if (entry.Length < 7 || !entry.EndsWith(".*", StringComparison.Ordinal) || entry[2] != '[' || entry[entry.Length - 3] != ']')
            {
                throw new ArgumentException($"Unsupported word list pattern '{entry}'");
            }

            var chars = new HashSet<char>();
            foreach (var c in entry.Substring(3, entry.Length - 6))
            {
                AddCharSetMember(c, chars);
            }

            var result = new char[chars.Count];
            chars.CopyTo(result);
            return result;
        }

        private static void ValidateCharSetMember(char c)
        {
            if (c == '\\' || c == '-' || c == '^' || c == '[' || c == ']')
            {
                throw new ArgumentException($"Unsupported character set member '{c}' in word list pattern");
            }
        }

        private static void AddCharSetMember(char c, HashSet<char> set)
        {
            ValidateCharSetMember(c);
            set.Add(c);
            set.Add(char.ToUpperInvariant(c));
            set.Add(char.ToLowerInvariant(c));
        }

        private static List<string> ExpandPattern(string pattern)
        {
            var position = 0;
            var result = ParseAlternation(pattern, ref position);
            if (position != pattern.Length)
            {
                throw new ArgumentException($"Unsupported word list pattern '{pattern}'");
            }

            var distinct = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var literal in result)
            {
                if (seen.Add(literal))
                {
                    distinct.Add(literal);
                }
            }

            return distinct;
        }

        private static List<string> ParseAlternation(string pattern, ref int position)
        {
            var result = ParseSequence(pattern, ref position);
            while (position < pattern.Length && pattern[position] == '|')
            {
                position++;
                result.AddRange(ParseSequence(pattern, ref position));
            }

            return result;
        }

        private static List<string> ParseSequence(string pattern, ref int position)
        {
            var result = new List<string> { string.Empty };
            while (position < pattern.Length && pattern[position] != '|' && pattern[position] != ')')
            {
                List<string> atom;
                var c = pattern[position];
                if (c == '[')
                {
                    position++;
                    var members = new HashSet<char>();
                    while (position < pattern.Length && pattern[position] != ']')
                    {
                        ValidateCharSetMember(pattern[position]);
                        members.Add(pattern[position]);
                        position++;
                    }

                    if (position >= pattern.Length)
                    {
                        throw new ArgumentException($"Unterminated character set in word list pattern '{pattern}'");
                    }

                    position++;
                    atom = new List<string>();
                    foreach (var member in members)
                    {
                        atom.Add(member.ToString());
                    }
                }
                else if (c == '(')
                {
                    position++;
                    atom = ParseAlternation(pattern, ref position);
                    if (position >= pattern.Length || pattern[position] != ')')
                    {
                        throw new ArgumentException($"Unterminated group in word list pattern '{pattern}'");
                    }

                    position++;
                }
                else if (c == '.' || c == '*' || c == '+' || c == '?' || c == '\\' || c == '{' || c == '}' || c == '^' || c == '$' || c == ']')
                {
                    throw new ArgumentException($"Unsupported word list pattern '{pattern}'");
                }
                else
                {
                    atom = new List<string> { c.ToString() };
                    position++;
                }

                if (position < pattern.Length && pattern[position] == '?')
                {
                    atom.Add(string.Empty);
                    position++;
                }

                var combined = new List<string>(result.Count * atom.Count);
                foreach (var prefix in result)
                {
                    foreach (var suffix in atom)
                    {
                        combined.Add(prefix + suffix);
                    }
                }

                result = combined;
            }

            return result;
        }

        private readonly struct Slot
        {
            public readonly string Key;
            public readonly int Hash;
            public readonly int Next;
            public readonly int FirstCandidate;
            public readonly int CandidateCount;

            public Slot(string key, int hash, int next, int firstCandidate, int candidateCount)
            {
                Key = key;
                Hash = hash;
                Next = next;
                FirstCandidate = firstCandidate;
                CandidateCount = candidateCount;
            }
        }

        /// <summary>A whole-word entry (Tail null) or a phrase: whole word followed by Tail.</summary>
        private readonly struct Candidate
        {
            public readonly int ListId;
            public readonly int EntryIndex;
            public readonly string Tail;

            public Candidate(int listId, int entryIndex, string tail)
            {
                ListId = listId;
                EntryIndex = entryIndex;
                Tail = tail;
            }
        }

        /// <summary>"[Pp]řed.*": a token starting with Head consumes the rest of its line.</summary>
        private readonly struct PrefixEntry
        {
            public readonly int ListId;
            public readonly int EntryIndex;
            public readonly string Head;

            public PrefixEntry(int listId, int entryIndex, string head)
            {
                ListId = listId;
                EntryIndex = entryIndex;
                Head = head;
            }
        }

        /// <summary>
        /// An entry starting with a non-word character (" pouvoir"): \b then requires the
        /// preceding character to be a word character, so it can only match at a token end.
        /// </summary>
        private readonly struct LeadingEntry
        {
            public readonly int ListId;
            public readonly string Text;

            public LeadingEntry(int listId, string text)
            {
                ListId = listId;
                Text = text;
            }
        }

        /// <summary>".*[Řř].*": one match per line containing any of Chars.</summary>
        private readonly struct LineContainsEntry
        {
            public readonly int ListId;
            public readonly char[] Chars;

            public LineContainsEntry(int listId, char[] chars)
            {
                ListId = listId;
                Chars = chars;
            }
        }

        private sealed class ArrayReferenceComparer : IEqualityComparer<string[]>
        {
            public static readonly ArrayReferenceComparer Instance = new ArrayReferenceComparer();

            public bool Equals(string[] x, string[] y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(string[] obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
