namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Settings for the "llama.cpp (advanced)" auto-translate engine: batch/context options plus
/// sampling overrides. Sampling values of -1 mean "not set" and fall back to the curated
/// model's recommended value, then to the server default.
/// </summary>
public class SeLlamaCppAdvanced
{
    public int BatchSize { get; set; } = 10;
    public int HistoryPairs { get; set; } = 12;
    public string Synopsis { get; set; } = string.Empty;
    public string Glossary { get; set; } = string.Empty;
    public string Formality { get; set; } = FormalityDefault;
    public bool KeepLineBreaks { get; set; } = true;
    public string Prompt { get; set; } = string.Empty;
    public double Temperature { get; set; } = -1;
    public double TopP { get; set; } = -1;
    public int TopK { get; set; } = -1;
    public double RepeatPenalty { get; set; } = -1;
    public int MaxTokens { get; set; } = -1;
    public int ContextSize { get; set; } = 16384;

    public const string FormalityDefault = "Default";
    public const string FormalityFormal = "Formal";
    public const string FormalityInformal = "Informal";
}
