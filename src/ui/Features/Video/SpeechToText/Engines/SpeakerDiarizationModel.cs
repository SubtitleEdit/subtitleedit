using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// NVIDIA's Nemotron-3-Diarization (streaming Sortformer v3), which CrispASR runs after any
/// backend to put "(speaker N)" in front of each line ("Detect speakers").
/// </summary>
/// <remarks>
/// End-to-end: up to 8 speakers, numbered in the order they first speak, from one pass over the
/// whole file - so the numbers stay stable through a long file, unlike a per-slice diarizer.
/// Licence OpenMDW-1.1 (permissive). The labels land on CrispASR's own segments, so a backend
/// whose segments run across a change of speaker (SenseVoice without VAD) gets them wrong.
/// </remarks>
public static class SpeakerDiarizationModel
{
    public const string FileName = "Nemotron-3-Diarization.q8_0.gguf";
    public const string Url = "https://huggingface.co/nvidia/Nemotron-3-Diarization/resolve/main/" + FileName;
    public const string Size = "107 MB";
    public const string DisplayName = "Nemotron-3-Diarization (Sortformer)";

    /// <summary>The first CrispASR with "--diarize-method sortformer"; older ones abort on "--diarize-model".</summary>
    public static readonly Version MinimumCrispAsrVersion = new(0, 8, 37);

    public static string BuildArguments(string modelFileName)
    {
        return $"--diarize --diarize-method sortformer --diarize-model \"{modelFileName}\"";
    }

    /// <summary>
    /// False only for a CrispASR known to be too old. A version that cannot be read (a manual or
    /// odd build) gets the benefit of the doubt - refusing it would block a binary that works.
    /// </summary>
    public static bool IsSupportedBy(string? crispAsrVersion)
    {
        if (string.IsNullOrWhiteSpace(crispAsrVersion))
        {
            return true;
        }

        var numeric = crispAsrVersion.Trim().TrimStart('v', 'V');
        var end = 0;
        while (end < numeric.Length && (char.IsDigit(numeric[end]) || numeric[end] == '.'))
        {
            end++;
        }

        if (!Version.TryParse(numeric[..end].TrimEnd('.'), out var version))
        {
            return true;
        }

        return version >= MinimumCrispAsrVersion;
    }

    public static WhisperModel ToWhisperModel() => new()
    {
        Name = FileName,
        Size = Size,
        Urls = new[] { Url },
    };
}
