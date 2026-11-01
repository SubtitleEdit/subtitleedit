namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAudioToText
{
    public string Title { get; set; }
    public string Transcribe { get; set; }
    public string TranslateToEnglish { get; set; }
    public string Transcribing { get; set; }
    public string TranscribingXOfY { get; set; }
    public string LinesTranscribedXOfYFailedZ { get; set; }
    public string LinesTranscribedXOfYCancelled { get; set; }
    public string NoLinesTranscribed { get; set; }
    public string ApplyTranscribedLines { get; set; }
    public string RetryFailedLines { get; set; }
    public string IsolateSpeech { get; set; }
    public string IsolateSpeechHint { get; set; }
    public string IsolatingSpeech { get; set; }
    public string IsolateSpeechFailed { get; set; }
    public string DetectSpeakers { get; set; }
    public string DetectSpeakersHint { get; set; }
    public string DetectSpeakersNeedsNewerCrispAsr { get; set; }
    public string XNeedsNewerCrispAsr { get; set; }
    public string Vad { get; set; }
    public string VadHint { get; set; }
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
    public string CustomModelHelpGguf { get; set; }
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
    public string QualityReportTooShortHint { get; set; }
    public string QualityReportTooLongHint { get; set; }
    public string QualityReportOverlapHint { get; set; }
    public string QualityReportNonSpeech { get; set; }
    public string QualityReportNonSpeechHint { get; set; }
    public string QualityReportRepeated { get; set; }
    public string QualityReportRepeatedHint { get; set; }
    public string QualityReportRemoved { get; set; }
    public string QualityReportAll { get; set; }
    public string QualityReportIssue { get; set; }
    public string QualityReportTip { get; set; }
    public string QualityReportDoNotShowAgain { get; set; }

    public string EngineSettings { get; set; }
    public string EngineSettingsSubtitle { get; set; }
    public string BackendAndUpdateStatus { get; set; }
    public string AddLanguageCodeToFileName { get; set; }
    public string AddLanguageCodeToFileNameHint { get; set; }
    public string KeepPartialTranscriptionQuestion { get; set; }
    public string KeepXLinesQuestion { get; set; }
    public string EngineCouldNotStart { get; set; }
    public string SharedLibraryXMissing { get; set; }
    public string SharedLibraryBundledRedownload { get; set; }
    public string SharedLibraryInstallIt { get; set; }
    public string IncompleteModel { get; set; }
    public string IncompleteModelMessage { get; set; }
    public string UnknownArgumentX { get; set; }
    public string UnknownArgumentMessage { get; set; }
    public string CudaFailed { get; set; }
    public string CudaOutOfMemory { get; set; }
    public string CuBlasFailed { get; set; }
    public string CuBlasNotSupportedCause { get; set; }
    public string TryAnotherModelEngineOrCpu { get; set; }
    public string ComputeTypeAlreadySet { get; set; }
    public string AddingXFixesThisAddNow { get; set; }
    public string CudaNotAvailable { get; set; }
    public string WhisperXCpuOnly { get; set; }
    public string EngineBuiltWithoutCuda { get; set; }
    public string RemoveGpuSettingsOrRunOnCpu { get; set; }
    public string RemoveGpuSettingsQuestionX { get; set; }
    public string NoResultFile { get; set; }
    public string NoResultFileMessage { get; set; }
    public string NoTranscriptionResult { get; set; }
    public string CrispAsrNoSubtitles { get; set; }
    public string CouldNotGenerateAudioFileX { get; set; }
    public string NoVideoFile { get; set; }
    public string NoVideoFileFound { get; set; }
    public string UnableToStartX { get; set; }
    public string UnableToStartXY { get; set; }
    public string ProgramFileXNotFoundYRedownload { get; set; }
    public string XRequiresEngineSelectVersion { get; set; }
    public string XRequiresCrispAsrSelectVersion { get; set; }
    public string XRequiresForcedAlignerUseY { get; set; }
    public string UpdatedModelXAvailable { get; set; }
    public string XSelectedNotInstalledUseY { get; set; }
    public string EnglishModelContinue { get; set; }
    public string NoAudioTrackFound { get; set; }
    public string NoAudioTrackFoundInX { get; set; }

    public LanguageAudioToText()
    {
        KeepPartialTranscriptionQuestion = "Keep partial transcription?";
        KeepXLinesQuestion = "Do you want to keep {0} lines?";
        EngineCouldNotStart = "Speech to text engine could not start";
        SharedLibraryXMissing = "The speech to text engine could not start - the shared library \"{0}\" is missing.";
        SharedLibraryBundledRedownload = "This library is part of the engine download, so the installed engine is incomplete.\n\nRe-download the engine (Download button next to the engine) and try again.";
        SharedLibraryInstallIt = "Install it with your package manager and try again.";
        IncompleteModel = "Incomplete model";
        IncompleteModelMessage = "The model is incomplete. Please download the full model.";
        UnknownArgumentX = "Unknown argument: {0}";
        UnknownArgumentMessage = "Unknown argument. Please check the advanced settings.";
        CudaFailed = "CUDA failed";
        CudaOutOfMemory = "Whisper ran out of CUDA memory - try a smaller model or run on CPU.";
        CuBlasFailed = "cuBLAS failed";
        CuBlasNotSupportedCause = "The GPU could not run the model with the compute type the engine picked - cuBLAS returned CUBLAS_STATUS_NOT_SUPPORTED, and no text was transcribed.";
        TryAnotherModelEngineOrCpu = "Try another model, another engine, or run on CPU.";
        ComputeTypeAlreadySet = "The parameters already set \"--compute_type\" - try another value, such as \"float16\", \"float32\", or \"bfloat16\" - the \"int8\" types fail this way on many newer GPUs.";
        AddingXFixesThisAddNow = "Adding \"{0}\" to the parameters normally fixes this.\n\nAdd it now?";
        CudaNotAvailable = "CUDA not available";
        WhisperXCpuOnly = "This WhisperX build runs on the CPU only - it cannot use \"--device cuda\", so no text was transcribed.";
        EngineBuiltWithoutCuda = "This engine was built without CUDA support, so it cannot run on the GPU and no text was transcribed.";
        RemoveGpuSettingsOrRunOnCpu = "Remove any GPU settings from the parameters, or run on CPU.";
        RemoveGpuSettingsQuestionX = "Remove the GPU settings from the parameters so it runs on the CPU?\n\nNew parameters: {0}";
        NoResultFile = "No result file";
        NoResultFileMessage = "No result file was generated by Whisper, but some text was captured from standard output.";
        NoTranscriptionResult = "No transcription result";
        CrispAsrNoSubtitles = "Crisp ASR finished without generating subtitles. Please check the tools log for engine output.";
        CouldNotGenerateAudioFileX = "Could not generate the audio file (ffmpeg exit code {0}).\nPlease check the tools log for the ffmpeg output.";
        NoVideoFile = "No video file";
        NoVideoFileFound = "No video file found!";
        UnableToStartX = "Unable to start {0}";
        UnableToStartXY = "Unable to start {0}: {1}";
        ProgramFileXNotFoundYRedownload = "The {0} program file was not found:\n{1}\n\nIt may have been deleted or quarantined by antivirus software - if so, please restore it or add an exclusion for the folder.\n\nDo you want to re-download \"{0}\"?";
        XRequiresEngineSelectVersion = "\"{0}\" requires downloading the engine.\n\nSelect a version to download:";
        XRequiresCrispAsrSelectVersion = "\"{0}\" requires downloading the CrispASR engine.\n\nSelect a version to download:";
        XRequiresForcedAlignerUseY = "\"{0}\" requires a forced aligner to create timestamps.\nDownload and use {1}?";
        UpdatedModelXAvailable = "An updated {0} is available (re-converted for Crisp ASR v0.8.36).\nDownload and use it?";
        XSelectedNotInstalledUseY = "\"{0}\" is selected but not installed.\nDownload and use {1}?";
        EnglishModelContinue = "English model should only be used with English language.\nContinue anyway?";
        NoAudioTrackFound = "No audio track found";
        NoAudioTrackFoundInX = "No audio track was found in {0}";
        Title = "Speech to text";
        Transcribe = "Transcribe";
        TranslateToEnglish = "Translate to English";
        Transcribing = "Transcribing...";
        TranscribingXOfY = "Transcribing {0} of {1}...";
        LinesTranscribedXOfYFailedZ = "{0} of {1} lines were transcribed - {2} failed or had no speech.\n\nFailed lines keep their current text. Apply the transcribed lines, or retry the failed ones (you can change engine or settings first)?";
        LinesTranscribedXOfYCancelled = "Transcription was cancelled after {0} of {1} lines.\n\nApply the transcribed lines? The other lines keep their current text.";
        NoLinesTranscribed = "None of the {0} lines could be transcribed.\nPlease check the tools log for details.";
        ApplyTranscribedLines = "Apply transcribed";
        RetryFailedLines = "Retry failed";
        IsolateSpeech = "Isolate speech (slow)";
        IsolateSpeechHint = "Removes music and sound effects before transcribing. Gives fewer missed lines and better timing on audio with loud music, but takes about as long as the audio itself with a GPU - and many times longer without one.";
        IsolatingSpeech = "Isolating speech...";
        IsolateSpeechFailed = "Could not isolate the speech - transcribing the original audio instead.";
        DetectSpeakers = "Detect speakers";
        DetectSpeakersHint = "Starts each line with the speaker, like \"(speaker 0)\", using NVIDIA's Sortformer diarization (up to 8 speakers). Works best with backends that split lines where the speaker changes, like Parakeet and Cohere.";
        DetectSpeakersNeedsNewerCrispAsr = "\"Detect speakers\" needs Crisp ASR v{0} or newer, but v{1} is installed.\n\nUpdate Crisp ASR (the gear button next to the engine) or turn off \"Detect speakers\".";
        XNeedsNewerCrispAsr = "{0} needs Crisp ASR v{1} or newer, but v{2} is installed.\n\nDownload and install the update now?";
        Vad = "VAD";
        VadHint = "Voice activity detection: only the speech is transcribed, cut where the speaker pauses.\nAuto: Silero, and only for the engines that need it (Cohere, Mega, Index-Echo, Granite).\nSilero, FireRedVAD, Whisper-VAD, WebRTC: always on, for every Crisp ASR engine. FireRedVAD is the most accurate, Whisper-VAD gives the fewest, cleanest cuts but is the slowest.\nIndex-Echo always uses Silero. --vad, --vad-model or --chunk-seconds in the advanced settings take precedence.";
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
        CustomModelHelpGguf = "You can use your own model: pick a qwen3-asr.cpp '.gguf' file (one file holding both the audio encoder and the language model, e.g. from OpenVoiceOS or FlippyDora on Hugging Face). It is copied to the models folder and added to the list above. GGUF files made for llama.cpp, transcribe.cpp or CapsWriter use a different layout and cannot be used.";
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
        QualityReportTooShortHint = "Shorter than the minimum display time, or reading speed above the maximum";
        QualityReportTooLongHint = "Longer than the maximum display time, or very few words over a long time (often a hallucination)";
        QualityReportOverlapHint = "Ends after the next line starts";
        QualityReportNonSpeech = "Non-speech";
        QualityReportNonSpeechHint = "Only a sound or music description, like \"[Music]\"";
        QualityReportRepeated = "Repeated";
        QualityReportRepeatedHint = "Same text as the previous line (engine loop)";
        QualityReportRemoved = "Removed";
        QualityReportAll = "All";
        QualityReportIssue = "Issue";
        QualityReportTip = "Lines are listed with their numbers in the new subtitle. Use \"Fix common errors\" or the post-processing settings to fix more automatically, or try another model/engine if many lines are affected.";
        QualityReportDoNotShowAgain = "Do not show this report again";

        EngineSettings = "Speech-to-text engine settings";
        EngineSettingsSubtitle = "Speech-to-text engine";
        BackendAndUpdateStatus = "Backend and update status";
        AddLanguageCodeToFileName = "Add language code to file name";
        AddLanguageCodeToFileNameHint = "Name generated subtitles like \"video.en.srt\" instead of \"video.srt\"";
    }
}