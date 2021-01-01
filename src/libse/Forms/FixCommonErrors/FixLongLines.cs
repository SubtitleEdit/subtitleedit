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
            string fixAction = Language.BreakLongLine;
            int noOfLongLines = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var lines = p.Text.SplitToLines();
                bool tooLong = false;
                foreach (string line in lines)
                {
                    if (line.CountCharacters(false, Configuration.Settings.General.IgnoreArabicDiacritics) > Configuration.Settings.General.SubtitleLineMaximumLength)
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
