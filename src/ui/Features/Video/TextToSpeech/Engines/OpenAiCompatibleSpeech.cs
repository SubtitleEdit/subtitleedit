using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Any server implementing OpenAI's <c>POST /v1/audio/speech</c> (JSON in, raw audio out).
/// The provider (OpenAI, OpenRouter or a custom URL) is picked in the "Region" combo, which
/// the TTS window relabels "Provider" for this engine. Each provider keeps its own API key and
/// model, so switching back and forth doesn't lose either.
/// </summary>
public class OpenAiCompatibleSpeech : ITtsEngine
{
    public const string ProviderOpenAi = "OpenAI";
    public const string ProviderOpenRouter = "OpenRouter";
    public const string ProviderCustom = "Custom";

    public const string FormatAuto = "auto";
    public const string FormatMp3 = "mp3";
    public const string FormatPcm = "pcm";

    // Raw pcm has no header; OpenAI and Gemini TTS both return 16-bit mono at 24 kHz.
    private const int DefaultPcmSampleRate = 24000;

    public const string OpenAiUrl = "https://api.openai.com/v1/audio/speech";
    public const string OpenRouterUrl = "https://openrouter.ai/api/v1/audio/speech";

    private const string OpenRouterModelsFileName = "OpenRouterTtsModels.json";

    // tts-1 / tts-1-hd only know the first nine; gpt-4o-mini-tts knows them all.
    private static readonly string[] OpenAiClassicVoices = ["alloy", "ash", "coral", "echo", "fable", "onyx", "nova", "sage", "shimmer"];
    private static readonly string[] OpenAiNewVoices = ["ballad", "verse", "marin", "cedar"];
    private static readonly string[] OpenAiModels = ["gpt-4o-mini-tts", "tts-1", "tts-1-hd"];

    // "provider|model" keys that rejected mp3 while the format was "auto" - asked for pcm
    // straight away from then on, so each line doesn't pay for a failed request first.
    private static readonly ConcurrentDictionary<string, bool> PcmOnlyModels = new(StringComparer.OrdinalIgnoreCase);

    public string Name => "OpenAI-compatible";
    public string Description => "pay/fast/good";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => true;
    public bool HasRegion => true;
    public bool HasModel => true;
    public bool HasKeyFile => false;
    public bool SupportsVoiceCloning => false;
    public bool SupportsPerLineVoiceCloning => false;

    private readonly ITtsDownloadService _ttsDownloadService;

    public OpenAiCompatibleSpeech(ITtsDownloadService ttsDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
    }

    public override string ToString()
    {
        return Name;
    }

    public static string[] Providers => [ProviderOpenAi, ProviderOpenRouter, ProviderCustom];

    public static string ResolveProvider(string? provider)
    {
        var match = Providers.FirstOrDefault(p => string.Equals(p, provider, StringComparison.OrdinalIgnoreCase));
        return match ?? ProviderOpenAi;
    }

    public static string[] ResponseFormats => [FormatAuto, FormatMp3, FormatPcm];

