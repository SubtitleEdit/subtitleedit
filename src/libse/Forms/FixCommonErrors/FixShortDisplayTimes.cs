using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortDisplayTimes : IFixCommonError
    {
        public static class Language
        {
            public static string FixShortDisplayTime { get; set; } = "Fix short display time";
            public static string FixShortDisplayTimes { get; set; } = "Fix short display times";
            public static string UnableToFixTextXY { get; set; } = "Unable to fix text number {0}: {1}";
        }

        private IFixCallbacks _callbacks;

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            _callbacks = callbacks;
            string fixAction = Language.FixShortDisplayTime;
            int noOfShortDisplayTimes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var skip = p.StartTime.IsMaxTime || p.EndTime.IsMaxTime;
                double displayTime = p.Duration.TotalMilliseconds;
                if (!skip && displayTime < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                    Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);
                    if (next == null || (p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines) < next.StartTime.TotalMilliseconds)
                    {
                        var temp = new Paragraph(p) { EndTime = { TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds } };
                        if (Utilities.GetCharactersPerSecond(temp) <= Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                        {
                            if (callbacks.AllowFix(p, fixAction))
                            {
                                string oldCurrent = p.ToString();
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                                noOfShortDisplayTimes++;
                                callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                            }
                        }
                    }
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines))
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            if (next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines > p.EndTime.TotalMilliseconds)
                            {
                                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            }

                            p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                            noOfShortDisplayTimes++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else
                    {
                        callbacks.LogStatus(Language.FixShortDisplayTimes, string.Format(Language.UnableToFixTextXY, i + 1, p));
                        callbacks.AddToTotalErrors(1);
                        skip = true;
                    }
                }

                double charactersPerSecond = Utilities.GetCharactersPerSecond(p);
                if (!skip && charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    var temp = new Paragraph(p);
                    var numberOfCharacters = temp.Text.CountCharacters(Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace, Configuration.Settings.General.IgnoreArabicDiacritics);
                    while (Utilities.GetCharactersPerSecond(temp, numberOfCharacters) > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        temp.EndTime.TotalMilliseconds++;
                    }
                    Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                    Paragraph nextNext = subtitle.GetParagraphOrDefault(i + 2);
                    Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);
                    double diffMs = temp.Duration.TotalMilliseconds - p.Duration.TotalMilliseconds;

                    // Normal - just make current subtitle duration longer
                    if (next == null || temp.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines < next.StartTime.TotalMilliseconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds = temp.EndTime.TotalMilliseconds;
                            noOfShortDisplayTimes++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    // Start current subtitle earlier (max 50 ms)
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             diffMs < 50 && (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines))
                    {
                        noOfShortDisplayTimes = MoveStartTime(fixAction, noOfShortDisplayTimes, p, temp, next);
                    }
                    // Make current subtitle duration longer + move next subtitle
                    else if (diffMs < 1000 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime &&
                             p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             (nextNext == null || next.EndTime.TotalMilliseconds + diffMs + Configuration.Settings.General.MinimumMillisecondsBetweenLines * 2 < nextNext.StartTime.TotalMilliseconds))
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds;
                            var nextDurationMs = next.Duration.TotalMilliseconds;
                            next.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            next.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds + nextDurationMs;
                            noOfShortDisplayTimes++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    // Make next subtitle duration shorter + make current subtitle duration longer
                    else if (diffMs < 1000 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && Utilities.GetCharactersPerSecond(new Paragraph(next.Text, p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines, next.EndTime.TotalMilliseconds)) < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            next.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            noOfShortDisplayTimes++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    // Make next-next subtitle duration shorter + move next + make current subtitle duration longer
                    else if (diffMs < 500 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && nextNext != null &&
                             Utilities.GetCharactersPerSecond(new Paragraph(nextNext.Text, nextNext.StartTime.TotalMilliseconds + diffMs + Configuration.Settings.General.MinimumMillisecondsBetweenLines, nextNext.EndTime.TotalMilliseconds - (diffMs))) < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds += diffMs;
                            next.StartTime.TotalMilliseconds += diffMs;
                            next.EndTime.TotalMilliseconds += diffMs;
                            nextNext.StartTime.TotalMilliseconds += diffMs;
                            noOfShortDisplayTimes++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    // Start current subtitle earlier (max 200 ms)
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             diffMs < 200 && (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines))
                    {
                        noOfShortDisplayTimes = MoveStartTime(fixAction, noOfShortDisplayTimes, p, temp, next);
                    }
                    else
                    {
                        // move some... though not enough
                        var improvedEndtime = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (improvedEndtime > p.EndTime.TotalMilliseconds)
                        {
                            if (callbacks.AllowFix(p, fixAction))
                            {
                                string oldCurrent = p.ToString();
                                p.EndTime.TotalMilliseconds = improvedEndtime;
                                noOfShortDisplayTimes++;
                                callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                            }
                        }

                        callbacks.LogStatus(Language.FixShortDisplayTimes, string.Format(Language.UnableToFixTextXY, i + 1, p));
                        callbacks.AddToTotalErrors(1);
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfShortDisplayTimes, fixAction);
        }

        private int MoveStartTime(string fixAction, int noOfShortDisplayTimes, Paragraph p, Paragraph temp, Paragraph next)
        {
            if (_callbacks.AllowFix(p, fixAction))
            {
                string oldCurrent = p.ToString();
                if (next != null && next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines > p.EndTime.TotalMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }

                p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds;
                noOfShortDisplayTimes++;
                _callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
            }
            return noOfShortDisplayTimes;
        }

    }
}
