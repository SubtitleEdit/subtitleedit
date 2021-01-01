using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixLongDisplayTimes : IFixCommonError
    {
        public static class Language
        {
            public static string FixLongDisplayTime { get; set; } = "Fix long display time";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
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

                bool allowFix = callbacks.AllowFix(p, Language.FixLongDisplayTime);
                if (allowFix && displayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    string oldCurrent = p.ToString();
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                    noOfLongDisplayTimes++;
                    callbacks.AddFixToListView(p, Language.FixLongDisplayTime, oldCurrent, p.ToString());
                }
                else if (allowFix && maxDisplayTime < displayTime)
                {
                    string oldCurrent = p.ToString();
                    displayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + displayTime;
                    noOfLongDisplayTimes++;
                    callbacks.AddFixToListView(p, Language.FixLongDisplayTime, oldCurrent, p.ToString());
                }
            }

            callbacks.UpdateFixStatus(noOfLongDisplayTimes, Language.FixLongDisplayTime);
        }

    }
}
