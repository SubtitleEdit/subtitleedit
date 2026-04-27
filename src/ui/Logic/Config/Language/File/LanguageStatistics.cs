namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageStatistics
{
    public string Title { get; set; }
    public string TitleWithFileName { get; set; }
    public string GeneralStatistics { get; set; }
    public string MostUsed { get; set; }
    public string MostUsedLines { get; set; }
    public string MostUsedWords { get; set; }
    public string NothingFound { get; set; }
    public string NumberOfLinesX { get; set; }
    public string NumberOfFilesX { get; set; }
    public string LengthInFormatXinCharactersY { get; set; }
    public string NumberOfCharactersInTextOnly { get; set; }
    public string TotalDuration { get; set; }
    public string TotalCharsPerSecond { get; set; }
    public string TotalWords { get; set; }
    public string NumberOfItalicTags { get; set; }
    public string NumberOfBoldTags { get; set; }
    public string NumberOfUnderlineTags { get; set; }
    public string NumberOfFontTags { get; set; }
    public string NumberOfAlignmentTags { get; set; }
    public string LineLengthMinimum { get; set; }
    public string LineLengthMaximum { get; set; }
    public string LineLengthAverage { get; set; }
    public string LinesPerSubtitleAverage { get; set; }
    public string SingleLineLengthMinimum { get; set; }
    public string SingleLineLengthMaximum { get; set; }
    public string SingleLineLengthAverage { get; set; }
    public string SingleLineLengthExceedingMaximum { get; set; }
    public string SingleLineWidthMinimum { get; set; }
    public string SingleLineWidthMaximum { get; set; }
    public string SingleLineWidthAverage { get; set; }
    public string SingleLineWidthExceedingMaximum { get; set; }
    public string DurationMinimum { get; set; }
    public string DurationMaximum { get; set; }
    public string DurationAverage { get; set; }
    public string DurationExceedingMinimum { get; set; }
    public string DurationExceedingMaximum { get; set; }
    public string CharactersPerSecondMinimum { get; set; }
    public string CharactersPerSecondMaximum { get; set; }
    public string CharactersPerSecondAverage { get; set; }
    public string CharactersPerSecondExceedingOptimal { get; set; }
    public string CharactersPerSecondExceedingMaximum { get; set; }
    public string WordsPerMinuteMinimum { get; set; }
    public string WordsPerMinuteMaximum { get; set; }
    public string WordsPerMinuteAverage { get; set; }
    public string WordsPerMinuteExceedingMaximum { get; set; }
    public string GapMinimum { get; set; }
    public string GapMaximum { get; set; }
    public string GapAverage { get; set; }
    public string GapExceedingMinimum { get; set; }
    public string Export { get; set; }

    public LanguageStatistics()
    {
        Title = "Statistics";
        TitleWithFileName = "Statistics - {0}";
        GeneralStatistics = "General statistics";
        NothingFound = "Nothing found";
        MostUsed = "Most used...";
        MostUsedWords = "Most used words";
        MostUsedLines = "Most used lines";
        NumberOfLinesX = "Number of subtitle lines: {0:#,##0}";
        NumberOfFilesX = "Number of subtitle files: {0:#,##0}";
        LengthInFormatXinCharactersY = "Number of characters as {0}: {1:#,###,##0}";
        NumberOfCharactersInTextOnly = "Number of characters in text only: {0:#,###,##0}";
        NumberOfItalicTags = "Number of italic tags: {0:#,##0}";
        TotalDuration = "Total duration of all subtitles: {0:#,##0}";
        TotalCharsPerSecond = "Total characters/second: {0:0.0} seconds";
        TotalWords = "Total words in subtitle: {0:#,##0}";
        NumberOfBoldTags = "Number of bold tags: {0:#,##0}";
        NumberOfUnderlineTags = "Number of underline tags: {0:#,##0}";
        NumberOfFontTags = "Number of font tags: {0:#,##0}";
        NumberOfAlignmentTags = "Number of alignment tags: {0:#,##0}";
        LineLengthMinimum = "Subtitle length - minimum: {0}";
        LineLengthMaximum = "Subtitle length - maximum: {0}";
        LineLengthAverage = "Subtitle length - average: {0:#.###}";
        LinesPerSubtitleAverage = "Subtitle, number of lines - average: {0:0.###}";
        SingleLineLengthMinimum = "Single line length - minimum: {0}";
        SingleLineLengthMaximum = "Single line length - maximum: {0}";
        SingleLineLengthAverage = "Single line length - average: {0:#.###}";
        SingleLineLengthExceedingMaximum = "Single line length - exceeding maximum ({0} chars): {1} ({2:0.00}%)";
        SingleLineWidthMinimum = "Single line width - minimum: {0} pixels";
        SingleLineWidthMaximum = "Single line width - maximum: {0} pixels";
        SingleLineWidthAverage = "Single line width - average: {0:#.###} pixels";
        SingleLineWidthExceedingMaximum = "Single line width - exceeding maximum ({0} pixels): {1} ({2:0.00}%)";
        DurationMinimum = "Duration - minimum: {0:0.000} seconds";
        DurationMaximum = "Duration - maximum: {0:0.000} seconds";
        DurationAverage = "Duration - average: {0:0.000} seconds";
        DurationExceedingMinimum = "Duration - below minimum ({0:0.###} sec): {1} ({2:0.00}%)";
        DurationExceedingMaximum = "Duration - exceeding maximum ({0:0.###} sec): {1} ({2:0.00}%)";
        CharactersPerSecondMinimum = "Characters/sec - minimum: {0:0.000}";
        CharactersPerSecondMaximum = "Characters/sec - maximum: {0:0.000}";
        CharactersPerSecondAverage = "Characters/sec - average: {0:0.000}";
        CharactersPerSecondExceedingOptimal = "Characters/sec - exceeding optimal ({0:0.##} cps): {1} ({2:0.00}%)";
        CharactersPerSecondExceedingMaximum = "Characters/sec - exceeding maximum ({0:0.##} cps): {1} ({2:0.00}%)";
        WordsPerMinuteMinimum = "Words/min - minimum: {0:0.000}";
        WordsPerMinuteMaximum = "Words/min - maximum: {0:0.000}";
        WordsPerMinuteAverage = "Words/min - average: {0:0.000}";
        WordsPerMinuteExceedingMaximum = "Words/min - exceeding maximum ({0} wpm): {1} ({2:0.00}%)";
        GapMinimum = "Gap - minimum: {0:#,##0} ms";
        GapMaximum = "Gap - maximum: {0:#,##0} ms";
        GapAverage = "Gap - average: {0:#,##0.##} ms";
        GapExceedingMinimum = "Gap - below minimum ({0:#,##0} ms): {1} ({2:0.00}%)";
        Export = "Export...";
    }
}
