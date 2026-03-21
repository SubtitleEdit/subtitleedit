using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public interface IInsertService
{
    void InsertBefore(SubtitleFormat format, Subtitle subtitle, ObservableCollection<SubtitleLineViewModel> subtitles, int? index, string text);
    void InsertAfter(SubtitleFormat format, Subtitle subtitle, ObservableCollection<SubtitleLineViewModel> subtitles, int? index, string text);
    int InsertInCorrectPosition(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel paragraph);
}

public class InsertService : IInsertService
{
    public void InsertBefore(SubtitleFormat format, Subtitle subtitle, ObservableCollection<SubtitleLineViewModel> subtitles, int? index, string text)
    {
        int firstSelectedIndex = 0;
        if (index != null)
        {
            firstSelectedIndex = index.Value;
        }

        var minGapBetweenLines = Se.Settings.General.MinimumMillisecondsBetweenLines;
        int addMilliseconds = minGapBetweenLines + 1;
        if (addMilliseconds < 1)
        {
            addMilliseconds = 1;
        }

        var newParagraph = new SubtitleLineViewModel() { Text = text };

        SetStyleForNewParagraph(format, subtitle, subtitles, newParagraph, firstSelectedIndex);

        var prev = subtitles.GetOrNull(firstSelectedIndex - 1);
        var next = subtitles.GetOrNull(firstSelectedIndex);
        if (prev != null && next != null)
        {
            newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - addMilliseconds);
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(newParagraph.EndTime.TotalMilliseconds - 2000));
            if (newParagraph.StartTime.TotalMilliseconds <= prev.EndTime.TotalMilliseconds)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + 1));
            }

            if (newParagraph.Duration.TotalMilliseconds < 100)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.EndTime.TotalMilliseconds + 100);
            }

            if (next.StartTime.IsMaxTime() && prev.EndTime.IsMaxTime())
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds);
            }
            else if (next.StartTime.TotalMilliseconds == 0 && prev.EndTime.TotalMilliseconds == 0)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(0));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(0);
            }
            else if (prev.StartTime.TotalMilliseconds == next.StartTime.TotalMilliseconds &&
                     prev.EndTime.TotalMilliseconds == next.EndTime.TotalMilliseconds)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.StartTime.TotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds);
            }
        }
        else if (prev != null)
        {
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + addMilliseconds));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds + Se.Settings.General.NewEmptyDefaultMs);
            if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + 1));
            }
        }
        else if (next != null)
        {
            newParagraph.StartTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - (2000 + minGapBetweenLines));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - minGapBetweenLines);

            if (next.StartTime.IsMaxTime())
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds);
            }
            else if (next.StartTime.TotalMilliseconds == 0 && next.EndTime.TotalMilliseconds == 0)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(0));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(0);
            }
        }
        else
        {
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(1000));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(3000);
            if (newParagraph.Duration.TotalMilliseconds < Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds +
                                                         Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
            }
        }

        if (newParagraph.Duration.TotalMilliseconds < 100)
        {
            newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
        }

        subtitles.Insert(firstSelectedIndex, newParagraph);
    }

    public void InsertAfter(SubtitleFormat format, Subtitle subtitle, ObservableCollection<SubtitleLineViewModel> subtitles, int? index, string text)
    {
        int firstSelectedIndex = 0;
        if (index != null)
        {
            firstSelectedIndex = index.Value;
        }

        var newParagraph = new SubtitleLineViewModel { Text = text };

        SetStyleForNewParagraph(format, subtitle, subtitles, newParagraph, firstSelectedIndex);

        var prev = subtitles.GetOrNull(firstSelectedIndex);
        var next = subtitles.GetOrNull(firstSelectedIndex + 1);

        var minGapBetweenLines = Se.Settings.General.MinimumMillisecondsBetweenLines;
        var addMilliseconds = minGapBetweenLines;
        if (addMilliseconds < 1)
        {
            addMilliseconds = 0;
        }

        if (prev != null)
        {
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + addMilliseconds));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds + Se.Settings.General.NewEmptyDefaultMs);
            if (next != null && newParagraph.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - minGapBetweenLines);
            }

            if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + addMilliseconds));
            }

            if (next != null && next.StartTime.IsMaxTime() && prev.EndTime.IsMaxTime())
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds);
            }
            else if (next != null && next.StartTime.TotalMilliseconds == 0 && prev.EndTime.TotalMilliseconds == 0)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(0));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(0);
            }
            else if (next == null && prev.EndTime.IsMaxTime())
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(TimeCode.MaxTimeTotalMilliseconds);
            }
            else if (next == null && prev.EndTime.TotalMilliseconds == 0)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(0));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(0);
            }
            else if (next != null &&
                     prev.StartTime.TotalMilliseconds == next.StartTime.TotalMilliseconds &&
                     prev.EndTime.TotalMilliseconds == next.EndTime.TotalMilliseconds)
            {
                newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.StartTime.TotalMilliseconds));
                newParagraph.EndTime = TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds);
            }
        }
        else if (next != null)
        {
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - 2000));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - addMilliseconds);
        }
        else
        {
            newParagraph.SetStartTimeOnly(TimeSpan.FromMilliseconds(1000));
            newParagraph.EndTime = TimeSpan.FromMilliseconds(3000);
            if (newParagraph.Duration.TotalMilliseconds < Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds +
                                                         Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
            }
        }

        if (newParagraph.Duration.TotalMilliseconds < 100)
        {
            newParagraph.EndTime = TimeSpan.FromMilliseconds(newParagraph.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
            newParagraph.UpdateDuration();
            newParagraph.Duration = newParagraph.EndTime - newParagraph.StartTime;
        }

        subtitles.Insert(firstSelectedIndex + 1, newParagraph);
    }

    public int InsertInCorrectPosition(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel paragraph)
    {
        for (var i = 0; i < subtitles.Count; i++)
        {
            if (paragraph.StartTime < subtitles[i].StartTime)
            {
                subtitles.Insert(i, paragraph);
                return i;
            }
        }

        // If not inserted earlier, it belongs at the end
        subtitles.Add(paragraph);
        return subtitles.Count - 1;
    }

    private static void SetStyleForNewParagraph(SubtitleFormat format, Subtitle subtitle, ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel newParagraph, int nearestIndex)
    {
        bool hasStyleSupport = format.HasStyleSupport;
        var formatType = format.GetType();
        var styles = new List<string>();
        if (formatType == typeof(AdvancedSubStationAlpha) || formatType == typeof(SubStationAlpha))
        {
            styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
        }
        else if (formatType == typeof(TimedText10) || formatType == typeof(ItunesTimedText) || formatType == typeof(TimedTextImsc11))
        {
            styles = TimedText10.GetStylesFromHeader(subtitle.Header);
        }
        else if (formatType == typeof(Sami) || formatType == typeof(SamiModern))
        {
            styles = Sami.GetStylesFromHeader(subtitle.Header);
        }

        string style = "Default";
        if (styles.Count > 0)
        {
            style = styles[0];
        }

        if (hasStyleSupport)
        {
            newParagraph.Style = style;
            if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText) || formatType == typeof(TimedTextImsc11))
            {
                if (styles.Count > 0)
                {
                    newParagraph.Style = style;
                }

                var c = subtitles.GetOrNull(nearestIndex);
                if (c != null)
                {
                    newParagraph.Style = c.Style;
                    newParagraph.Region = c.Region;
                    newParagraph.Language = c.Language;
                    newParagraph.Actor = c.Actor;
                }

                newParagraph.Extra = TimedText10.SetExtra(new Paragraph
                {
                    StartTime = new TimeCode(newParagraph.StartTime.TotalMilliseconds),
                    EndTime = new TimeCode(newParagraph.EndTime.TotalMilliseconds),
                    Text = newParagraph.Text,
                    Style = newParagraph.Style,
                    Region = newParagraph.Region,
                    Language = newParagraph.Language
                });
            }
            else if (format.GetType() == typeof(AdvancedSubStationAlpha))
            {
                var c = subtitles.GetOrNull(nearestIndex);
                if (c != null)
                {
                    newParagraph.Extra = c.Extra;
                    newParagraph.Actor = c.Actor;
                }
            }
        }
    }
}
