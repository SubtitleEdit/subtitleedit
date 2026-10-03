using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// One of the voice activity detectors CrispASR can cut the audio with before the speech model
/// sees it (the "VAD" combo box).
/// </summary>
public sealed class CrispAsrVadOption
{
    public string Choice { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>The model file in the Crisp ASR models folder, or null when there is none to download.</summary>
    public string? FileName { get; init; }
    public string? Url { get; init; }
    public string? Size { get; init; }

    public bool NeedsDownload => FileName != null;

    public WhisperModel ToWhisperModel() => new()
    {
        Name = FileName ?? string.Empty,
        Size = Size ?? string.Empty,
        Urls = Url == null ? Array.Empty<string>() : new[] { Url },
    };

    public override string ToString() => DisplayName;
}

/// <summary>
/// The voice activity detectors CrispASR ships, for any of its speech-to-text backends (#15563).
/// </summary>
/// <remarks>
/// CrispASR runs the VAD before the backend: it cuts the audio into speech regions and transcribes
/// each one on its own, so the choice of VAD does not depend on the model. It tells the detectors
/// apart by file name - a "--vad-model" path with "firered" and "vad" in it is FireRedVAD - and
/// "webrtc" needs no file at all. MarbleNet is left out: on a test clip it dropped nearly all speech.
/// Index-Echo is the exception: it loads the VAD model itself, as Silero, to place its own windows.
/// TEN VAD is not in CrispASR (upstream PLAN.md #106, held back by its licence).
/// </remarks>
public static class CrispAsrVadModel
{
    /// <summary>SE's old behaviour: Silero, and only for the backends that need VAD to work at all.</summary>
    public const string Automatic = "auto";
    public const string Silero = "silero";
    public const string FireRed = "firered";
    public const string WebRtc = "webrtc";

    public static IReadOnlyList<CrispAsrVadOption> Options { get; } = new List<CrispAsrVadOption>
    {
        new() { Choice = Automatic, DisplayName = Se.Language.General.Auto },
        new() { Choice = Silero, DisplayName = "Silero" },
        new()
        {
            Choice = FireRed,
            DisplayName = "FireRedVAD",
            FileName = "firered-vad.gguf",
            Url = "https://huggingface.co/cstr/firered-vad-GGUF/resolve/main/firered-vad.gguf",
            Size = "2.4 MB",
        },
        new() { Choice = WebRtc, DisplayName = "WebRTC" },
    };

    public static CrispAsrVadOption Get(string? choice)
    {
        return Options.FirstOrDefault(p => p.Choice == choice) ?? Options[0];
    }

    /// <summary>
    /// The detector a run with this engine actually uses: "Auto" means Silero, and Index-Echo
    /// can only read a Silero model.
    /// </summary>
    public static CrispAsrVadOption GetEffective(string? choice, ISpeechToTextEngine engine)
    {
        var option = Get(choice);
        return option.Choice == Automatic || engine is CrispAsrIndexEcho
            ? Get(Silero)
            : option;
    }

    /// <summary>
    /// What goes after "--vad-model": a local model file, or "webrtc". Null when the model is not
    /// on disk - CrispASR would otherwise download its own copy into ~/.cache mid-run.
    /// </summary>
    public static string? GetModelPath(CrispAsrVadOption option, ISpeechToTextEngine engine)
    {
        if (option.Choice == WebRtc)
        {
            return WebRtc;
        }

        if (option.FileName != null)
        {
            var path = engine.GetModelForCmdLine(option.FileName);
            return File.Exists(path) ? path : null;
        }

        return FindSilero(engine.GetAndCreateWhisperFolder());
    }

    /// <summary>The newest Silero VAD model in the Crisp ASR folder, or null.</summary>
    public static string? FindSilero(string crispAsrFolder)
    {
        if (!Directory.Exists(crispAsrFolder))
        {
            return null;
        }

        var newest = Directory.GetFiles(crispAsrFolder, "ggml-silero-v*.bin", SearchOption.TopDirectoryOnly)
            .OrderByDescending(p => p, StringComparer.Ordinal)
            .FirstOrDefault();
        if (newest != null)
        {
            return newest;
        }

        var fallback = Path.Combine(crispAsrFolder, "ggml-silero-vad.bin");
        return File.Exists(fallback) ? fallback : null;
    }

    public static string BuildArguments(string modelPath)
    {
        return modelPath == WebRtc
            ? $"--vad --vad-model {WebRtc}"
            : $"--vad --vad-model \"{modelPath}\"";
    }
}
