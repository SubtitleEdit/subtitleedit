using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class ToolsSettings
    {
        public bool FixShortDisplayTimesAllowMoveStartTime { get; set; }
        public bool RemoveEmptyLinesBetweenText { get; set; }
        public string MusicSymbol { get; set; }
        public string MusicSymbolReplace { get; set; }
        public bool RememberUseAlwaysList { get; set; }
        public bool OcrFixUseHardcodedRules { get; set; }
        public bool OcrGoogleCloudVisionSeHandlesTextMerge { get; set; }
        public bool OcrUseWordSplitList { get; set; }
        public string MicrosoftTranslatorApiKey { get; set; }
        public string MicrosoftTranslatorTokenEndpoint { get; set; }
        public string MicrosoftTranslatorCategory { get; set; }
        public string GoogleApiV2Key { get; set; }
        public string AutoTranslateNllbApiUrl { get; set; }
        public string AutoTranslateNllbServeUrl { get; set; }
        public string AutoTranslateLibreUrl { get; set; }
        public string AutoTranslateLibreApiKey { get; set; }
        public string AutoTranslateMyMemoryApiKey { get; set; }
        public string AutoTranslateSeamlessM4TUrl { get; set; }
        public string AutoTranslateCrispAsrExe { get; set; }
        public string AutoTranslateCrispAsrModel { get; set; }
        public string AutoTranslateDeepLApiKey { get; set; }
        public string AutoTranslateDeepLUrl { get; set; }
        public string AutoTranslateDeepLFormality { get; set; }
        public string AutoTranslateDeepLXUrl { get; set; }
        public string AutoTranslatePapagoApiKeyId { get; set; }
        public string AutoTranslatePapagoApiKey { get; set; }
        public string AutoTranslateMistralApiKey { get; set; }
        public string AutoTranslateMistralUrl { get; set; }
        public string AutoTranslateMistralModel { get; set; }
        public string AutoTranslateMistralPrompt { get; set; }
        public string ChatGptUrl { get; set; }
        public string ChatGptPrompt { get; set; }
        public string ChatGptApiKey { get; set; }
        public string ChatGptModel { get; set; }
        public string OpenAiCompatibleTranslateUrl { get; set; }
        public string OpenAiCompatibleTranslatePrompt { get; set; }
        public string OpenAiCompatibleTranslateApiKey { get; set; }
        public string OpenAiCompatibleTranslateModel { get; set; }
        public string GroqUrl { get; set; }
        public string GroqPrompt { get; set; }
        public string GroqApiKey { get; set; }
        public string GroqModel { get; set; }
        public string DeepSeekUrl { get; set; }
        public string DeepSeekPrompt { get; set; }
        public string DeepSeekApiKey { get; set; }
        public string DeepSeekModel { get; set; }
        public string NvidiaUrl { get; set; }
        public string NvidiaPrompt { get; set; }
        public string NvidiaApiKey { get; set; }
        public string NvidiaModel { get; set; }
        public string AvalAiUrl { get; set; }
        public string AvalAiPrompt { get; set; }
        public string AvalAiApiKey { get; set; }
        public string AvalAiModel { get; set; }
        
        public string PerplexityUrl { get; set; }
        public string PerplexityPrompt { get; set; }
        public string PerplexityApiKey { get; set; }
        public string PerplexityModel { get; set; }

        public string LaraUrl { get; set; }
        public string LaraApiId { get; set; }
        public string LaraApiSecret { get; set; }




        public string OpenRouterUrl { get; set; }
        public string OpenRouterPrompt { get; set; }
        public string OpenRouterApiKey { get; set; }
        public string OpenRouterModel { get; set; }
        public string LmStudioApiUrl { get; set; }
        public string LmStudioModel { get; set; }
        public string LmStudioPrompt { get; set; }
        public string LlamaCppApiUrl { get; set; }
        public string LlamaCppPrompt { get; set; }
        public string LlamaCppModelPrompt { get; set; }
        public double LlamaCppModelTemperature { get; set; }
        public double LlamaCppModelTopP { get; set; }
        public int LlamaCppModelTopK { get; set; }
        public double LlamaCppModelRepeatPenalty { get; set; }

        public string OllamaApiUrl { get; set; }
        public string OllamaModel { get; set; }
        public string OllamaPrompt { get; set; }
        public string KoboldCppUrl { get; set; }
        public string KoboldCppPrompt { get; set; }
        public decimal KoboldCppTemperature { get; set; }
        public string AnthropicApiUrl { get; set; }
        public string AnthropicPrompt { get; set; }
        public string AnthropicApiKey { get; set; }
        public string AnthropicApiModel { get; set; }
        public string BaiduUrl { get; set; }
        public string BaiduApiKey { get; set; }
        public int AutoTranslateDelaySeconds { get; set; }
        public int AutoTranslateMaxBytes { get; set; }
        public string GeminiProApiKey { get; set; }
        public string GeminiModel { get; set; }
        public string GeminiPrompt { get; set; }
        public bool ExportBluRayRemoveSmallGaps { get; set; }
        public bool FixCommonErrorsFixOverlapAllowEqualEndStart { get; set; }
        public string MusicSymbolStyle { get; set; }
        public bool UseNoLineBreakAfter { get; set; }
        public bool AutoBreakCommaBreakEarly { get; set; }
        public bool AutoBreakDashEarly { get; set; }
        public bool AutoBreakLineEndingEarly { get; set; }
        public bool AutoBreakUsePixelWidth { get; set; }
        public bool AutoBreakPreferBottomHeavy { get; set; }
        public double AutoBreakPreferBottomPercent { get; set; }
        public int MergeShortLinesMaxGap { get; set; }
        public bool MergeShortLinesOnlyContinuous { get; set; }

        public string WhisperChoice { get; set; }

        public string WhisperLocation { get; set; }
        public string WhisperCtranslate2Location { get; set; }
        public string WhisperXLocation { get; set; }
        public string WhisperStableTsLocation { get; set; }
        public string WhisperCppModelLocation { get; set; }

        public ToolsSettings()
        {
            FixShortDisplayTimesAllowMoveStartTime = false;
            RemoveEmptyLinesBetweenText = true;
            MusicSymbol = "♪";
            MusicSymbolReplace = "â™ª,â™«," + // ♪ + ♫ in UTF-8 opened as ANSI
                                 "<s M/>,<s m/>," + // music symbols by subtitle creator
                                 "#,*,¶"; // common music symbols
            OcrFixUseHardcodedRules = true;
            OcrGoogleCloudVisionSeHandlesTextMerge = true;
            OcrUseWordSplitList = true;
            MicrosoftTranslatorTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            AutoTranslateNllbServeUrl = "http://127.0.0.1:6060/";
            AutoTranslateNllbApiUrl = "http://localhost:7860/api/v4/";
            AutoTranslateLibreUrl = "http://localhost:5000/";
            AutoTranslateSeamlessM4TUrl = "http://localhost:5000/";
            AutoTranslateCrispAsrExe = string.Empty;
            AutoTranslateCrispAsrModel = string.Empty;
            AutoTranslateDeepLUrl = "https://api-free.deepl.com/";
            AutoTranslateDeepLXUrl = "http://localhost:1188";
            AutoTranslateMistralUrl = "https://api.mistral.ai/v1/chat/completions";
            AutoTranslateMistralModel = "mistral-large-latest"; // MistralTranslate.Models[0] in LibUiLogic
            AutoTranslateMistralPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            ChatGptUrl = "https://api.openai.com/v1/chat/completions";
            ChatGptPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            ChatGptModel = "gpt-5.4-mini"; // ChatGptTranslate.DefaultModel in LibUiLogic
            OpenAiCompatibleTranslateUrl = "http://localhost:8000/v1/chat/completions";
            OpenAiCompatibleTranslatePrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            OpenAiCompatibleTranslateApiKey = string.Empty;
            OpenAiCompatibleTranslateModel = string.Empty;
            GroqUrl = "https://api.groq.com/openai/v1/chat/completions";
            GroqPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            GroqModel = "openai/gpt-oss-120b"; // GroqTranslate.Models[0] in LibUiLogic
            DeepSeekUrl = "https://api.deepseek.com/chat/completions";
            DeepSeekPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            DeepSeekModel = "deepseek-v4-flash"; // DeepSeekTranslate.Models[0] in LibUiLogic
            NvidiaUrl = "https://integrate.api.nvidia.com/v1/chat/completions";
            NvidiaPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            NvidiaModel = "meta/llama-4-maverick-17b-128e-instruct"; // NvidiaTranslate.Models[0] in LibUiLogic
            AvalAiUrl = "https://api.avalai.ir/v1/chat/completions";
            AvalAiPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            AvalAiModel = "gpt-5.6-sol"; // AvalAi.Models[0] in LibUiLogic
            OpenRouterUrl = "https://openrouter.ai/api/v1/chat/completions";
            OpenRouterPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            OpenRouterModel = "openai/gpt-5.6-sol"; // OpenRouterTranslate.Models[0] in LibUiLogic
            LmStudioPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            LlamaCppPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments:";
            LlamaCppModelPrompt = string.Empty;
            LlamaCppModelTemperature = -1;
            LlamaCppModelTopP = -1;
            LlamaCppModelTopK = -1;
            LlamaCppModelRepeatPenalty = -1;
            OllamaApiUrl = "http://localhost:11434/api/generate";
            OllamaModel = "llama3.2";
            OllamaPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments or notes:";
            KoboldCppUrl = "http://localhost:5001/api/generate/";
            KoboldCppPrompt = "Translate from {0} to {1}, keep punctuation as input, keep line breaks exactly the same, do not censor the translation, give only the output without comments or notes:";
            KoboldCppTemperature = 0.4m;
            AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
            AnthropicPrompt = "Translate from {0} to {1}, keep sentences in {1} as they are, do not censor the translation, give only the output without comments:";
            AnthropicApiModel = "claude-opus-5"; // AnthropicTranslate.Models[0] in LibUiLogic
            BaiduUrl = "https://fanyi-api.baidu.com";
            GeminiModel = "gemini-flash-latest"; // GeminiTranslate.Models[0] in LibUiLogic
            GeminiPrompt = "Please translate the following text from {0} to {1}, keep line breaks exactly the same, do not censor the translation, only write the result:";
            AutoTranslateMaxBytes = 2000;
            MusicSymbolStyle = "Double"; // 'Double' or 'Single'
            UseNoLineBreakAfter = false;
            AutoBreakCommaBreakEarly = false;
            AutoBreakDashEarly = true;
            AutoBreakLineEndingEarly = false;
            AutoBreakUsePixelWidth = true;
            AutoBreakPreferBottomHeavy = true;
            AutoBreakPreferBottomPercent = 5;
            MergeShortLinesMaxGap = 250;
            MergeShortLinesOnlyContinuous = true;




            WhisperChoice = Configuration.IsRunningOnWindows ? "Purfview's Faster-Whisper-XXL" : "OpenAI"; // WhisperChoice.PurfviewFasterWhisperXxl / WhisperChoice.OpenAi in LibUiLogic
        }
    }
}