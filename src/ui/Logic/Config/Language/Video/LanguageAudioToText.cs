namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAudioToText
{
    public string Title { get; set; }
    public string Transcribe { get; set; }
    public string TranslateToEnglish { get; set; }
    public string Transcribing { get; set; }
    public string TranscribingXOfY { get; set; }
    public string InputLanguage { get; set; }
    public string AdvancedWhisperSettings { get; set; }
    public string DownloadingSpeechToTextEngine { get; set; }
    public string UnpackingSpeechToTextEngine { get; set; }
    public string EnableVad { get; set; }
    public string WhisperXxlStandard { get; set; }
    public string WhisperXxlStandardAsia { get; set; }
    public string WhisperXxlSentence { get; set; }
    public string WhisperXxlSingleWords { get; set; }
    public string WhisperXxlHighlightWord { get; set; }
    public string SelectModel { get; set; }
    public string AddCustomModelDotDotDot { get; set; }
    public string CustomModelHelp { get; set; }
    public string ViewToolsLogFile { get; set; }
    public string UpdateXTitle { get; set; }
    public string UpdateXMessage { get; set; }
    public string? DownloadingSpeechToTextModel { get; set; }
    public string WhisperPostProcessingTitle { get; set; }
    public string AdjustTimings { get; set; }
    public string MergeShortLines { get; set; }
    public string BreakSplitLongLines { get; set; }
    public string FixShortDuration { get; set; }
    public string FixCasing { get; set; }
    public string AddPeriods { get; set; }
    public string ChangeUnderlineToColor { get; set; }
    public string RemoveNonSpeechLines { get; set; }
    public string RemoveNonSpeechLinesHint { get; set; }
    public string RemoveRepeatedLines { get; set; }
    public string RemoveRepeatedLinesHint { get; set; }
    public string ShowQualityReport { get; set; }
    public string QualityReportTitle { get; set; }
    public string QualityReportNoIssues { get; set; }
    public string QualityReportSummaryX { get; set; }
    public string QualityReportLinesChecked { get; set; }
    public string QualityReportIssuesFound { get; set; }
    public string QualityReportLinesRemoved { get; set; }
    public string QualityReportTooShort { get; set; }
    public string QualityReportTooShortHint { get; set; }
    public string QualityReportTooLong { get; set; }
    public string QualityReportTooLongHint { get; set; }
    public string QualityReportOverlap { get; set; }
    public string QualityReportOverlapHint { get; set; }
    public string QualityReportNonSpeech { get; set; }
    public string QualityReportNonSpeechHint { get; set; }
    public string QualityReportRepeated { get; set; }
    public string QualityReportRepeatedHint { get; set; }
    public string QualityReportRemoved { get; set; }
    public string QualityReportAll { get; set; }
    public string QualityReportIssue { get; set; }
    public string QualityReportDetail { get; set; }
    public string QualityReportTip { get; set; }
    public string QualityReportDoNotShowAgain { get; set; }

    public string EngineSettings { get; set; }
    public string EngineSettingsSubtitle { get; set; }
    public string BackendAndUpdateStatus { get; set; }
    public string AddLanguageCodeToFileName { get; set; }
    public string AddLanguageCodeToFileNameHint { get; set; }

    public LanguageAudioToText()
    {
        Title = "Speech to text";
        Transcribe = "Transcribe";
        TranslateToEnglish = "Translate to English";
        Transcribing = "Transcribing...";
        TranscribingXOfY = "Transcribing {0} of {1}...";
        InputLanguage = "Input language";
        AdvancedWhisperSettings = "Advanced speech-to-text parameters";
        DownloadingSpeechToTextEngine = "Downloading speech-to-text engine";
        UnpackingSpeechToTextEngine = "Unpacking speech-to-text engine";
        EnableVad = "Enable VAD";
        WhisperXxlStandard = "Standard";
        WhisperXxlStandardAsia = "Standard Asia";
        WhisperXxlSentence = "Sentence-level";
        WhisperXxlSingleWords = "Single words";
        WhisperXxlHighlightWord = "Highlight word";
        SelectModel = "Select model";
        AddCustomModelDotDotDot = "Add custom model...";
        CustomModelHelp = "You can use your own model: pick the model file (whisper.cpp ggml '.bin') or model folder (faster-whisper folder with a 'model.bin' inside). It is copied to the models folder and added to the list above.";
        ViewToolsLogFile = "View tools log file";
        UpdateXTitle = "Update {0}?";
        UpdateXMessage = "A newer version of {0} is available.{1}{1}Download and install the update now?";
        DownloadingSpeechToTextModel = "Downloading speech-to-text model";
        WhisperPostProcessingTitle = "Whisper post-processing";
        AdjustTimings = "Adjust timings";
        MergeShortLines = "Merge short lines";
        BreakSplitLongLines = "Break/split long lines";
        FixShortDuration = "Fix short duration";
        FixCasing = "Fix casing";
        AddPeriods = "Add periods";
        ChangeUnderlineToColor = "Change underline to color";
        RemoveNonSpeechLines = "Remove non-speech lines";
        RemoveNonSpeechLinesHint = "Drop lines that only describe sound, like \"[Music]\" or \"(waves crashing)\"";
        RemoveRepeatedLines = "Remove repeated lines";
        RemoveRepeatedLinesHint = "Drop lines that repeat the previous line word for word (engine loops)";
        ShowQualityReport = "Show quality report after transcription";
        QualityReportTitle = "Transcription quality report";
        QualityReportNoIssues = "No issues found - the transcription looks good.";
        QualityReportSummaryX = "{0} issue(s) found in {1} line(s)";
        QualityReportLinesChecked = "Lines checked";
        QualityReportIssuesFound = "Issues found";
        QualityReportLinesRemoved = "Lines removed";
        QualityReportTooShort = "Too short";
        QualityReportTooShortHint = "Shorter than the minimum display time, or reading speed above the maximum";
        QualityReportTooLong = "Too long";
        QualityReportTooLongHint = "Longer than the maximum display time, or very few words over a long time (often a hallucination)";
        QualityReportOverlap = "Overlapping";
        QualityReportOverlapHint = "Ends after the next line starts";
        QualityReportNonSpeech = "Non-speech";
        QualityReportNonSpeechHint = "Only a sound or music description, like \"[Music]\"";
        QualityReportRepeated = "Repeated";
        QualityReportRepeatedHint = "Same text as the previous line (engine loop)";
        QualityReportRemoved = "Removed";
        QualityReportAll = "All";
        QualityReportIssue = "Issue";
        QualityReportDetail = "Detail";
        QualityReportTip = "Lines are listed with their numbers in the new subtitle. Use \"Fix common errors\" or the post-processing settings to fix more automatically, or try another model/engine if many lines are affected.";
        QualityReportDoNotShowAgain = "Do not show this report again";

        EngineSettings = "Speech-to-text engine settings";
        EngineSettingsSubtitle = "Speech-to-text engine";
        BackendAndUpdateStatus = "Backend and update status";
        AddLanguageCodeToFileName = "Add language code to file name";
        AddLanguageCodeToFileNameHint = "Name generated subtitles like \"video.en.srt\" instead of \"video.srt\"";
    }
}