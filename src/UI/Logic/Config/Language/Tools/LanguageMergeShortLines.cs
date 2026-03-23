namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeShortLines
{
    public string Title { get; set; }
    public string HighlightParts { get; internal set; }

    public LanguageMergeShortLines()
    {
        Title = "Merge short lines";
        HighlightParts = "Highlight parts (karaoke)";
    }
}