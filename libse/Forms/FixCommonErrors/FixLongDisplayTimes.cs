using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixLongDisplayTimes : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixLongDisplayTime;
            int noOfLongDisplayTimes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                double maxDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 8.0;
                if (maxDisplayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    maxDisplayTime = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }

                double displayTime = p.Duration.TotalMilliseconds;

                bool allowFix = callbacks.AllowFix(p, fixAction);
                if (allowFix && displayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    string oldCurrent = p.ToString();
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                    noOfLongDisplayTimes++;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                }
                else if (allowFix && maxDisplayTime < displayTime)
                {
                    string oldCurrent = p.ToString();
                    displayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + displayTime;
                    noOfLongDisplayTimes++;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                }
            }

            callbacks.UpdateFixStatus(noOfLongDisplayTimes, language.FixLongDisplayTimes, string.Format(language.XDisplayTimesShortned, noOfLongDisplayTimes));
        }

    }
}
