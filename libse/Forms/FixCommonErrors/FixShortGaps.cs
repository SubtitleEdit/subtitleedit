using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortGaps : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            double minGap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            string fixAction = language.FixShortGap;
            int noOfShortGaps = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                var p = subtitle.Paragraphs[i];
                var next = subtitle.Paragraphs[i + 1];
                if (!next.StartTime.IsMaxTime && !p.EndTime.IsMaxTime)
                {
                    double gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                    bool allowFix = callbacks.AllowFix(p, fixAction);
                    if (allowFix && gap < minGap)
                    {
                        string oldCurrent = p.ToString();
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minGap;
                        noOfShortGaps++;
                        callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfShortGaps, language.FixShortGaps, string.Format(language.XGapsFixed, noOfShortGaps));
        }
    }
}
