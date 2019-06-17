using Nikse.SubtitleEdit.Core.Interfaces;
using System;

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
                if (!callbacks.AllowFix(p, fixAction))
                {
                    continue;
                }

                // predict
                double maxDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 8.0;
                if (maxDisplayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    maxDisplayTime = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }

                double oldDuration = p.Duration.TotalMilliseconds;
                double newDuration = oldDuration;

                // calculate
                if (oldDuration > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    newDuration = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }
                else if (oldDuration > maxDisplayTime)
                {
                    newDuration = maxDisplayTime / 8.0;
                }

                // update
                if (Math.Abs(oldDuration - newDuration) > .5)
                {
                    string oldCurrent = p.ToString();
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + newDuration;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                    noOfLongDisplayTimes++;
                }
                else
                {
                    callbacks.AddToTotalErrors(1); // unable to fix
                }
            }

            callbacks.UpdateFixStatus(noOfLongDisplayTimes, language.FixLongDisplayTimes, string.Format(language.XDisplayTimesShortned, noOfLongDisplayTimes));
        }

    }
}
