namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImport
{
    public string ImportTimeCodes { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string TitleImportPlainText { get; set; }
    public string CsvXlsxCustomColumnsDotDotDot { get; set; }
    public string TitleImportCsvXlsxCustomColumns { get; set; }
    public string DetectedSeparatorX { get; set; }
    public string CsvXlsxFilterTitle { get; set; }
    public string SeparatorTab { get; set; }
    public string SeparatorSpace { get; set; }
    public string ImagesForOcrDotDotDot { get; set; }
    public string TimeCodesDotDotDot { get; set; }
    public string SubtitleWithManuallyChosenEncodingDotDotDot { get; set; }
    public string TitleImportImages { get; set; }
    public string ImportFileLabel { get; set; }
    public string ImportFilesInfo { get; set; }
    public string FormattingDotDotDot { get; set; }
    public string ImageBasedSubtitleForEditDotDotDot { get;  set; }
    public string ImageBasedSubtitleForOcrDotDotDot { get;  set; }
    public string SplitTextAt { get; set; }
    public string BlankLines { get; set; }
    public string OneLineIsOneSubtitle { get; set; }
    public string TwoLinesAreOneSubtitle { get; set; }
    public string ImportFilesDotDotDot { get; set; }
    public string MultipleFiles { get; set; }
    public string ImportOptions { get; set; }
    public string AutoSplitText { get; set; }
    public string LineMode { get; set; }
    public string MaxLineLength { get; set; }
    public string MaxLinesPerSubtitle { get; set; }
    public string MinGapBetweenSubtitles { get; set; }
    public string MergeShortLines { get; set; }
    public string RemoveLinesWithoutLetters { get; set; }
    public string SplitAtEndCharsSetting { get; set; }
    public string TakeTimeFromCurrentFile { get; set; }
    public string Fixed { get; set; }
    public string NumberOfSubtitlesX { get; set; }
    public string GapMs { get; set; }
    public string UseFixedDuration { get; set; }
    public string FixedDurationMs { get; set; }
    public string AlignViaSpeechToText { get; set; }

    public LanguageImport()
    {
        PlainTextDotDotDot = "Plain text...";
        TitleImportPlainText = "Import plain text";
        ImportTimeCodes = "Import time codes...";
        ImagesForOcrDotDotDot = "Images for OCR...";
        TitleImportImages = "Import images";
        TimeCodesDotDotDot = "Time codes...";
        FormattingDotDotDot = "Formatting...";
        SubtitleWithManuallyChosenEncodingDotDotDot = "_Subtitle with manually chosen encoding...";
        ImportFileLabel = "Choose images to import (time codes in file names supported)";
        ImportFilesInfo = @"Use time-coded filenames:
start_HH_MM_SS_MMM__end_HH_MM_SS_MMM[_index].ext

Examples:
0_00_01_042__0_00_03_919_0001.png
0_00_01_042__0_00_03_919.png

Rules:
• HH_MM_SS_MMM for start and end times
• Double underscore separates start/end
• Optional index after end time";
        ImageBasedSubtitleForEditDotDotDot = "Image-based subtitle for edit...";
        ImageBasedSubtitleForOcrDotDotDot = "Image-based subtitle for OCR...";
        SplitTextAt = "Split text at";
        BlankLines = "Blank lines";
        OneLineIsOneSubtitle = "One line is one subtitle";
        TwoLinesAreOneSubtitle = "Two lines are one subtitle";
        ImportFilesDotDotDot = "Import files...";
        MultipleFiles = "Import from multiple text files (one file is one subtitle)";
        ImportOptions = "Import options";
        AutoSplitText = "Auto split text";
        LineMode = "Line mode";
        MaxLineLength = "Max line length";
        MaxLinesPerSubtitle = "Max lines per subtitle";
        MinGapBetweenSubtitles = "Min gap between subtitles";
        MergeShortLines = "Merge short lines";
        RemoveLinesWithoutLetters = "Remove lines without letters";
        SplitAtEndCharsSetting = "Split at end chars";
        TakeTimeFromCurrentFile = "Take time from current file";
        Fixed = "Fixed";
        NumberOfSubtitlesX = "Number of subtitles: {0}";
        GapMs = "Gap (ms)";
        UseFixedDuration = "Use fixed duration";
        FixedDurationMs = "Fixed duration (ms)";
        AlignViaSpeechToText = "Align time codes via \"Speech to text\"...";
        CsvXlsxCustomColumnsDotDotDot = "CSV/XLSX with custom columns...";
        TitleImportCsvXlsxCustomColumns = "Import CSV/XLSX with custom columns";
        DetectedSeparatorX = "Detected separator: {0}";
        CsvXlsxFilterTitle = "CSV/XLSX";
        SeparatorTab = "Tab";
        SeparatorSpace = "Space";
    }
}