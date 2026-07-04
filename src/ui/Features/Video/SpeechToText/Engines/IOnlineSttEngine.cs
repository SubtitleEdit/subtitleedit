using System;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// A speech-to-text transport that uploads audio to a cloud transcription
/// service and returns segments/words in the shared
/// <see cref="OpenAiCompatibleSttResponse"/> shape. Implemented per provider:
/// OpenAI-compatible multipart (<see cref="OpenAiSttService"/>), OpenRouter's
/// base64-JSON body, and DashScope's multimodal-generation request. Sharing the
/// response shape lets the view model drive them all through one chunk/ingest
/// pipeline.
/// </summary>
public interface ISttTranscriber
{
    Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        string audioFilePath,
        string? language,
        IProgress<string>? progress,
        IProgress<OpenAiCompatibleSegment>? segmentProgress,
        CancellationToken cancellationToken);
}

/// <summary>
/// Marker for cloud STT engines that transcribe by calling an online API rather
/// than running a local executable. The SpeechToText view model routes every
/// engine implementing this through the shared upload/chunk/ingest pipeline
/// (see ProcessOnlineSttTranscription), so provider-specific differences are
/// confined to the returned <see cref="ISttTranscriber"/>.
/// </summary>
public interface IOnlineSttEngine : ISpeechToTextEngine
{
    /// <summary>
    /// Build a transcriber from the current settings. Returns null and sets
    /// <paramref name="configErrorMessage"/> to a user-facing message when the
    /// engine is not configured (e.g. missing API key or endpoint).
    /// </summary>
    ISttTranscriber? CreateTranscriber(out string? configErrorMessage);

    /// <summary>
    /// URL whose host is pre-flight probed for reachability before uploading.
    /// </summary>
    string ProbeUrl { get; }

    /// <summary>
    /// Upload size (bytes) above which the audio is split into chunks to stay
    /// under the provider's per-request cap.
    /// </summary>
    long UploadThresholdBytes { get; }

    /// <summary>
    /// Target size (bytes) for each chunk when splitting.
    /// </summary>
    long ChunkSizeBytes { get; }
}
