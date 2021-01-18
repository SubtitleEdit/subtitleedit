using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDoubleDash : IFixCommonError
    {
        public static class Language
        {
            public static string FixDoubleDash { get; set; } = "Fix '--' -> '...'";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, Language.FixDoubleDash))
                {
                    string text = p.Text;
                    string oldText = p.Text;

                    while (text.Contains("---", StringComparison.Ordinal))
                    {
                        text = text.Replace("---", "--");
                    }

                    if (text.Contains("--", StringComparison.Ordinal))
                    {
                        text = text.Replace("--", "... ");
                        text = text.Replace("...  ", "... ");
                        text = text.Replace(" ...", "...");
                        text = text.TrimEnd();
                        text = text.Replace("... " + Environment.NewLine, "..." + Environment.NewLine);
                        text = text.Replace("... </", "...</"); // </i>, </font>...
                        text = text.Replace("... ?", "...?");
                        text = text.Replace("... !", "...!");

                        if (text.IndexOf(Environment.NewLine, StringComparison.Ordinal) > 1)
                        {
                            var lines = text.SplitToLines();
                            for (int k = 0; k < lines.Count; k++)
                            {
                                lines[k] = Helper.RemoveSpacesBeginLineAfterEllipses(lines[k]);
                            }

                            text = string.Join(Environment.NewLine, lines);
                        }
                        else
                        {
                            text = Helper.RemoveSpacesBeginLineAfterEllipses(text);
                        }
                    }

                    if (text.StartsWith('—') && text.Length > 1)
                    {
                        text = text.Substring(1).Insert(0, "...");
                    }
                    if (text.EndsWith('—') && text.Length > 1)
                    {
                        text = text.Substring(0, text.Length - 1) + "...";
                        text = text.Replace(" ...", "...");
                    }

                    if (text != oldText)
                    {
                        p.Text = text;
                        fixCount++;
                        callbacks.AddFixToListView(p, Language.FixDoubleDash, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Language.FixDoubleDash);
        }

    }
}
