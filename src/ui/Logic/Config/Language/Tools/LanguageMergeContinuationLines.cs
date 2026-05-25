namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeContinuationLines
{
    public string Title { get; set; }
    public string CandidatesFoundX { get; set; }
    public string NoCandidatesFound { get; set; }
    public string MaxMillisecondsBetweenLines { get; set; }
    public string MaxCharacters { get; set; }
    public string ColumnMerge { get; set; }
    public string ColumnFirst { get; set; }
    public string ColumnSecond { get; set; }
    public string ColumnMerged { get; set; }
    public string SelectAll { get; set; }
    public string InverseSelection { get; set; }

    public LanguageMergeContinuationLines()
    {
        Title = "Merge continuation lines";
        CandidatesFoundX = "Continuation candidates found: {0}";
        NoCandidatesFound = "No continuation candidates found";
        MaxMillisecondsBetweenLines = "Max gap between lines (ms)";
        MaxCharacters = "Max characters per paragraph";
        ColumnMerge = "Merge";
        ColumnFirst = "First line";
        ColumnSecond = "Second line";
        ColumnMerged = "Merged";
        SelectAll = "Select all";
        InverseSelection = "Inverse selection";
    }
}
