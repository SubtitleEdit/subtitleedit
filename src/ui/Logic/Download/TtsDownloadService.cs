using Google.Cloud.TextToSpeech.V1;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ITtsDownloadService
{
    Task DownloadPiper(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiper(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiperModel(string destinationFileName, PiperVoice voice, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiperVoice(string modelUrl, MemoryStream downloadStream, Progress<float> downloadProgress, CancellationToken token);
    Task DownloadPiperVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadAllTalkVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task<string> AllTalkVoiceSpeak(string text, AllTalkVoice voice, string language, CancellationToken cancellationToken);
    Task<bool> AllTalkIsInstalled();
    Task DownloadElevenLabsVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadMurfVoiceList(MemoryStream stream, IProgress<float>? progress, CancellationToken cancellationToken);

    Task<(bool Ok, string Error)> DownloadElevenLabsVoiceSpeak(
        string inputText,
        ElevenLabVoice voice,
        string model,
        string apiKey,
        string languageCode,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken);

    Task<bool> DownloadAzureVoiceSpeak(
        string inputText,
        AzureVoice voice,
        string model,
        string apiKey,
        string languageCode,
        string region,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken);

    Task<bool> DownloadMurfSpeak(
        string text,
        MurfVoice murfVoice,
        string? overrideStyle,
        string murfApiKey,
        MemoryStream ms,
        CancellationToken cancellationToken);

    Task<bool> DownloadGoogleVoiceList(string googleKeyFile, MemoryStream ms, CancellationToken cancellationToken);
    Task<bool> DownloadGoogleVoiceSpeak(string text, GoogleVoice googleVoice, string model, string googleKeyFile, MemoryStream ms, CancellationToken cancellationToken);

    Task DownloadMistralSpeechVoiceList(MemoryStream stream, IProgress<float>? progress, CancellationToken cancellationToken);

    Task<bool> DownloadMistralSpeechSpeak(
        string inputText,
        MistralVoice voice,
        string model,
        string apiKey,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken);
}

public class TtsDownloadService : ITtsDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_windows_amd64.zip";
    private const string MacPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_macos_x64.tar.gz";
    private const string LinuxPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_linux_x86_64.tar.gz";
    private const string LinuxPiperArmUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_linux_aarch64.tar.gz";

    public TtsDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadPiper(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = OperatingSystem.IsWindows() ? WindowsPiperUrl : MacPiperUrl;
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadPiper(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = WindowsPiperUrl;
        if (OperatingSystem.IsLinux())
        {
            url = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxPiperArmUrl : LinuxPiperUrl;
        }
        else if (OperatingSystem.IsMacOS())
        {
            url = MacPiperUrl;
        }

        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadPiperModel(string destinationFileName, PiperVoice voice, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, voice.Model, destinationFileName, progress, cancellationToken);
        await DownloadHelper.DownloadFileAsync(_httpClient, voice.Config, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadPiperVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://huggingface.co/rhasspy/piper-voices/resolve/main/voices.json?download=true";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadPiperVoice(string url, MemoryStream stream, Progress<float> progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadAllTalkVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = Se.Settings.Video.TextToSpeech.AllTalkUrl.TrimEnd('/') + "/api/voices";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task<string> AllTalkVoiceSpeak(string inputText, AllTalkVoice voice, string language, CancellationToken cancellationToken)
    {
        using var multipartContent = new MultipartFormDataContent();
        var text = Utilities.UnbreakLine(inputText);
        // Plain multipart form field - no JSON escaping: Json.EncodeJsonText turned quotes and
        // backslashes into \" and \\ that AllTalk then spoke/mangled.
        multipartContent.Add(new StringContent(text), "text_input");
        multipartContent.Add(new StringContent("standard"), "text_filtering");
        multipartContent.Add(new StringContent(voice.Voice), "character_voice_gen");
        multipartContent.Add(new StringContent("false"), "narrator_enabled");
        multipartContent.Add(new StringContent(voice.Voice), "narrator_voice_gen");
        multipartContent.Add(new StringContent("character"), "text_not_inside");
        multipartContent.Add(new StringContent(language), "language");
        multipartContent.Add(new StringContent("output"), "output_file_name");
        multipartContent.Add(new StringContent("false"), "output_file_timestamp");
        multipartContent.Add(new StringContent("false"), "autoplay");
        multipartContent.Add(new StringContent("1.0"), "autoplay_volume");

        HttpResponseMessage result;
        try
        {
            // Honor the caller's token - a bulk-generate Cancel used to leave the AllTalk
            // request running to completion.
            result = await _httpClient.PostAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl.TrimEnd('/') + "/api/tts-generate", multipartContent, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            SeLogger.Error(ex, "AllTalk TTS server connection failed.");
            throw new HttpRequestException("AllTalk TTS server is not reachable. Please check that the server is running.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            SeLogger.Error(ex, "AllTalk TTS server request timed out.");
            throw new HttpRequestException("AllTalk TTS server request timed out. Please check that the server is running.", ex);
        }

        using (result)
        {
            if (!result.IsSuccessStatusCode)
            {
                var errorBody = await result.Content.ReadAsStringAsync(cancellationToken);
                SeLogger.Error($"AllTalk TTS failed calling API at {_httpClient.BaseAddress}: Status code={result.StatusCode}" + Environment.NewLine + errorBody);
                throw new HttpRequestException($"AllTalk TTS server returned error {(int)result.StatusCode} ({result.StatusCode}).");
            }

            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            var resultJson = Encoding.UTF8.GetString(bytes);

            var jsonParser = new SeJsonParser();
            var allTalkOutput = jsonParser.GetFirstObject(resultJson, "output_file_path");
            return allTalkOutput.Replace("\\\\", "\\");
        }
    }

    public async Task<bool> AllTalkIsInstalled()
    {
        // The old Task.WhenAny check reported "installed" whenever the request completed before
        // the 2 s timeout - including completing by *faulting*, so a connection refused (server
        // not running, ~instant) counted as installed and generation failed later instead.
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await _httpClient.GetAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false; // connection refused, timeout, bad URL - the server is not reachable
        }
    }

    public async Task DownloadElevenLabsVoiceList(Stream ms, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/voices";
        
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");

        if (!string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.ElevenLabsApiKey))
        {
            requestMessage.Headers.TryAddWithoutValidation("xi-api-key", Se.Settings.Video.TextToSpeech.ElevenLabsApiKey);
        }

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);

        // Throw on failure instead of copying the error body into the stream: the caller writes
        // the stream over its cached voice-list JSON, and since the bundled fallback only kicks
        // in when the file is *missing*, one failed refresh (expired key, network error) used to
        // permanently empty the voice list.
        if (!result.IsSuccessStatusCode)
        {
            var error = (await result.Content.ReadAsStringAsync(cancellationToken)).Trim();
            SeLogger.Error($"ElevenLabs TTS failed calling API address {url} : Status code={result.StatusCode} {TruncateForLog(error)}");
            throw new HttpRequestException($"ElevenLabs voice list request failed: HTTP {(int)result.StatusCode} {result.StatusCode}");
        }

        await result.Content.CopyToAsync(ms, cancellationToken);
    }

    public async Task DownloadMurfVoiceList(MemoryStream ms, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.murf.ai/v1/speech/voices";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("api-key", Se.Settings.Video.TextToSpeech.MurfApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);

        // See DownloadElevenLabsVoiceList: an error body must not reach the cached voice list.
        if (!result.IsSuccessStatusCode)
        {
            var error = (await result.Content.ReadAsStringAsync(cancellationToken)).Trim();
            SeLogger.Error($"Murf TTS failed calling API address {url} : Status code={result.StatusCode} {TruncateForLog(error)}");
            throw new HttpRequestException($"Murf voice list request failed: HTTP {(int)result.StatusCode} {result.StatusCode}");
        }

        await result.Content.CopyToAsync(ms, cancellationToken);
    }

    public async Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        // Azure's official voice-list endpoint (the previous URL pointed at the ElevenLabs API,
        // whose response Azure's parser cannot read). Requires the user's region + subscription
        // key; the response is a JSON array with DisplayName/ShortName/Gender/Locale fields -
        // the exact shape AzureSpeech.Map parses. Throws on failure so a refresh cannot
        // overwrite the cached voice list with an error body.
        var region = Se.Settings.Video.TextToSpeech.AzureRegion;
        if (string.IsNullOrWhiteSpace(region))
        {
            throw new InvalidOperationException("Azure region is not set - enter it in the TTS engine settings before refreshing voices.");
        }

        var url = $"https://{region.Trim()}.tts.speech.microsoft.com/cognitiveservices/voices/list";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", Se.Settings.Video.TextToSpeech.AzureApiKey.Trim());
        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        result.EnsureSuccessStatusCode();
        await result.Content.CopyToAsync(stream, cancellationToken);
    }

    public async Task<(bool Ok, string Error)> DownloadElevenLabsVoiceSpeak(
        string inputText,
        ElevenLabVoice voice,
        string model,
        string apiKey,
        string languageCode,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        if (model == "eleven_v3")
        {
            return await DownloadElevenLabsVoiceSpeak3(inputText, voice, model, apiKey, languageCode, stream, progress, cancellationToken);
        }

        var url = "https://api.elevenlabs.io/v1/text-to-speech/" + voice.VoiceId;
        var text = Utilities.UnbreakLine(inputText);

        // Only send language_code when the user actually picked a language: cross-engine cast
        // rows pass none, and forcing "en" made ElevenLabs apply English normalization and
        // pronunciation hints to non-English lines.
        var language = string.Empty;
        if (model is "eleven_turbo_v2_5" && !string.IsNullOrEmpty(languageCode))
        {
            language = $", \"language_code\": \"{languageCode}\"";
        }

        var stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability.ToString(CultureInfo.InvariantCulture);
        var similarityBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity.ToString(CultureInfo.InvariantCulture);
        var speed = Se.Settings.Video.TextToSpeech.ElevenLabsSpeed.ToString(CultureInfo.InvariantCulture);
        var styleExaggeration = Se.Settings.Video.TextToSpeech.ElevenLabsStyleeExaggeration.ToString(CultureInfo.InvariantCulture);

        // ElevenLabs use_speaker_boost is a boolean; the UI exposes it as a 0-100 slider, so any
        // non-zero value enables it. Previously this setting was collected and saved but never
        // placed in the request body, so toggling it had no effect on the generated audio.
        var useSpeakerBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSpeakerBoost > 0 ? "true" : "false";

        // ElevenLabs only parses SSML when enable_ssml_parsing is true - it defaults to false,
        // so <break time="1.5s"/> tags were sent but ignored/spoken as plain text even with
        // normalization off. The flag is only documented on the websocket endpoints but the
        // HTTP endpoint accepts it as a body field too. Set it only when the text actually
        // contains a break tag, so ordinary subtitle text with stray '<' or formatting tags
        // is never run through the SSML parser (#12093).
        var ssmlParsing = text.Contains("<break", StringComparison.OrdinalIgnoreCase)
            ? ", \"enable_ssml_parsing\": true"
            : string.Empty;

        // Disable ElevenLabs text normalization so the user's pause cues survive: SSML
        // <break time="1.5s"/> tags and punctuation pauses (",,," "..." "---") are otherwise
        // collapsed/rewritten by the "auto" normalizer on multilingual models. All standard
        // (non-v3) models honor break tags; v3 is handled on the text-to-dialogue path (#12093).
        var data = "{ \"text\": \"" + Json.EncodeJsonText(text) + $"\", \"model_id\": \"{model}\"{language}, \"voice_settings\": {{ \"stability\": {stability}, \"similarity_boost\": {similarityBoost}, \"speed\": {speed}, \"style\": {styleExaggeration}, \"use_speaker_boost\": {useSpeakerBoost} }}, \"apply_text_normalization\": \"off\"{ssmlParsing} }}";

        return await SendElevenLabsSpeakRequestAsync(url, data, apiKey, acceptAudioMpeg: true, stream, "ElevenLabs TTS", cancellationToken);
    }

    /// <summary>
    /// Posts an ElevenLabs speak request and copies the audio response into <paramref name="stream"/>.
    /// ElevenLabs enforces per-plan concurrency/request limits, and bulk generation fires one request
    /// per subtitle line - so HTTP 429 (and transient 5xx) responses are retried with a backoff that
    /// honors the Retry-After header instead of failing the segment outright (#12093). Returns a
    /// human-readable error for the UI when all attempts fail.
    /// </summary>
    private async Task<(bool Ok, string Error)> SendElevenLabsSpeakRequestAsync(
        string url,
        string jsonData,
        string apiKey,
        bool acceptAudioMpeg,
        MemoryStream stream,
        string logContext,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 4;
        for (var attempt = 1; ; attempt++)
        {
            // A new request message per attempt - HttpRequestMessage cannot be re-sent.
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = new StringContent(jsonData, Encoding.UTF8);
            requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            if (acceptAudioMpeg)
            {
                requestMessage.Headers.TryAddWithoutValidation("Accept", "audio/mpeg");
            }
            requestMessage.Headers.TryAddWithoutValidation("xi-api-key", apiKey.Trim());

            using var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                stream.SetLength(0);
                await result.Content.CopyToAsync(stream, cancellationToken);
                return (true, string.Empty);
            }

            var errorBody = TruncateForLog((await result.Content.ReadAsStringAsync(cancellationToken)).Trim());
            var retryable = result.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)result.StatusCode >= 500;
            if (retryable && attempt < maxAttempts)
            {
                var delay = GetRetryDelay(result, attempt);
                SeLogger.Error($"{logContext}: HTTP {(int)result.StatusCode} {result.StatusCode} - retrying in {delay.TotalSeconds:0.#}s (attempt {attempt} of {maxAttempts}): {errorBody}");
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            var error = result.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                ? $"ElevenLabs rate limit (HTTP 429) still hit after {maxAttempts} attempts with backoff - the plan's concurrency/request limit is likely exceeded. {errorBody}"
                : $"HTTP {(int)result.StatusCode} {result.StatusCode}: {errorBody}";
            SeLogger.Error($"{logContext} failed calling {url}: {error}" + Environment.NewLine + "Data=" + jsonData);
            return (false, error);
        }
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        // Prefer the server's Retry-After (delta or absolute date); fall back to exponential
        // backoff (2 s, 4 s, 8 s). Capped so a bogus header cannot stall generation for minutes.
        var maxDelay = TimeSpan.FromSeconds(30);

        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta <= maxDelay ? delta : maxDelay;
        }

        if (retryAfter?.Date is { } date)
        {
            var untilDate = date - DateTimeOffset.UtcNow;
            if (untilDate > TimeSpan.Zero)
            {
                return untilDate <= maxDelay ? untilDate : maxDelay;
            }
        }

        return TimeSpan.FromSeconds(Math.Pow(2, attempt));
    }

    private static string TruncateForLog(string text)
    {
        const int maxLength = 300;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }

    private async Task<(bool Ok, string Error)> DownloadElevenLabsVoiceSpeak3(
        string inputText, 
        ElevenLabVoice voice, 
        string model, 
        string apiKey, 
        string languageCode, 
        MemoryStream stream, 
        IProgress<float>? progress, 
        CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/text-to-dialogue";

        // Eleven v3 does not support SSML <break> tags - it uses expressive audio tags instead.
        // Translate any <break time="Xs"/> the user wrote into the nearest [short pause]/[pause]/
        // [long pause] so their pacing still applies on v3. Punctuation pauses ("..." etc.) pass
        // through unchanged and are honored by v3 directly (#12093).
        var text = ConvertBreakTagsToV3AudioTags(Utilities.UnbreakLine(inputText));

        var stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability.ToString(CultureInfo.InvariantCulture);

        // Per the text-to-dialogue schema each "inputs" item accepts ONLY text + voice_id;
        // language_code is a top-level field and stability goes in the top-level "settings"
        // object. They used to be placed inside inputs[0], where the API silently ignored
        // them - every v3 generation ran with default stability and no language enforcement.
        // The endpoint has no speed parameter at all, so the speed setting is not sent.
        // Normalization is turned off for the same reason as the v2 path: the "auto"
        // normalizer can rewrite the user's punctuation pause cues ("..." etc.) (#12093).
        var languageFragment = string.IsNullOrEmpty(languageCode)
            ? string.Empty
            : ", \"language_code\": \"" + languageCode + "\"";
        var data = "{ \"inputs\": [{ " +
                   "\"text\": \"" + Json.EncodeJsonText(text) + "\", " +
                   "\"voice_id\": \"" + voice.VoiceId + "\"" +
                   " }]" +
                   languageFragment +
                   ", \"settings\": { \"stability\": " + stability + " }" +
                   ", \"apply_text_normalization\": \"off\" }";

        return await SendElevenLabsSpeakRequestAsync(url, data, apiKey, acceptAudioMpeg: false, stream, "ElevenLabs TTS v3", cancellationToken);
    }

    // Matches SSML break tags like <break time="1.5s"/>, <break time="500ms" />, <break time='2s'>.
    private static readonly Regex BreakTagRegex = new(
        "<break\\s+time\\s*=\\s*[\"']?(?<value>\\d+(?:\\.\\d+)?)\\s*(?<unit>ms|s)[\"']?\\s*/?>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Eleven v3 ignores SSML <break> tags, so translate each one into the closest v3 audio
    /// pause tag ([short pause] &lt; 0.75 s, [pause] &lt;= 1.5 s, [long pause] otherwise). Text
    /// without break tags is returned unchanged. (#12093)
    /// </summary>
    internal static string ConvertBreakTagsToV3AudioTags(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf("<break", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return text;
        }

        return BreakTagRegex.Replace(text, match =>
        {
            var seconds = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
            if (string.Equals(match.Groups["unit"].Value, "ms", StringComparison.OrdinalIgnoreCase))
            {
                seconds /= 1000.0;
            }

            if (seconds < 0.75)
            {
                return "[short pause]";
            }

            return seconds <= 1.5 ? "[pause]" : "[long pause]";
        });
    }

    public async Task<bool> DownloadAzureVoiceSpeak(
        string inputText,
        AzureVoice voice,
        string model,
        string apiKey,
        string languageCode,
        string region,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        var url = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        var text = Utilities.UnbreakLine(inputText);

        // Use the voice's own locale in the SSML instead of hardcoded en-US - the voice name
        // usually wins, but a mismatched xml:lang can affect pronunciation of locale-ambiguous
        // text (numbers, dates) for non-English voices.
        var locale = string.IsNullOrWhiteSpace(voice.Locale) ? "en-US" : voice.Locale;
        var data = $"<speak version='1.0' xml:lang='{locale}'><voice xml:lang='{locale}' xml:gender='{voice.Gender}' name='{voice.ShortName}'>{System.Net.WebUtility.HtmlEncode(text)}</voice></speak>";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);

        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "ssml+xml");
        requestMessage.Headers.TryAddWithoutValidation("accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
        requestMessage.Headers.TryAddWithoutValidation("User-Agent", "SubtitleEdit");
        requestMessage.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(stream, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(stream.ToArray()).Trim();
            SeLogger.Error($"Azure TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        return true;
    }

    public async Task<bool> DownloadMurfSpeak(
        string text,
        MurfVoice voice,
        string? overrideStyle,
        string murfApiKey,
        MemoryStream ms,
        CancellationToken cancellationToken)
    {
        var url = "https://api.murf.ai/v1/speech/generate";

        if (string.IsNullOrEmpty(overrideStyle))
        {
            overrideStyle = "Conversational";
        }

        var body = new
        {
            voiceId = voice.VoiceId,
            style = voice.AvailableStyles.Contains(overrideStyle)
                ? overrideStyle
                : voice.AvailableStyles.FirstOrDefault(),
            text,
            rate = 0,
            pitch = 0,
            sampleRate = 48000,
            format = "MP3",
            channelType = "MONO",
            pronunciationDictionary = new { },
            encodeAsBase64 = false,
            modelVersion = "GEN2"
        };
        var json = JsonSerializer.Serialize(body);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

        requestMessage.Content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("api-key", murfApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(ms, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            SeLogger.Error($"Murf TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + body);
            return false;
        }

        var parser = new SeJsonParser();
        var fileUrl = parser.GetFirstObject(Encoding.UTF8.GetString(ms.ToArray()), "audioFile");

        // A 200 response without an audioFile URL (schema change, quota message) would make
        // GetAsync throw on a null/empty URI and abort the whole generation run instead of
        // failing this one segment.
        if (string.IsNullOrWhiteSpace(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out _))
        {
            SeLogger.Error($"Murf TTS returned no usable audioFile URL (\"{fileUrl}\") - response: {TruncateForLog(Encoding.UTF8.GetString(ms.ToArray()).Trim())}");
            return false;
        }

        var audioResult = await _httpClient.GetAsync(fileUrl, cancellationToken);
        if (!audioResult.IsSuccessStatusCode)
        {
            SeLogger.Error($"Murf TTS failed calling API as base address {fileUrl} : Status code={audioResult.StatusCode}");
            return false;
        }

        // The stream still holds the generate-call's JSON response (parsed above for the audio
        // URL) - without resetting, the MP3 would be appended after it and every Murf segment
        // file would start with JSON garbage.
        ms.SetLength(0);
        await audioResult.Content.CopyToAsync(ms, cancellationToken);

        return true;
    }

    /// <summary>
    /// Downloads a list of available Google Text-to-Speech voices in raw JSON format.
    /// Authenticates using the service account JSON key file.
    /// </summary>
    /// <param name="googleJsonKeyFileName">The full path to the Google service account JSON key file.</param>
    /// <param name="ms">The MemoryStream to write the JSON content to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the voice list is successfully retrieved and written to the stream, false otherwise.</returns>
    public async Task<bool> DownloadGoogleVoiceList(string googleJsonKeyFileName, MemoryStream ms, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(googleJsonKeyFileName) || !File.Exists(googleJsonKeyFileName))
        {
            SeLogger.Error("Google Service Account JSON key file path is invalid or file does not exist.");
            return false;
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleJsonKeyFileName);

        TextToSpeechClient client;
        try
        {
            client = await TextToSpeechClient.CreateAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            SeLogger.Error($"Failed to create Google Text-to-Speech client for voice list. Ensure JSON key file is valid and has 'Cloud Text-to-Speech API User' permissions: {ex.Message}");
            return false;
        }

        try
        {
            var response = await client.ListVoicesAsync(new ListVoicesRequest(), cancellationToken: cancellationToken);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            string jsonContent = JsonSerializer.Serialize(response, jsonSerializerOptions);

            var buffer = Encoding.UTF8.GetBytes(jsonContent);
            await ms.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            ms.Position = 0; // Reset position for reading from the beginning of the stream

            return true;
        }
        catch (Google.GoogleApiException gex)
        {
            SeLogger.Error($"Google TTS API error getting voice list: {gex.HttpStatusCode} - {gex.Message} (Details: {gex.Error?.Message})");
            return false;
        }
        catch (Exception ex)
        {
            SeLogger.Error($"An unexpected exception occurred while getting Google TTS voice list: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Downloads speech audio from Google Text-to-Speech API using a service account JSON key file for authentication.
    /// </summary>
    /// <param name="inputText">The text to synthesize.</param>
    /// <param name="googleVoice">Object containing LanguageCode and Name for the voice.</param>
    /// <param name="model">Currently unused, but kept for signature compatibility.</param>
    /// <param name="googleJsonKeyFileName">The full path to the Google service account JSON key file.</param>
    /// <param name="ms">The MemoryStream to write the audio content to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if speech synthesis is successful, false otherwise.</returns>
    public async Task<bool> DownloadGoogleVoiceSpeak(string inputText, GoogleVoice googleVoice, string model, string googleJsonKeyFileName, MemoryStream ms, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(googleJsonKeyFileName) || !File.Exists(googleJsonKeyFileName))
        {
            SeLogger.Error("Google Service Account JSON key file path is invalid or file does not exist.");
            return false;
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleJsonKeyFileName);

        TextToSpeechClient client;
        try
        {
            client = await TextToSpeechClient.CreateAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            SeLogger.Error($"Failed to create Google Text-to-Speech client. Ensure the JSON key file is valid and has 'Cloud Text-to-Speech API User' permissions: {ex.Message}");
            return false;
        }

        try
        {
            var input = new SynthesisInput
            {
                Text = inputText
            };

            var voice = new VoiceSelectionParams
            {
                LanguageCode = googleVoice.LanguageCode,
                Name = googleVoice.Name
            };

            // You can specify more audio settings here, e.g., Pitch, SpeakingRate
            AudioConfig audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                // Uncomment and use if your GoogleVoice class has SpeakingRate:
                // SpeakingRate = googleVoice.SpeakingRate ?? 1.0f // Ensure this is a float/double
            };

            var response = await client.SynthesizeSpeechAsync(input, voice, audioConfig, cancellationToken: cancellationToken);

            if (response?.AudioContent?.Length > 0)
            {
                await ms.WriteAsync(response.AudioContent.ToByteArray(), cancellationToken);
                ms.Position = 0;
                return true;
            }
            else
            {
                SeLogger.Error("Google TTS: Audio content is null or empty in the response.");
                return false;
            }
        }
        catch (Google.GoogleApiException gex)
        {
            SeLogger.Error($"Google TTS API error: {gex.HttpStatusCode} - {gex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            SeLogger.Error($"An unexpected exception occurred during Google TTS synthesis: {ex}");
            return false;
        }
    }

    private class GoogleTtsResponse
    {
        public string? audioContent { get; set; }
    }

    public async Task DownloadMistralSpeechVoiceList(MemoryStream ms, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.mistral.ai/v1/audio/voices?limit=10000";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Se.Settings.Video.TextToSpeech.MistralApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);

        // Throw instead of returning with an empty stream: the caller writes the stream over its
        // cached voice-list JSON, so a silently failed refresh permanently emptied the voice list
        // (the bundled fallback only restores when the file is missing).
        if (!result.IsSuccessStatusCode)
        {
            SeLogger.Error($"Mistral TTS failed calling API address {url} : Status code={result.StatusCode}");
            throw new HttpRequestException($"Mistral voice list request failed: HTTP {(int)result.StatusCode} {result.StatusCode}");
        }

        await result.Content.CopyToAsync(ms, cancellationToken);
    }

    public async Task<bool> DownloadMistralSpeechSpeak(
        string inputText,
        MistralVoice voice,
        string model,
        string apiKey,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        var url = "https://api.mistral.ai/v1/audio/speech";
        var text = Utilities.UnbreakLine(inputText);

        var data = "{ \"model\": \"" + Json.EncodeJsonText(model) +
                   "\", \"input\": \"" + Json.EncodeJsonText(text) +
                   "\", \"voice_id\": \"" + Json.EncodeJsonText(voice.VoiceId) +
                   "\", \"response_format\": \"mp3\" }";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseBytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(responseBytes).Trim();
            SeLogger.Error($"Mistral TTS failed calling API at {url} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        // Response is JSON with base64-encoded audio in "audio_data" field
        var responseJson = Encoding.UTF8.GetString(responseBytes);
        var parser = new SeJsonParser();
        var audioData = parser.GetFirstObject(responseJson, "audio_data");
        if (string.IsNullOrEmpty(audioData))
        {
            SeLogger.Error("Mistral TTS: No audio_data in response" + Environment.NewLine + responseJson);
            return false;
        }

        var audioBytes = Convert.FromBase64String(audioData);
        await stream.WriteAsync(audioBytes, cancellationToken);
        return true;
    }
}