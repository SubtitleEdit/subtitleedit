using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace Nikse.SubtitleEdit.UiLogic.Translate.Llm;

public class ProfileLlmTranslator : IAutoTranslator, IDisposable
{
    protected readonly TranslationProfile Profile;
    protected ILlmClient? Client;
    public ProfileLlmTranslator(TranslationProfile profile) => Profile = profile.Snapshot();
    public string Name => Profile.Name;
    public string Url => Profile.ApiUrl;
    public string Error { get; set; } = "";
    public int MaxCharacters => int.MaxValue;
    public void Initialize()
    {
        Client?.Dispose();
        Client = CreateClient(Profile);
    }
    internal static ILlmClient CreateClient(TranslationProfile profile) => profile.Protocol == LlmApiProtocol.Anthropic
        ? new AnthropicLlmClient(profile.Policy, profile.ApiKey)
        : new OpenAiLlmClient(profile.Policy, profile.ApiKey, profile.Location == LlmLocation.Local && profile.EngineId == "LlamaCpp");
    public List<TranslationPair> GetSupportedSourceLanguages() => ChatGptTranslate.ListLanguages();
    public List<TranslationPair> GetSupportedTargetLanguages() => ChatGptTranslate.ListLanguages();
    public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var client = Client ?? throw new InvalidOperationException("Initialize() not called");
        var stripped = StrippedLine.Strip(text);
        if (string.IsNullOrWhiteSpace(stripped.Text)) return text;
        try
        {
            var prompt = Profile.LinePrompt.Replace("{0}", sourceLanguageCode).Replace("{1}", targetLanguageCode);
            var reply = await client.ChatAsync(Profile.ApiUrl, prompt, stripped.Text, null, cancellationToken, Profile.Model, 8192);
            if (string.IsNullOrWhiteSpace(reply)) throw new HttpRequestException("The server returned an empty translation.");
            return stripped.Restore(ChatGptTranslate.RemovePreamble(stripped.Text, reply.Trim()));
        }
        catch { Error = client.Error; throw; }
    }
    public void Dispose() => Client?.Dispose();
}

public sealed class ProfileAdvancedLlmTranslator : AdvancedTranslatorBase
{
    private readonly TranslationProfile _profile;
    public ProfileAdvancedLlmTranslator(TranslationProfile profile) => _profile = profile.Snapshot();
    public override string Name => _profile.Name;
    public override string Url => _profile.ApiUrl;
    protected override AdvancedTranslationSettings Settings => _profile.Policy;
    protected override string GetApiUrl() => _profile.ApiUrl;
    protected override string? GetModel() => string.IsNullOrWhiteSpace(_profile.Model) ? null : _profile.Model;
    protected override ILlmClient CreateClient() => ProfileLlmTranslator.CreateClient(_profile);
}
