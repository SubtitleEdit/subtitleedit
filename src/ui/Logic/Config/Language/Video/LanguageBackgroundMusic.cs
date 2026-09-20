namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBackgroundMusic
{
    public string Title { get; set; }
    public string GenerateBackgroundMusicDotDotDot { get; set; }
    public string Preset { get; set; }
    public string Prompt { get; set; }
    public string TempoBpm { get; set; }
    public string GenerateSeconds { get; set; }
    public string Seed { get; set; }
    public string RandomSeed { get; set; }
    public string OutputLength { get; set; }
    public string OutputLengthVideoX { get; set; }
    public string MusicVolumePercent { get; set; }
    public string RemoveExistingAudioTracks { get; set; }
    public string OriginalAudioVolumePercent { get; set; }
    public string SaveAudioDotDotDot { get; set; }
    public string AddToVideoDotDotDot { get; set; }
    public string EngineInfo { get; set; }
    public string PresetCooking { get; set; }
    public string PresetTech { get; set; }
    public string PresetForest { get; set; }
    public string PresetRelaxingPiano { get; set; }
    public string PresetLofiVlog { get; set; }
    public string PresetCorporate { get; set; }
    public string LoadingModel { get; set; }
    public string Composing { get; set; }
    public string GeneratingAudio { get; set; }
    public string DecodingAudio { get; set; }
    public string CreatingLoop { get; set; }
    public string ResultLoopXYZ { get; set; }
    public string ResultNoLoopX { get; set; }
    public string AddingMusicToVideo { get; set; }
    public string DownloadModelTitle { get; set; }
    public string DownloadModelQuestionX { get; set; }
    public string DownloadingModel { get; set; }
    public string SaveAudioTitle { get; set; }
    public string AudioFileSavedX { get; set; }
    public string UnableToGenerateMusic { get; set; }
    public string UnableToAddMusicToVideo { get; set; }
    public string OutputLengthTextToSpeech { get; set; }
    public string AddBackgroundMusic { get; set; }
    public string AddBackgroundMusicHint { get; set; }
    public string GeneratingBackgroundMusicDotDotDot { get; set; }
    public string MixingBackgroundMusicDotDotDot { get; set; }

    public LanguageBackgroundMusic()
    {
        Title = "Generate background music";
        GenerateBackgroundMusicDotDotDot = "Generate background music...";
        Preset = "Preset";
        Prompt = "Prompt";
        TempoBpm = "Tempo (BPM)";
        GenerateSeconds = "Generate (seconds)";
        Seed = "Seed";
        RandomSeed = "Random";
        OutputLength = "Output length";
        OutputLengthVideoX = "{0} (video length - the music loops seamlessly)";
        MusicVolumePercent = "Music volume (%)";
        RemoveExistingAudioTracks = "Remove existing audio tracks";
        OriginalAudioVolumePercent = "Original audio volume (%)";
        SaveAudioDotDotDot = "Save audio...";
        AddToVideoDotDotDot = "Add to video...";
        EngineInfo = "ACE-Step 1.5 Turbo via audio.cpp (MIT license) - runs locally";
        PresetCooking = "Cooking / baking";
        PresetTech = "Tech / gadget review";
        PresetForest = "Forest / nature";
        PresetRelaxingPiano = "Relaxing piano";
        PresetLofiVlog = "Lo-fi vlog";
        PresetCorporate = "Corporate / explainer";
        LoadingModel = "Loading model...";
        Composing = "Composing...";
        GeneratingAudio = "Generating audio...";
        DecodingAudio = "Decoding audio...";
        CreatingLoop = "Creating seamless loop...";
        ResultLoopXYZ = "Loop: {0} at {1} BPM, seam match {2}%";
        ResultNoLoopX = "Music ready: {0}";
        AddingMusicToVideo = "Adding music to video...";
        DownloadModelTitle = "Download music model?";
        DownloadModelQuestionX = "Generating music requires the ACE-Step 1.5 model ({0}).\n\nDownload model?";
        DownloadingModel = "Downloading ACE-Step 1.5 model...";
        SaveAudioTitle = "Save background music";
        AudioFileSavedX = "Audio file saved: {0}";
        UnableToGenerateMusic = "Unable to generate music";
        UnableToAddMusicToVideo = "Unable to add music to video";
        OutputLengthTextToSpeech = "Follows the generated speech - the music loops seamlessly and dips while someone talks";
        AddBackgroundMusic = "Add background music";
        AddBackgroundMusicHint = "Generate music locally with ACE-Step 1.5 and mix it under the speech - it dips while someone talks";
        GeneratingBackgroundMusicDotDotDot = "Generating background music...";
        MixingBackgroundMusicDotDotDot = "Mixing background music...";
    }
}
