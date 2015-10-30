﻿using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixEmptyLines : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;

            string fixAction0 = language.RemovedEmptyLine;
            string fixAction1 = language.RemovedEmptyLineAtTop;
            string fixAction2 = language.RemovedEmptyLineAtBottom;

            if (subtitle.Paragraphs.Count == 0)
                return;

            int emptyLinesRemoved = 0;

            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (!string.IsNullOrEmpty(p.Text))
                {
                    string text = p.Text.Trim(' ');
                    var oldText = text;
                    var pre = string.Empty;
                    var post = string.Empty;

                    // Ssa Tags
                    if (text.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        var endIDx = text.IndexOf('}', 2);
                        if (endIDx > 2)
                        {
                            pre = text.Substring(0, endIDx + 1);
                            text = text.Remove(0, endIDx + 1);
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
                                break;

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

                    if (callbacks.AllowFix(p, fixAction1) && text.StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (pre.Length > 0)
                            text = pre + text.TrimStart(Utilities.NewLineChars);
                        else
                            text = text.TrimStart(Utilities.NewLineChars);
                        p.Text = text;
                        emptyLinesRemoved++;
                        callbacks.AddFixToListView(p, fixAction1, oldText, p.Text);
                    }
                    else
                    {
                        text = pre + text;
                    }

                    if (callbacks.AllowFix(p, fixAction2) && text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (post.Length > 0)
                            text = text.TrimEnd(Utilities.NewLineChars) + post;
                        else
                            text = text.TrimEnd(Utilities.NewLineChars);
                        p.Text = text;
                        emptyLinesRemoved++;
                        callbacks.AddFixToListView(p, fixAction2, oldText, p.Text);
                    }
                }
            }

            // this must be the very last action done, or line numbers will be messed up!!!
            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true).Trim();
                if (callbacks.AllowFix(p, fixAction0) && string.IsNullOrEmpty(text))
                {
                    subtitle.Paragraphs.RemoveAt(i);
                    emptyLinesRemoved++;
                    callbacks.AddFixToListView(p, fixAction0, p.Text, string.Format("[{0}]", language.RemovedEmptyLine));
                    callbacks.AddToDeleteIndices(i);
                }
            }

            if (emptyLinesRemoved > 0)
            {
                callbacks.UpdateFixStatus(emptyLinesRemoved, language.RemovedEmptyLinesUnsedLineBreaks, string.Format(language.EmptyLinesRemovedX, emptyLinesRemoved));
                subtitle.Renumber();
            }
        }

    }
}