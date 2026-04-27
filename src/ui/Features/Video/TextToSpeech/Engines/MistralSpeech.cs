using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class MistralSpeech : ITtsEngine
{
    public string Name => "MistralSpeech";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => true;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(!string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.MistralApiKey));
    }

    private const string JsonFileName = "MistralVoices.json";
    private readonly ITtsDownloadService _ttsDownloadService;

    public MistralSpeech(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var mistralFolder = GetSetMistralFolder();

        var voiceFileName = Path.Combine(mistralFolder, JsonFileName);
        if (!File.Exists(voiceFileName))
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/MistralVoices.json");
            using var stream = AssetLoader.Open(uri);
            using var fileStream = File.Create(voiceFileName);
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(Map(voiceFileName));
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
        var items = parser.GetArrayElementsByName(json, "items");
        if (items.Count == 0)
        {
            items = parser.GetArrayElements(json);
        }

        foreach (var item in items)
        {
            var name = parser.GetFirstObject(item, "name");
            var voiceId = parser.GetFirstObject(item, "id");
            var gender = parser.GetFirstObject(item, "gender");
            var languages = parser.GetArrayElementsByName(item, "languages");
            var language = languages.Count > 0 ? languages[0].Trim('"') : string.Empty;

            if (!string.IsNullOrEmpty(voiceId))
            {
                result.Add(new Voice(new MistralVoice(name, voiceId, gender, language)));
            }
        }

        return result.ToArray();
    }

    public static string GetSetMistralFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var mistralFolder = Path.Combine(Se.TextToSpeechFolder, "Mistral");
        if (!Directory.Exists(mistralFolder))
        {
            Directory.CreateDirectory(mistralFolder);
        }

        return mistralFolder;
    }

    public bool IsVoiceInstalled(Voice voice)
    {
        return true;
    }

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model)
    {
        return Task.FromResult(Array.Empty<TtsLanguage>());
    }

    public async Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await _ttsDownloadService.DownloadMistralSpeechVoiceList(ms, null, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetMistralFolder(), JsonFileName), ms.ToArray(), cancellationToken);
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
        if (voice.EngineVoice is not MistralVoice mistralVoice)
        {
            throw new ArgumentException("Voice is not a MistralVoice");
        }

        var ms = new MemoryStream();
        var ok = await _ttsDownloadService.DownloadMistralSpeechSpeak(
            text,
            mistralVoice,
            model ?? "voxtral-mini-tts-2603",
            Se.Settings.Video.TextToSpeech.MistralApiKey,
            ms,
            null,
            cancellationToken);
        if (!ok)
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        var fileName = Path.Combine(GetSetMistralFolder(), Guid.NewGuid() + ".mp3");
        await File.WriteAllBytesAsync(fileName, ms.ToArray(), cancellationToken);
        return new TtsResult { Text = text, FileName = fileName };
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(new[]
        {
            "voxtral-mini-tts-2603",
        });
    }

    public bool ImportVoice(string fileName)
    {
        return false;
    }
}
