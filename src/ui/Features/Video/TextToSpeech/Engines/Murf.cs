using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class Murf : ITtsEngine
{
    public string Name => "Murf";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => true;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(!string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.MurfApiKey));
    }

    private const string JsonFileName = "MurfVoices.json";
    private readonly ITtsDownloadService _ttsDownloadService;

    public Murf(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            languageCode = "en-US";
        }

        // https://murf.ai/api/reference/endpoints/list-voices
        var murfFolder = GetSetMurfFolder();

        var voiceFileName = Path.Combine(murfFolder, JsonFileName);
        if (!File.Exists(voiceFileName))
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/MurfVoices.json");
            using var stream = AssetLoader.Open(uri);
            using var fileStream = File.Create(voiceFileName);
            stream.CopyTo(fileStream);
        }

        var voices = Map(voiceFileName);
        var resultVoices = voices.Where(p => (p.EngineVoice as MurfVoice)?.Locale == languageCode).ToArray();
        if (resultVoices.Length == 0)
        {
            resultVoices = voices.Where(p => (p.EngineVoice as MurfVoice)?.Locale == "en-US").ToArray();
        }

        return Task.FromResult(resultVoices);
    }

    private static Voice[] Map(string voiceFileName)
    {
        if (!File.Exists(voiceFileName))
        {
            return [];
        }

        var result = new List<Voice>();
        var json = File.ReadAllText(voiceFileName);
        var parser = new SeJsonParser();
        var voices = parser.GetArrayElements(json);
        foreach (var voice in voices)
        {
            var displayName = parser.GetFirstObject(voice, "displayName");
            var voiceId = parser.GetFirstObject(voice, "voiceId");
            var locale = parser.GetFirstObject(voice, "locale");
            var description = parser.GetFirstObject(voice, "description");
            var gender = parser.GetFirstObject(voice, "gender");
            var styles = parser.GetArrayElementsByName(voice, "availableStyles");
            result.Add(new Voice(new MurfVoice(displayName, voiceId, locale, description, gender, styles.ToArray())));
        }

        return result.ToArray();
    }

    public static string GetSetMurfFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var murfFolder = Path.Combine(Se.TtsFolder, "Murf");
        if (!Directory.Exists(murfFolder))
        {
            Directory.CreateDirectory(murfFolder);
        }

        return murfFolder;
    }

    public bool IsVoiceInstalled(Voice voice)
    {
        return true;
    }

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model)
    {
        var languages = new List<TtsLanguage>
        {
            new("Spanish", "es-ES"),
            new("English (US and Canada)", "en-US"),
            new("English (Australia)", "en-AU"),
            new("English (UK)", "en-UK"),
            new("English (India)", "en-IN"),
            new("English (Scottish)", "en-SCOTT"),
            new("France", "fr-FR"),
            new("German", "de-DE"),
            new("Spanish (Mexico)", "es-MX"),
            new("Italian", "it-IT"),
            new("Portuguese (Brazil)", "pt-BR"),
            new("Portuguese (Portugal)", "pl-PL"),
        };

        return Task.FromResult(languages.OrderBy(p => p.Name).ToArray());
    }

    public async Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await _ttsDownloadService.DownloadMurfVoiceList(ms, null, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetMurfFolder(), JsonFileName), ms.ToArray(), cancellationToken);
        return await GetVoices(language);
    }

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not MurfVoice murfVoice)
        {
            throw new ArgumentException("Voice is not a MurfVoice");
        }

        var ms = new MemoryStream();
        var ok = await _ttsDownloadService
            .DownloadMurfSpeak(
                text,
                murfVoice,
                Se.Settings.Video.TextToSpeech.MurfStyle,
                Se.Settings.Video.TextToSpeech.MurfApiKey,
                ms,
                cancellationToken);
        if (!ok)
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        var fileName = Path.Combine(GetSetMurfFolder(), Guid.NewGuid() + ".mp3");
        await File.WriteAllBytesAsync(fileName, ms.ToArray(), cancellationToken);
        return new TtsResult { Text = text, FileName = fileName };
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(new[] { "GEN2" });
    }
}
