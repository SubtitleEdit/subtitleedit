using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public const string ModelKey06B = "0.6B";
    public const string ModelKey17BBase = "1.7B Base";
    public const string DefaultModelKey = ModelKey06B;

    public const string TtsModelFileName06B = "qwen3-tts-0.6b-q8_0.gguf";
    public const string TtsModelFileName17BBase = "Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf";
    public const string TokenizerModelFileName = "qwen3-tts-tokenizer-q8_0.gguf";

    public static string GetModelFileName(string? modelKey) => modelKey switch
    {
        ModelKey17BBase => TtsModelFileName17BBase,
        _ => TtsModelFileName06B,
    };

    public static string ResolveModelKey(string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            var saved = Se.Settings.Video.TextToSpeech.Qwen3TtsCppModel;
            return string.IsNullOrEmpty(saved) ? DefaultModelKey : saved;
        }
        return modelKey;
    }

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelFileName;
    private static bool _processExitHooked;

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetExecutableFileName()));
    }

    public static bool IsModelsInstalled(string? modelKey = null)
    {
        var modelsFolder = GetSetModelsFolder();
        var resolved = ResolveModelKey(modelKey);
        return File.Exists(Path.Combine(modelsFolder, GetModelFileName(resolved))) &&
               File.Exists(Path.Combine(modelsFolder, TokenizerModelFileName));
    }

    public override string ToString() => Name;

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "Qwen3TtsCpp");
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
        return Path.Combine(GetSetFolder(), OperatingSystem.IsWindows() ? "qwen3-tts-server.exe" : "qwen3-tts-server");
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
        return Task.FromResult(new[] { ModelKey06B, ModelKey17BBase });
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

        var modelFileName = GetModelFileName(ResolveModelKey(model));
        await EnsureServerRunningAsync(modelFileName, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = Utilities.UnbreakLine(text);

        using HttpResponseMessage response = string.IsNullOrEmpty(qwen3Voice.FilePath)
            ? await SynthesizeAsync(inputText, cancellationToken)
            : await SynthesizeWithVoiceAsync(inputText, qwen3Voice.FilePath, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await SafeReadErrorAsync(response, cancellationToken);
            Se.LogError($"Qwen3 TTS server error {(int)response.StatusCode} {response.StatusCode} - "
                + $"Voice: {qwen3Voice}, Text: {text}, Body: {errorBody}");
            throw new InvalidOperationException(
                $"Qwen3 TTS synthesis failed ({(int)response.StatusCode}): {errorBody}");
        }

        await using (var fileStream = File.Create(outputFileName))
        await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    private static async Task<HttpResponseMessage> SynthesizeAsync(string text, CancellationToken ct)
    {
        var body = JsonSerializer.Serialize(new { text });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await HttpClient.PostAsync($"{ServerBaseUrl}/v1/synthesize", content, ct);
    }

    private static async Task<HttpResponseMessage> SynthesizeWithVoiceAsync(string text, string referenceWav, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(text, Encoding.UTF8), "text");
        var refBytes = await File.ReadAllBytesAsync(referenceWav, ct);
        var fileContent = new ByteArrayContent(refBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        form.Add(fileContent, "reference_audio", Path.GetFileName(referenceWav));
        return await HttpClient.PostAsync($"{ServerBaseUrl}/v1/synthesize_with_voice", form, ct);
    }

    private static async Task<string> SafeReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            return $"<failed to read error body: {ex.Message}>";
        }
    }

    private static async Task EnsureServerRunningAsync(string modelFileName, CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelFileName == modelFileName)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelFileName == modelFileName)
            {
                return;
            }

            // Different model selected, or server not running — (re)start with the requested model.
            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetExecutableFileName();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException("Qwen3 TTS server executable not found.", exe);
            }

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = GetSetFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(GetSetModelsFolder());
            psi.ArgumentList.Add("--model-name");
            psi.ArgumentList.Add(modelFileName);
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start qwen3-tts-server");

            var stderrBuffer = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (stderrBuffer) stderrBuffer.AppendLine(e.Data);
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (stderrBuffer) stderrBuffer.AppendLine(e.Data);
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelFileName = modelFileName;
            HookProcessExitOnce();

            var deadline = DateTime.UtcNow.AddSeconds(120);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotStderr(stderrBuffer);
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelFileName = null;
                    throw new InvalidOperationException(
                        $"qwen3-tts-server exited during startup (code {process.ExitCode}). Output: {tail}");
                }
                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(1), ct))
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
            }

            var lastOutput = SnapshotStderr(stderrBuffer);
            StopServerInternal();
            throw new TimeoutException(
                $"qwen3-tts-server did not report healthy within 120s. Last output: {lastOutput}");
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static string SnapshotStderr(StringBuilder buffer)
    {
        lock (buffer)
        {
            var s = buffer.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            using var resp = await HttpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static int FindFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void HookProcessExitOnce()
    {
        if (_processExitHooked) return;
        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    public static void StopServer()
    {
        ServerLock.Wait();
        try
        {
            StopServerInternal();
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelFileName = null;
        if (p == null) return;
        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(2000);
            }
        }
        catch
        {
            // best effort
        }
        finally
        {
            p.Dispose();
        }
    }

    private static string GetUniqueDestinationFileName(string folder, string baseName)
    {
        var candidate = Path.Combine(folder, baseName + ".wav");
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var number = 1;
        do
        {
            candidate = Path.Combine(folder, $"{baseName}_{number}.wav");
            number++;
        } while (File.Exists(candidate));

        return candidate;
    }

    public bool ImportVoice(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = GetUniqueDestinationFileName(voicesFolder, baseName);

        if (Path.GetExtension(fileName).Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(fileName, destinationFileName, overwrite: false);
            return true;
        }

        var process = FfmpegGenerator.ConvertFormat(fileName, destinationFileName);
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            _ = process.Start();
        }
        else
        {
            throw new PlatformNotSupportedException("Process.Start() is not supported on this platform.");
        }

        process.WaitForExit();

        return File.Exists(destinationFileName);
    }
}
