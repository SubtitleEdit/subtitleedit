using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class EdgeTts : ITtsEngine
{
    public string Name => "EdgeTts";
    public string Description => "free/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    private static string? _cachedExecutableFileName;

    private static readonly Regex VoiceLineRegex =
        new(@"^(?<name>\S+)\s+(?<gender>Male|Female|Neutral)\s+", RegexOptions.Compiled);

    public override string ToString()
    {
        return Name;
    }

    public async Task<bool> IsInstalled(string? region)
    {
        var (ok, _, _) = await RunEdgeTtsCommand("--version", CancellationToken.None);
        return ok;
    }

    public async Task<Voice[]> GetVoices(string languageCode)
    {
        var voices = await GetEdgeVoices(CancellationToken.None);
        if (voices.Count == 0)
        {
            voices = GetFallbackVoices();
        }

        return voices.Select(v => new Voice(v)).ToArray();
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
        var voices = await GetEdgeVoices(cancellationToken);
        if (voices.Count == 0)
        {
            voices = GetFallbackVoices();
        }

        return voices.Select(v => new Voice(v)).ToArray();
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
        if (voice.EngineVoice is not EdgeTtsVoice edgeVoice)
        {
            throw new ArgumentException("Voice is not an EdgeTtsVoice");
        }

        var folder = GetSetEdgeTtsFolder();
        var fileName = Path.Combine(folder, Guid.NewGuid() + ".mp3");
        var escapedText = text.Replace("\"", "\\\"");
        var escapedVoice = edgeVoice.Name.Replace("\"", "\\\"");
        var args =
            $"--voice \"{escapedVoice}\" --text \"{escapedText}\" --write-media \"{fileName}\"";

        // Add optional prosody parameters from settings
        var rate = Se.Settings.Video.TextToSpeech.EdgeTtsRate;
        if (!string.IsNullOrWhiteSpace(rate))
        {
            args += $" --rate={rate}";
        }

        var pitch = Se.Settings.Video.TextToSpeech.EdgeTtsPitch;
        if (!string.IsNullOrWhiteSpace(pitch))
        {
            args += $" --pitch={pitch}";
        }

        var volume = Se.Settings.Video.TextToSpeech.EdgeTtsVolume;
        if (!string.IsNullOrWhiteSpace(volume))
        {
            args += $" --volume={volume}";
        }

        var (ok, stdOut, stdErr) = await RunEdgeTtsCommand(args, cancellationToken);
        if (!ok || !File.Exists(fileName))
        {
            Se.LogError($"EdgeTts speak failed. StdOut: {stdOut} StdErr: {stdErr}");
            return new TtsResult { Text = text, FileName = string.Empty, Error = true };
        }

        return new TtsResult { Text = text, FileName = fileName };
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<string[]> GetModels()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public static string GetSetEdgeTtsFolder()
    {
        if (!Directory.Exists(Se.TtsFolder))
        {
            Directory.CreateDirectory(Se.TtsFolder);
        }

        var edgeFolder = Path.Combine(Se.TtsFolder, "EdgeTts");
        if (!Directory.Exists(edgeFolder))
        {
            Directory.CreateDirectory(edgeFolder);
        }

        return edgeFolder;
    }

    private static async Task<(bool Ok, string StdOut, string StdErr)> RunEdgeTtsCommand(string args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_cachedExecutableFileName))
        {
            _cachedExecutableFileName = FileUtil.FindPythonModuleExecutableFileName("edge-tts", Se.Settings.Video.TextToSpeech.EdgeTtsPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _cachedExecutableFileName ?? "edge-tts",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        try
        {
#pragma warning disable CA1416
            process.Start();
#pragma warning restore CA1416
        }
        catch
        {
            return (false, string.Empty, "edge-tts executable not found");
        }

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;
        return (process.ExitCode == 0, stdOut, stdErr);
    }

    private static async Task<List<EdgeTtsVoice>> GetEdgeVoices(CancellationToken cancellationToken)
    {
        var (ok, stdOut, _) = await RunEdgeTtsCommand("--list-voices", cancellationToken);
        if (!ok || string.IsNullOrWhiteSpace(stdOut))
        {
            return new List<EdgeTtsVoice>();
        }

        var list = new List<EdgeTtsVoice>();
        var lines = stdOut.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var match = VoiceLineRegex.Match(line.Trim());
            if (!match.Success)
            {
                continue;
            }

            var name = match.Groups["name"].Value;
            var gender = match.Groups["gender"].Value;
            list.Add(new EdgeTtsVoice(name, gender));
        }

        return list;
    }

    private static List<EdgeTtsVoice> GetFallbackVoices()
    {
        return
        [
            new EdgeTtsVoice("en-US-AriaNeural", "Female"),
            new EdgeTtsVoice("en-US-GuyNeural", "Male"),
            new EdgeTtsVoice("pl-PL-MarekNeural", "Male"),
            new EdgeTtsVoice("pl-PL-ZofiaNeural", "Female"),
        ];
    }

    public bool ImportVoice(string fileName)
    {
        return false;
    }
}
