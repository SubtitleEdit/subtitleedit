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
using System.Text;
using System.Text.Json;
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
    Task<string> AllTalkVoiceSpeak(string text, AllTalkVoice voice, string language);
    Task<bool> AllTalkIsInstalled();
    Task DownloadElevenLabsVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadMurfVoiceList(MemoryStream stream, IProgress<float>? progress, CancellationToken cancellationToken);

    Task<bool> DownloadElevenLabsVoiceSpeak(
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
}

public class TtsDownloadService : ITtsDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_windows_amd64.zip";
    private const string MacPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_macos_x64.tar.gz";

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
        var url = OperatingSystem.IsWindows() ? WindowsPiperUrl : MacPiperUrl;
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

    public async Task<string> AllTalkVoiceSpeak(string inputText, AllTalkVoice voice, string language)
    {
        var multipartContent = new MultipartFormDataContent();
        var text = Utilities.UnbreakLine(inputText);
        multipartContent.Add(new StringContent(Json.EncodeJsonText(text)), "text_input");
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
        var result = await _httpClient.PostAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl.TrimEnd('/') + "/api/tts-generate", multipartContent);
        var bytes = await result.Content.ReadAsByteArrayAsync();
        var resultJson = Encoding.UTF8.GetString(bytes);

        if (!result.IsSuccessStatusCode)
        {
            SeLogger.Error($"All Talk TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode}" + Environment.NewLine + resultJson);
        }

        var jsonParser = new SeJsonParser();
        var allTalkOutput = jsonParser.GetFirstObject(resultJson, "output_file_path");
        return allTalkOutput.Replace("\\\\", "\\");
    }

    public async Task<bool> AllTalkIsInstalled()
    {
        var timeout = Task.Delay(2000); // 2 seconds timeout
        var request = _httpClient.GetAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl);

        await Task.WhenAny(timeout, request); // wait for either timeout or the request

        if (timeout.IsCompleted) // if the timeout ended first, then handle it
        {
            return false;
        }

        return true;
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
        await result.Content.CopyToAsync(ms, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            SeLogger.Error($"ElevenLabs TTS failed calling API address {url} : Status code={result.StatusCode}");
        }
    }

    public async Task DownloadMurfVoiceList(MemoryStream ms, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.murf.ai/v1/speech/voices";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("api-key", Se.Settings.Video.TextToSpeech.MurfApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(ms, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            SeLogger.Error($"Murf TTS failed calling API address {url} : Status code={result.StatusCode} {error}");
        }
    }

    public async Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/voices";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task<bool> DownloadElevenLabsVoiceSpeak(
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

        var language = string.Empty;
        if (model is "eleven_turbo_v2_5")
        {
            language = $", \"language_code\": \"{languageCode}\"";
        }

        var stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability.ToString(CultureInfo.InvariantCulture);
        var similarityBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity.ToString(CultureInfo.InvariantCulture);
        var speed = Se.Settings.Video.TextToSpeech.ElevenLabsSpeed.ToString(CultureInfo.InvariantCulture);
        var styleExaggeration = Se.Settings.Video.TextToSpeech.ElevenLabsStyleeExaggeration.ToString(CultureInfo.InvariantCulture);
        var data = "{ \"text\": \"" + Json.EncodeJsonText(text) + $"\", \"model_id\": \"{model}\"{language}, \"voice_settings\": {{ \"stability\": {stability}, \"similarity_boost\": {similarityBoost}, \"speed\": {speed}, \"style\": {styleExaggeration} }} }}";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("xi-api-key", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(stream, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(stream.ToArray()).Trim();
            SeLogger.Error($"ElevenLabs TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        return true;
    }

    private async Task<bool> DownloadElevenLabsVoiceSpeak3(
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
        var text = Utilities.UnbreakLine(inputText);

        var stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability.ToString(CultureInfo.InvariantCulture);
        var speed = Se.Settings.Video.TextToSpeech.ElevenLabsSpeed.ToString(CultureInfo.InvariantCulture);
    
        var data = "{ \"inputs\": [{ " +
                   "\"text\": \"" + Json.EncodeJsonText(text) + "\", " +
                   "\"voice_id\": \"" + voice.VoiceId + "\", " +
                   "\"language_code\": \"" + languageCode + "\", " +
                   "\"stability\": " + stability + ", " +
                   "\"speed\": " + speed + 
                   " }] }";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("xi-api-key", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(stream, cancellationToken);
    
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(stream.ToArray()).Trim();
            SeLogger.Error($"ElevenLabs TTS v3 failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        return true;
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

        var data = $"<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='{voice.Gender}' name='{voice.ShortName}'>{System.Net.WebUtility.HtmlEncode(text)}</voice></speak>";
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
        var audioResult = await _httpClient.GetAsync(fileUrl, cancellationToken);
        if (!audioResult.IsSuccessStatusCode)
        {
            SeLogger.Error($"Murf TTS failed calling API as base address {fileUrl} : Status code={audioResult.StatusCode}");
            return false;
        }
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
}