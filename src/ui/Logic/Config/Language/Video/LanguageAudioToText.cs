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
    public string ReDownloadX { get; set; }
    public string DownloadX { get; set; }
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
    public string CueBuilding { get; set; }
    public string CueRebuild { get; set; }
    public string CueMaxChars { get; set; }
    public string CueMaxSeconds { get; set; }
    public string CueMaxCps { get; set; }
    public string VocabularyPrompt { get; set; }

    public string EngineSettings { get; set; }
    public string EngineSettingsSubtitle { get; set; }
    public string BackendAndUpdateStatus { get; set; }

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
        ReDownloadX = "Re-download {0}";
        DownloadX = "Download {0}";
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
        CueBuilding = "Cue building (MLX Whisper / Faster Whisper Mac)";
        CueRebuild = "Rebuild cues from word timestamps";
        CueMaxChars = "Max characters per cue";
        CueMaxSeconds = "Max seconds per cue";
        CueMaxCps = "Max characters per second";
        VocabularyPrompt = "Vocabulary prompt (names, places, terms)";

        EngineSettings = "Speech-to-text engine settings";
        EngineSettingsSubtitle = "Speech-to-text engine";
        BackendAndUpdateStatus = "Backend and update status";
    }
}