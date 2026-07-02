namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAiReview
{
    public string Engine { get; set; }
    public string OllamaUrl { get; set; }
    public string OllamaModel { get; set; }
    public string LlamaCppModelFileName { get; set; }
    public string OpenAiCompatibleUrl { get; set; }
    public string OpenAiCompatibleModel { get; set; }
    public string OpenAiCompatibleApiKey { get; set; }
    public string Prompt { get; set; }
    public int MaxLinesPerBatch { get; set; }

    public const string EngineOllama = "Ollama";
    public const string EngineLlamaCpp = "llama.cpp";
    public const string EngineOpenAiCompatible = "OpenAI-compatible";

    public static string DefaultPrompt =>
        "You are a subtitle proofreader. Fix typos, spelling, grammar and punctuation in {language}." +
        "\n\nDo not rephrase, do not change meaning, tone or style. Keep names, slang and intentional dialect as they are. " +
        "Keep all formatting tags (like <i> or {\\an8}) and line breaks exactly as they are. Only correct actual errors.";

    public SeAiReview()
    {
        Engine = EngineLlamaCpp;
        OllamaUrl = "http://localhost:11434/v1/chat/completions";
        OllamaModel = string.Empty;
        LlamaCppModelFileName = string.Empty;
        OpenAiCompatibleUrl = "http://localhost:1234/v1/chat/completions";
        OpenAiCompatibleModel = string.Empty;
        OpenAiCompatibleApiKey = string.Empty;
        Prompt = DefaultPrompt;
        MaxLinesPerBatch = 15;
    }
}
