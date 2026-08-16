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

    /// <summary>
    /// Whether <see cref="ImportVoice"/> clones a speaker from a reference recording, i.e. whether
    /// this engine can imitate a voice it is given a sample of. False for engines with a fixed set
    /// of voices, and for Piper, whose import takes a trained model rather than somebody's voice.
    /// </summary>
    /// <remarks>
    /// The single answer to "can this engine clone?": it decides whether the voice-settings dialog
    /// offers an import, whether <see cref="VoiceCloningConsent"/> is owed before that import, and
    /// which engines the waveform's "Clone voice to" menu lists.
    /// </remarks>
    bool SupportsVoiceCloning { get; }

    /// <summary>
    /// Whether this engine takes its cloning reference per synthesis call, so a different voice
    /// can be cloned for every subtitle line without paying to reload the model.
    /// </summary>
    /// <remarks>
    /// The dividing line for <see cref="PerLineVoiceClone"/>. False for every engine that reads
    /// the reference once when its server starts (most CrispASR-backed ones): those are perfectly
    /// good cloning engines, but a per-line reference would restart and reload the model for each
    /// line, which is minutes per line rather than seconds.
    /// </remarks>
    bool SupportsPerLineVoiceCloning { get; }

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
    bool ImportVoice(string fileName);
}
