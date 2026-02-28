using Nikse.SubtitleEdit.Core.AutoTranslate;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAutoTranslate
{
    public string AutoTranslateLastName { get; set; } = string.Empty;
    public string AutoTranslateLastSource { get; set; } = string.Empty;
    public string AutoTranslateLastTarget { get; set; } = string.Empty;
    public string ChatGptUrl { get; set; }
    public string ChatGptPrompt { get; set; }
    public string ChatGptApiKey { get; set; }
    public string ChatGptModel { get; set; }
    public string OllamaPrompt { get; set; }
    public string OllamaModel { get; set; }
    public string OllamaModels { get; set; }
    public string OllamaUrl { get; set; }
    public string LmStudioApiUrl { get; set; }
    public string LmStudioModel { get; set; }
    public string LmStudioPrompt { get; set; }
    public string LlamaCppApiUrl { get; set; }
    public string LlamaCppModel { get; set; }
    public string LlamaCppPrompt { get; set; }
    public string GroqUrl { get; set; }
    public string GroqPrompt { get; set; }
    public string GroqApiKey { get; set; }
    public string GroqModel { get; set; }
    public string GoogleApiV2Key { get; set; }
    public string MicrosoftBingApiId { get; set; }
    public string MicrosoftTranslatorApiKey { get; set; }
    public string MicrosoftTranslatorTokenEndpoint { get; set; }
    public string MicrosoftTranslatorCategory { get; set; }
    public decimal RequestMaxBytes { get; set; }
    public decimal RequestDelaySeconds { get; set; }
    public int CopyPasteMaxBlockSize { get; set; }
    public string CopyPasteLineSeparator { get; set; }
    public string OpenRouterUrl { get; set; }
    public string OpenRouterPrompt { get; set; }
    public string OpenRouterApiKey { get; set; }
    public string OpenRouterModel { get; set; }
    public string NnlbServeUrl { get; set; }
    public string NnlbApiUrl { get; set; }
    public string LibreTranslateApiKey { get; set; }
    public string LibreTranslateUrl { get; set; }
    public string DeepLApiKey { get; set; }
    public string DeepLUrl { get; set; }
    public string DeepLFormality { get; set; }
    public string DeepLXUrl { get; set; }
    public string MyMemoryApiKey { get; set; }
    public string NllbApiUrl { get; set; }
    public string NllbServeUrl { get; set; }
    public string NllbServeModel { get; set; }
    public string DeepSeekUrl { get; set; }
    public string DeepSeekPrompt { get; set; }
    public string DeepSeekApiKey { get; set; }
    public string DeepSeekModel { get; set; }
    public string PapagoApiKeyId { get; set; }
    public string PapagoApiKey { get; set; }
    public string MistralApiKey { get; set; }
    public string MistralUrl { get; set; }
    public string MistralModel { get; set; }
    public string MistralPrompt { get; set; }
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


    public string KoboldCppUrl { get; set; }
    public string KoboldCppPrompt { get; set; }
    public decimal KoboldCppTemperature { get; set; }
    public string AnthropicApiUrl { get; set; }
    public string AnthropicPrompt { get; set; }
    public string AnthropicApiKey { get; set; }
    public string AnthropicApiModel { get; set; }
    public string BaiduUrl { get; set; }
    public string BaiduApiKey { get; set; }
    public string GeminiProApiKey { get; set; }
    public string GeminiModel { get; set; }
    public string GeminiPrompt { get; set; }
    public string SeamlessM4TUrl { get; set; }

    public SeAutoTranslate()
    {
        AnthropicApiKey = string.Empty;
        AnthropicApiModel = AnthropicTranslate.Models[0];
        AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
        AnthropicPrompt = "Translate from {0} to {1}, keep sentences in {1} as they are, do not censor the translation, give only the output without comments:";
        AvalAiApiKey = string.Empty;
        AvalAiModel = AvalAi.Models[0];
        AvalAiPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        AvalAiUrl = "https://api.avalai.ir/v1/chat/completions";
        PerplexityApiKey = string.Empty;
        PerplexityModel = PerplexityTranslate.Models[0];
        PerplexityPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        PerplexityUrl = "https://api.perplexity.ai/v1/responses";
        LaraUrl = "https://api.laratranslate.com";
        BaiduApiKey = string.Empty;
        BaiduUrl = "https://fanyi-api.baidu.com";
        ChatGptApiKey = string.Empty;
        ChatGptModel = ChatGptTranslate.Models[0];
        ChatGptPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        ChatGptUrl = "https://api.openai.com/v1/chat/completions";
        CopyPasteLineSeparator = "(...)";
        CopyPasteMaxBlockSize = 5000;
        DeepLApiKey = string.Empty;
        DeepLFormality = string.Empty;
        DeepLUrl = "https://api-free.deepl.com/";
        DeepLXUrl = "http://localhost:1188";
        DeepSeekApiKey = string.Empty;
        DeepSeekModel = DeepSeekTranslate.Models[0];
        DeepSeekPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        DeepSeekUrl = "https://api.deepseek.com/chat/completions";
        GeminiModel = GeminiTranslate.Models[0];
        GeminiProApiKey = string.Empty;
        GeminiPrompt = "Please translate the following text from {0} to {1}, do not censor the translation, only write the result:";
        GoogleApiV2Key = string.Empty;
        GroqApiKey = string.Empty;
        GroqModel = GroqTranslate.Models[0];
        GroqPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        GroqUrl = "https://api.groq.com/openai/v1/chat/completions";
        KoboldCppPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments or notes:";
        KoboldCppTemperature = 0.4m;
        KoboldCppUrl = "http://localhost:5001/api/generate/";
        LibreTranslateApiKey = string.Empty;
        LibreTranslateUrl = "http://localhost:5000/";
        LibreTranslateUrl = "http://localhost:5000/";
        LlamaCppApiUrl = "http://localhost:8080/v1/chat/completions";
        LlamaCppModel = string.Empty;
        LlamaCppPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        LmStudioApiUrl = "http://localhost:1234/v1/chat/completions/";
        LmStudioModel = string.Empty;
        LmStudioPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        MicrosoftBingApiId = string.Empty;
        MicrosoftTranslatorApiKey = string.Empty;
        MicrosoftTranslatorCategory = string.Empty;
        MicrosoftTranslatorTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        MistralApiKey = string.Empty;
        MistralModel = MistralTranslate.Models[0];
        MistralPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        MistralUrl = "https://api.mistral.ai/v1/chat/completions";
        MyMemoryApiKey = string.Empty;
        NllbApiUrl = "http://localhost:7860/api/v4/";
        NllbServeModel = string.Empty;
        NllbServeUrl = "http://127.0.0.1:6060/";
        NnlbApiUrl = "http://localhost:7860/api/v4/";
        NnlbApiUrl = string.Empty;
        NnlbServeUrl = "http://127.0.0.1:6060/";
        NnlbServeUrl = string.Empty;
        OllamaModel = string.Empty;
        OllamaModels = "llama3.2,llama3.2:1b,phi3,gemma2,qwen2,mistral";
        OllamaPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments or notes:";
        OllamaUrl = "http://localhost:11434/api/generate";
        OpenRouterApiKey = string.Empty;
        OpenRouterModel = OpenRouterTranslate.Models[0];
        OpenRouterPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
        OpenRouterUrl = "https://openrouter.ai/api/v1/chat/completions";
        PapagoApiKey = string.Empty;
        PapagoApiKeyId = string.Empty;
        RequestMaxBytes = 1000;
        SeamlessM4TUrl = "http://localhost:5000/";
        LaraApiId = string.Empty;
        LaraApiSecret = string.Empty;
    }
}