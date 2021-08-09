using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixEmptyLines : IFixCommonError
    {
        public static class Language
        {
            public static string RemovedEmptyLine { get; set; } = "Remove empty line";
            public static string RemovedEmptyLineAtTop { get; set; } = "Remove empty line at top";
            public static string RemovedEmptyLineAtBottom { get; set; } = "Remove empty line at bottom";
            public static string RemovedEmptyLineInMiddle { get; set; } = "Remove empty line in middle";
            public static string RemovedEmptyLinesUnsedLineBreaks { get; set; } = "Remove empty lines/unused line breaks";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction0 = Language.RemovedEmptyLine;
            string fixAction1 = Language.RemovedEmptyLineAtTop;
            string fixAction2 = Language.RemovedEmptyLineAtBottom;
            string fixAction3 = Language.RemovedEmptyLineInMiddle;

            if (subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            int emptyLinesRemoved = 0;

            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = subtitle.Paragraphs[i];
                if (!string.IsNullOrEmpty(p.Text))
                {
                    string text = p.Text.Trim(' ');
                    var oldText = text;
                    var pre = string.Empty;
                    var post = string.Empty;

                    // Ssa Tags
                    if (text.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        var endIdx = text.IndexOf('}', 2);
                        if (endIdx > 2)
                        {
                            pre = text.Substring(0, endIdx + 1);
                            text = text.Remove(0, endIdx + 1);
                        }
                    }

                    while (text.LineStartsWithHtmlTag(true, true))
                    {
                        // Three length tag
                        if (text[2] == '>')
                        {
                            pre += text.Substring(0, 3);
                            text = text.Remove(0, 3);
                        }
                        else // <font ...>
                        {
                            var closeIdx = text.IndexOf('>');
                            if (closeIdx <= 2)
                            {
                                break;
                            }

                            pre += text.Substring(0, closeIdx + 1);
                            text = text.Remove(0, closeIdx + 1);
                        }
                    }
                    while (text.LineEndsWithHtmlTag(true, true))
                    {
                        var len = text.Length;

                        // Three length tag
                        if (text[len - 4] == '<')
                        {
                            post = text.Substring(text.Length - 4) + post;
                            text = text.Remove(text.Length - 4);
                        }
                        else // </font>
                        {
                            post = text.Substring(text.Length - 7) + post;
                            text = text.Remove(text.Length - 7);
                        }
                    }

                    if (callbacks.AllowFix(p, fixAction1) && text.TrimStart(StringExtensions.UnicodeControlChars).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (pre.Length > 0)
                        {
                            text = pre + text.TrimStart(StringExtensions.UnicodeControlChars).TrimStart(Utilities.NewLineChars);
                        }
                        else
                        {
                            text = text.TrimStart(StringExtensions.UnicodeControlChars).TrimStart(Utilities.NewLineChars);
                        }

                        p.Text = text;
                        emptyLinesRemoved++;
                        callbacks.AddFixToListView(p, fixAction1, oldText, p.Text);
                    }
                    else
                    {
                        text = pre + text;
                    }

                    if (callbacks.AllowFix(p, fixAction2) && text.TrimEnd(StringExtensions.UnicodeControlChars).EndsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (post.Length > 0)
                        {
                            text = text.TrimEnd(StringExtensions.UnicodeControlChars).TrimEnd(Utilities.NewLineChars) + post;
                        }
                        else
                        {
                            text = text.TrimEnd(StringExtensions.UnicodeControlChars).TrimEnd(Utilities.NewLineChars);
                        }

                        p.Text = text;
                        emptyLinesRemoved++;
                        callbacks.AddFixToListView(p, fixAction2, oldText, p.Text);
                    }

                    if (Configuration.Settings.Tools.RemoveEmptyLinesBetweenText &&
                        callbacks.AllowFix(p, fixAction3) && text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        int beforeLength = text.Length;
                        while (text.Contains(Environment.NewLine + Environment.NewLine))
                        {
                            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        }
                        p.Text = text;
                        emptyLinesRemoved += (beforeLength - text.Length) / Environment.NewLine.Length;
                        callbacks.AddFixToListView(p, fixAction3, oldText, p.Text);
                    }

                    var arr = text.SplitToLines();
                    if (text.Contains('-') && arr.Count == 2 && callbacks.AllowFix(p, fixAction3))
                    {
                        if (arr[0].Trim() == "-" && arr[1].Length > 2)
                        {
                            text = arr[1].TrimStart('-').TrimStart();
                            p.Text = text;
                            emptyLinesRemoved++;
                            callbacks.AddFixToListView(p, fixAction1, oldText, p.Text);
                        }
                        else if (arr[1].Trim() == "-" && arr[0].Length > 2)
                        {
                            text = arr[0].TrimStart('-').TrimStart();
                            p.Text = text;
                            emptyLinesRemoved++;
                            callbacks.AddFixToListView(p, fixAction2, oldText, p.Text);
                        }
                    }
                }
            }

            // this must be the very last action done, or line numbers will be messed up!!!
            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = subtitle.Paragraphs[i];
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true).Trim();
                if (callbacks.AllowFix(p, fixAction0) && string.IsNullOrEmpty(text.RemoveControlCharacters().RemoveChar(StringExtensions.UnicodeControlChars)))
                {
                    subtitle.Paragraphs.RemoveAt(i);
                    emptyLinesRemoved++;
                    callbacks.AddFixToListView(p, fixAction0, p.Text, $"[{Language.RemovedEmptyLine}]");
                    callbacks.AddToDeleteIndices(i);
                }
            }

            if (emptyLinesRemoved > 0)
            {
                callbacks.UpdateFixStatus(emptyLinesRemoved, Language.RemovedEmptyLinesUnsedLineBreaks);
                subtitle.Renumber();
            }
        }

    }
}
