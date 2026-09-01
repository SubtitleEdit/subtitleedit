using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageTextToSpeech
{
    public string Title { get; set; }
    public string ReviewAudioSegments { get; set; }
    public string ReviewAudioSegmentsHistory { get; set; }
    public string Stability { get; set; }
    public string Similarity { get; set; }
    public string SpeakerBoost { get; set; }
    public string GenerateSpeechFromText { get; set; }
    public string TestVoice { get; set; }
    public string AddAudioToVideoFile { get; set; }
    public string EngineAndVoice { get; set; }
    public string Output { get; set; }
    public string XLinesFromY { get; set; }
    public string XLines { get; set; }
    public string XVoices { get; set; }
    public string ReviewAudioSegmentsHint { get; set; }
    public string AddAudioToVideoFileHint { get; set; }
    public string XElapsedYLeft { get; set; }
    public string XElapsed { get; set; }
    public string VoiceSettings { get; set; }
    public string VoiceSampleText { get; set; }
    public string RefreshVoices { get; set; }
    public string VideoEncodingSettings { get; set; }
    public string ElevenLabsSettings { get; set; }
    public string ElevenLabsSettingsResetHint { get; set; }
    public string RegenerateAudio { get; set; }
    public string AutoContinuePlaying { get; set; }
    public string PlayLine { get; set; }
    public string FitDurationToGeneratedAudio { get; set; }
    public string ResetTiming { get; set; }
    public string AddingAudioToVideoFileDotDotDot { get; set; }
    public string PreparingMergeDotDotDot { get; set; }
    public string ImportVoiceDotDotDot { get; set; }
    public string VoiceImportSuccessTitle { get; set; }
    public string VoiceXImported { get; set; }
    public string VoiceXCouldNotBeImported { get; set; }
    public string ImportPiperVoiceTitle { get; set; }
    public string PiperVoiceConfigMissingTitle { get; set; }
    public string PiperVoiceConfigMissingMessage { get; set; }
    public string DropAudioFileHereToImportVoice { get; set; }
    public string DropAudioFileHereHint { get; set; }
    public string VoiceCloneTranscriptTitle { get; set; }
    public string UseSpeechToTextDotDotDot { get; set; }

    // Auto cast: find the voices in the video and clone them
    public string AutoCastMenuItem { get; set; }
    public string AutoCastSpeakersTitle { get; set; }
    public string AutoCastSpeakersSubtitle { get; set; }
    public string AutoCastFoundXSpeakersInYLines { get; set; }
    public string AutoCastLines { get; set; }
    public string AutoCastAudio { get; set; }
    public string AutoCastSays { get; set; }
    public string AutoCastNeedsVideo { get; set; }
    public string AutoCastNoSpeakersFound { get; set; }
    public string AutoCastCloningX { get; set; }
    public string AutoCastDoneXVoices { get; set; }
    public string AutoCastNothingCloned { get; set; }
    public string AutoCastReplaceSubtitleQuestion { get; set; }

    // Per-line voice cloning ("Clone from video")
    public string CloneVoicePerLine { get; set; }
    public string CloneVoicePerLinePreparing { get; set; }
    public string CloneVoicePerLineNeedsVideo { get; set; }
    public string CloneVoicePerLineNoClips { get; set; }

    // First-clone consent dialog
    public string VoiceCloneConsentTitle { get; set; }
    public string VoiceCloneConsentHeader { get; set; }
    public string VoiceCloneConsentIntro { get; set; }
    public string VoiceCloneConsentPointPermission { get; set; }
    public string VoiceCloneConsentPointDisclose { get; set; }
    public string VoiceCloneConsentPointNoMarking { get; set; }
    public string VoiceCloneConsentPointNoImpersonation { get; set; }
    public string VoiceCloneConsentPointLocal { get; set; }
    public string VoiceCloneConsentPointModelLicense { get; set; }
    public string VoiceCloneConsentReadMore { get; set; }
    public string VoiceCloneConsentCheckBox { get; set; }
    public string VoiceCloneConsentAccept { get; set; }
    public string VoiceCloneConsentDeclined { get; set; }
    public string AdvancedTtsSettings { get; set; }
    public string AdvancedTtsAudioProcessing { get; set; }
    public string AdvancedTtsOutput { get; set; }
    public string ProAudioPostProcessing { get; set; }
    public string ProAudioPostProcessingDescription { get; set; }
    public string AudioDucking { get; set; }
    public string AudioDuckingDescription { get; set; }
    public string OriginalVolumePercent { get; set; }
    public string VadSilenceCompression { get; set; }
    public string VadSilenceCompressionDescription { get; set; }
    public string MaxSilenceMs { get; set; }
    public string HighQualityTimeStretch { get; set; }
    public string HighQualityTimeStretchDescription { get; set; }
    public string RubberbandInstalled { get; set; }
    public string RubberbandNotFound { get; set; }
    public string SilencePaddingMs { get; set; }
    public string SilencePaddingMsDescription { get; set; }
    public string OutputSampleRate { get; set; }
    public string OutputSampleRateDescription { get; set; }
    public string GenerationFolder { get; set; }
    public string GenerationFolderDescription { get; set; }
    public string DeleteTempFiles { get; set; }
    public string DeleteTempFilesDescription { get; set; }
    public string EdgeTtsRate { get; set; }
    public string EdgeTtsRateDescription { get; set; }
    public string EdgeTtsPitch { get; set; }
    public string EdgeTtsPitchDescription { get; set; }
    public string EdgeTtsVolume { get; set; }
    public string EdgeTtsVolumeDescription { get; set; }
    public string DownloadPiperPrompt { get; set; }

    public string OmniVoiceTtsSettings { get; set; }
    public string ReDownloadOmniVoiceTts { get; set; }
    public string SelectTheBuildToDownload { get; set; }
    public string DownloadTheLatestOmniVoiceTtsPrompt { get; set; }
    public string VulkanRuntimeMayBeRequired { get; set; }
    public string VulkanRuntimeNotDetectedMessage { get; set; }
    public string Qwen3TtsSettings { get; set; }
    public string KokoroTtsSettings { get; set; }
    public string ChatterboxTtsSettings { get; set; }
    public string PiperSettings { get; set; }
    public string VoiceInstruction { get; set; }
    public string VoiceInstructionHint { get; set; }
    public string VoiceGender { get; set; }
    public string VoiceAge { get; set; }
    public string VoicePitch { get; set; }
    public string VoiceAccent { get; set; }
    public string VoiceInstructionClonedVoiceNote { get; set; }

    // Cast dialog
    public string ActorVoicesTitle { get; set; }
    public string ActorVoicesSubtitle { get; set; }
    public string ActorVoicesAssignedXOfY { get; set; }
    public string ActorOrVoice { get; set; }
    public string ApplyDefaultToAll { get; set; }
    public string ClearAllAssignmentsConfirm { get; set; }
    public string SetupCast { get; set; }
    public string SetupCastHint { get; set; }
    public string ActorVoicesRowSettingsTitle { get; set; }
    public string VoiceSettingsForX { get; set; }
    public string VoiceInstructionFreeTextHint { get; set; }
    public string VoiceDesign { get; set; }
    public string NoActorsFoundMessage { get; set; }
    public string NoWebVttVoicesFoundMessage { get; set; }
    public string MergeContinuationLinesPromptTitle { get; set; }
    public string MergeContinuationLinesPromptMessage { get; set; }
    public string SkipNoiseLinesPromptTitle { get; set; }
    public string SkipNoiseLinesPromptMessage { get; set; }
    public string SkipNoiseLinesTitle { get; set; }
    public string SkipNoiseLinesFoundX { get; set; }
    public string SkipNoiseLinesColumnSkip { get; set; }
    public string DetectSpeakersPromptTitle { get; set; }
    public string DetectSpeakersPromptMessage { get; set; }
    public string DetectSpeakersTitle { get; set; }
    public string DetectSpeakersFoundX { get; set; }
    public string DetectSpeakersColumnUse { get; set; }
    public string DetectSpeakersSticky { get; set; }

    // Applying the TTS window's changes (review text edits, merged lines) back to the subtitle
    public string SubtitleUpdatedFromReviewSingular { get; set; }
    public string SubtitleUpdatedFromReviewPlural { get; set; }
    public string SubtitleMergedLinesAppliedSingular { get; set; }
    public string SubtitleMergedLinesAppliedPlural { get; set; }

    public LanguageTextToSpeech()
    {
        Title = "Text to speech";
        ReviewAudioSegments = "TTS - Review audio segments";
        ReviewAudioSegmentsHistory = "TTS - Review audio history";
        Stability = "Stability";
        Similarity = "Similarity";
        SpeakerBoost = "Speaker boost";
        GenerateSpeechFromText = "Generate speech from text";
        TestVoice = "Test voice";
        AddAudioToVideoFile = "Add audio to video file";
        EngineAndVoice = "Engine & voice";
        Output = "Output";
        XLinesFromY = "{0} lines from {1}";
        XLines = "{0} lines";
        XVoices = "{0} voices";
        ReviewAudioSegmentsHint = "Check each line before the final mix";
        AddAudioToVideoFileHint = "Mux the result into a new video file";
        XElapsedYLeft = "{0} elapsed \u00b7 ~{1} left";
        XElapsed = "{0} elapsed";
        VoiceSettings = "TTS - Voice settings";
        VoiceSampleText = "Voice sample text";
        RefreshVoices = "Refresh voices";
        VideoEncodingSettings = "TTS - Video encoding settings";
        ElevenLabsSettings = "TTS - ElevenLabs settings";
        ElevenLabsSettingsResetHint = "Reset ElevenLabs settings to default values";
        RegenerateAudio = "Regenerate audio";
        AutoContinuePlaying = "Auto-continue playing";
        PlayLine = "Play line";
        FitDurationToGeneratedAudio = "Fit duration to generated audio";
        ResetTiming = "Reset timing";
        AddingAudioToVideoFileDotDotDot = "Adding audio to video file...";
        PreparingMergeDotDotDot = "Preparing merge...";
        ImportVoiceDotDotDot = "Import voice...";
        VoiceImportSuccessTitle = "Voice imported";
        VoiceXImported = "Voice '{0}' imported successfully";
        VoiceXCouldNotBeImported = "Voice '{0}' could not be imported - see the log for details";
        ImportPiperVoiceTitle = "Open Piper voice model (.onnx)";
        PiperVoiceConfigMissingTitle = "Config file missing";
        PiperVoiceConfigMissingMessage = "A Piper voice needs its config file next to the model file: {0}";
        DropAudioFileHereToImportVoice = "Drop audio file here to import voice";
        DropAudioFileHereHint = ".wav or .mp3";
        VoiceCloneTranscriptTitle = "Enter transcript of the audio (required for voice cloning)";
        UseSpeechToTextDotDotDot = "Use speech-to-text...";
        AutoCastMenuItem = "Find voices in video and clone...";
        AutoCastSpeakersTitle = "Voices found in the video";
        AutoCastSpeakersSubtitle = "Name each speaker - the name becomes the actor in the subtitle and the name of the cloned voice. Give two speakers the same name to merge them into one voice.";
        AutoCastFoundXSpeakersInYLines = "{0} speakers in {1} lines";
        AutoCastLines = "Lines";
        AutoCastAudio = "Audio";
        AutoCastSays = "Says";
        AutoCastNeedsVideo = "Finding the voices needs an open video to listen to.";
        AutoCastNoSpeakersFound = "No speakers were found. The transcription has to come from a speech-to-text engine that tells speakers apart, such as Crisp ASR MOSS Diarize.";
        AutoCastCloningX = "Cloning {0}...";
        AutoCastDoneXVoices = "{0} voices cloned - open Text to speech to generate the dubbing";
        AutoCastNothingCloned = "None of the voices could be cloned - see the log for details.";
        AutoCastReplaceSubtitleQuestion = "This replaces the subtitle you have open with the transcription of the video. Continue?";
        CloneVoicePerLine = "Clone from video (voice of each line)";
        CloneVoicePerLinePreparing = "Taking the voice of each line from the video...";
        CloneVoicePerLineNeedsVideo = "Cloning the voice of each line needs the video the subtitle belongs to. Open the video and try again.";
        CloneVoicePerLineNoClips = "No audio could be taken from the video, so there is nothing to clone from. Check that the video has an audio track.";
        VoiceCloneConsentTitle = "Voice cloning - before you start";
        VoiceCloneConsentHeader = "You are about to clone a voice";
        VoiceCloneConsentIntro = "Cloning copies a real person's voice. That comes with rules in most places, and in the EU with legal duties that fall on you, not on Subtitle Edit. Please read this once.";
        VoiceCloneConsentPointPermission = "Only clone a voice you have the right to use - your own, or one where the speaker has given permission. A voice is personal data and a personality right, so cloning without permission can be unlawful.";
        VoiceCloneConsentPointDisclose = "If you publish audio that imitates a real person, you must say that it is AI-generated. In the EU this is required by the AI Act (Regulation (EU) 2024/1689, article 50), which applies from 2 August 2026.";
        VoiceCloneConsentPointNoMarking = "Subtitle Edit turns off the engine's spoken AI disclaimer, inaudible watermark and C2PA signature so the audio can be used in your video unchanged. That means nothing marks the result as AI-generated for you.";
        VoiceCloneConsentPointNoImpersonation = "Do not use a cloned voice to impersonate someone, to make a person appear to say things they never said, or for fraud, harassment or deception.";
        VoiceCloneConsentPointLocal = "The reference recording stays on this computer. Cloning runs locally and the audio is not uploaded to Subtitle Edit or anywhere else.";
        VoiceCloneConsentPointModelLicense = "Each speech model also has its own license, which may add further limits on commercial use.";
        VoiceCloneConsentReadMore = "About the EU AI Act transparency rules";
        VoiceCloneConsentCheckBox = "I have the right to clone this voice, and I will disclose that the generated audio is AI-generated";
        VoiceCloneConsentAccept = "Accept and continue";
        VoiceCloneConsentDeclined = "Voice cloning is not available until these terms are accepted.";
        AdvancedTtsSettings = "Advanced TTS settings";
        AdvancedTtsAudioProcessing = "Audio processing";
        AdvancedTtsOutput = "Output";
        ProAudioPostProcessing = "Pro audio post-processing";
        ProAudioPostProcessingDescription = "Adds EQ, noise gate, compression, loudness normalization (-16 LUFS) and a short fade in/out to every clip.";
        AudioDucking = "Audio ducking";
        AudioDuckingDescription = "Turns the original video sound down and mixes the speech over it, so the original track stays faintly audible. Only applies when the speech is added to the video file.";
        OriginalVolumePercent = "Original volume %";
        VadSilenceCompression = "VAD silence compression";
        VadSilenceCompressionDescription = "Shortens the pauses between words instead of speeding up the speech, so a clip fits without any loss of quality.";
        MaxSilenceMs = "Max silence (ms)";
        HighQualityTimeStretch = "High-quality time-stretch";
        HighQualityTimeStretchDescription = "Speeds up speech with the rubberband algorithm, which sounds more natural than the default. Needs librubberband in FFmpeg; otherwise the default is used.";
        RubberbandInstalled = "(installed)";
        RubberbandNotFound = "(not found in FFmpeg)";
        SilencePaddingMs = "Silence padding (ms)";
        SilencePaddingMsDescription = "Extra silence added at the end of every clip, to give room between sentences.";
        OutputSampleRate = "Output sample rate";
        OutputSampleRateDescription = "Resamples every clip, e.g. to 44100 or 48000. Use 0 to keep the original rate.";
        GenerationFolder = "Generation folder";
        GenerationFolderDescription = "Where clips are written while generating. Leave empty for the system temp folder. Each run gets its own sub-folder.";
        DeleteTempFiles = "Delete generated clips when closing";
        DeleteTempFilesDescription = "Deletes the generation folder when the Text to speech window closes. Saved audio and video files are kept.";
        EdgeTtsRate = "Edge-TTS rate";
        EdgeTtsRateDescription = "Speech rate, e.g. \"+50%\" or \"-30%\". Use \"+0%\" for the default.";
        EdgeTtsPitch = "Edge-TTS pitch";
        EdgeTtsPitchDescription = "Pitch adjustment, e.g. \"+10Hz\" or \"-5Hz\". Use \"+0Hz\" for the default.";
        EdgeTtsVolume = "Edge-TTS volume";
        EdgeTtsVolumeDescription = "Volume adjustment, e.g. \"+20%\" or \"-10%\". Use \"+0%\" for the default.";
        DownloadPiperPrompt = $"\"Text to speech\" requires Piper.{Environment.NewLine}{Environment.NewLine}Download and use Piper?";

        OmniVoiceTtsSettings = "OmniVoice TTS settings";
        ReDownloadOmniVoiceTts = "Re-download OmniVoice TTS";
        SelectTheBuildToDownload = $"{Environment.NewLine}Select the build to download:";
        DownloadTheLatestOmniVoiceTtsPrompt = $"{Environment.NewLine}Download the latest OmniVoice TTS now?";
        VulkanRuntimeMayBeRequired = "Vulkan runtime may be required";
        VulkanRuntimeNotDetectedMessage = $"The Vulkan build needs the Vulkan runtime (vulkan-1.dll). It usually ships with current GPU drivers but was not detected.{Environment.NewLine}{Environment.NewLine}Install it from:{Environment.NewLine}{{0}}{Environment.NewLine}{Environment.NewLine}Continue with Vulkan anyway?";
        Qwen3TtsSettings = "Qwen3 TTS settings";
        KokoroTtsSettings = "Kokoro TTS settings";
        ChatterboxTtsSettings = "Chatterbox TTS settings";
        PiperSettings = "Piper settings";
        VoiceInstruction = "Voice design";
        VoiceInstructionHint = "Optional - e.g. \"Speak in a calm and friendly tone\"";
        VoiceGender = "Gender";
        VoiceAge = "Age";
        VoicePitch = "Pitch";
        VoiceAccent = "Accent";
        VoiceInstructionClonedVoiceNote = "Voice design only affects the \"Default\" voice - a cloned voice keeps its own characteristics.";

        ActorVoicesTitle = "TTS - Cast";
        ActorVoicesSubtitle = "Assign a TTS voice (and optional voice-design instruction) to each actor or voice.";
        ActorVoicesAssignedXOfY = "{0} of {1} assigned";
        ActorOrVoice = "Actor / Voice";
        ApplyDefaultToAll = "Apply default to all";
        ClearAllAssignmentsConfirm = "Clear all voice assignments?";
        SetupCast = "Cast...";
        SetupCastHint = "Assign a TTS voice to each actor (ASSA) or voice (WebVTT).";
        ActorVoicesRowSettingsTitle = "TTS - Voice settings";
        VoiceSettingsForX = "Voice settings for \"{0}\"";
        VoiceInstructionFreeTextHint = "Free text used by the engine to shape the voice's tone.";
        VoiceDesign = "Voice design";
        NoActorsFoundMessage = "No actors found. Set the Actor field on subtitle lines first.";
        NoWebVttVoicesFoundMessage = "No <v Name> voices found in the WebVTT file.";
        MergeContinuationLinesPromptTitle = "Merge continuation lines?";
        MergeContinuationLinesPromptMessage = "Some lines appear to be a single sentence split across multiple subtitles." + Environment.NewLine + Environment.NewLine +
                                              "Merging them before generation lets the TTS engine speak each thought as one breath group, which usually sounds more natural." + Environment.NewLine + Environment.NewLine +
                                              "Review and apply merges now?";
        SkipNoiseLinesPromptTitle = "Skip sound and music lines?";
        SkipNoiseLinesPromptMessage = "Some lines contain only sounds or music - like ♪ or [door slams]." + Environment.NewLine + Environment.NewLine +
                                      "Speech engines try to read such lines aloud and often make up words." + Environment.NewLine + Environment.NewLine +
                                      "Review these lines and leave them silent?";
        SkipNoiseLinesTitle = "Lines with only sounds or music";
        SkipNoiseLinesFoundX = "{0} lines contain only sounds or music - checked lines are left silent";
        SkipNoiseLinesColumnSkip = "Skip";
        DetectSpeakersPromptTitle = "Speaker names found in the text?";
        DetectSpeakersPromptMessage = "Some lines start with a speaker name - like \"MIKE:\" or \"[NARRATOR]\"." + Environment.NewLine + Environment.NewLine +
                                      "Moved into the actor field, each speaker can get their own voice via \"Set up cast\", and the name is not read aloud." + Environment.NewLine + Environment.NewLine +
                                      "Review the names now? (only the speech generation is affected - the subtitle itself is not changed)";
        DetectSpeakersTitle = "Speaker names in the text";
        DetectSpeakersFoundX = "{0} speaker tags found ({1} speakers) - checked names become actors and are not read aloud";
        DetectSpeakersColumnUse = "Use";
        DetectSpeakersSticky = "Lines without a name continue the previous speaker";

        SubtitleUpdatedFromReviewSingular = "Updated one line from the speech review";
        SubtitleUpdatedFromReviewPlural = "Updated {0} lines from the speech review";
        SubtitleMergedLinesAppliedSingular = "Applied one line merge from text to speech";
        SubtitleMergedLinesAppliedPlural = "Applied {0} line merges from text to speech";
    }
}