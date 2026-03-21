using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public interface ITtsEngine
{
    string Name { get; }
    string Description { get; }
    bool HasLanguageParameter { get; }
    bool HasApiKey { get; }
    bool HasRegion { get; }
    bool HasModel { get; }
    bool HasKeyFile { get; }
    Task<bool> IsInstalled(string? region);
    string ToString();
    Task<Voice[]> GetVoices(string languageCode);
    Task<string[]> GetRegions();
    Task<string[]> GetModels();
    Task<TtsLanguage[]> GetLanguages(Voice voice, string? model);
    bool IsVoiceInstalled(Voice voice);
    Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken);
    Task<TtsResult> Speak(
        string text, 
        string outputFolder, 
        Voice voice, 
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken);
}
