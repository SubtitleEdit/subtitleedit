using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class AddMissingQuotes : IFixCommonError
    {
        public static class Language
        {
            public static string AddMissingQuote { get; set; } = "Add missing quote (\")";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.AddMissingQuote;
            int noOfFixes = 0;
            var skipTo = -1;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (i < skipTo)
                {
                    continue;
                }

                var p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                p.Text = FixDifferentQuotes(p.Text); //TODO: extract to own rule

                if (Utilities.CountTagInText(p.Text, '"') == 1)
                {
                    var next = subtitle.GetParagraphOrDefault(i + 1);
                    if (next != null)
                    {
                        double betweenMilliseconds = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                        if (betweenMilliseconds > 1500)
                        {
                            next = null; // cannot be quote spanning several lines of more than 1.5 seconds between lines!
                        }
                        else if (next.Text.Replace("<i>", string.Empty).TrimStart().TrimStart('-').TrimStart().StartsWith('"') &&
                                 next.Text.Replace("</i>", string.Empty).TrimEnd().EndsWith('"') &&
                                 Utilities.CountTagInText(next.Text, '"') == 2)
                        {
                            next = null; // seems to have valid quotes, so no spanning
                        }
                    }
                    if (next != null && !HtmlUtil.RemoveHtmlTags(p.Text).EndsWith('"') && next.Text.TrimEnd('.', '!', '?').EndsWith('"'))
                    {
                        skipTo = i + 2;
                        continue;
                    }

                    if (next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 500 && !HtmlUtil.RemoveHtmlTags(p.Text).TrimEnd('.', '!', '?').EndsWith('"') && next.Text.IndexOf('"') < 0)
                    {
                        var next2 = subtitle.GetParagraphOrDefault(i + 2);
                        if (next2 != null && next2.StartTime.TotalMilliseconds - next.EndTime.TotalMilliseconds < 500 && Utilities.CountTagInText(next2.Text, '"') == 1 && next2.Text.TrimEnd('.', '!', '?').EndsWith('"'))
                        {
                            skipTo = i + 3;
                            continue;
                        }
                        var next3 = subtitle.GetParagraphOrDefault(i + 3);
                        if (next2 != null && next3 != null && next2.StartTime.TotalMilliseconds - next.EndTime.TotalMilliseconds < 500 && next3.StartTime.TotalMilliseconds - next2.EndTime.TotalMilliseconds < 500 &&
                            next2.Text.IndexOf('"') < 0 && Utilities.CountTagInText(next3.Text, '"') == 1 && next3.Text.TrimEnd('.', '!', '?').EndsWith('"'))
                        {
                            skipTo = i + 4;
                            continue;
                        }
                    }

                    var prev = subtitle.GetParagraphOrDefault(i - 1);
                    if (prev != null)
                    {
                        double betweenMilliseconds = p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds;
                        if (betweenMilliseconds > 1500 || // cannot be quote spanning several lines of more than 1.5 seconds between lines!

                            // seems to have valid quotes, so no spanning
                            prev.Text.Replace("<i>", string.Empty).TrimStart().TrimStart('-').TrimStart().StartsWith('"') &&
                            prev.Text.Replace("</i>", string.Empty).TrimEnd().EndsWith('"') &&
                            Utilities.CountTagInText(prev.Text, '"') == 2)
                        {
                            prev = null;
                        }
                    }

                    var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                    if (lines.Count == 2 && lines[0].TrimStart().StartsWith('-') && lines[1].TrimStart().StartsWith('-'))
                    { // dialog
                        lines = p.Text.SplitToLines();
                        string line = lines[0].Trim();

                        if (line.Length > 5 && line.TrimStart().StartsWith("- \"", StringComparison.Ordinal) && (line.EndsWith('.') || line.EndsWith('!') || line.EndsWith('?')))
                        {
                            p.Text = p.Text.Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
                            p.Text = p.Text.Replace(Environment.NewLine, "\"" + Environment.NewLine);
                        }
                        else if (line.Length > 5 && line.EndsWith('"') && line.Contains("- ") && line.IndexOf("- ", StringComparison.Ordinal) < 4)
                        {
                            p.Text = p.Text.Insert(line.IndexOf("- ", StringComparison.Ordinal) + 2, "\"");
                        }
                        else if (line.Contains('"') && line.IndexOf('"') > 2 && line.IndexOf('"') < line.Length - 3)
                        {
                            int index = line.IndexOf('"');
                            if (line[index - 1] == ' ')
                            {
                                p.Text = p.Text.Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
                                p.Text = p.Text.Replace(Environment.NewLine, "\"" + Environment.NewLine);
                            }
                            else if (line[index + 1] == ' ')
                            {
                                if (line.Length > 5 && line.Contains("- ") && line.IndexOf("- ", StringComparison.Ordinal) < 4)
                                {
                                    p.Text = p.Text.Insert(line.IndexOf("- ", StringComparison.Ordinal) + 2, "\"");
                                }
                            }
                        }
                        else if (lines[1].Contains('"'))
                        {
                            line = lines[1].Trim();
                            if (line.Length > 5 && line.TrimStart().StartsWith("- \"", StringComparison.Ordinal) && (line.EndsWith('.') || line.EndsWith('!') || line.EndsWith('?')))
                            {
                                p.Text = p.Text.Trim() + "\"";
                            }
                            else if (line.Length > 5 && line.EndsWith('"') && p.Text.Contains(Environment.NewLine + "- "))
                            {
                                p.Text = p.Text.Insert(p.Text.IndexOf(Environment.NewLine + "- ", StringComparison.Ordinal) + Environment.NewLine.Length + 2, "\"");
                            }
                            else if (line.Contains('"') && line.IndexOf('"') > 2 && line.IndexOf('"') < line.Length - 3)
                            {
                                int index = line.IndexOf('"');
                                if (line[index - 1] == ' ')
                                {
                                    p.Text = p.Text.Trim() + "\"";
                                }
                                else if (line[index + 1] == ' ')
                                {
                                    if (line.Length > 5 && p.Text.Contains(Environment.NewLine + "- "))
                                    {
                                        p.Text = p.Text.Insert(p.Text.IndexOf(Environment.NewLine + "- ", StringComparison.Ordinal) + Environment.NewLine.Length + 2, "\"");
                                    }
                                }
                            }
                        }
                    }
                    else
                    { // not dialog
                        if (p.Text.StartsWith('"'))
                        {
                            if (next == null || !next.Text.Contains('"') && p.Text.HasSentenceEnding(callbacks.Language))
                            {
                                p.Text += "\"";
                            }
                        }
                        else if (p.Text.StartsWith("<i>\"", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "</i>") == 1)
                        {
                            if (next == null || !next.Text.Contains('"'))
                            {
                                p.Text = p.Text.Replace("</i>", "\"</i>");
                            }
                        }
                        else if (p.Text.EndsWith('"'))
                        {
                            if (prev == null || !prev.Text.Contains('"') || (prev.Text.TrimEnd('"').HasSentenceEnding() && prev.Text.TrimEnd('"').IndexOf('"') < 0))
                            {
                                p.Text = "\"" + p.Text;
                            }
                        }
                        else if (p.Text.Contains(Environment.NewLine + "\"") && Utilities.GetNumberOfLines(p.Text) == 2)
                        {
                            if (next == null || !next.Text.Contains('"'))
                            {
                                p.Text = p.Text + "\"";
                            }
                        }
                        else if ((p.Text.Contains(Environment.NewLine + "\"") || p.Text.Contains(Environment.NewLine + "-\"") || p.Text.Contains(Environment.NewLine + "- \"")) &&
                                 Utilities.GetNumberOfLines(p.Text) == 2 && p.Text.Length > 3)
                        {
                            if (next == null || !next.Text.Contains('"'))
                            {
                                if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "</i>") == 1)
                                {
                                    p.Text = p.Text.Replace("</i>", "\"</i>");
                                }
                                else
                                {
                                    p.Text = p.Text + "\"";
                                }
                            }
                        }
                        else if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "<i>") == 1)
                        {
                            if (prev == null || !prev.Text.Contains('"'))
                            {
                                p.Text = p.Text.Replace("<i>", "<i>\"");
                            }
                        }
                        else if (p.Text.Contains('"'))
                        {
                            string text = p.Text;
                            int indexOfQuote = p.Text.IndexOf('"');
                            if (text.Contains('"') && indexOfQuote > 2 && indexOfQuote < text.Length - 3)
                            {
                                int index = text.IndexOf('"');
                                if (text[index - 1] == ' ')
                                {
                                    if (p.Text.EndsWith(','))
                                    {
                                        p.Text = p.Text.Insert(p.Text.Length - 1, "\"").Trim();
                                    }
                                    else
                                    {
                                        p.Text = p.Text.Trim() + "\"";
                                    }
                                }
                                else if (text[index + 1] == ' ')
                                {
                                    p.Text = "\"" + p.Text;
                                }
                            }
                        }
                    }
                }

                if (oldText != p.Text)
                {
                    if (callbacks.AllowFix(p, fixAction))
                    {
                        noOfFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        p.Text = oldText;
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, fixAction);
        }

        private string FixDifferentQuotes(string text)
        {
            if (text.Contains("„"))
            {
                return text;
            }

            if (Utilities.CountTagInText(text, "\"") == 1 && Utilities.CountTagInText(text, "”") == 1)
            {
                return text.Replace("”", "\"");
            }
            if (Utilities.CountTagInText(text, "\"") == 1 && Utilities.CountTagInText(text, "“") == 1)
            {
                return text.Replace("“", "\"");
            }
            return text;
        }
    }
}
