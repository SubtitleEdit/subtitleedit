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

public class GoogleSpeech : ITtsEngine
{
    public string Name => "GoogleSpeech";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => true;

    public Task<bool> IsInstalled(string? region)
    {
        var ok = !string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.GoogleApiKey);
        return Task.FromResult(ok);
    }

    private const string JsonFileName = "GoogleVoices.json";
    private readonly ITtsDownloadService _ttsDownloadService;

    public GoogleSpeech(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var googleFolder = GetSetGoogleFolder();

        var voiceFileName = Path.Combine(googleFolder, JsonFileName);
        if (!File.Exists(voiceFileName) || new FileInfo(voiceFileName).Length < 100)
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/GoogleVoices.json");
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
        var arr = parser.GetArrayElementsByName(json, "voices");
        foreach (var item in arr)
        {
            var name = parser.GetFirstObject(item, "name");
            var languageCode = name.Substring(0,5);
            var ssmlGender = parser.GetFirstObject(item, "ssmlGender");
            var naturalSampleRateHertz = parser.GetFirstObject(item, "naturalSampleRateHertz");

            var gender = "Unspecified";
            if (ssmlGender == "1")
            {
                gender = "Male";
            }
            else if (ssmlGender == "2")
            {
                gender = "Female";
            }
            else if (ssmlGender == "3")
            { 
                gender = "Neutral";
            }

            result.Add(new Voice(new GoogleVoice
            {
                Name = name,
                LanguageCode = languageCode,
                SsmlGender = gender,
                NaturalSampleRateHertz = naturalSampleRateHertz,
            }));
        }

        return result.ToArray();
    }

    public static string GetSetGoogleFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var googleFolder = Path.Combine(Se.TtsFolder, "Google");
        if (!Directory.Exists(googleFolder))
        {
            Directory.CreateDirectory(googleFolder);
        }

        return googleFolder;
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
        await _ttsDownloadService.DownloadGoogleVoiceList(Se.Settings.Video.TextToSpeech.GoogleKeyFile, ms, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetGoogleFolder(), JsonFileName), ms.ToArray(), cancellationToken);
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
        if (voice.EngineVoice is not GoogleVoice googleVoice)
        {
            throw new ArgumentException("Voice is not a GoogleVoice");
        }

        var ms = new MemoryStream();
        var ok = await _ttsDownloadService.DownloadGoogleVoiceSpeak(
            text,
            googleVoice,
            model ?? "Standard",
            Se.Settings.Video.TextToSpeech.GoogleKeyFile,
            ms,
            cancellationToken);

        if (!ok)
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        var fileName = Path.Combine(GetSetGoogleFolder(), Guid.NewGuid() + ".mp3");
        await File.WriteAllBytesAsync(fileName, ms.ToArray(), cancellationToken);
        return new TtsResult { Text = text, FileName = fileName };
    }

    public Task<string[]> GetRegions()
    {
        // Google TTS doesn't use regions like Azure
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(new[]
        {
            "Standard",
            "WaveNet",
            "Neural2",
            "Polyglot",
            "Studio"
        });
    }
}