namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageErrorList
{
    public string Title { get; set; }
    public string SummaryX { get; set; }
    public string NoErrors { get; set; }
    public string SummaryFilesX { get; set; }
    public string All { get; set; }
    public string TooManyLines { get; set; }
    public string CharactersPerSecond { get; set; }
    public string DurationTooShort { get; set; }
    public string DurationTooLong { get; set; }
    public string LineTooLong { get; set; }
    public string LineTooWide { get; set; }
    public string Overlap { get; set; }
    public string GapTooShort { get; set; }
    public string TooManyLinesHint { get; set; }
    public string CharactersPerSecondHint { get; set; }
    public string DurationTooShortHint { get; set; }
    public string DurationTooLongHint { get; set; }
    public string LineTooLongHint { get; set; }
    public string LineTooWideHint { get; set; }
    public string OverlapHint { get; set; }
    public string GapTooShortHint { get; set; }
    public string DetailXGreaterThanY { get; set; }
    public string DetailXLessThanY { get; set; }
    public string DetailOverlapFromPrevious { get; set; }
    public string DetailOverlapToNext { get; set; }
    public string DetailGapToPrevious { get; set; }
    public string DetailGapToNext { get; set; }
    public string Detail { get; set; }
    public string Tip { get; set; }

    public LanguageErrorList()
    {
        Title = "List errors";
        SummaryX = "{0} error(s) in {1} of {2} line(s)";
        NoErrors = "No errors found.";
        SummaryFilesX = "{0} error(s) in {1} line(s) across {2} file(s)";
        All = "All";
        TooManyLines = "Too many lines";
        CharactersPerSecond = "Reading speed";
        DurationTooShort = "Too short";
        DurationTooLong = "Too long";
        LineTooLong = "Line too long";
        LineTooWide = "Line too wide";
        Overlap = "Overlapping";
        GapTooShort = "Gap too short";
        TooManyLinesHint = "More lines than the maximum number of lines";
        CharactersPerSecondHint = "Characters per second above the maximum";
        DurationTooShortHint = "Shorter than the minimum display time";
        DurationTooLongHint = "Longer than the maximum display time";
        LineTooLongHint = "A line has more characters than the maximum line length";
        LineTooWideHint = "A line is wider (in pixels) than the maximum width";
        OverlapHint = "Overlaps the previous or next line";
        GapTooShortHint = "Gap to the previous or next line is below the minimum";
        DetailXGreaterThanY = "{0} > {1}";
        DetailXLessThanY = "{0} < {1}";
        DetailOverlapFromPrevious = "from previous: {0} ms";
        DetailOverlapToNext = "to next: {0} ms";
        DetailGapToPrevious = "to previous: {0} < {1} ms";
        DetailGapToNext = "to next: {0} < {1} ms";
        Detail = "Detail";
        Tip = "Double-click a line (or press Enter) to go to it. Which checks run, and their limits, are set in Settings > General.";
    }
}
