using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public class KokoroTtsCpp : ITtsEngine
{
    public string Name => "Kokoro TTS";
    public string Description => "free/fast/multilingual";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;

    public const string TtsModelFileName     = "kokoro-v1.1-zh.onnx";
    public const string VoicesModelFileName  = "voices-v1.1-zh.bin";
    public const string DefaultVoice         = "af_maple";

    // The voices baked into voices-v1.1-zh.bin. Hard-coded so the dropdown is
    // populated immediately without spinning up the server. RefreshVoices()
    // syncs from /v1/speakers if the engine is installed.
    private static readonly string[] BakedVoices =
    {
        "af_maple", "af_sol", "bf_vale",
        "zf_001", "zf_002", "zf_003", "zf_004", "zf_005", "zf_006", "zf_007", "zf_008",
        "zf_017", "zf_018", "zf_019", "zf_021", "zf_022", "zf_023", "zf_024", "zf_026",
        "zf_027", "zf_028", "zf_032", "zf_036", "zf_038", "zf_039", "zf_040", "zf_042",
        "zf_043", "zf_044", "zf_046", "zf_047", "zf_048", "zf_049", "zf_051", "zf_059",
        "zf_060", "zf_067", "zf_070", "zf_071", "zf_072", "zf_073", "zf_074", "zf_075",
        "zf_076", "zf_077", "zf_078", "zf_079", "zf_083", "zf_084", "zf_085", "zf_086",
        "zf_087", "zf_088", "zf_090", "zf_092", "zf_093", "zf_094", "zf_099",
        "zm_009", "zm_010", "zm_011", "zm_012", "zm_013", "zm_014", "zm_015", "zm_016",
        "zm_020", "zm_025", "zm_029", "zm_030", "zm_031", "zm_033", "zm_034", "zm_035",
        "zm_037", "zm_041", "zm_045", "zm_050", "zm_052", "zm_053", "zm_054", "zm_055",
        "zm_056", "zm_057", "zm_058", "zm_061", "zm_062", "zm_063", "zm_064", "zm_065",
        "zm_066", "zm_068", "zm_069", "zm_080", "zm_081", "zm_082", "zm_089", "zm_091",
        "zm_095", "zm_096", "zm_097", "zm_098", "zm_100",
    };

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static bool _processExitHooked;

    private static List<string>? _cachedVoiceNames;

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetExecutableFileName()));
    }

    public static bool AreModelsInstalled()
    {
        var modelsFolder = GetSetModelsFolder();
        return File.Exists(Path.Combine(modelsFolder, TtsModelFileName)) &&
               File.Exists(Path.Combine(modelsFolder, VoicesModelFileName));
    }

    public override string ToString() => Name;

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "KokoroTtsCpp");
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

    public static string GetExecutableFileName()
    {
        return Path.Combine(GetSetFolder(), OperatingSystem.IsWindows() ? "kokoro-tts-server.exe" : "kokoro-tts-server");
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var names = _cachedVoiceNames ?? new List<string>(BakedVoices);
        var result = new List<Voice>(names.Count);
        foreach (var name in names)
        {
            result.Add(new Voice(new KokoroTtsVoice(name)));
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

    // Refresh the voice cache from the running server (if available). Falls back
    // to GetVoices() with the baked list if the server can't be reached.
    public async Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(GetExecutableFileName()) && AreModelsInstalled())
            {
                await EnsureServerRunningAsync(cancellationToken);
                using var resp = await HttpClient.GetAsync($"{ServerBaseUrl}/v1/speakers", cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("speakers", out var arr) && arr.ValueKind == JsonValueKind.Array)
                    {
                        var names = new List<string>(arr.GetArrayLength());
                        foreach (var item in arr.EnumerateArray())
                        {
                            var s = item.GetString();
                            if (!string.IsNullOrEmpty(s)) names.Add(s);
                        }
                        if (names.Count > 0)
                        {
                            _cachedVoiceNames = names;
                        }
                    }
                }
            }
        }
        catch
        {
            // Refresh is best-effort; fall through to GetVoices() with whatever cache we have.
        }

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
        if (voice.EngineVoice is not KokoroTtsVoice kokoroVoice)
        {
            throw new ArgumentException("Voice is not a KokoroTtsVoice");
        }

        await EnsureServerRunningAsync(cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = Utilities.UnbreakLine(text);
        var voiceName = string.IsNullOrEmpty(kokoroVoice.Voice) ? DefaultVoice : kokoroVoice.Voice;

        var body = JsonSerializer.Serialize(new { text = inputText, voice = voiceName });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync($"{ServerBaseUrl}/v1/synthesize", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await SafeReadErrorAsync(response, cancellationToken);
            Se.LogError($"Kokoro TTS server error {(int)response.StatusCode} {response.StatusCode} - "
                + $"Voice: {voiceName}, Text: {text}, Body: {errorBody}");
            throw new InvalidOperationException(
                $"Kokoro TTS synthesis failed ({(int)response.StatusCode}): {errorBody}");
        }

        await using (var fileStream = File.Create(outputFileName))
        await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
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

    private static async Task EnsureServerRunningAsync(CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false } && _serverPort != 0)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0)
            {
                return;
            }

            var exe = GetExecutableFileName();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException("Kokoro TTS server executable not found.", exe);
            }

            var modelsFolder = GetSetModelsFolder();
            var modelPath  = Path.Combine(modelsFolder, TtsModelFileName);
            var voicesPath = Path.Combine(modelsFolder, VoicesModelFileName);
            if (!File.Exists(modelPath) || !File.Exists(voicesPath))
            {
                throw new FileNotFoundException("Kokoro TTS model or voices file missing.",
                    File.Exists(modelPath) ? voicesPath : modelPath);
            }

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                // Working directory must be GetSetFolder() so the server can find dict/vocab.txt
                // (relative path baked into the binary's default --vocab arg).
                WorkingDirectory = GetSetFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(modelPath);
            psi.ArgumentList.Add("-V");
            psi.ArgumentList.Add(voicesPath);
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start kokoro-tts-server");

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
            HookProcessExitOnce();

            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotStderr(stderrBuffer);
                    _serverProcess = null;
                    _serverPort = 0;
                    throw new InvalidOperationException(
                        $"kokoro-tts-server exited during startup (code {process.ExitCode}). Output: {tail}");
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
                $"kokoro-tts-server did not report healthy within 60s. Last output: {lastOutput}");
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

    public bool ImportVoice(string fileName)
    {
        // Kokoro uses fixed pre-trained voice style vectors; voice cloning is not supported.
        return false;
    }
}
