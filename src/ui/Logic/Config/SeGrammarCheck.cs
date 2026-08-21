using Nikse.SubtitleEdit.UiLogic.Grammar;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeGrammarCheck
{
    /// <summary>Base url of the LanguageTool server - the public API or a self-hosted one.</summary>
    public string ServerUrl { get; set; }

    /// <summary>A long code like "en-US", or "auto" for server side detection.</summary>
    public string Language { get; set; }

    public bool Picky { get; set; }

    /// <summary>Comma separated rule ids to switch off, e.g. "WHITESPACE_RULE".</summary>
    public string DisabledRules { get; set; }

    public string Username { get; set; }
    public string ApiKey { get; set; }

    /// <summary>Lines per request - a whole subtitle in one call would give no progress and no way to stop.</summary>
    public int MaxLinesPerBatch { get; set; }

    public SeGrammarCheck()
    {
        ServerUrl = LanguageToolClient.DefaultServerUrl;
        Language = LanguageToolLanguage.AutoCode;
        Picky = false;
        DisabledRules = string.Empty;
        Username = string.Empty;
        ApiKey = string.Empty;
        MaxLinesPerBatch = 25;
    }
}
