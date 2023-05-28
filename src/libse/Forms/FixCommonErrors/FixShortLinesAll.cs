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
            var fixAction = Language.MergeShortLineAll;
            var noOfShortLines = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (!callbacks.AllowFix(p, fixAction))
                {
                    continue;
                }

                var s = HtmlUtil.RemoveHtmlTags(p.Text, true);
                if (!s.Contains(Environment.NewLine) || s.Replace(Environment.NewLine, " ").Replace("  ", " ").CountCharacters(false) >= Configuration.Settings.General.MergeLinesShorterThan)
                {
                    continue;
                }

                s = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                if (s != p.Text)
                {
                    var oldCurrent = p.Text;
                    p.Text = s;
                    noOfShortLines++;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfShortLines, Language.RemoveLineBreaks);
        }
    }
}
