namespace Nikse.SubtitleEdit.Logic.Config.Language.Waveform;

public class LanguageWaveform
{
    public string GuessTimeCodes { get; set; }
    public string GuessTimeCodesScanBlockSize { get; set; }
    public string GuessTimeCodesScanBlockAverageMin { get; set; }
    public string GuessTimeCodesScanBlockAverageMax { get; set; }
    public string GuessTimeCodesSplitLongSubtitlesAt { get; set; }
    public object SpeechToTextSelectedLinesDotDotDot { get; internal set; }
    public string SeekSilence { get; set; }
    public string MinSilenceDurationSeconds { get; set; }
    public string MaxSilenceVolume { get; set; }
    public string GuessTimeCodesDotDotDot { get; set; }
    public string SeekSilenceDotDotDot { get; set; }
    public string ToggleShotChange { get; set; }
    public string ResetWaveformZoomAndSpeed { get; set; }
    public object ShowOnlyWaveform { get; set; }
    public object ShowOnlySpectrogram { get; set; }
    public object ShowWaveformAndSpectrogram { get; set; }
    public string SpectrogramClassic { get; set; }
    public string SpectrogramClassicViridis { get; set; }
    public string SpectrogramClassicPlasma { get; set; }
    public string SpectrogramClassicInferno { get; set; }
    public string SpectrogramClassicTurbo { get; set; }
    public string WaveformDrawStyleClassic { get; set; }
    public string WaveformDrawStyleFancy { get; set; }
    public string SetVideoPositionAndPauseAndSelectSubtitle { get; set; }
    public string SetVideopositionAndPauseAndSelectSubtitleAndCenter { get; set; }
    public string SetVideoPositionAndPause { get; set; }
    public string SetVideopositionAndPauseAndCenter { get; set; }
    public string SetVideoposition { get; set; }

    public LanguageWaveform()
    {
        GuessTimeCodes = "Guess time codes";
        GuessTimeCodesDotDotDot = "Guess time codes...";
        GuessTimeCodesScanBlockSize = "Scan block size (ms):";
        GuessTimeCodesScanBlockAverageMin = "Scan block average minimum (% of max):";
        GuessTimeCodesScanBlockAverageMax = "Scan block average maximum (% of max):";
        GuessTimeCodesSplitLongSubtitlesAt = "Split long subtitles at (ms):";

        SpeechToTextSelectedLinesDotDotDot = "Speech to text selected lines...";
        SeekSilence = "Seek silence";
        SeekSilenceDotDotDot = "Seek silence...";
        MinSilenceDurationSeconds = "Min. silence duration (seconds):";
        MaxSilenceVolume = "Max. silence volume (0.0 - 1.0):";
        ToggleShotChange = "Toggle shot change";
        ResetWaveformZoomAndSpeed = "Reset waveform zoom & speed";
        ShowOnlyWaveform = "Show only waveform";
        ShowOnlySpectrogram = "Show only spectrogram";
        ShowWaveformAndSpectrogram = "Show waveform and spectrogram";
        SpectrogramClassic = "Classic";
        SpectrogramClassicViridis = "Viridis";
        SpectrogramClassicPlasma = "Plasma";
        SpectrogramClassicInferno = "Inferno";
        SpectrogramClassicTurbo = "Turbo";
        WaveformDrawStyleClassic = "Classic";
        WaveformDrawStyleFancy = "Fancy";

        SetVideoPositionAndPauseAndSelectSubtitle = "Set video position, pause, and select subtitle";
        SetVideopositionAndPauseAndSelectSubtitleAndCenter = "Set video position, pause, select subtitle, and center";
        SetVideoPositionAndPause = "Set video position and pause";
        SetVideopositionAndPauseAndCenter = "Set video position, pause, and center";
        SetVideoposition = "Set video position";
    }
}