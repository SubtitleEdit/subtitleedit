using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public interface ISplitManager
{
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex);
}

public class SplitManager : ISplitManager
{
    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex)
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
            newSubtitle.Text = lines[1].Trim();
            subtitle.Text = lines[0].Trim();
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
            var middleIndex = text.Length / 2;
            var splitIndex = middleIndex;
            while (splitIndex > 0 && !char.IsWhiteSpace(text[splitIndex]) && !char.IsPunctuation(text[splitIndex]))
            {
                splitIndex--;
            }
            subtitle.Text = text.Substring(0, splitIndex).Trim();
            newSubtitle.Text = text.Substring(splitIndex).Trim();
        }

        subtitles.Insert(idx + 1, newSubtitle);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle)
    {
        Split(subtitles, subtitle, -1, -1);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds)
    {
        Split(subtitles, subtitle, videoPositionSeconds, -1);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex)
    {
        Split(subtitles, subtitle, -1, textIndex);
    }
}