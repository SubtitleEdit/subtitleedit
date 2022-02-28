using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixLongLines : IFixCommonError
    {
        public static class Language
        {
            public static string BreakLongLine { get; set; } = "Break long line";
            public static string BreakLongLines { get; set; } = "Break long lines";
            public static string UnableToFixTextXY { get; set; } = "Unable to fix text number {0}: {1}";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.BreakLongLine;
            var noOfLongLines = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var lines = p.Text.SplitToLines();
                var tooLong = false;
                foreach (var line in lines)
                {
                    if (line.CountCharacters(false) > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        tooLong = true;
                        break;
                    }
                }
                if (callbacks.AllowFix(p, fixAction) && tooLong)
                {
                    string oldText = p.Text;
                    p.Text = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                    if (oldText != p.Text)
                    {
                        noOfLongLines++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        callbacks.LogStatus(fixAction, string.Format(Language.UnableToFixTextXY, i + 1, p));
                        callbacks.AddToTotalErrors(1);
                    }
                }
            }

            callbacks.UpdateFixStatus(noOfLongLines, Language.BreakLongLines);
        }
    }
}
