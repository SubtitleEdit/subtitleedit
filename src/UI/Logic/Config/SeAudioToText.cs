using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAudioToText
{
    public bool PostProcessing { get; set; } = true;

    public string WhisperChoice { get; set; } = "WhisperCPP"; //TODO: WhisperEngineCpp.StaticName;

    public bool WhisperIgnoreVersion { get; set; } = false;

    public bool WhisperDeleteTempFiles { get; set; } = true;

    public string? WhisperModel { get; set; } = string.Empty;

    public string WhisperLanguageCode { get; set; } = string.Empty;

    public string WhisperLocation { get; set; } = string.Empty;

    public string WhisperCtranslate2Location { get; set; } = string.Empty;

    public string WhisperPurfviewFasterWhisperLocation { get; set; } = string.Empty;

    public string WhisperPurfviewFasterWhisperDefaultCmd { get; set; } = string.Empty;

    public string WhisperXLocation { get; set; } = string.Empty;

    public string WhisperStableTsLocation { get; set; } = string.Empty;

    public string WhisperCppModelLocation { get; set; } = string.Empty;

    public string WhisperCustomCommandLineArguments { get; set; } = string.Empty;
    public bool WhisperCustomCommandLineArgumentsPurfviewBlank { get; set; }

    public string WhisperExtraSettingsHistory { get; set; } = string.Empty;

    public bool WhisperAutoAdjustTimings { get; set; } = true;

    public bool WhisperUseLineMaxChars { get; set; } = true;

    public bool WhisperPostProcessingAddPeriods { get; set; } = false;

    public bool WhisperPostProcessingMergeLines { get; set; } = true;

    public bool WhisperPostProcessingSplitLines { get; set; } = true;

    public bool WhisperPostProcessingFixCasing { get; set; } = false;

    public bool WhisperPostProcessingFixShortDuration { get; set; } = true;
    public bool WhisperPostProcessingChangeUnderlineToColor { get; set; }
    public string WhisperPostProcessingChangeUnderlineToColorColor { get; set; } = Colors.Red.FromColorToHex();
    public string WhisperCppVulkanGpuDevice { get; set; } = string.Empty;
}