namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>Video &gt; More &gt; Generate background music.</summary>
public class SeVideoBackgroundMusic
{
    /// <summary>Key of the selected preset (<c>BackgroundMusicPreset.Key</c>), or "custom".</summary>
    public string Preset { get; set; }

    /// <summary>The prompt as last edited — kept even when it no longer matches the preset.</summary>
    public string Prompt { get; set; }

    public int Bpm { get; set; }

    /// <summary>How much music to generate; longer videos loop it.</summary>
    public int GenerateSeconds { get; set; }

    public bool UseRandomSeed { get; set; }
    public long Seed { get; set; }

    public int MusicVolumePercent { get; set; }
    public bool RemoveExistingAudioTracks { get; set; }
    public int OriginalAudioVolumePercent { get; set; }

    /// <summary>TTS window: mix generated music under the dubbed speech.</summary>
    public bool AddToTextToSpeech { get; set; }

    /// <summary>TTS window: music level between lines (it ducks further while someone talks).</summary>
    public int TextToSpeechMusicVolumePercent { get; set; }

    public SeVideoBackgroundMusic()
    {
        Preset = "cooking";
        Prompt = string.Empty;
        Bpm = 120;
        GenerateSeconds = 60;
        UseRandomSeed = true;
        Seed = 42;
        MusicVolumePercent = 100;
        RemoveExistingAudioTracks = true;
        OriginalAudioVolumePercent = 100;
        AddToTextToSpeech = false;
        TextToSpeechMusicVolumePercent = 30;
    }
}
