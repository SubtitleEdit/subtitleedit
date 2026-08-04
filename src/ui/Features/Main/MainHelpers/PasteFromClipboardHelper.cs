using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public class PasteFromClipboardHelper : IPasteFromClipboardHelper
{

    private readonly IInsertService _insertService;

    public PasteFromClipboardHelper(IInsertService insertService)
    {
        _insertService = insertService;
    }

    public List<SubtitleLineViewModel> PasteFromClipboard(
        string text, 
        double videoPositionInMilliseconds, 
        ObservableCollection<SubtitleLineViewModel> subtitles, 
        SubtitleFormat subtitleFormat)
    {
        var minGapBetweenLines = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var tmp = new Subtitle();
        SubtitleFormat format = new SubRip();
        var list = new List<string>(text.SplitToLines());
        var result = new List<SubtitleLineViewModel>(); 

        // Try the current subtitle format first, so a line copied from the waveform/grid (which
        // serializes in the current format) round-trips no matter what that format is.
        if (subtitleFormat.IsMine(list, string.Empty))
        {
            format = subtitleFormat;
        }
        else if (new AdvancedSubStationAlpha().IsMine(list, string.Empty))
        {
            format = new AdvancedSubStationAlpha();
        }

        format.LoadSubtitle(tmp, list, null);

        if (tmp.Paragraphs.Count > 0 && videoPositionInMilliseconds >= 0)
        {
            // SE4 parity: anchor the pasted block at the paste position - the first line starts
            // there and the rest keep their relative offsets - instead of keeping original times.
            var offset = videoPositionInMilliseconds - tmp.Paragraphs[0].StartTime.TotalMilliseconds;
            tmp.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(offset));
        }

        if (tmp.Paragraphs.Count == 0 && videoPositionInMilliseconds >= 0)
        {
            var start = videoPositionInMilliseconds;
            foreach (var line in list)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var duration = Utilities.GetOptimalDisplayMilliseconds(line);
                    tmp.Paragraphs.Add(new Paragraph(line.Trim(), start, start + duration));
                    start += duration + minGapBetweenLines;
                }
            }
        }

        if (tmp.Paragraphs.Count > 0)
        {
            var selectIndices = new List<int>();
            for (int i = 0; i < tmp.Paragraphs.Count; i++)
            {
                var p = new SubtitleLineViewModel(tmp.Paragraphs[i], subtitleFormat);
                var idx = _insertService.InsertInCorrectPosition(subtitles, p);
                result.Add(p);

                if (tmp.Paragraphs.Count == 1)
                {
                    var next = subtitles.GetOrNull(idx + 1);
                    if (next != null && next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
                    {
                        var newDuration = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines - p.StartTime.TotalMilliseconds;
                        if (newDuration >= Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                        {
                            var cps = p.GetCharactersPerSecond();
                            if (cps <= Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                            {
                                p.EndTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds + newDuration);
                            }
                        }
                    }
                }

                selectIndices.Add(idx);
            }
        }

        return result;
    }
}

public interface IPasteFromClipboardHelper
{
    List<SubtitleLineViewModel> PasteFromClipboard(
        string text, 
        double videoPositionInMilliseconds, 
        ObservableCollection<SubtitleLineViewModel> subtitles, 
        SubtitleFormat selectedSubtitleFormat);
}