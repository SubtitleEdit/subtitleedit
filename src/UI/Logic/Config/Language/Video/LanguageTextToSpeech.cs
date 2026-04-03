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
    }
}