    public static string ResolveResponseFormat(string? format)
    {
        var match = ResponseFormats.FirstOrDefault(f => string.Equals(f, format?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? FormatAuto;
    }

    public static string SavedProvider => ResolveProvider(Se.Settings.Video.TextToSpeech.OpenAiCompatibleProvider);

    public static string GetApiKey(string? provider)
    {
        var s = Se.Settings.Video.TextToSpeech;
        return ResolveProvider(provider) switch
        {
            ProviderOpenRouter => s.OpenRouterTtsApiKey,
            ProviderCustom => s.OpenAiCompatibleCustomApiKey,
            _ => s.OpenAiApiKey,
        } ?? string.Empty;
    }

    public static void SetApiKey(string? provider, string? apiKey)
    {
        var s = Se.Settings.Video.TextToSpeech;
        switch (ResolveProvider(provider))
        {
            case ProviderOpenRouter:
                s.OpenRouterTtsApiKey = apiKey ?? string.Empty;
                break;
            case ProviderCustom:
                s.OpenAiCompatibleCustomApiKey = apiKey ?? string.Empty;
                break;
            default:
                s.OpenAiApiKey = apiKey ?? string.Empty;
                break;
        }
    }

    public static string GetSavedModel(string? provider)
    {
        var s = Se.Settings.Video.TextToSpeech;
        return ResolveProvider(provider) switch
        {
            ProviderOpenRouter => s.OpenRouterTtsModel,
            ProviderCustom => s.OpenAiCompatibleCustomModel,
            _ => s.OpenAiModel,
        } ?? string.Empty;
    }

    public static void SetSavedModel(string? provider, string? model)
    {
        var s = Se.Settings.Video.TextToSpeech;
        switch (ResolveProvider(provider))
        {
            case ProviderOpenRouter:
                s.OpenRouterTtsModel = model ?? string.Empty;
                break;
            case ProviderCustom:
                s.OpenAiCompatibleCustomModel = model ?? string.Empty;
                break;
            default:
                s.OpenAiModel = model ?? string.Empty;
                break;
        }
    }

    public static string GetUrl(string? provider)
    {
        return ResolveProvider(provider) switch
        {
            ProviderOpenRouter => OpenRouterUrl,
            ProviderCustom => Se.Settings.Video.TextToSpeech.OpenAiCompatibleCustomUrl?.Trim() ?? string.Empty,
            _ => OpenAiUrl,
        };
    }

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(IsConfigured(region ?? SavedProvider));
    }

    public static bool IsConfigured(string? provider = null)
    {
        // A local custom server usually needs no key, just a URL.
        var resolved = ResolveProvider(provider ?? SavedProvider);
        return resolved == ProviderCustom
            ? !string.IsNullOrWhiteSpace(GetUrl(resolved))
            : !string.IsNullOrWhiteSpace(GetApiKey(resolved));
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var provider = SavedProvider;
        var model = GetSavedModel(provider);
        var voiceIds = provider switch
        {
            ProviderOpenRouter => GetOpenRouterVoices(model),
            ProviderCustom => SplitList(Se.Settings.Video.TextToSpeech.OpenAiCompatibleCustomVoices),
            _ => GetOpenAiVoices(model),
        };

        // OpenRouter models with no published voice list (e.g. Fish Audio) accept whatever
        // the provider does - fall back to the user's own list.
        if (voiceIds.Count == 0)
        {
            voiceIds = SplitList(Se.Settings.Video.TextToSpeech.OpenAiCompatibleCustomVoices);
        }

        return Task.FromResult(voiceIds
            .Select(id => new Voice(new OpenAiCompatibleVoice(MakeDisplayName(id), id)))
            .ToArray());
    }

    private static List<string> GetOpenAiVoices(string model)
    {
        var voices = new List<string>(OpenAiClassicVoices);
        if (!model.StartsWith("tts-1", StringComparison.OrdinalIgnoreCase))
        {
            voices.AddRange(OpenAiNewVoices);
        }

        return voices;
    }

    private static string MakeDisplayName(string voiceId)
    {
        // "alloy" -> "Alloy"; leave ids that already carry casing/structure alone.
        if (voiceId.Length > 0 && voiceId.All(c => char.IsLower(c)))
        {
            return char.ToUpperInvariant(voiceId[0]) + voiceId[1..];
        }

        return voiceId;
    }

    private static List<string> SplitList(string? list)
    {
        if (string.IsNullOrWhiteSpace(list))
        {
            return [];
        }

        return list
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
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
        // Only OpenRouter has a discoverable model/voice list.
        if (SavedProvider == ProviderOpenRouter)
        {
            var ms = new MemoryStream();
            await _ttsDownloadService.DownloadOpenRouterTtsModelList(ms, cancellationToken);
            var json = ms.ToArray();
            if (ParseOpenRouterModels(System.Text.Encoding.UTF8.GetString(json)).Count > 0)
            {
                await File.WriteAllBytesAsync(Path.Combine(GetSetFolder(), OpenRouterModelsFileName), json, cancellationToken);
            }
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
        if (voice.EngineVoice is not OpenAiCompatibleVoice openAiVoice)
        {
            throw new ArgumentException("Voice is not an OpenAiCompatibleVoice");
        }

        // Callers pass a null region/model when this engine isn't the globally selected one
        // (per-actor cast rows) - use the saved provider/model then.
        var provider = Providers.Contains(region) ? region! : SavedProvider;
        if (string.IsNullOrEmpty(model))
        {
            model = GetSavedModel(provider);
        }

        if (string.IsNullOrEmpty(model))
        {
            model = (await GetModelsForProvider(provider)).FirstOrDefault() ?? string.Empty;
        }

        var url = GetUrl(provider);
        if (string.IsNullOrEmpty(url))
        {
            return new TtsResult { Text = text, FileName = string.Empty, Error = true, ErrorMessage = "No URL set for the custom OpenAI-compatible server" };
        }

        var savedFormat = ResolveResponseFormat(Se.Settings.Video.TextToSpeech.OpenAiCompatibleResponseFormat);
        var pcmOnlyKey = provider + "|" + model;
        var format = savedFormat == FormatAuto
            ? PcmOnlyModels.ContainsKey(pcmOnlyKey) ? FormatPcm : FormatMp3
            : savedFormat;

        var input = Utilities.UnbreakLine(text);
        var (ok, error, contentType, audio) = await Request(format);

        // "Auto": models like Gemini TTS answer mp3 with a 400 - ask again for pcm.
        if (!ok && savedFormat == FormatAuto && format == FormatMp3 && IsMp3NotSupportedError(error))
        {
            Se.WriteToolsLog($"OpenAI-compatible TTS: {model} does not return mp3 - retrying with pcm");
            PcmOnlyModels[pcmOnlyKey] = true;
            format = FormatPcm;
            (ok, error, contentType, audio) = await Request(format);
        }

        if (!ok)
        {
            Se.WriteToolsLog($"OpenAI-compatible TTS: request failed (provider={provider}, model={model}, voice={openAiVoice.VoiceId}, format={format}): {error}", true);
            return new TtsResult { Text = text, FileName = string.Empty, Error = true, ErrorMessage = error };
        }

        var (fileBytes, extension) = ToAudioFile(audio, format, contentType);
        var fileName = Path.Combine(TtsOutputFolder.Resolve(outputFolder, GetSetFolder), Guid.NewGuid() + extension);
        await File.WriteAllBytesAsync(fileName, fileBytes, cancellationToken);
        return new TtsResult { Text = text, FileName = fileName };

        async Task<(bool Ok, string Error, string ContentType, byte[] Audio)> Request(string responseFormat)
        {
            var body = BuildRequestJson(input, model, openAiVoice.VoiceId, responseFormat);
            Se.WriteToolsLog($"OpenAI-compatible TTS: POST {url} (provider={provider}, model={model}, voice={openAiVoice.VoiceId}, format={responseFormat}, textLen={text.Length})");
            var ms = new MemoryStream();
            var (requestOk, requestError, requestContentType) = await _ttsDownloadService.DownloadOpenAiCompatibleSpeak(url, GetApiKey(provider), body, ms, cancellationToken);
            return (requestOk, requestError, requestContentType, ms.ToArray());
        }
    }

    internal static bool IsMp3NotSupportedError(string? error)
    {
        // e.g. HTTP 400: {"error":{"message":"Gemini TTS only supports response_format=\"pcm\". Got \"mp3\"."}}
        return !string.IsNullOrEmpty(error) &&
               error.Contains("response_format", StringComparison.OrdinalIgnoreCase) &&
               error.Contains(FormatPcm, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the bytes to save and their file extension. Raw pcm gets a wav header; audio that
    /// already has a container (a server may ignore response_format) is kept as it is.
    /// </summary>
    internal static (byte[] Data, string Extension) ToAudioFile(byte[] audio, string requestedFormat, string? contentType)
    {
        if (audio.Length >= 12 && audio[0] == 'R' && audio[1] == 'I' && audio[2] == 'F' && audio[3] == 'F')
        {
            return (audio, ".wav");
        }

        var isMp3 = audio.Length >= 3 &&
                    ((audio[0] == 'I' && audio[1] == 'D' && audio[2] == '3') || (audio[0] == 0xFF && (audio[1] & 0xE0) == 0xE0));
        var isPcm = !isMp3 &&
                    (requestedFormat == FormatPcm || (contentType?.Contains(FormatPcm, StringComparison.OrdinalIgnoreCase) ?? false));
        if (!isPcm)
        {
            return (audio, ".mp3");
        }

        var sampleRate = GetContentTypeInt(contentType, "rate", "sample_rate", "samplerate") ?? DefaultPcmSampleRate;
        var channels = GetContentTypeInt(contentType, "channels") ?? 1;
        const int bytesPerSample = 2;
        var sampleCount = audio.Length / (bytesPerSample * channels);
        var dataLength = sampleCount * bytesPerSample * channels;

        using var wav = new MemoryStream(44 + dataLength);
        WaveHeader2.WriteHeader(wav, sampleRate, channels, bytesPerSample * 8, sampleCount);
        wav.Write(audio, 0, dataLength);
        return (wav.ToArray(), ".wav");
    }

    private static int? GetContentTypeInt(string? contentType, params string[] names)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return null;
        }

        // e.g. "audio/pcm; rate=24000; channels=1"
        foreach (var part in contentType.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = part.IndexOf('=');
            if (idx > 0 &&
                names.Contains(part[..idx].Trim(), StringComparer.OrdinalIgnoreCase) &&
                int.TryParse(part[(idx + 1)..].Trim().Trim('"'), out var value) &&
                value > 0)
            {
                return value;
            }
        }

        return null;
    }

    internal static string BuildRequestJson(string text, string model, string voiceId, string responseFormat = FormatMp3)
    {
        var s = Se.Settings.Video.TextToSpeech;
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", model);
            writer.WriteString("input", text);
            writer.WriteString("voice", voiceId);
            writer.WriteString("response_format", responseFormat);

            // Only sent when changed from the default: not every compatible server knows them,
            // and OpenAI rejects "instructions" for tts-1/tts-1-hd.
            if (Math.Abs(s.OpenAiCompatibleSpeed - 1.0) > 0.001 && s.OpenAiCompatibleSpeed > 0)
            {
                writer.WriteNumber("speed", Math.Round(s.OpenAiCompatibleSpeed, 2));
            }

            var instructions = s.OpenAiCompatibleInstructions?.Trim();
            if (!string.IsNullOrEmpty(instructions) && !model.StartsWith("tts-1", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteString("instructions", instructions);
            }

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    public Task<string[]> GetRegions()
    {
        return Task.FromResult(Providers);
    }

    public Task<string[]> GetModels()
    {
        return GetModelsForProvider(SavedProvider);
    }

    private static Task<string[]> GetModelsForProvider(string provider)
    {
        var models = provider switch
        {
            ProviderOpenRouter => LoadOpenRouterModels().Select(m => m.Id).ToList(),
            ProviderCustom => SplitList(Se.Settings.Video.TextToSpeech.OpenAiCompatibleCustomModels),
            _ => OpenAiModels.ToList(),
        };

        return Task.FromResult(models.ToArray());
    }

    public bool ImportVoice(string fileName)
    {
        return false;
    }

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "OpenAiCompatible");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    private static List<string> GetOpenRouterVoices(string model)
    {
        var models = LoadOpenRouterModels();
        var match = models.FirstOrDefault(m => m.Id == model) ?? models.FirstOrDefault();
        return match?.Voices.ToList() ?? [];
    }

    internal record OpenRouterModel(string Id, string Name, string[] Voices);

    private static List<OpenRouterModel> LoadOpenRouterModels()
    {
        var fileName = Path.Combine(GetSetFolder(), OpenRouterModelsFileName);
        try
        {
            if (File.Exists(fileName))
            {
                var cached = ParseOpenRouterModels(File.ReadAllText(fileName));
                if (cached.Count > 0)
                {
                    return cached;
                }
            }

            // Snapshot bundled with SE; "Refresh voices" replaces it with the live list.
            using var stream = AssetLoader.Open(new Uri("avares://SubtitleEdit/Assets/TextToSpeech/" + OpenRouterModelsFileName));
            using var reader = new StreamReader(stream);
            return ParseOpenRouterModels(reader.ReadToEnd());
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "OpenAI-compatible TTS: reading the OpenRouter model list failed");
            return [];
        }
    }

    internal static List<OpenRouterModel> ParseOpenRouterModels(string json)
    {
        var result = new List<OpenRouterModel>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in data.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? id : id;
                var voices = new List<string>();
                if (item.TryGetProperty("supported_voices", out var voicesElement) && voicesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in voicesElement.EnumerateArray())
                    {
                        var voice = v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                        if (!string.IsNullOrEmpty(voice))
                        {
                            voices.Add(voice);
                        }
                    }
                }

                result.Add(new OpenRouterModel(id, name, voices.ToArray()));
            }
        }
        catch (JsonException)
        {
            // Treated as "no models" - callers fall back to the bundled snapshot.
        }

        return result.OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
