using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortLinesAll : IFixCommonError
    {
        public static class Language
        {
            public static string MergeShortLineAll { get; set; } = "Merge short line (all except dialogs)";
            public static string RemoveLineBreaks { get; set; } = "Remove line breaks in short texts with only one sentence";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.MergeShortLineAll;
            int noOfShortLines = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    string s = HtmlUtil.RemoveHtmlTags(p.Text, true);
                    if (s.Contains(Environment.NewLine) && s.Replace(Environment.NewLine, " ").Replace("  ", " ").CountCharacters(false, Configuration.Settings.General.IgnoreArabicDiacritics) < Configuration.Settings.General.MergeLinesShorterThan)
                    {
                        s = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                        if (s != p.Text)
                        {
                            string oldCurrent = p.Text;
                            p.Text = s;
                            noOfShortLines++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.Text);
                        }
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfShortLines, Language.RemoveLineBreaks);
        }
    }
}
