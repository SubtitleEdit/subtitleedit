namespace Nikse.SubtitleEdit.UiLogic.Grammar;

/// <summary>
/// A language as listed by LanguageTool's /v2/languages: "English (US)", code "en", long code "en-US".
/// </summary>
public class LanguageToolLanguage
{
    /// <summary>The value to send as the "language" parameter for automatic detection.</summary>
    public const string AutoCode = "auto";

    public string Name { get; init; } = string.Empty;

    /// <summary>Two/three letter code without the variant, e.g. "en".</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Code including the variant, e.g. "en-US" - this is what gets sent to the server.</summary>
    public string LongCode { get; init; } = string.Empty;

    public bool IsAuto => LongCode == AutoCode;

    public override string ToString()
    {
        return Name;
    }
}
