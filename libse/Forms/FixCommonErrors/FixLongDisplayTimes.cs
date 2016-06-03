namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixLongDisplayTimes : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixLongDisplayTime;
            int noOfLongDisplayTimes = 0;
            double defaultMaxDisplayTime = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                double optimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 8.0;
                if (optimalDisplayTime > defaultMaxDisplayTime)
                    optimalDisplayTime = defaultMaxDisplayTime;
                if (callbacks.AllowFix(p, fixAction) && p.Duration.TotalMilliseconds > optimalDisplayTime)
                {
                    if (optimalDisplayTime != defaultMaxDisplayTime)
                        optimalDisplayTime /= 8.0D; // Cancels: * 8.0
                    string oldCurrent = p.ToString();
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + optimalDisplayTime;
                    noOfLongDisplayTimes++;
                    callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                }
            }

            callbacks.UpdateFixStatus(noOfLongDisplayTimes, language.FixLongDisplayTimes, string.Format(language.XDisplayTimesShortned, noOfLongDisplayTimes));
        }

    }
}
