using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public interface ICrispAsrEngine : ISpeechToTextEngine
{
    /// <summary>The value passed to --backend on the command line.</summary>
    string BackendName { get; }

    /// <summary>The default language code used when no language is selected.</summary>
    string DefaultLanguage { get; }

    /// <summary>Whether the -l (language) flag should be included in the command line.</summary>
    bool IncludeLanguage { get; }

    /// <summary>Whether the backend produces native word/segment timestamps without an external CTC aligner.</summary>
    bool HasNativeTimestamps { get; }

    /// <summary>
    /// The oldest CrispASR that can run <paramref name="model"/> with this backend, or null when any
    /// supported runtime will do. An older binary can load a newer model and crash mid-run (#16900).
    /// </summary>
    Version? GetMinimumCrispAsrVersion(WhisperModel model);
}

