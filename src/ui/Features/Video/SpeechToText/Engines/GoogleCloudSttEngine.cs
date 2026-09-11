using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.GoogleCloud;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Online speech-to-text via Google Cloud Speech-to-Text v2 (Chirp), with real
/// word timings. See <see cref="GoogleCloudSttService"/> for the request flow.
/// </summary>
public class GoogleCloudSttEngine : IOnlineSttEngine
{
    public static string StaticName => "Google Cloud Speech-to-Text";
    public string Name => StaticName;
    public string Choice => WhisperChoice.GoogleCloud;
    public string Url => "https://cloud.google.com/speech-to-text/v2/docs";

    public override string ToString() => Name;

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();
    public List<WhisperModel> Models => new();

    public string Extension => string.Empty;
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled() => true;
    public bool CanBeDownloaded() => false;

    public ISttTranscriber? CreateTranscriber(out string? configErrorMessage)
    {
        var settings = GoogleCloudSttSettings.FromConfiguration();

        // No key file is fine when the machine already has Application Default Credentials,
        // which is often the only route available: creating a service account key is
        // commonly blocked by org policy.
        // A configured key file must exist though: the service uses it whenever one is set,
        // so a stale path would otherwise fail at transcription time instead of here.
        var hasKeyFile = !string.IsNullOrWhiteSpace(settings.KeyFile);
        if (hasKeyFile ? !File.Exists(settings.KeyFile) : !GoogleCloudSttService.HasApplicationDefaultCredentials())
        {
            configErrorMessage = Se.Language.General.OnlineSttApiKeyMissing;
            return null;
        }

        configErrorMessage = null;
        return new GoogleCloudSttService(settings);
    }

    public string ProbeUrl => $"https://{GoogleCloudSttService.GetSpeechHost(Se.Settings.Tools.GoogleCloudSttRegion)}/";

    // Batch jobs read from Cloud Storage, so size is no concern - but with word
    // timings enabled Google caps a file at 20 minutes, so split by time instead.
    public long UploadThresholdBytes => long.MaxValue;
    public long ChunkSizeBytes => long.MaxValue;
    public double MaxChunkSeconds => 18 * 60;

    public string GetAndCreateWhisperFolder() => WhisperHelper.GetWhisperFolder(WhisperChoice.GoogleCloud) ?? string.Empty;
    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => new WhisperModel().ModelFolder;
    public string GetExecutable() => string.Empty;
    public bool IsModelInstalled(WhisperModel model) => true;
    public string GetModelForCmdLine(string modelName) => modelName;
    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => string.Empty;

    public Task<string> GetHelpText() => Task.FromResult(
        "Google Cloud Speech-to-Text v2 with word level timings.\n\n" +
        "Setup (Speech-to-Text v2 does not accept API keys):\n" +
        " 1. In the Google Cloud console create a project with billing, and enable the Speech-to-Text API.\n" +
        " 2. Create a service account with the roles 'Cloud Speech Client' and 'Storage Admin'.\n" +
        " 3. Add a JSON key to the service account, download it, and pick it as the key file.\n\n" +
        "If your organisation does not allow service account keys, leave the key file empty and run " +
        "'gcloud auth application-default login' instead. Those credentials rarely name a project, so also set " +
        "GoogleCloudSttProjectId in Settings.json.\n\n" +
        "Long audio must be read from a Cloud Storage bucket, so one named <project>-subtitle-edit-stt is created " +
        "on first use (another name can be set as GoogleCloudSttBucketName in Settings.json). Uploaded audio is " +
        "deleted after each run.\n\n" +
        "Cost: about $0.003 per minute of audio, because dynamic batching is used by default. Google gives no " +
        "latency guarantee for it, so set GoogleCloudSttDynamicBatching to false in Settings.json to transcribe " +
        "at the normal rate of about $0.016 per minute instead.\n\n" +
        "Region: 'us' or 'eu' for chirp_3, or a specific region for other models. " +
        "Model: chirp_3, chirp_2, long, latest_long... " +
        "Language hint: leave empty for automatic detection (Chirp models), or a BCP-47 code such as en-US.");

    public string CommandLineParameter
    {
        get => string.Empty;
        set { }
    }
}
