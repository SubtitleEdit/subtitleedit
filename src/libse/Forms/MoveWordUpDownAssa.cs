using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    /// <summary>
    /// Word moves for text with ASSA override blocks ("{\c&amp;H0000FF&amp;}"). An override has no
    /// closing tag: it stays in effect until the end of the subtitle. So the moved word gets the
    /// tags in effect where it was, and nothing more, instead of a copy of every block. Before
    /// this, every block was a never-closed opener, and moving a word back and forth piled the
    /// tags up (issue #15756).
    /// </summary>
    public partial class MoveWordUpDown
    {
        private enum TokenKind
        {
            Text,
            Block,
            Space,
        }

        private sealed class Token
        {
            public TokenKind Kind { get; }
            public string Value { get; }

            public Token(TokenKind kind, string value)
            {
                Kind = kind;
                Value = value;
            }
        }

        private sealed class AssaTag
        {
            public string Name { get; }
            public string Value { get; }
            public string Key { get; }

            public AssaTag(string name, string value)
            {
                Name = name;
                Value = value;
                Key = CanonicalKey(name);
            }

            public override string ToString() => "\\" + Name + Value;
        }

        /// <summary>
        /// Override tags in effect, one stack per tag. A stack, so a block right after a word
        /// (SE closes a color selection with "{\c&amp;H(style color)&amp;}") restores the value
        /// before it instead of counting as a new color.
        /// </summary>
        private sealed class TagState
        {
            private readonly List<string> _order = new List<string>();
            private readonly Dictionary<string, List<AssaTag>> _stacks = new Dictionary<string, List<AssaTag>>();

            public bool IsEmpty => _order.Count == 0;

            public bool Has(string key) => _stacks.ContainsKey(key);

            public AssaTag Top(string key) => _stacks.TryGetValue(key, out var stack) ? stack[stack.Count - 1] : null;

            public List<AssaTag> Snapshot() => _order.Select(Top).ToList();

            public TagState Flatten()
            {
                var state = new TagState();
                foreach (var tag in Snapshot())
                {
                    state.Push(tag);
                }

                return state;
            }

            public void Apply(AssaTag tag, bool isSuffix)
            {
                if (tag.Key == "r")
                {
                    _order.Clear();
                    _stacks.Clear();
                    if (tag.Value.Length > 0)
                    {
                        Push(tag);
                    }

                    return;
                }

                if (IsReset(tag))
                {
                    Remove(tag.Key);
                }
                else if (isSuffix && _stacks.TryGetValue(tag.Key, out var stack))
                {
                    stack.RemoveAt(stack.Count - 1);
                    if (stack.Count == 0)
                    {
                        Remove(tag.Key);
                    }
                }
                else
                {
                    Push(tag);
                }
            }

            public bool SameAs(TagState other)
            {
                if (_order.Count != other._order.Count)
                {
                    return false;
                }

                foreach (var key in _order)
                {
                    var otherTag = other.Top(key);
                    if (otherTag == null || !string.Equals(Top(key).Value, otherTag.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }

            private void Push(AssaTag tag)
            {
                if (!_stacks.TryGetValue(tag.Key, out var stack))
                {
                    stack = new List<AssaTag>();
                    _stacks.Add(tag.Key, stack);
                    _order.Add(tag.Key);
                }

                stack.Add(tag);
            }

            private void Remove(string key)
            {
                _stacks.Remove(key);
                _order.Remove(key);
            }
        }

        // Longest first, so "\bord" is not read as "\b" + "ord".
        private static readonly string[] AssaTagNames = new[]
        {
            "xbord", "ybord", "xshad", "yshad", "iclip", "alpha",
            "fscx", "fscy", "bord", "shad", "blur", "move", "fade", "clip",
            "pos", "org", "fsp", "fax", "fay", "frx", "fry", "frz", "pbo", "fad",
            "an", "fn", "fs", "fe", "fr", "be", "kf", "ko",
            "a", "b", "c", "i", "k", "K", "p", "q", "r", "s", "t", "u",
        };

        // Tags for the whole subtitle (position, alignment, fade, clip): they stay where they are.
        private static readonly HashSet<string> EventLevelKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "pos", "move", "org", "an", "a", "fade", "clip", "iclip", "q",
        };

        // Karaoke, animation and drawing tags belong to the word they precede and travel with it.
        private static readonly HashSet<string> WordLocalKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "k", "kf", "ko", "K", "t", "p", "pbo",
        };

        private static string CanonicalKey(string name)
        {
            switch (name)
            {
                case "c": return "1c";
                case "fr": return "frz";
                case "fad": return "fade";
                default: return name;
            }
        }

        private static bool IsToggle(string key) => key == "i" || key == "b" || key == "u" || key == "s";

        private static bool IsReset(AssaTag tag) => tag.Value.Length == 0 || (IsToggle(tag.Key) && tag.Value == "0");

        // "{\c}" at the very end of a line changes nothing; "{\i0}" is kept as the usual close.
        private static bool IsNoOpAtLineEnd(AssaTag tag) => tag.Value.Length == 0 && !IsToggle(tag.Key) && tag.Key != "r";

        private static AssaTag ResetOf(AssaTag tag) => new AssaTag(tag.Name, IsToggle(tag.Key) ? "0" : string.Empty);

        private bool UseAssaPath()
        {
            var text = S1 + S2;
            return text.Contains("{\\") && !HtmlTagRegex.IsMatch(text);
        }

        private static List<Token> Tokenize(string s)
        {
            var tokens = new List<Token>();
            var i = 0;
            while (i < s.Length)
            {
                var start = i;
                if (s[i] == '{' && s.IndexOf('}', i) > i)
                {
                    i = s.IndexOf('}', i) + 1;
                    tokens.Add(new Token(TokenKind.Block, s.Substring(start, i - start)));
                }
                else if (char.IsWhiteSpace(s[i]))
                {
                    while (i < s.Length && char.IsWhiteSpace(s[i]))
                    {
                        i++;
                    }

                    tokens.Add(new Token(TokenKind.Space, s.Substring(start, i - start)));
                }
                else
                {
                    i++;
                    while (i < s.Length && !char.IsWhiteSpace(s[i]) && s[i] != '{')
                    {
                        i++;
                    }

                    tokens.Add(new Token(TokenKind.Text, s.Substring(start, i - start)));
                }
            }

            return tokens;
        }

        private static List<AssaTag> ParseBlock(string block)
        {
            var tags = new List<AssaTag>();
            var content = block.Substring(1, block.Length - 2);
            if (!content.StartsWith("\\", StringComparison.Ordinal))
            {
                return tags; // comment block
            }

            var i = 0;
            while (i < content.Length)
            {
                if (content[i] != '\\')
                {
                    i++;
                    continue;
                }

                i++;
                string name;
                if (i + 1 < content.Length && char.IsDigit(content[i]) && (content[i + 1] == 'c' || content[i + 1] == 'a'))
                {
                    name = content.Substring(i, 2);
                }
                else
                {
                    name = AssaTagNames.FirstOrDefault(n => string.CompareOrdinal(content, i, n, 0, n.Length) == 0);
                    if (name == null)
                    {
                        var end = i;
                        while (end < content.Length && char.IsLetter(content[end]))
                        {
                            end++;
                        }

                        name = content.Substring(i, end - i);
                    }
                }

                i += name.Length;
                var valueStart = i;
                var depth = 0;
                while (i < content.Length && (content[i] != '\\' || depth > 0))
                {
                    if (content[i] == '(')
                    {
                        depth++;
                    }
                    else if (content[i] == ')' && depth > 0)
                    {
                        depth--;
                    }

                    i++;
                }

                if (name.Length > 0)
                {
                    tags.Add(new AssaTag(name, content.Substring(valueStart, i - valueStart)));
                }
            }

            return tags;
        }

        private static string BuildBlock(IEnumerable<AssaTag> tags)
        {
            var sb = new StringBuilder();
            foreach (var tag in tags)
            {
                sb.Append(tag);
            }

            return sb.Length == 0 ? string.Empty : "{" + sb + "}";
        }

        private static string Join(List<Token> tokens, int start, int end)
        {
            var sb = new StringBuilder();
            for (var k = start; k < end; k++)
            {
                sb.Append(tokens[k].Value);
            }

            return sb.ToString();
        }

        private static bool HasText(List<Token> tokens, int start, int end)
        {
            for (var k = start; k < end; k++)
            {
                if (tokens[k].Kind == TokenKind.Text)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A block glued to the end of a word ("word{\c&amp;HFFFFFF&amp;} next") closes what the
        /// word opened, so it restores the earlier value.
        /// </summary>
        private static bool IsSuffixBlock(List<Token> tokens, int index)
        {
            var before = index - 1;
            while (before >= 0 && tokens[before].Kind == TokenKind.Block)
            {
                before--;
            }

            var after = index + 1;
            while (after < tokens.Count && tokens[after].Kind == TokenKind.Block)
            {
                after++;
            }

            return before >= 0 && tokens[before].Kind == TokenKind.Text && (after >= tokens.Count || tokens[after].Kind == TokenKind.Space);
        }

        private static TagState Simulate(List<Token> tokens, int start, int end, TagState state = null)
        {
            state = state ?? new TagState();
            for (var k = start; k < end; k++)
            {
                if (tokens[k].Kind != TokenKind.Block)
                {
                    continue;
                }

                var isSuffix = IsSuffixBlock(tokens, k);
                foreach (var tag in ParseBlock(tokens[k].Value))
                {
                    if (!EventLevelKeys.Contains(tag.Key) && !WordLocalKeys.Contains(tag.Key))
                    {
                        state.Apply(tag, isSuffix);
                    }
                }
            }

            return state;
        }

        private static List<AssaTag> BlockTags(List<Token> tokens, int start, int end)
        {
            var tags = new List<AssaTag>();
            for (var k = start; k < end; k++)
            {
                if (tokens[k].Kind == TokenKind.Block)
                {
                    tags.AddRange(ParseBlock(tokens[k].Value));
                }
            }

            return tags;
        }

        private static int LastTextIndex(List<Token> tokens)
        {
            for (var k = tokens.Count - 1; k >= 0; k--)
            {
                if (tokens[k].Kind == TokenKind.Text)
                {
                    return k;
                }
            }

            return -1;
        }

        private static int FirstTextIndex(List<Token> tokens)
        {
            return tokens.FindIndex(t => t.Kind == TokenKind.Text);
        }

        /// <summary>Tags that turn <paramref name="from"/> into <paramref name="to"/>.</summary>
        private static List<AssaTag> Diff(TagState from, TagState to)
        {
            var result = new List<AssaTag>();
            var toR = to.Top("r");
            var fromR = from.Top("r");
            if (toR != null || fromR != null)
            {
                if (toR == null || fromR == null || !string.Equals(toR.Value, fromR.Value, StringComparison.Ordinal))
                {
                    if (toR == null)
                    {
                        result.Add(new AssaTag("r", string.Empty));
                    }

                    result.AddRange(to.Snapshot());
                    return result;
                }
            }

            foreach (var tag in from.Snapshot())
            {
                if (!to.Has(tag.Key))
                {
                    result.Add(ResetOf(tag));
                }
            }

            foreach (var tag in to.Snapshot())
            {
                var old = from.Top(tag.Key);
                if (old == null || !string.Equals(old.Value, tag.Value, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(tag);
                }
            }

            return result;
        }

        /// <summary>The word's tokens, without the closing tags that do nothing at the end of a line.</summary>
        private static string JoinForLineEnd(List<Token> word, bool dropSuffix)
        {
            var lastText = LastTextIndex(word);
            var sb = new StringBuilder(Join(word, 0, lastText + 1));
            if (!dropSuffix)
            {
                for (var k = lastText + 1; k < word.Count; k++)
                {
                    sb.Append(BuildBlock(ParseBlock(word[k].Value).Where(t => !IsNoOpAtLineEnd(t))));
                }
            }

            return sb.ToString();
        }

        private void MoveWordDownAssa()
        {
            var t1 = Tokenize(S1.Trim());
            var lastText = LastTextIndex(t1);
            if (lastText < 0)
            {
                return;
            }

            var chunkStart = lastText;
            while (chunkStart > 0 && t1[chunkStart - 1].Kind != TokenKind.Space)
            {
                chunkStart--;
            }

            if (SameSubtitle)
            {
                // Overrides carry across the line break, so just move the break.
                S2 = (Join(t1, chunkStart, t1.Count).Trim() + " " + S2.Trim()).Trim();
                S1 = Join(t1, 0, chunkStart).Trim();
                S2 = AutoBreakIfNeeded(S2);
                return;
            }

            var wordStart = t1.FindIndex(chunkStart, t => t.Kind == TokenKind.Text);
            var leadTags = BlockTags(t1, chunkStart, wordStart);
            var events = leadTags.Where(t => EventLevelKeys.Contains(t.Key)).ToList();
            var locals = leadTags.Where(t => WordLocalKeys.Contains(t.Key)).ToList();
            var word = t1.Skip(wordStart).Where(t => t.Kind != TokenKind.Space).ToList();

            var startState = Simulate(t1, 0, wordStart);
            var endState = Simulate(word, 0, word.Count, startState.Flatten());

            // What stays on the first line
            if (!HasText(t1, 0, chunkStart))
            {
                S1 = string.Empty;
            }
            else
            {
                var restState = Simulate(t1, 0, chunkStart);
                var closing = BlockTags(word, LastTextIndex(word) + 1, word.Count)
                    .Where(t => restState.Has(t.Key) && !IsNoOpAtLineEnd(t));
                S1 = Join(t1, 0, chunkStart).Trim() + BuildBlock(events) + BuildBlock(closing);
            }

            // The word goes in front of the second line
            var t2 = Tokenize(S2.Trim());
            var firstText2 = FirstTextIndex(t2);
            var lead2End = firstText2 < 0 ? t2.Count : firstText2;
            var lead2Tags = BlockTags(t2, 0, lead2End);
            var prefix = BuildBlock(lead2Tags.Where(t => EventLevelKeys.Contains(t.Key))
                .Concat(startState.Snapshot())
                .Concat(locals));

            if (firstText2 < 0)
            {
                S2 = prefix + JoinForLineEnd(word, false);
            }
            else
            {
                var lead2State = Simulate(t2, 0, firstText2);
                var lead2Locals = lead2Tags.Where(t => WordLocalKeys.Contains(t.Key));
                var rest2 = Join(t2, firstText2, t2.Count);
                if (endState.SameAs(lead2State))
                {
                    S2 = prefix + Join(word, 0, word.Count) + " " + BuildBlock(lead2Locals) + rest2;
                }
                else
                {
                    var closer = endState.Snapshot().Where(t => !lead2State.Has(t.Key)).Select(ResetOf);
                    var lead2Inline = lead2Tags.Where(t => !EventLevelKeys.Contains(t.Key));
                    S2 = prefix + Join(word, 0, word.Count) + BuildBlock(closer) + " " + BuildBlock(lead2Inline) + rest2;
                }
            }

            S2 = AutoBreakIfNeeded(S2);
        }

        private void MoveWordUpAssa()
        {
            var t2 = Tokenize(S2.Trim());
            var firstText = FirstTextIndex(t2);
            if (firstText < 0)
            {
                return;
            }

            var wordEnd = t2.FindIndex(firstText, t => t.Kind == TokenKind.Space);
            if (wordEnd < 0)
            {
                wordEnd = t2.Count;
            }

            if (SameSubtitle)
            {
                // Overrides carry across the line break, so just move the break.
                S1 = (S1.Trim() + " " + Join(t2, 0, wordEnd).Trim()).Trim();
                S2 = Join(t2, wordEnd, t2.Count).Trim();
                S1 = AutoBreakIfNeeded(S1);
                return;
            }

            var leadTags = BlockTags(t2, 0, firstText);
            var events = leadTags.Where(t => EventLevelKeys.Contains(t.Key)).ToList();
            var locals = leadTags.Where(t => WordLocalKeys.Contains(t.Key)).ToList();
            var word = t2.GetRange(firstText, wordEnd - firstText);

            var startState = Simulate(t2, 0, firstText);
            var endState = Simulate(word, 0, word.Count, startState.Flatten());

            // The word goes at the end of the first line
            var t1 = Tokenize(S1.Trim());
            var lastText1 = LastTextIndex(t1);
            var toggleClose = endState.Snapshot().Where(t => IsToggle(t.Key)).Select(ResetOf).ToList();
            if (lastText1 < 0)
            {
                S1 = BuildBlock(startState.Snapshot().Concat(locals)) + JoinForLineEnd(word, false) + BuildBlock(toggleClose);
            }
            else
            {
                var hasTrail = lastText1 + 1 < t1.Count;
                var beforeTrail = Simulate(t1, 0, lastText1 + 1);
                if (hasTrail && !startState.IsEmpty && beforeTrail.SameAs(startState))
                {
                    // "Hello {\i1}world{\i0}" + "{\i1}today" -> "Hello {\i1}world today{\i0}"
                    S1 = Join(t1, 0, lastText1 + 1) + " " + BuildBlock(locals) + JoinForLineEnd(word, true) + Join(t1, lastText1 + 1, t1.Count).Trim();
                }
                else
                {
                    var diff = Diff(Simulate(t1, 0, t1.Count), startState);
                    S1 = S1.Trim() + " " + BuildBlock(diff.Concat(locals)) + JoinForLineEnd(word, false) + BuildBlock(toggleClose);
                }
            }

            S1 = AutoBreakIfNeeded(S1);

            // What stays on the second line
            if (!HasText(t2, wordEnd, t2.Count))
            {
                S2 = string.Empty;
                return;
            }

            var restState = Simulate(t2, 0, wordEnd);
            S2 = BuildBlock(events.Concat(restState.Snapshot())) + Join(t2, wordEnd, t2.Count).Trim();
        }
    }
}
