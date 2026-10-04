using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    /// <summary>
    /// Fixes a quote pair where one quote is a straight double quote and the other a curly one:
    ///   "Hello” -> "Hello"
    ///   “Hello" -> "Hello"
    /// Texts using „ (e.g. German „Hallo“) are left alone.
    /// </summary>
    public class FixDifferentQuotes : IFixCommonError
    {
        public static class Language
        {
            public static string FixDifferentQuotes { get; set; } = "Fix mismatched curly quote (“Hello\" -> \"Hello\")";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.FixDifferentQuotes;
            var noOfFixes = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var newText = FixText(p.Text);
                if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    p.Text = newText;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }

            callbacks.UpdateFixStatus(noOfFixes, fixAction);
        }

        public static string FixText(string text)
        {
            if (text.Contains("„"))
            {
                return text;
            }

            const string doubleQuote = "\"";
            if (Utilities.CountTagInText(text, doubleQuote) == 1)
            {
                if (Utilities.CountTagInText(text, "”") == 1)
                {
                    return text.Replace("”", doubleQuote);
                }

                if (Utilities.CountTagInText(text, "“") == 1)
                {
                    return text.Replace("“", doubleQuote);
                }
            }

            return text;
        }
    }
}
