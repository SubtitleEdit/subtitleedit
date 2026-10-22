namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageTranslate
{
    public string ManageProfiles { get; set; } = "Manage profiles";
    public string SaveProfiles { get; set; } = "Save";
    public string SelectProfileToEdit { get; set; } = "Select a profile to edit, or create a new one.";
    public string ProfilesSelected { get; set; } = "{0} profiles selected. Delete removes all selected profiles.";
    public string TranslationServices { get; set; } = "Translation services";
    public string LanguageModels { get; set; } = "Language models";
    public string LocalApi { get; set; } = "Local API";
    public string CloudApi { get; set; } = "Cloud API";
    public string Local { get; set; } = "Local";
    public string Cloud { get; set; } = "Cloud";
    public string Location { get; set; } = "Location";
    public string ChooseInferenceEngine { get; set; } = "Choose Inference Engine";
    public string ChooseProvider { get; set; } = "Choose Provider";
    public string ApiProtocol { get; set; } = "API protocol";
    public string TranslationPolicy { get; set; } = "Translation policy";
    public string UniqueProfileNameRequired { get; set; } = "Enter a unique, non-empty profile name.";
    public string ValidApiUrlRequired { get; set; } = "Enter a valid HTTP or HTTPS API URL.";
    public string LoadingModels { get; set; } = "Loading models...";
    public string NoModelsReturned { get; set; } = "No models were returned. You can enter the model name manually.";
    public string ModelNameRequired { get; set; } = "Enter a model name.";
    public string ReviewMigratedConnection { get; set; } = "Review the migrated API URL and select a chat-compatible model before saving.";
    public string ChatCompatibleModelRequired { get; set; } = "This completion-only model cannot use the chat API. Choose a chat-compatible model.";

    public string TranslateViaCopyPaste { get; set; }
    public string MaxBlockSize { get; set; }
    public string LineSeparator { get; set; }
    public string BlockCopyInfo { get; set; }
    public string BlockCopyGetFromClipboard { get; set; }
    public string ReCopyTextToClipboard { get; set; }
    public string BlockXOfY { get; set; }
    public string TextInClipboardIsSameAsSourceText { get; set; }
    public string LineMerge { get; set; }
    public string DelayInSecondsBetweenRequests { get; set; }
    public string MaxBytesPerRequest { get; set; }
    public string KeepMusicLinesUntranslated { get; set; }
    public string PromptText { get; set; }
    public string ResetPromptToDefault { get; set; }
    public string TranslateEachLineSeparately { get; set; }
    public string TranslationError { get; set; }
    public string TranslationFailedMessage { get; set; }
    public string TranslationFailedHint { get; set; }
    public string ShowTechnicalDetails { get; set; }
    public string HideTechnicalDetails { get; set; }
    public string TechnicalDetails { get; set; }
    public string PleaseSelectATargetLanguage { get; set; }
    public string ReadyToTranslate { get; set; }
    public string Translating { get; set; }
    public string TranslationComplete { get; set; }
    public string TranslateInPlaceNoOriginal { get; set; }
    public string TranslationCancelled { get; set; }
    public string SwapLanguages { get; set; }
    public string XIsAlreadyDownloadedReDownload { get; set; }
    public string AdvancedSettingsSubtitle { get; set; }
    public string Context { get; set; }
    public string Synopsis { get; set; }
    public string SynopsisHint { get; set; }
    public string Glossary { get; set; }
    public string GlossaryHint { get; set; }
    public string PreviousLinesAsContext { get; set; }
    public string LinesPerBatch { get; set; }
    public string KeepLineBreaks { get; set; }
    public string SamplingParameters { get; set; }
    public string MinusOneMeansDefault { get; set; }
    public string TopP { get; set; }
    public string TopK { get; set; }
    public string RepeatPenalty { get; set; }
    public string MaxTokensPerReply { get; set; }
    public string ServerContextSizeTokens { get; set; }
    public string ExtraServerParameters { get; set; }
    public string ExtraServerParametersHint { get; set; }
    public string UseOnlyExtraServerParameters { get; set; }
    public string UseOnlyExtraServerParametersHint { get; set; }
    public string ServerRunningAtX { get; set; }
    public string CustomPromptHint { get; set; }
    public string EditTranslationHint { get; set; }
    public string LlamaCppDownloadEngineAndModelPrompt { get; set; }
    public string LlamaCppDownloadModelPrompt { get; set; }

    public LanguageTranslate()
    {
        TranslateViaCopyPaste = "Auto-translate via copy/paste";
        MaxBlockSize = "Max block size";
        LineSeparator = "Line separator";
        BlockCopyInfo = "Go to translator/AI and paste text (already in clipboard), copy result back to clipboard and click the button below.";
        BlockCopyGetFromClipboard = "Get text from clipboard (from translator/AI)";
        ReCopyTextToClipboard = "Re-copy text to clipboard (for translator/AI)";
        BlockXOfY = "Block {0} of {1}";
        TextInClipboardIsSameAsSourceText = "The text in clipboard is the same as the source text, please try again.";
        LineMerge = "Line merge";
        DelayInSecondsBetweenRequests = "Delay in seconds between requests";
        MaxBytesPerRequest = "Max bytes per request";
        KeepMusicLinesUntranslated = "Do not translate lines in music symbols (♪ ... ♪)";
        PromptText = "Prompt text";
        ResetPromptToDefault = "Reset prompt to default";
        TranslateEachLineSeparately = "Translate each line separately";
        TranslationError = "Translation error";
        TranslationFailedMessage = "{0} translation failed.";
        TranslationFailedHint = "This may be due to a network issue, an invalid API key, or API usage limits.\nPlease check your connection and API settings, then try again.";
        ShowTechnicalDetails = "Show technical details";
        HideTechnicalDetails = "Hide technical details";
        TechnicalDetails = "Technical details";
        PleaseSelectATargetLanguage = "Please select a target language";
        ReadyToTranslate = "Ready to translate";
        Translating = "Translating...";
        TranslationComplete = "Translation complete";
        TranslateInPlaceNoOriginal = "Translate in place (do not create original)";
        TranslationCancelled = "Translation cancelled";
        SwapLanguages = "Swap source and target languages";
        XIsAlreadyDownloadedReDownload = "{0} is already downloaded. Re-download?";
        AdvancedSettingsSubtitle = "Batch, context and sampling options for the advanced llama.cpp engine";
        Context = "Context";
        Synopsis = "Synopsis";
        SynopsisHint = "Optional description of the content (genre, setting, tone) - included in every request";
        Glossary = "Glossary";
        GlossaryHint = "One per line: source term = translation";
        PreviousLinesAsContext = "Previous lines as context";
        LinesPerBatch = "Lines per batch";
        KeepLineBreaks = "Keep line breaks";
        SamplingParameters = "Sampling parameters";
        MinusOneMeansDefault = "-1 = model/server default";
        TopP = "Top-p";
        TopK = "Top-k";
        RepeatPenalty = "Repeat penalty";
        MaxTokensPerReply = "Max tokens per reply";
        ServerContextSizeTokens = "Server context size (tokens)";
        ExtraServerParameters = "Extra server parameters";
        ExtraServerParametersHint = "Additional llama-server command-line arguments, e.g. \"-ngl 30 --no-mmap\" - applied when the local server starts";
        UseOnlyExtraServerParameters = "Use only these parameters";
        UseOnlyExtraServerParametersHint = "Start llama-server with the parameters above instead of Subtitle Edit's own tuning - only the model, host and port are still set";
        ServerRunningAtX = "Server running at {0}";
        CustomPromptHint = "Custom instructions ({0} = source language, {1} = target language); empty = built-in prompt";
        EditTranslationHint = "Click the selected row's translation to edit it (Enter saves, Escape cancels)";
        LlamaCppDownloadEngineAndModelPrompt = "llama.cpp requires the llama-server engine and a translation model to be downloaded. Download now?";
        LlamaCppDownloadModelPrompt = "llama.cpp requires the selected translation model to be downloaded. Download now?";
    }
}