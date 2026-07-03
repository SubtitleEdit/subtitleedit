using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

    private const int SpeakMaxAttempts = 3;

    // edge-tts requires rate/volume as "[+-]N%" and pitch as "[+-]NHz" (signed integer + unit).
    // Users frequently type "10%" or "10" — which fails edge-tts's own input validation before
    // any network call, so every TTS request errors instantly. Coerce common inputs into the
    // canonical form; return empty for hopeless values so the caller drops the arg entirely.
    public static string NormalizeProsodyValue(string? value, string unit)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var sign = '+';
        var i = 0;
        if (trimmed[0] == '+' || trimmed[0] == '-')
        {
            sign = trimmed[0];
            i = 1;
        }

        var digits = new StringBuilder();
        while (i < trimmed.Length && char.IsDigit(trimmed[i]))
        {
            digits.Append(trimmed[i]);
            i++;
        }

        if (digits.Length == 0)
        {
            return string.Empty;
        }

        return $"{sign}{digits}{unit}";
    }

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
        var escapedText = EscapeQuotedArgValue(text);
        var escapedVoice = EscapeQuotedArgValue(edgeVoice.Name);
        var args =
            $"--voice \"{escapedVoice}\" --text \"{escapedText}\" --write-media \"{fileName}\"";

        var rate = NormalizeProsodyValue(Se.Settings.Video.TextToSpeech.EdgeTtsRate, "%");
        if (!string.IsNullOrEmpty(rate))
        {
            args += $" --rate={rate}";
        }

        var pitch = NormalizeProsodyValue(Se.Settings.Video.TextToSpeech.EdgeTtsPitch, "Hz");
        if (!string.IsNullOrEmpty(pitch))
        {
            args += $" --pitch={pitch}";
        }

        var volume = NormalizeProsodyValue(Se.Settings.Video.TextToSpeech.EdgeTtsVolume, "%");
        if (!string.IsNullOrEmpty(volume))
        {
            args += $" --volume={volume}";
        }

        Se.WriteToolsLog($"EdgeTts: {_cachedExecutableFileName ?? "edge-tts"} {RedactTextArg(args)} (voice={edgeVoice.Name}, textLen={text.Length})");

        for (var attempt = 1; attempt <= SpeakMaxAttempts; attempt++)
        {
            if (File.Exists(fileName))
            {
                try { File.Delete(fileName); } catch { /* ignore */ }
            }

            var (ok, stdOut, stdErr) = await RunEdgeTtsCommand(args, cancellationToken);
            if (ok && File.Exists(fileName) && new FileInfo(fileName).Length > 0)
            {
                return new TtsResult { Text = text, FileName = fileName };
            }

            // Validation errors (bad rate/pitch/volume, unknown voice) will fail identically on
            // every retry — bail immediately so we don't waste seconds per segment.
            var isValidationError =
                stdErr.Contains("ValueError", StringComparison.Ordinal) ||
                stdErr.Contains("Invalid voice", StringComparison.OrdinalIgnoreCase);

            if (isValidationError || attempt >= SpeakMaxAttempts || cancellationToken.IsCancellationRequested)
            {
                var msg = $"EdgeTts speak failed (attempt {attempt}/{SpeakMaxAttempts}). StdOut: {stdOut} StdErr: {stdErr}";
                Se.LogError(msg);
                Se.WriteToolsLog(msg);
                return new TtsResult { Text = text, FileName = string.Empty, Error = true };
            }

            Se.WriteToolsLog($"EdgeTts: attempt {attempt} failed, retrying. StdErr: {stdErr}");
            try
            {
                await Task.Delay(3000 * attempt, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new TtsResult { Text = text, FileName = string.Empty, Error = true };
            }
        }

        return new TtsResult { Text = text, FileName = string.Empty, Error = true };
    }

    /// <summary>
    /// Escapes a value for use inside a quoted command-line argument ("{value}") per the
    /// MSVCRT/.NET parsing rules: quotes are backslash-escaped, and any run of backslashes
    /// immediately preceding a quote (or the end of the value, where our closing quote follows)
    /// is doubled. Escaping only quotes used to let a line *ending* in a backslash produce \",
    /// turning the closing quote into a literal and swallowing the following --write-media
    /// argument - failing the segment through all retries.
    /// </summary>
    internal static string EscapeQuotedArgValue(string value)
    {
        var sb = new StringBuilder(value.Length + 8);
        var backslashes = 0;
        foreach (var ch in value)
        {
            if (ch == '\\')
            {
                backslashes++;
                continue;
            }

            if (ch == '"')
            {
                sb.Append('\\', backslashes * 2 + 1).Append('"');
            }
            else
            {
                sb.Append('\\', backslashes).Append(ch);
            }

            backslashes = 0;
        }

        sb.Append('\\', backslashes * 2);
        return sb.ToString();
    }

    // Strip the --text "..." value from the logged command so subtitle content
    // doesn't end up in tools-log.txt for successful runs (failures still log
    // full args via Se.LogError).
    private static string RedactTextArg(string args)
    {
        const string marker = "--text \"";
        var start = args.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return args;
        }
        var contentStart = start + marker.Length;
        var i = contentStart;
        while (i < args.Length)
        {
            if (args[i] == '\\' && i + 1 < args.Length)
            {
                i += 2;
                continue;
            }
            if (args[i] == '"')
            {
                break;
            }
            i++;
        }
        if (i >= args.Length)
        {
            return args;
        }
        return args.Substring(0, contentStart) + "<text redacted>" + args.Substring(i);
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
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var edgeFolder = Path.Combine(Se.TextToSpeechFolder, "EdgeTts");
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
            process.StartProcess();
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
