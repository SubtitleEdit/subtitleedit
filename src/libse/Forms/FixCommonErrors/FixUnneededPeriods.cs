using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUnneededPeriods : IFixCommonError
    {
        public static class Language
        {
            public static string UnneededPeriod { get; set; } = "Unneeded period";
            public static string RemoveUnneededPeriods { get; set; } = "Remove unneeded periods";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.UnneededPeriod;
            int removedCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    // Returns processed text.
                    string procText = RemoveDotAfterPunctuation(p.Text);

                    while (procText.Contains("....", StringComparison.Ordinal))
                    {
                        procText = procText.Replace("....", "...");
                    }

                    while (procText.Contains("……", StringComparison.Ordinal))
                    {
                        procText = procText.Replace("……", "…");
                    }

                    while (procText.Contains(".…", StringComparison.Ordinal))
                    {
                        procText = procText.Replace(".…", "…");
                    }

                    while (procText.Contains("….", StringComparison.Ordinal))
                    {
                        procText = procText.Replace("….", "…");
                    }

                    var l = callbacks.Language;
                    if (procText.Contains('.') && LanguageAutoDetect.IsLanguageWithoutPeriods(l))
                    {
                        var sb = new StringBuilder();
                        foreach (var line in procText.SplitToLines())
                        {
                            var s = line;
                            if (s.EndsWith('.') && !s.EndsWith("..", StringComparison.Ordinal))
                            {
                                s = s.TrimEnd('.');
                            }
                            else if (s.EndsWith(".</i>", StringComparison.Ordinal) && !s.EndsWith("..</i>", StringComparison.Ordinal))
                            {
                                s = s.Remove(s.Length - 5, 1);
                            }

                            sb.AppendLine(s);
                        }

                        procText = sb.ToString().TrimEnd();
                    }

                    int diff = p.Text.Length - procText.Length;
                    if (diff > 0)
                    {
                        // Calculate total removed dots.
                        removedCount += diff;
                        callbacks.AddFixToListView(p, fixAction, p.Text, procText);
                        p.Text = procText;
                    }
                }
            }
            callbacks.UpdateFixStatus(removedCount, Language.RemoveUnneededPeriods);
        }

        public static string RemoveDotAfterPunctuation(string input)
        {
            for (int i = input.Length - 1; i > 0; i--)
            {
                // Expecting pre characters: [?!]
                if (input[i] == '.' && (input[i - 1] == '?' || input[i - 1] == '!'))
                {
                    int j = i;
                    // Fix recursive dot after ?/!
                    while (j + 1 < input.Length && input[j + 1] == '.')
                    {
                        j++;
                    }
                    // Expecting post characters: [\r\n ]
                    if (j + 1 == input.Length || input[j + 1] == ' ' || input[j + 1] == '\r' || input[j + 1] == '\n')
                    {
                        input = input.Remove(i, j - i + 1);
                    }
                }
            }
            return input;
        }

    }
}
