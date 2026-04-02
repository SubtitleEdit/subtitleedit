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
    public string DownloadingWhisperEngine { get; set; }
    public string EnableVad { get; set; }
    public string WhisperXxlStandard { get; set; }
    public string WhisperXxlStandardAsia { get; set; }
    public string WhisperXxlSentence { get; set; }
    public string WhisperXxlSingleWords { get; set; }
    public string WhisperXxlHighlightWord { get; set; }
    public string SelectModel { get; set; }
    public string ViewWhisperLogFile { get; set; }
    public string ReDownloadX { get; set; }
    public string? DownloadingSpeechToTextModel { get; set; }
    public string WhisperPostProcessingTitle { get; set; }
    public string AdjustTimings { get; set; }
    public string MergeShortLines { get; set; }
    public string BreakSplitLongLines { get; set; }
    public string FixShortDuration { get; set; }
    public string FixCasing { get; set; }
    public string AddPeriods { get; set; }
    public string ChangeUnderlineToColor { get; set; }

    public LanguageAudioToText()
    {
        Title = "Speech to text";
        Transcribe = "Transcribe";
        TranslateToEnglish = "Translate to English";
        Transcribing = "Transcribing...";
        TranscribingXOfY = "Transcribing {0} of {1}...";
        InputLanguage = "Input language";
        AdvancedWhisperSettings = "Advanced Whisper settings";
        DownloadingWhisperEngine = "Downloading Whisper engine";
        EnableVad = "Enable VAD";
        WhisperXxlStandard = "Standard";
        WhisperXxlStandardAsia = "Standard Asia";
        WhisperXxlSentence = "Sentence-level";
        WhisperXxlSingleWords = "Single words";
        WhisperXxlHighlightWord = "Highlight word";
        SelectModel = "Select model";
        ViewWhisperLogFile = "View Whisper log file";
        ReDownloadX = "Re-download {0}";
        DownloadingSpeechToTextModel = "Downloading speech-to-text model";
        WhisperPostProcessingTitle = "Whisper post-processing";
        AdjustTimings = "Adjust timings";
        MergeShortLines = "Merge short lines";
        BreakSplitLongLines = "Break/split long lines";
        FixShortDuration = "Fix short duration";
        FixCasing = "Fix casing";
        AddPeriods = "Add periods";
        ChangeUnderlineToColor = "Change underline to color";
    }
}