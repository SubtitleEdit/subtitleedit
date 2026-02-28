using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageApplyMinGaps
{
    public string Title { get; set; }
    public string NumberOfGapsFixedX { get; set; }
    public string MinFramesBetweenLines { get; set; }
    public string MinMsBetweenLines { get; set; }
    public string ChangedGapFromXToYCommentZ { get; set; }

    public LanguageApplyMinGaps()
    {
        Title = "Apply minimum gaps between subtitles";
        NumberOfGapsFixedX = "Number of minimum gaps applied: {0}";
        MinFramesBetweenLines = "Minimum frames between lines";
        MinMsBetweenLines = "Minimum milliseconds between lines";
        ChangedGapFromXToYCommentZ = "Changed gap from {0} to {1} {2}";
    }
}