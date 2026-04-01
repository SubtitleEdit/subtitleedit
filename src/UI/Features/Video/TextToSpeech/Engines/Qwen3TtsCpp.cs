using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class Qwen3TtsCpp : ITtsEngine
{
    public string Name => "Qwen3 TTS";
    public string Description => "free/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public const string TtsModelFileName = "Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf";
    public const string TokenizerModelFileName = "qwen3-tts-tokenizer-q8_0.gguf";
    public const string WavTokenizerModelFileName = "WavTokenizer-Large-75-Q4_0.gguf";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetExecutableFileName()));
    }

    public static bool IsModelsInstalled()
    {
        var modelsFolder = GetSetModelsFolder();
        return File.Exists(Path.Combine(modelsFolder, TtsModelFileName)) &&
               File.Exists(Path.Combine(modelsFolder, TokenizerModelFileName)) &&
               File.Exists(Path.Combine(modelsFolder, WavTokenizerModelFileName));
    }

    public override string ToString() => Name;

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var folder = Path.Combine(Se.TtsFolder, "Qwen3TtsCpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        var modelsFolder = Path.Combine(GetSetFolder(), "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetSetVoicesFolder()
    {
        var voicesFolder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(voicesFolder))
        {
            Directory.CreateDirectory(voicesFolder);
        }

        return voicesFolder;
    }

    public static string GetExecutableFileName()
    {
        return Path.Combine(GetSetFolder(), OperatingSystem.IsWindows() ? "qwen3-tts-cli.exe" : "qwen3-tts-cli");
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>
        {
            new Voice(new Qwen3TtsVoice("Default", string.Empty))
        };

        var voicesFolder = GetSetVoicesFolder();
        foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
        {
            var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
            result.Add(new Voice(new Qwen3TtsVoice(name, file)));
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice)
    {
        return true;
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model)
    {
        return Task.FromResult(Array.Empty<TtsLanguage>());
    }

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        return GetVoices(language);
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
        if (voice.EngineVoice is not Qwen3TtsVoice qwen3Voice)
        {
            throw new ArgumentException("Voice is not a Qwen3TtsVoice");
        }

        var outputFileNameOnly = Guid.NewGuid() + ".wav";
        var outputFileName = Path.Combine(GetSetFolder(), outputFileNameOnly);

        var process = StartProcess(qwen3Voice, text, outputFileNameOnly);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderrOutput = await stderrTask;

        if (process.ExitCode != 0)
        {
            Se.LogError($"Qwen3 TTS process exited with code {process.ExitCode} - "
                + $"Voice: {qwen3Voice}, "
                + $"Text: {text}, "
                + $"FileName: {process.StartInfo.FileName}"
                + (string.IsNullOrWhiteSpace(stderrOutput) ? string.Empty : $", StdErr: {stderrOutput.Trim()}"));
        }

        return new TtsResult(outputFileName, text);
    }

    private static Process StartProcess(Qwen3TtsVoice voice, string inputText, string outputFileNameOnly)
    {
        var processInfo = new ProcessStartInfo
        {
            WorkingDirectory = GetSetFolder(),
            FileName = GetExecutableFileName(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
        };

        processInfo.ArgumentList.Add("-m");
        processInfo.ArgumentList.Add("models");
        processInfo.ArgumentList.Add("--tts-model");
        processInfo.ArgumentList.Add(TtsModelFileName);
        processInfo.ArgumentList.Add("--tokenizer-model");
        processInfo.ArgumentList.Add(TokenizerModelFileName);
        processInfo.ArgumentList.Add("-t");
        processInfo.ArgumentList.Add(Utilities.UnbreakLine(inputText));
        processInfo.ArgumentList.Add("-o");
        processInfo.ArgumentList.Add(outputFileNameOnly);

        if (!string.IsNullOrEmpty(voice.FilePath))
        {
            processInfo.ArgumentList.Add("-r");
            processInfo.ArgumentList.Add(voice.FilePath);
        }

        var process = new Process { StartInfo = processInfo };

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            _ = process.Start();
        }
        else
        {
            throw new PlatformNotSupportedException("Process.Start() is not supported on this platform.");
        }

        return process;
    }
}
