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
    }
}