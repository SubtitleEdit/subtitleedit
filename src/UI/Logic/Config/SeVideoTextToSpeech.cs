namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideoTextToSpeech
{
    public string Engine { get; set; }
    public string Voice { get; set; }
    public bool ReviewAudioClips { get; set; }
    public bool CustomAudio { get; set; }
    public bool CustomAudioStereo { get; set; }
    public string CustomAudioEncoding { get; set; }
    public bool GenerateVideoFile { get; set; }
    public string VoiceTestText { get; set; }
    public string AllTalkUrl { get; set; }
    public string AzureApiKey { get; set; }
    public string AzureRegion { get; set; }
    public string ElevenLabsApiKey { get; set; }
    public string ElevenLabsModel { get; set; }
    public string ElevenLabsLanguage { get; set; }
    public double ElevenLabsStability { get; set; }
    public double ElevenLabsSimilarity { get; set; }
    public double ElevenLabsSpeakerBoost { get; set; }
    public double ElevenLabsSpeed { get; set; }
    public double ElevenLabsStyleeExaggeration { get; set; }
    public string MurfApiKey { get; set; }
    public string MurfStyle { get; set; }
    public string MistralApiKey { get; set; }
    public string MistralModel { get; set; }
    public string Qwen3TtsCppModel { get; set; }
    public string GoogleApiKey { get; set; }
    public string GoogleKeyFile { get; set; }

    // Pro audio post-processing
    public bool ProAudioChainEnabled { get; set; }

    // Audio ducking (mix original audio at reduced volume)
    public bool AudioDuckingEnabled { get; set; }
    public int AudioDuckingOriginalVolume { get; set; }

    // Edge-TTS prosody parameters
    public string EdgeTtsRate { get; set; }
    public string EdgeTtsPitch { get; set; }
    public string EdgeTtsVolume { get; set; }

    // Optional override for the edge-tts executable path
    public string EdgeTtsPath { get; set; }

    // VAD-based internal silence compression (shorten pauses between words before time-stretching)
    public bool VadSilenceCompressionEnabled { get; set; }
    public double VadMaxSilenceSeconds { get; set; }

    // High-quality time-stretching using rubberband (WSOLA) instead of atempo
    public bool HighQualityTimeStretchEnabled { get; set; }

    // Silence padding between segments (ms)
    public int SilencePaddingMs { get; set; }

    // Output sample rate (0 = default)
    public int OutputSampleRate { get; set; }

    public SeVideoTextToSpeech()
    {
        Engine = "Piper";
        Voice = string.Empty;
        ElevenLabsApiKey = string.Empty;
        AzureApiKey = string.Empty;
        AzureRegion = string.Empty;
        ElevenLabsModel = "eleven_turbo_v2_5";
        ElevenLabsLanguage = string.Empty;
        ElevenLabsStability = 0.5;
        ElevenLabsSimilarity = 0.5;
        ElevenLabsSpeakerBoost = 0;
        ElevenLabsStyleeExaggeration = 0;
        ElevenLabsSpeed = 1.0;
        ReviewAudioClips = true;
        CustomAudio = false;
        CustomAudioStereo = true;
        CustomAudioEncoding = string.Empty;
        GenerateVideoFile = true;
        VoiceTestText = "Hello, how are you doing?";
        AllTalkUrl = "http://127.0.0.1:7851";
        MurfApiKey = string.Empty;
        MurfStyle = "Conversational";
        MistralApiKey = string.Empty;
        MistralModel = "voxtral-mini-tts-2603";
        Qwen3TtsCppModel = "0.6B";
        GoogleApiKey = string.Empty;
        GoogleKeyFile = string.Empty;
        ProAudioChainEnabled = false;
        AudioDuckingEnabled = false;
        AudioDuckingOriginalVolume = 15;
        EdgeTtsRate = string.Empty;
        EdgeTtsPitch = string.Empty;
        EdgeTtsVolume = string.Empty;
        EdgeTtsPath = string.Empty;
        VadSilenceCompressionEnabled = false;
        VadMaxSilenceSeconds = 0.15;
        HighQualityTimeStretchEnabled = false;
        SilencePaddingMs = 0;
        OutputSampleRate = 0;
    }
}