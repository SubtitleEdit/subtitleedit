using Nikse.SubtitleEdit.Core.Common;
using System;
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
        /// </summary>
        public static bool IsUntranslatedEcho(string source, string reply, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(reply) ||
                string.IsNullOrWhiteSpace(sourceLanguage) || string.IsNullOrWhiteSpace(targetLanguage) ||
                string.Equals(sourceLanguage.Trim(), targetLanguage.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var normalizedSource = Normalize(source);
            if (!NeedsTranslation(normalizedSource))
            {
                return false;
            }

            return normalizedSource == Normalize(reply);
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
