using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortGaps : IFixCommonError
    {
        public static class Language
        {
            public static string FixShortGap { get; set; } = "Fix short gap";
            public static string FixShortGaps { get; set; } = "Fix short gaps";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            double minGap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            string fixAction = Language.FixShortGap;
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
            callbacks.UpdateFixStatus(noOfShortGaps, Language.FixShortGaps);
        }
    }
}
