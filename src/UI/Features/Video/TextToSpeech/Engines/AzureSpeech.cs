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

public class AzureSpeech : ITtsEngine
{
    public string Name => "AzureSpeech";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => true;
    public bool HasRegion => true;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public Task<bool> IsInstalled(string? region)
    {
        var ok = !string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.AzureApiKey) &&
                      !string.IsNullOrEmpty(region);

        return Task.FromResult(ok);
    }

    private const string JsonFileName = "AzureVoices.json";
    private readonly ITtsDownloadService _ttsDownloadService;

    public AzureSpeech(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var azureFolder = GetSetAzureFolder();

        var voiceFileName = Path.Combine(azureFolder, JsonFileName);
        if (!File.Exists(voiceFileName))
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/AzureVoices.json");
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
        var arr = parser.GetArrayElements(json);
        foreach (var item in arr)
        {
            var displayName = parser.GetFirstObject(item, "DisplayName");
            var localName = parser.GetFirstObject(item, "LocalName");
            var shortName = parser.GetFirstObject(item, "ShortName");
            var gender = parser.GetFirstObject(item, "Gender");
            var locale = parser.GetFirstObject(item, "Locale");

            result.Add(new Voice(new AzureVoice
            {
                DisplayName = displayName,
                LocalName = localName,
                ShortName = shortName,
                Gender = gender,
                Locale = locale,
            }));
        }

        return result.ToArray();
    }

    public static string GetSetAzureFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var azureFolder = Path.Combine(Se.TtsFolder, "Azure");
        if (!Directory.Exists(azureFolder))
        {
            Directory.CreateDirectory(azureFolder);
        }

        return azureFolder;
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
        await _ttsDownloadService.DownloadElevenLabsVoiceList(ms, null, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetAzureFolder(), JsonFileName), ms.ToArray(), cancellationToken);
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
        if (voice.EngineVoice is not AzureVoice azureVoice)
        {
            throw new ArgumentException("Voice is not an AzureVoice");
        }

        var ms = new MemoryStream();
        var ok = await _ttsDownloadService.DownloadAzureVoiceSpeak(text, azureVoice, model!, Se.Settings.Video.TextToSpeech.AzureApiKey, "en", region!, ms, null, cancellationToken);
        if (!ok)
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        var fileName = Path.Combine(GetSetAzureFolder(), Guid.NewGuid() + ".mp3");
        await File.WriteAllBytesAsync(fileName, ms.ToArray(), cancellationToken);
        return new TtsResult { Text = text, FileName = fileName };
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(new[]
        {
            "australiaeast",
            "brazilsouth",
            "canadacentral",
            "centralus",
            "eastasia",
            "eastus",
            "eastus2",
            "francecentral",
            "germanywestcentral",
            "centralindia",
            "japaneast",
            "japanwest",
            "jioindiawest",
            "koreacentral",
            "northcentralus",
            "northeurope",
            "norwayeast",
            "southcentralus",
            "southeastasia",
            "swedencentral",
            "switzerlandnorth",
            "switzerlandwest",
            "uaenorth",
            "usgovarizona",
            "usgovvirginia",
            "uksouth",
            "westcentralus",
            "westeurope",
            "westus",
            "westus2",
            "westus3"
        });
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(Array.Empty<string>());
    }
}