using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDoubleDash : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixDoubleDash;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
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
                    //if (text.EndsWith('-'))
                    //{
                    //    text = text.Substring(0, text.Length - 1) + "...";
                    //    text = text.Replace(" ...", "...");
                    //}
                    //if (text.EndsWith("-</i>"))
                    //{
                    //    text = text.Replace("-</i>", "...</i>");
                    //    text = text.Replace(" ...", "...");
                    //}

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
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixDoubleDash, language.XFixDoubleDash);
        }

    }
}
