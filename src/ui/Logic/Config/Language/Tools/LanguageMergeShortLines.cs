using System;
using static Nikse.SubtitleEdit.UiLogic.Translate.MergeAndSplitHelper;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeShortLines
{
    public string Title { get; set; }
    public string HighlightParts { get; set; }
    public string MergedLineInfo { get; set; }
    public string LinesMergedX { get; set; }
    public string MaxCharacters { get; set; }
    public string MaxMillisecondsBetweenLines { get; set; }
    public string OnlyContinuationLines { get; set; }

    public LanguageMergeShortLines()
    {
        Title = "Merge short lines";
        HighlightParts = "Highlight parts (karaoke)";
        MergedLineInfo = "Merged line {0} into {1} - {2}";
        LinesMergedX = "Lines merged: {0}";
        MaxCharacters = "Maximum characters in one paragraph";
        MaxMillisecondsBetweenLines = "Maximum milliseconds between lines";
        OnlyContinuationLines = "Only continuation lines";
    }
}