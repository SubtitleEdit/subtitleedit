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
                    string[] lines = text.SplitToLines();
                    for (int k = 0; k < lines.Length; k++)
                    {
                        lines[k] = Helper.FixEllipsesStartHelper(lines[k]);
                    }
                    text = string.Join(Environment.NewLine, lines);
                    if (oldText.Length > text.Length)
                    {
                        p.Text = text;
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixEllipsesStart, language.XFixEllipsesStart);
        }

    }
}
