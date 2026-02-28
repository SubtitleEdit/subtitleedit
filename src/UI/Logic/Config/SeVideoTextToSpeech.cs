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
    public string GoogleApiKey { get; set; }
    public string GoogleKeyFile { get; set; }

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
        GoogleApiKey = string.Empty;
        GoogleKeyFile = string.Empty;
    }
}