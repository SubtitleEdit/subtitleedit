using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixEllipsesStart : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixEllipsesStart;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var text = p.Text;
                if (text.Contains("..") && callbacks.AllowFix(p, fixAction))
                {
                    var oldText = text;
                    if (!text.Contains(Environment.NewLine))
                    {
                        text = Helper.FixEllipsesStartHelper(text);
                        if (oldText != text)
                        {
                            p.Text = text;
                            fixCount++;
                            callbacks.AddFixToListView(p, fixAction, oldText, text);
                        }
                    }
                    else
                    {
                        var lines = text.SplitToLines();
                        var fixedParagraph = string.Empty;
                        for (int k = 0; k < lines.Length; k++)
                        {
                            var line = lines[k];
                            fixedParagraph += Environment.NewLine + Helper.FixEllipsesStartHelper(line);
                            fixedParagraph = fixedParagraph.Trim();
                        }

                        if (fixedParagraph != text)
                        {
                            p.Text = fixedParagraph;
                            fixCount++;
                            callbacks.AddFixToListView(p, fixAction, oldText, fixedParagraph);
                        }
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixEllipsesStart, language.XFixEllipsesStart);
        }

    }
}
