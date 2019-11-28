using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixOverlappingDisplayTimes : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;

            // negative display time
            string fixAction = language.FixOverlappingDisplayTime;
            int noOfOverlappingDisplayTimesFixed = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var oldP = new Paragraph(p);
                if (p.Duration.TotalMilliseconds < 0) // negative display time...
                {
                    bool isFixed = false;
                    string status = string.Format(language.StartTimeLaterThanEndTime, i + 1, p.StartTime, p.EndTime, p.Text, Environment.NewLine);

                    var prev = subtitle.GetParagraphOrDefault(i - 1);
                    var next = subtitle.GetParagraphOrDefault(i + 1);

                    double wantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 0.9;

                    if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + wantedDisplayTime)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + wantedDisplayTime;
                            isFixed = true;
                        }
                    }
                    else if (next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + 500.0)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 500.0;
                            isFixed = true;
                        }
                    }
                    else if (prev == null || next.StartTime.TotalMilliseconds - wantedDisplayTime > prev.EndTime.TotalMilliseconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - wantedDisplayTime;
                            p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                            isFixed = true;
                        }
                    }
                    else
                    {
                        callbacks.LogStatus(language.FixOverlappingDisplayTimes, string.Format(language.UnableToFixStartTimeLaterThanEndTime, i + 1, p), true);
                        callbacks.AddToTotalErrors(1);
                    }

                    if (isFixed)
                    {
                        noOfOverlappingDisplayTimesFixed++;
                        status = string.Format(language.XFixedToYZ, status, Environment.NewLine, p);
                        callbacks.LogStatus(language.FixOverlappingDisplayTimes, status);
                        callbacks.AddFixToListView(p, fixAction, oldP.ToString(), p.ToString());
                    }
                }
            }

            // overlapping display time
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);
                Paragraph target = prev;
                string oldCurrent = p.ToString();
                string oldPrevious = prev.ToString();
                double prevWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds);
                double currentWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds);
                double prevOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text);
                double currentOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                bool canBeEqual = callbacks.Format != null && (callbacks.Format.GetType() == typeof(AdvancedSubStationAlpha) || callbacks.Format.GetType() == typeof(SubStationAlpha));
                if (!canBeEqual)
                {
                    canBeEqual = Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart;
                }

                double diff = prev.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds;
                if (!prev.StartTime.IsMaxTime && !p.StartTime.IsMaxTime && diff >= 0 && !(canBeEqual && Math.Abs(diff) < 0.001))
                {
                    int diffHalf = (int)(diff / 2);
                    if (!Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart && Math.Abs(p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 0.001 &&
                        prev.Duration.TotalMilliseconds > 100)
                    {
                        if (callbacks.AllowFix(target, fixAction))
                        {
                            if (!canBeEqual)
                            {
                                bool okEqual = true;
                                if (prev.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                                {
                                    prev.EndTime.TotalMilliseconds--;
                                }
                                else if (p.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                                {
                                    p.StartTime.TotalMilliseconds++;
                                }
                                else
                                {
                                    okEqual = false;
                                }

                                if (okEqual)
                                {
                                    noOfOverlappingDisplayTimesFixed++;
                                    callbacks.AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                                }
                            }
                        }
                    }
                    else if (prevOptimalDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        if (callbacks.AllowFix(target, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                            if (canBeEqual)
                            {
                                prev.EndTime.TotalMilliseconds++;
                            }

                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                        }
                    }
                    else if (diff > 0 && currentOptimalDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevOptimalDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= diffHalf;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (currentOptimalDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                            {
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                            }

                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (diff > 0 && currentWantedDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevWantedDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= diffHalf;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (prevWantedDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        if (callbacks.AllowFix(target, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                            if (canBeEqual)
                            {
                                prev.EndTime.TotalMilliseconds++;
                            }

                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                        }
                    }
                    else if (currentWantedDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                            {
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                            }

                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10 && p.Duration.TotalMilliseconds > 1)
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= 2;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                            {
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                            }

                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds) < 10 && Math.Abs(p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10)
                    { // merge lines with same time codes
                        if (callbacks.AllowFix(target, fixAction))
                        {
                            prev.Text = prev.Text.Replace(Environment.NewLine, " ");
                            p.Text = p.Text.Replace(Environment.NewLine, " ");

                            string stripped = HtmlUtil.RemoveHtmlTags(prev.Text).TrimStart();
                            if (!stripped.StartsWith("- ", StringComparison.Ordinal))
                            {
                                prev.Text = "- " + prev.Text.TrimStart();
                            }

                            stripped = HtmlUtil.RemoveHtmlTags(p.Text).TrimStart();
                            if (!stripped.StartsWith("- ", StringComparison.Ordinal))
                            {
                                p.Text = "- " + p.Text.TrimStart();
                            }

                            prev.Text = prev.Text.Trim() + Environment.NewLine + p.Text;
                            p.Text = string.Empty;
                            noOfOverlappingDisplayTimesFixed++;
                            callbacks.AddFixToListView(target, fixAction, oldCurrent, p.ToString());

                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                            {
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                            }
                        }
                    }
                    else
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            callbacks.LogStatus(language.FixOverlappingDisplayTimes, string.Format(language.UnableToFixTextXY, i + 1, Environment.NewLine + prev.Number + "  " + prev + Environment.NewLine + p.Number + "  " + p), true);
                            callbacks.AddToTotalErrors(1);
                        }
                    }
                }
            }

            callbacks.UpdateFixStatus(noOfOverlappingDisplayTimesFixed, fixAction, string.Format(language.XOverlappingTimestampsFixed, noOfOverlappingDisplayTimesFixed));
        }

    }
}
