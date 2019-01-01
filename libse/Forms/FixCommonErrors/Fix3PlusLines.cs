using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class Fix3PlusLines : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.Fix3PlusLine;
            int iFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (Utilities.GetNumberOfLines(p.Text) > 2 && callbacks.AllowFix(p, fixAction))
                {
                    var old = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX;
                    Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX = 2;
                    string oldText = p.Text;
                    try
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text);
                    }
                    finally
                    {
                        Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX = old;
                    }
                    if (oldText != p.Text)
                    {
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                   }
                }
            }
            callbacks.UpdateFixStatus(iFixes, language.Fix3PlusLines, language.X3PlusLinesFixed);
        }
    }
}
