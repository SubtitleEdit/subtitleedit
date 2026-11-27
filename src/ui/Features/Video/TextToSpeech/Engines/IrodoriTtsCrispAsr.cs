using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

public sealed class IrodoriTtsCrispAsr : ITtsEngine, IPerLineCloneEngine
{
    internal const string EngineName = "Irodori TTS v3 (CrispASR)";
    internal const string Backend = "irodori-tts";
    internal const string ModelFileName = "irodori-tts-500m-v3-q8_0.gguf";
    internal const string CodecFileName = "dacvae-ja-32dim-f16.gguf";
    internal const long ModelSize = 623279968;
    internal const long CodecSize = 196142560;

    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromMinutes(5) };
    private static readonly CrispAsrTtsServer Server = new($"crispasr ({Backend})", EngineName, Client);
    private static readonly SemaphoreSlim StartupLock = new(1, 1);

    public string Name => EngineName;
    public string Description => "Local Japanese speech with reference voice cloning and emoji style control (48 kHz)";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => false;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => true;
    public bool SupportsPerLineVoiceCloning => true;
    public override string ToString() => Name;

    internal static string ModelFolder => Path.Combine(Se.CrispAsrFolder, "models", "irodori-v3");
    internal static string Executable => ZonosTtsCrispAsr.GetCrispAsrExecutable();
    internal static bool IsRuntimeSupported() =>
        CrispAsrVersion.IsAtLeast(CrispAsrVersion.TryGet(Executable), new Version(0, 8, 55));

    internal static bool AreModelsInstalled() =>
        HasSize(ModelFileName, ModelSize) && HasSize(CodecFileName, CodecSize);

    internal static bool HasSize(string fileName, long size)
    {
        var file = new FileInfo(Path.Combine(ModelFolder, fileName));
        return file.Exists && file.Length == size;
    }

    private static string GetSetFolder() =>
        Directory.CreateDirectory(Path.Combine(Se.TextToSpeechFolder, "IrodoriTtsCrispAsr")).FullName;

    public static string GetSetVoicesFolder() =>
        Directory.CreateDirectory(Path.Combine(GetSetFolder(), "voices")).FullName;

    public Task<bool> IsInstalled(string? region) => Task.FromResult(File.Exists(Executable));
    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());
    public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());
    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> GetVoices(string languageCode)
    {
        var voices = new List<Voice> { new(new IrodoriTtsVoice("Default (Japanese)", string.Empty)) };
        voices.AddRange(Directory.EnumerateFiles(GetSetVoicesFolder(), "*.wav")
            .Where(file => !PerLineReferenceStaging.IsStaged(file))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .Select(file => new Voice(new IrodoriTtsVoice(Path.GetFileNameWithoutExtension(file), file))));
        return Task.FromResult(voices.ToArray());
    }

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) => GetVoices(language);

    public bool IsVoiceInstalled(Voice voice) => voice.EngineVoice is IrodoriTtsVoice reference &&
        (string.IsNullOrEmpty(reference.FilePath) || File.Exists(reference.FilePath));

    public bool ImportVoice(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return false;
        }

        var destination = VoiceCloneImporter.GetUniqueDestinationFileName(GetSetVoicesFolder(), Path.GetFileNameWithoutExtension(fileName));
        try
        {
            using var process = FfmpegGenerator.ConvertToMono24kHzWav(fileName, destination);
            if (process.Start())
            {
                process.WaitForExit();
                if (process.ExitCode == 0 && File.Exists(destination))
                {
                    return true;
                }
            }

            File.Delete(destination);
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Irodori voice import failed.");
        }

        return false;
    }

    public Voice? MakePerLineCloneVoice(string clipFileName, string voiceName) =>
        PerLineReferenceStaging.Stage(clipFileName, GetSetVoicesFolder(), EngineName) is { } staged
            ? new Voice(new IrodoriTtsVoice(voiceName, staged))
            : null;

    public string? GetPerLineReferenceClip(Voice voice) =>
        voice.EngineVoice is IrodoriTtsVoice reference && !string.IsNullOrEmpty(reference.FilePath) ? reference.FilePath : null;

    public void ResetStagedPerLineReferences() => PerLineReferenceStaging.Clear(GetSetVoicesFolder(), EngineName);

    public async Task<TtsResult> Speak(string text, string outputFolder, Voice voice, TtsLanguage? language,
        string? region, string? model, CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not IrodoriTtsVoice reference || !IsVoiceInstalled(voice))
        {
            throw new InvalidOperationException("The selected Irodori voice is unavailable.");
        }

        await EnsureServerAsync(cancellationToken);
        var payload = new Dictionary<string, object> { ["input"] = text, ["response_format"] = "wav", ["language"] = "ja" };
        if (!string.IsNullOrEmpty(reference.FilePath))
        {
            payload["voice"] = Path.GetFileName(reference.FilePath);
            CrispAsrTtsProvenance.AddSpeechAttestations(payload);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Server.BaseUrl}/v1/audio/speech")
        {
            Content = JsonContent.Create(payload),
        };
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await LocalServerHelper.SafeReadErrorAsync(response, cancellationToken);
            throw new InvalidOperationException($"{Name} synthesis failed ({(int)response.StatusCode}): {error}");
        }

        var fileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + ".wav");
        try
        {
            await using var file = File.Create(fileName);
            await response.Content.CopyToAsync(file, cancellationToken);
            return new TtsResult(fileName, text);
        }
        catch
        {
            File.Delete(fileName);
            throw;
        }
    }

    private static async Task EnsureServerAsync(CancellationToken cancellationToken)
    {
        await StartupLock.WaitAsync(cancellationToken);
        try
        {
            if (Server.IsAlive)
            {
                return;
            }

            Server.Stop();
            if (!AreModelsInstalled())
            {
                throw new InvalidOperationException("Download the Irodori model and codec before generating speech.");
            }

            var port = LocalServerHelper.FindFreeLoopbackPort();
            var voices = GetSetVoicesFolder();
            var start = new ProcessStartInfo
            {
                FileName = Executable,
                WorkingDirectory = voices,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var argument in new[]
            {
                "--server", "--host", "127.0.0.1", "--port", port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "--backend", Backend, "-m", Path.Combine(ModelFolder, ModelFileName),
                "--codec-model", Path.Combine(ModelFolder, CodecFileName), "--voice-dir", voices,
                "--punc-model", "none", "-l", "ja",
            })
            {
                start.ArgumentList.Add(argument);
            }

            CrispAsrTtsProvenance.AddServerMarkingArgs(start.ArgumentList, Executable);
            Server.Start(start, port);
            try
            {
                await Server.WaitUntilHealthyAsync(5, cancellationToken);
            }
            catch
            {
                Server.Stop();
                throw;
            }
        }
        finally
        {
            StartupLock.Release();
        }
    }

    public static void StopServer() => Server.Stop();
}
