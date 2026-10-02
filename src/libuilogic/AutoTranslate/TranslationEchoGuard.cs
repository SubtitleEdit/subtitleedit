using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Detects a local-LLM reply that is just the source text handed back untranslated. TranslateGemma
    /// 12B does this for merged multi-line requests (en -> de/ja: most of a file came back in English),
    /// and the reply looked like a valid translation, so it was written out as one.
    /// <para>
    /// Only lines that clearly need translating count: a short line, a number or a name ("Erik.",
    /// "Okay", "Hi, John.") is often the same in both languages, so those are never flagged.
    /// </para>
    /// </summary>
    public static class TranslationEchoGuard
    {
        private const int MinLetters = 10;
        private const int MinWords = 3;
        private const int MinLettersNoSpaceScript = 6;

        /// <summary>
        /// True when <paramref name="reply"/> is <paramref name="source"/> again (ignoring case,
        /// whitespace, line breaks and formatting tags) although the languages differ.
        /// <para>
        /// A multi-line block (several subtitles merged into one request) is compared line by
        /// line when the reply has the same number of lines, and counts as an echo when most of
        /// the lines that need translating came back unchanged - a single line that is the same in
        /// both languages must not hide that the rest was echoed, nor make the block an echo.
        /// </para>
        /// </summary>
        public static bool IsUntranslatedEcho(string source, string reply, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(reply) ||
                string.IsNullOrWhiteSpace(sourceLanguage) || string.IsNullOrWhiteSpace(targetLanguage) ||
                IsSameLanguage(sourceLanguage, targetLanguage))
            {
                return false;
            }

            var sourceLines = SplitLines(source);
            var replyLines = SplitLines(reply);
            if (sourceLines.Count > 1 && sourceLines.Count == replyLines.Count)
            {
                var qualifying = 0;
                var echoes = 0;
                for (var i = 0; i < sourceLines.Count; i++)
                {
                    var normalizedLine = Normalize(sourceLines[i]);
                    if (!NeedsTranslation(normalizedLine))
                    {
                        continue;
                    }

                    qualifying++;
                    if (normalizedLine == Normalize(replyLines[i]))
                    {
                        echoes++;
                    }
                }

                if (qualifying > 0)
                {
                    return echoes * 2 > qualifying;
                }

                // No single line is long enough to judge on its own - judge the block as a whole.
            }

            var normalizedSource = Normalize(source);
            if (!NeedsTranslation(normalizedSource))
            {
                return false;
            }

            return normalizedSource == Normalize(reply);
        }

        /// <summary>
        /// These engines use English language names as codes; "Spanish" and "Spanish (Latin America)",
        /// or "Chinese (Simplified)" and "Chinese (Traditional)", share most text, so a line coming
        /// back unchanged between them is expected - treat them as the same language.
        /// </summary>
        internal static bool IsSameLanguage(string sourceLanguage, string targetLanguage)
        {
            return string.Equals(BaseLanguageName(sourceLanguage), BaseLanguageName(targetLanguage), StringComparison.OrdinalIgnoreCase);
        }

        private static string BaseLanguageName(string language)
        {
            var s = language.Trim();
            var idx = s.IndexOf('(');
            return idx > 0 ? s.Substring(0, idx).Trim() : s;
        }

        private static List<string> SplitLines(string text)
        {
            var s = text.Replace("<br />", "\n").Replace("<br/>", "\n").Replace("<br>", "\n")
                        .Replace("\\N", "\n").Replace("\\n", "\n");
            return s.SplitToLines().Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }

        internal static string Normalize(string text)
        {
            var s = text.Replace("<br />", " ").Replace("<br/>", " ").Replace("<br>", " ")
                        .Replace("\\N", " ").Replace("\\n", " ");
            s = HtmlUtil.RemoveHtmlTags(s, true);

            var sb = new StringBuilder(s.Length);
            var pendingSpace = false;
            foreach (var c in s)
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = sb.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    sb.Append(' ');
                    pendingSpace = false;
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Enough text that an identical reply cannot be a legitimate translation: at least three
        /// words and ten letters - or, for scripts written without spaces (CJK, Thai), six letters.
        /// </summary>
        private static bool NeedsTranslation(string normalized)
        {
            var letters = 0;
            var words = 0;
            var inWord = false;
            var wordHasLetter = false;
            var noSpaceScript = false;
            foreach (var c in normalized)
            {
                if (c == ' ')
                {
                    if (inWord && wordHasLetter)
                    {
                        words++;
                    }

                    inWord = false;
                    wordHasLetter = false;
                    continue;
                }

                inWord = true;
                if (char.IsLetter(c))
                {
                    letters++;
                    wordHasLetter = true;
                    noSpaceScript |= IsNoSpaceScript(c);
                }
            }

            if (inWord && wordHasLetter)
            {
                words++;
            }

            return noSpaceScript
                ? letters >= MinLettersNoSpaceScript
                : letters >= MinLetters && words >= MinWords;
        }

        private static bool IsNoSpaceScript(char c)
        {
            return (c >= '぀' && c <= 'ヿ') || // Hiragana, Katakana
                   (c >= '㐀' && c <= '鿿') || // CJK ideographs
                   (c >= '가' && c <= '힯') || // Hangul
                   (c >= '฀' && c <= '๿');   // Thai
        }
    }
}
