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

public class ElevenLabs : ITtsEngine
{
    public string Name => "ElevenLabs";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => true;
    public bool HasApiKey => true;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(!string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.ElevenLabsApiKey));
    }

    private const string JsonFileName = "ElevenLabsVoices.json";
    private readonly ITtsDownloadService _ttsDownloadService;

    public ElevenLabs(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var elevenLabsFolder = GetSetElevenLabsFolder();

        var voiceFileName = Path.Combine(elevenLabsFolder, JsonFileName);
        if (!File.Exists(voiceFileName))
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/ElevenLabsVoices.json");
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
        var voices = parser.GetArrayElementsByName(json, "voices");
        foreach (var voice in voices)
        {
            var name = parser.GetFirstObject(voice, "name");
            var voiceId = parser.GetFirstObject(voice, "voice_id");
            var gender = parser.GetFirstObject(voice, "gender");
            var description = parser.GetFirstObject(voice, "description");
            var accent = parser.GetFirstObject(voice, "accent");
            var useCase = parser.GetFirstObject(voice, "use case");
            result.Add(new Voice(new ElevenLabVoice(string.Empty, name, gender, description, useCase, accent, voiceId)));
        }

        return result.ToArray();
    }

    private static string GetSetElevenLabsFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var elevenLabsFolder = Path.Combine(Se.TtsFolder, "ElevenLabs");
        if (!Directory.Exists(elevenLabsFolder))
        {
            Directory.CreateDirectory(elevenLabsFolder);
        }

        return elevenLabsFolder;
    }

    public bool IsVoiceInstalled(Voice voice)
    {
        return true;
    }

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model)
    {
        // see https://help.elevenlabs.io/hc/en-us/articles/17883183930129-What-models-do-you-offer-and-what-is-the-difference-between-them

        var languages = new List<TtsLanguage>();

        if (model == "eleven_v3")
        {
            languages = new List<TtsLanguage>
            {
                new("Afrikaans", "af"),
                new("Arabic", "ar"),
                new("Armenian", "hy"),
                new("Assamese", "as"),
                new("Azerbaijani", "az"),
                new("Belarusian", "be"),
                new("Bengali", "bn"),
                new("Bosnian", "bs"),
                new("Bulgarian", "bg"),
                new("Catalan", "ca"),
                new("Cebuano", "ceb"),
                new("Chichewa", "ny"),
                new("Croatian", "hr"),
                new("Czech", "cs"),
                new("Danish", "da"),
                new("Dutch", "nl"),
                new("English", "en"),
                new("Estonian", "et"),
                new("Filipino", "fil"),
                new("Finnish", "fi"),
                new("French", "fr"),
                new("Galician", "gl"),
                new("Georgian", "ka"),
                new("German", "de"),
                new("Greek", "el"),
                new("Gujarati", "gu"),
                new("Hausa", "ha"),
                new("Hebrew", "he"),
                new("Hindi", "hi"),
                new("Hungarian", "hu"),
                new("Icelandic", "is"),
                new("Indonesian", "id"),
                new("Irish", "ga"),
                new("Italian", "it"),
                new("Japanese", "ja"),
                new("Javanese", "jv"),
                new("Kannada", "kn"),
                new("Kazakh", "kk"),
                new("Kyrgyz", "ky"),
                new("Korean", "ko"),
                new("Latvian", "lv"),
                new("Lingala", "ln"),
                new("Lithuanian", "lt"),
                new("Luxembourgish", "lb"),
                new("Macedonian", "mk"),
                new("Malay", "ms"),
                new("Malayalam", "ml"),
                new("Mandarin Chinese", "zh"),
                new("Marathi", "mr"),
                new("Nepali", "ne"),
                new("Norwegian", "no"),
                new("Pashto", "ps"),
                new("Persian", "fa"),
                new("Polish", "pl"),
                new("Portuguese", "pt"),
                new("Punjabi", "pa"),
                new("Romanian", "ro"),
                new("Russian", "ru"),
                new("Serbian", "sr"),
                new("Sindhi", "sd"),
                new("Slovak", "sk"),
                new("Slovenian", "sl"),
                new("Somali", "so"),
                new("Spanish", "es"),
                new("Swahili", "sw"),
                new("Swedish", "sv"),
                new("Tamil", "ta"),
                new("Telugu", "te"),
                new("Thai", "th"),
                new("Turkish", "tr"),
                new("Ukrainian", "uk"),
                new("Urdu", "ur"),
                new("Vietnamese", "vi"),
                new("Welsh", "cy"),
            };
        }

        if (model is "eleven_multilingual_v2" or "eleven_turbo_v2_5")
        {
            languages = new List<TtsLanguage>
            {
                new("Arabic", "ar"),
                new("Bulgarian", "bg"),
                new("Chinese", "zh"),
                new("Croatian", "hr"),
                new("Czech", "cz"),
                new("Danish", "da"),
                new("Dutch", "nl"),
                new("English", "en"),
                new("Filipino", "ph"),
                new("Finnish", "fi"),
                new("French", "fr"),
                new("German", "de"),
                new("Greek", "el"),
                new("Hindi", "hi"),
                new("Indonesian", "id"),
                new("Italian", "it"),
                new("Japanese", "ja"),
                new("Korean", "kr"),
                new("Malay", "ms"),
                new("Polish", "pl"),
                new("Portuguese", "pt"),
                new("Romanian", "ro"),
                new("Russian", "ru"),
                new("Slovak", "sk"),
                new("Spanish", "es"),
                new("Swedish", "sv"),
                new("Tamil", "ta"),
                new("Turkish", "tr"),
                new("Ukrainian", "uk"),
            };

            if (model == "eleven_turbo_v2_5")
            {
                languages.Add(new TtsLanguage("Hungarian", "hu"));
                languages.Add(new TtsLanguage("Norwegian", "no"));
                languages.Add(new TtsLanguage("Vietnamese", "vi"));
                return Task.FromResult(languages.ToArray());
            }
        }

        if (model == "eleven_turbo_v2)")
        {
            languages = new List<TtsLanguage>
            {
                new("English", "en"),
            };
        }

        if (model == "eleven_multilingual_v1)")
        {
            languages = new List<TtsLanguage>
            {
                new("English", "en"),
                new("German", "de"),
                new("Polish", "pl"),
                new("Spanish", "es"),
                new("Italian", "it"),
                new("French", "fr"),
                new("Hindi", "hi"),
                new("Portuguese", "pt"),
            };
        }

        return Task.FromResult(languages.OrderBy(p => p.Name).ToArray());
    }

    public async Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await _ttsDownloadService.DownloadElevenLabsVoiceList(ms, null, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetElevenLabsFolder(), JsonFileName), ms.ToArray(), cancellationToken);
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
        if (voice.EngineVoice is not ElevenLabVoice elevenLabVoice)
        {
            throw new ArgumentException("Voice is not an ElevenLabVoice");
        }

        if (string.IsNullOrEmpty(model))
        {
            throw new ArgumentException("ElevenLabs model is empty");
        }

        var ms = new MemoryStream();
        var ok = await _ttsDownloadService.DownloadElevenLabsVoiceSpeak(text, elevenLabVoice, model, Se.Settings.Video.TextToSpeech.ElevenLabsApiKey, "en", ms, null, cancellationToken);
        if (!ok)
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        var fileName = Path.Combine(GetSetElevenLabsFolder(), Guid.NewGuid() + ".mp3");
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
            "eleven_turbo_v2_5",
            "eleven_v3",
            "eleven_multilingual_v2"
        });
    }
}