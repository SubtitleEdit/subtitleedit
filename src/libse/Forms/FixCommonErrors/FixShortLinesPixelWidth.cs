using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortLinesPixelWidth : IFixCommonError
    {
        public static class Language
        {
            public static string UnbreakShortLine { get; set; } = "Unbreak short line (pixel width)";
            public static string RemoveLineBreaks { get; set; } = "Unbreak subtitles that can fit on one line (pixel width)";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.UnbreakShortLine;
            var noOfShortLines = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (!callbacks.AllowFix(p, fixAction))
                {
                    continue;
                }

                if (!p.Text.Contains(Environment.NewLine))
                {
                    continue;
                }

                var unbreakResult = p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ");                
                var totalPixelWidth = Convert.ToInt32(callbacks.GetCustomCallbackData(this, unbreakResult));

                if (totalPixelWidth <= Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                {
                    var oldCurrent = p.Text;
                    p.Text = unbreakResult;
                    noOfShortLines++;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfShortLines, Language.RemoveLineBreaks);
        }
    }
}
