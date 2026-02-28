using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class Piper : ITtsEngine
{
    public string Name => "Piper";
    public string Description => "free/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetPiperExecutableFileName()));
    }

    private readonly ITtsDownloadService _ttsDownloadService;

    public Piper(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var piperFolder = GetSetPiperFolder();

        var voiceFileName = Path.Combine(piperFolder, "PiperVoices.json");
        if (!File.Exists(voiceFileName))
        {
            var uri = new Uri("avares://SubtitleEdit/Assets/TextToSpeech/PiperVoices.json");
            using var stream = AssetLoader.Open(uri);
            using var fileStream = File.Create(voiceFileName);
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(Map(voiceFileName));
    }

    public bool IsVoiceInstalled(Voice voice)
    {
        if (voice.EngineVoice is not PiperVoice piperVoice)
        {
            return false;
        }

        var modelFileName = Path.Combine(GetSetPiperFolder(), piperVoice.ModelShort);
        if (!File.Exists(modelFileName))
        {
            return false;
        }

        var configFileName = Path.Combine(GetSetPiperFolder(), piperVoice.ConfigShort);
        if (!File.Exists(configFileName))
        {
            return false;
        }

        return true;
    }

    private static string GetPiperExecutableFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(GetSetPiperFolder(), "piper.exe");
        }

        return "piper";
    }

    public static string GetSetPiperFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var piperFolder = Path.Combine(Se.TtsFolder, "Piper");
        if (!Directory.Exists(piperFolder))
        {
            Directory.CreateDirectory(piperFolder);
        }

        return piperFolder;
    }

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model)
    {
        return Task.FromResult(Array.Empty<TtsLanguage>());
    }

    private static Voice[] Map(string voiceFileName)
    {
        if (string.IsNullOrWhiteSpace(voiceFileName))
        {
            return [];
        }

        var result = new List<Voice>();
        var json = File.ReadAllText(voiceFileName);
        var parser = new SeJsonParser();
        var arr = parser.GetRootElements(json);

        foreach (var element in arr)
        {
            var elements = parser.GetRootElements(element.Json);
            var name = elements.FirstOrDefault(p => p.Name == "name");
            var quality = elements.FirstOrDefault(p => p.Name == "quality");
            var language = elements.FirstOrDefault(p => p.Name == "language");
            var files = elements.FirstOrDefault(p => p.Name == "files");

            if (name != null && quality != null && language != null && files != null)
            {
                var languageDisplay = parser.GetFirstObject(language.Json, "name_english");
                var languageFamily = parser.GetFirstObject(language.Json, "family");
                var languageCode = parser.GetFirstObject(language.Json, "code");

                var filesElements = parser.GetRootElements(files.Json);
                var model = filesElements.FirstOrDefault(p => p.Name.EndsWith(".onnx"));
                var config = filesElements.FirstOrDefault(p => p.Name.EndsWith("onnx.json"));
                if (model != null && config != null)
                {
                    var modelUrl = "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/" + model.Name;
                    var configUrl = "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/" + config.Name;

                    var piperVoice = new PiperVoice(name.Json, languageDisplay, quality.Json, modelUrl, configUrl);
                    result.Add(new Voice(piperVoice));
                }
            }
        }

        return result.ToArray();
    }

    public async Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await  _ttsDownloadService.DownloadPiperVoiceList(ms, null, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(GetSetPiperFolder(), "voices.json"), ms.ToArray(), cancellationToken);
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
        if (voice.EngineVoice is not PiperVoice piperVoice)
        {
            throw new ArgumentException("Voice is not a PiperVoice");
        }

        var fileNameOnly = Guid.NewGuid() + ".wav";
        var process = StartPiperProcess(piperVoice, text, fileNameOnly);
        await process.WaitForExitAsync(cancellationToken);

        var fileName = Path.Combine(GetSetPiperFolder(), fileNameOnly);
        return new TtsResult(fileName, text);
    }

    private Process StartPiperProcess(PiperVoice voice, string inputText, string outputFileName)
    {
        var processPiper = new Process
        {
            StartInfo =
            {
                WorkingDirectory = GetSetPiperFolder(),
                FileName = GetPiperExecutableFileName(),
                Arguments = $"-m \"{voice.ModelShort}\" -c \"{voice.ConfigShort}\" -f {outputFileName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            }
        };

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS())
        {
            _ = processPiper.Start();
        }
        else
        {
            throw new PlatformNotSupportedException("Process.Start() is not supported on this platform.");
        }

        var streamWriter = new StreamWriter(processPiper.StandardInput.BaseStream, Encoding.UTF8);
        var text = Utilities.UnbreakLine(inputText);
        streamWriter.Write(text);
        streamWriter.Flush();
        streamWriter.Close();

        return processPiper;
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(Array.Empty<string>());
    }
}