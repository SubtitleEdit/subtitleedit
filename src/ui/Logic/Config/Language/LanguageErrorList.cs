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
    public string LineTooLong { get; set; }
    public string LineTooWide { get; set; }
    public string GapTooShort { get; set; }
    public string GapInRange { get; set; }
    public string CustomRule { get; set; }
    public string TooManyLinesHint { get; set; }
    public string CharactersPerSecondHint { get; set; }
    public string DurationTooShortHint { get; set; }
    public string DurationTooLongHint { get; set; }
    public string LineTooLongHint { get; set; }
    public string LineTooWideHint { get; set; }
    public string OverlapHint { get; set; }
    public string GapTooShortHint { get; set; }
    public string GapInRangeHint { get; set; }
    public string CustomRuleHint { get; set; }
    public string DetailXGreaterThanY { get; set; }
    public string DetailXLessThanY { get; set; }
    public string DetailOverlapFromPrevious { get; set; }
    public string DetailOverlapToNext { get; set; }
    public string DetailGapToPrevious { get; set; }
    public string DetailGapToNext { get; set; }
    public string DetailGapInRangeToPrevious { get; set; }
    public string DetailGapInRangeToNext { get; set; }
    public string Tip { get; set; }
    public string ExportAsText { get; set; }
    public string ExportAsExcel { get; set; }
    public string ExportAsHtml { get; set; }
    public string ExportFileX { get; set; }
    public string ExportGeneratedX { get; set; }
    public string ExportMadeWithSubtitleEdit { get; set; }

    public LanguageErrorList()
    {
        Title = "List errors";
        SummaryX = "{0} error(s) in {1} of {2} line(s)";
        NoErrors = "No errors found.";
        SummaryFilesX = "{0} error(s) in {1} line(s) across {2} file(s)";
        All = "All";
        TooManyLines = "Too many lines";
        CharactersPerSecond = "Reading speed";
        LineTooLong = "Line too long";
        LineTooWide = "Line too wide";
        GapTooShort = "Gap too short";
        GapInRange = "Gap in range";
        CustomRule = "Custom rule";
        TooManyLinesHint = "More lines than the maximum number of lines";
        CharactersPerSecondHint = "Characters per second above the maximum";
        DurationTooShortHint = "Shorter than the minimum display time";
        DurationTooLongHint = "Longer than the maximum display time";
        LineTooLongHint = "A line has more characters than the maximum line length";
        LineTooWideHint = "A line is wider (in pixels) than the maximum width";
        OverlapHint = "Overlaps the previous or next line";
        GapTooShortHint = "Gap to the previous or next line is below the minimum";
        GapInRangeHint = "Gap to the previous or next line is inside the range to avoid";
        CustomRuleHint = "The text matches one of your custom error rules";
        DetailXGreaterThanY = "{0} > {1}";
        DetailXLessThanY = "{0} < {1}";
        DetailOverlapFromPrevious = "from previous: {0} ms";
        DetailOverlapToNext = "to next: {0} ms";
        DetailGapToPrevious = "to previous: {0} < {1} ms";
        DetailGapToNext = "to next: {0} < {1} ms";
        DetailGapInRangeToPrevious = "to previous: {0} ms (range {1}-{2} ms)";
        DetailGapInRangeToNext = "to next: {0} ms (range {1}-{2} ms)";
        Tip = "Double-click a line (or press Enter) to go to it. Which checks run, and their limits, are set in Settings > General.";
        ExportAsText = "Text file (.txt)...";
        ExportAsExcel = "Excel file (.xlsx)...";
        ExportAsHtml = "Web page (.html)...";
        ExportFileX = "File: {0}";
        ExportGeneratedX = "Generated: {0}";
        ExportMadeWithSubtitleEdit = "Made with Subtitle Edit";
    }
}
