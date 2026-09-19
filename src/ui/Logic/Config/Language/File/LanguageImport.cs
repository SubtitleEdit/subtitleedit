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
    public string MaxLineLength { get; set; }
    public string MergeShortLines { get; set; }
    public string Fixed { get; set; }
    public string NumberOfSubtitlesX { get; set; }
    public string NumberOfSubtitlesXForcedY { get; set; }
    public string GapMs { get; set; }
    public string UseFixedDuration { get; set; }
    public string FixedDurationMs { get; set; }
    public string AlignViaSpeechToText { get; set; }
    public string AlignViaForcedAligner { get; set; }
    public string ForcedAlignerProgress { get; set; }
    public string ForcedAlignerSetupTitle { get; set; }
    public string ForcedAlignerSetupIntro { get; set; }
    public string ForcedAlignerEndsFromSpeech { get; set; }
    public string ForcedAlignerEndsFromSpeechHint { get; set; }
    public string ForcedAlignerIsolatingSpeech { get; set; }
    public string ForcedAlignerIsolatingSpeechFailed { get; set; }
    public string ForcedAlignerModel { get; set; }
    public string ForcedAlignerDownloadEngine { get; set; }

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
        MaxLineLength = "Max line length";
        MergeShortLines = "Merge short lines";
        Fixed = "Fixed";
        NumberOfSubtitlesX = "Number of subtitles: {0}";
        NumberOfSubtitlesXForcedY = "Number of subtitles: {0} ({1} forced)";
        GapMs = "Gap (ms)";
        UseFixedDuration = "Use fixed duration";
        FixedDurationMs = "Fixed duration (ms)";
        AlignViaSpeechToText = "Align time codes via \"Speech to text\"...";
        AlignViaForcedAligner = "Align time codes via forced aligner...";
        ForcedAlignerProgress = "Aligning... window {0} of {1}, {2} of {3} lines";
        ForcedAlignerSetupTitle = "Align time codes via forced aligner";
        ForcedAlignerSetupIntro = "A forced aligner matches the subtitle text you already have against the audio, without transcribing it first. Long videos are aligned in chunks, so any length works.";
        ForcedAlignerEndsFromSpeech = "Set end times from isolated speech (slow)";
        ForcedAlignerEndsFromSpeechHint = "A forced aligner finds where a line starts, not where it ends, so end times normally come from reading time. With this on, music and sound effects are removed from the audio first and each line ends where its speech goes quiet. Takes about as long as the video itself with a GPU - and many times longer without one.";
        ForcedAlignerIsolatingSpeech = "Isolating speech... {0}";
        ForcedAlignerIsolatingSpeechFailed = "Could not isolate the speech - end times were set from reading time instead.";
        ForcedAlignerModel = "Aligner model";
        ForcedAlignerDownloadEngine = "Download / update engine...";
        CsvXlsxCustomColumnsDotDotDot = "CSV/XLSX/ODS with custom columns...";
        TitleImportCsvXlsxCustomColumns = "Import CSV/XLSX/ODS with custom columns";
        DetectedSeparatorX = "Detected separator: {0}";
        CsvXlsxFilterTitle = "CSV/XLSX/ODS";
        SeparatorTab = "Tab";
        SeparatorSpace = "Space";
    }
}