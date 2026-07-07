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

    public string CommandLineParameterCpp { get; set; } = string.Empty;
    public string CommandLineParameterCppCuBlas { get; set; } = string.Empty;
    public string CommandLineParameterCppVulkan { get; set; } = string.Empty;
    public string CommandLineParameterConstMe { get; set; } = string.Empty;
    public string CommandLineParameterCTranslate2 { get; set; } = "--vad_filter True";
    public string CommandLineParameterMlxWhisperMac { get; set; } = string.Empty;
    public string CommandLineParameterFasterWhisperMac { get; set; } = string.Empty;
    public string CommandLineParameterPurfviewFasterWhisperXxl { get; set; } = "--standard";
    public string CommandLineParameterOpenAi { get; set; } = string.Empty;
    public string CommandLineParameterQwen3AsrCpp { get; set; } = string.Empty;
    public string CommandLineParameterParakeetCpp { get; set; } = string.Empty;
    public string CommandLineParameterChatLlm { get; set; } = string.Empty;
    public string CommandLineParameterCrispAsrCanary { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrCohere { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrFireRed { get; set; } = "--max-len 50 --split-on-punct";
    // Fun-ASR targets CJK/Korean where ~50 chars is a very long subtitle line, so default to a
    // CJK-friendlier max line length (each char is roughly a whole word/syllable).
    public string CommandLineParameterCrispAsrFunAsrNano { get; set; } = "--max-len 20 --split-on-punct";
    public string CommandLineParameterCrispAsrFunAsrMltNano { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrGlm { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrGranite { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrParakeet { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrQwen3 { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrOmni { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrKyutai { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrMega { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrSenseVoice { get; set; } = "--max-len 50 --split-on-punct";
    public string CommandLineParameterCrispAsrArk { get; set; } = "--max-len 50 --split-on-punct";
    public string CrispAsrForcedAligner { get; set; } = "built-in";

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