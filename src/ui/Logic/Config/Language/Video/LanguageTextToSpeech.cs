using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageTextToSpeech
{
    public string Title { get; set; }
    public string TextToSpeechEngine { get; set; }
    public string ReviewAudioSegments { get; set; }
    public string ReviewAudioSegmentsHistory { get; set; }
    public string Stability { get; set; }
    public string Similarity { get; set; }
    public string SpeakerBoost { get; set; }
    public string StyleExaggeration { get; set; }
    public string RegenerateAudioSelectedLine { get; set; }
    public string GenerateSpeechFromText { get; set; }
    public string TestVoice { get; set; }
    public string AddAudioToVideoFile { get; set; }
    public string VoiceSettings { get; set; }
    public string VoiceSampleText { get; set; }
    public string RefreshVoices { get; set; }
    public string VideoEncodingSettings { get; set; }
    public string ElevenLabsSettings { get; set; }
    public string ElevenLabsSettingsResetHint { get; set; }
    public string RegenerateAudio { get; set; }
    public string AutoContinuePlaying { get; set; }
    public string AddingAudioToVideoFileDotDotDot { get; set; }
    public string PreparingMergeDotDotDot { get; set; }
    public string ImportVoiceDotDotDot { get; set; }
    public string VoiceImportSuccessTitle { get; set; }
    public string VoiceXImported { get; set; }
    public string DropAudioFileHereToImportVoice { get; set; }
    public string DropAudioFileHereHint { get; set; }
    public string VoiceCloneTranscriptTitle { get; set; }
    public string UseSpeechToTextDotDotDot { get; set; }
    public string AdvancedTtsSettings { get; set; }
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
    public string SilencePaddingMs { get; set; }
    public string SilencePaddingMsDescription { get; set; }
    public string OutputSampleRate { get; set; }
    public string OutputSampleRateDescription { get; set; }
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

    public LanguageTextToSpeech()
    {
        Title = "Text to speech";
        TextToSpeechEngine = "Text to speech engine";
        ReviewAudioSegments = "TTS - Review audio segments";
        ReviewAudioSegmentsHistory = "TTS - Review audio history";
        Stability = "Stability";
        Similarity = "Similarity";
        SpeakerBoost = "Speaker boost";
        StyleExaggeration = "Style exaggeration";
        RegenerateAudioSelectedLine = "Regenerate audio for selected line";
        GenerateSpeechFromText = "Generate speech from text";
        TestVoice = "Test voice";
        AddAudioToVideoFile = "Add audio to video file";
        VoiceSettings = "TTS - Voice settings";
        VoiceSampleText = "Voice sample text";
        RefreshVoices = "Refresh voices";
        VideoEncodingSettings = "TTS - Video encoding settings";
        ElevenLabsSettings = "TTS - ElevenLabs settings";
        ElevenLabsSettingsResetHint = "Reset ElevenLabs settings to default values";
        RegenerateAudio = "Regenerate audio";
        AutoContinuePlaying = "Auto-continue playing";
        AddingAudioToVideoFileDotDotDot = "Adding audio to video file...";
        PreparingMergeDotDotDot = "Preparing merge...";
        ImportVoiceDotDotDot = "Import voice...";
        VoiceImportSuccessTitle = "Voice imported";
        VoiceXImported = "Voice '{0}' imported successfully";
        DropAudioFileHereToImportVoice = "Drop audio file here to import voice";
        DropAudioFileHereHint = ".wav or .mp3";
        VoiceCloneTranscriptTitle = "Enter transcript of the audio (required for voice cloning)";
        UseSpeechToTextDotDotDot = "Use speech-to-text...";
        AdvancedTtsSettings = "Advanced TTS settings";
        ProAudioPostProcessing = "Pro audio post-processing";
        ProAudioPostProcessingDescription = "Applies EQ warmth, noise gate, compression, loudness normalization (-16 LUFS), and fade in/out to each segment.";
        AudioDucking = "Audio ducking";
        AudioDuckingDescription = "Reduces the original video audio volume and mixes it with the TTS audio, so the original soundtrack is still faintly audible.";
        OriginalVolumePercent = "Original volume %";
        VadSilenceCompression = "VAD silence compression";
        VadSilenceCompressionDescription = "Shortens pauses between words before changing tempo. Uses Voice Activity Detection to compress only silence gaps while keeping speech untouched. This is the preferred first step — it reduces duration without any quality loss.";
        MaxSilenceMs = "Max silence (ms)";
        HighQualityTimeStretch = "High-quality time-stretch (WSOLA/rubberband)";
        HighQualityTimeStretchDescription = "Uses the rubberband algorithm (WSOLA) instead of the default atempo filter for pitch-preserving speed changes. Produces more natural-sounding speech, especially at higher speed factors. Requires librubberband in your FFmpeg build — falls back to atempo automatically if unavailable.";
        SilencePaddingMs = "Silence padding (ms)";
        SilencePaddingMsDescription = "Adds a short silence at the end of each segment. Useful for breathing room between sentences.";
        OutputSampleRate = "Output sample rate (0 = default)";
        OutputSampleRateDescription = "Resamples all segments to the specified sample rate (e.g. 44100, 48000). Set to 0 to keep the original rate.";
        EdgeTtsRate = "Edge-TTS rate";
        EdgeTtsRateDescription = "Speech rate for Edge-TTS, e.g. \"+50%\", \"-30%\", or \"+0%\" for default.";
        EdgeTtsPitch = "Edge-TTS pitch";
        EdgeTtsPitchDescription = "Pitch adjustment for Edge-TTS, e.g. \"+10Hz\", \"-5Hz\", or \"+0Hz\" for default.";
        EdgeTtsVolume = "Edge-TTS volume";
        EdgeTtsVolumeDescription = "Volume adjustment for Edge-TTS, e.g. \"+20%\", \"-10%\", or \"+0%\" for default.";
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
    }
}