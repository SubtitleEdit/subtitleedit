using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public interface ISplitManager
{
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex, string languageCode);
}

public class SplitManager : ISplitManager
{
    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex, string languageCode)
    {
        var idx = subtitles.IndexOf(subtitle);
        if (idx < 0 || idx >= subtitles.Count)
        {
            return; // Subtitle not found in the collection
        }

        var newSubtitle = new SubtitleLineViewModel(subtitle, true);
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 2.0;

        var dividePositionMs = subtitle.StartTime.TotalMilliseconds + subtitle.Duration.TotalMilliseconds / 2.0 + gap;
        if (videoPositionSeconds > 0 && videoPositionSeconds > subtitle.StartTime.TotalSeconds && videoPositionSeconds < subtitle.EndTime.TotalSeconds)
        {
            dividePositionMs = videoPositionSeconds * 1000.0;
        }

        newSubtitle.SetStartTimeOnly(TimeSpan.FromMilliseconds(dividePositionMs));
        subtitle.EndTime = TimeSpan.FromMilliseconds(newSubtitle.StartTime.TotalMilliseconds - gap);

        var text = subtitle.Text;
        var lines = text.SplitToLines();
        if (textIndex > 0 && textIndex <= subtitle.Text.Length)
        {
            subtitle.Text = text.Substring(0, textIndex).Trim();
            newSubtitle.Text = text.Substring(textIndex).Trim();
        }
        else if (lines.Count == 2)
        {
            var dialogHelper = new DialogSplitMerge { DialogStyle = Configuration.Settings.General.DialogStyle, TwoLetterLanguageCode = languageCode };
            if (dialogHelper.IsDialog(lines))
            {
                newSubtitle.Text = lines[1].TrimStart(' ', DialogSplitMerge.GetDashChar(), DialogSplitMerge.GetAlternateDashChar()).Trim();
                subtitle.Text = lines[0].TrimStart(' ', DialogSplitMerge.GetDashChar(), DialogSplitMerge.GetAlternateDashChar()).Trim();
            }
            else
            {
                newSubtitle.Text = lines[1].Trim();
                subtitle.Text = lines[0].Trim();
            }
        }
        else if (lines.Count > 2)
        {
            var splitIndex = lines.Count / 2;

            if (lines.Count % 2 == 1) // odd number of lines
            {
                if (Se.Settings.Tools.SplitOddLinesAction == nameof(SplitOddLinesActionType.WeightTop))
                {
                    splitIndex = splitIndex + 1;
                }
                else if (Se.Settings.Tools.SplitOddLinesAction == nameof(SplitOddLinesActionType.WeightBottom))
                {
                    // no changes
                }
                else // SplitUnevenLineActionType.Smart
                {
                    var try1First = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex + 1)).Trim();
                    var try1Second = string.Join(Environment.NewLine, lines.GetRange(splitIndex + 1, lines.Count - (splitIndex + 1))).Trim();

                    var try2First = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex)).Trim();
                    var try2Second = string.Join(Environment.NewLine, lines.GetRange(splitIndex, lines.Count - splitIndex)).Trim();

                    if (try1First.EndsWith('.') && !try2First.EndsWith('.'))
                    {
                        splitIndex = splitIndex + 1;
                    }
                    else if (!try1First.EndsWith(".") && try2First.EndsWith('.'))
                    {
                        // no changes
                    }
                    else if (Math.Abs(try1First.Length - try1Second.Length) < Math.Abs(try2First.Length - try2Second.Length))
                    {
                        splitIndex = splitIndex + 1;
                    }
                }
            }

            subtitle.Text = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex)).Trim();
            newSubtitle.Text = string.Join(Environment.NewLine, lines.GetRange(splitIndex, lines.Count - splitIndex)).Trim();
        }
        else
        {
            var brokenLines = Utilities.AutoBreakLine(text, Se.Settings.General.SubtitleLineMaximumLength, 0, languageCode).SplitToLines();
            if (brokenLines.Count == 2)
            {
                subtitle.Text = brokenLines[0].Trim();
                newSubtitle.Text = brokenLines[1].Trim();
            }
            else
            {
                subtitle.Text = text;
                newSubtitle.Text = string.Empty;
            }
        }

        subtitles.Insert(idx + 1, newSubtitle);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, string languageCode)
    {
        Split(subtitles, subtitle, -1, -1, languageCode);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, string languageCode)
    {
        Split(subtitles, subtitle, videoPositionSeconds, -1, languageCode);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex, string languageCode)
    {
        Split(subtitles, subtitle, -1, textIndex, languageCode);
    }
}