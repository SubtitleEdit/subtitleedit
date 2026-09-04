namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public interface ICrispAsrEngine : ISpeechToTextEngine
{
    /// <summary>The value passed to --backend on the command line.</summary>
    string BackendName { get; }

    /// <summary>
    /// The --backend value for one particular model. Normally <see cref="BackendName"/>; a backend
    /// whose catalog spans two crispasr runtimes (Parakeet's pure-CTC models run on
    /// fastconformer-ctc, not on the transducer backend) picks per model.
    /// </summary>
    string GetBackendName(string modelName);

    /// <summary>
    /// Extra command-line arguments one particular model needs on top of the user's
    /// <see cref="ISpeechToTextEngine.CommandLineParameter"/>. Empty for most models, and never
    /// repeats a flag the user already set in <paramref name="userArguments"/>.
    /// </summary>
    string GetModelArguments(string modelName, string? userArguments);

    /// <summary>The default language code used when no language is selected.</summary>
    string DefaultLanguage { get; }

    /// <summary>Whether the -l (language) flag should be included in the command line.</summary>
    bool IncludeLanguage { get; }

    /// <summary>Whether the backend produces native word/segment timestamps without an external CTC aligner.</summary>
    bool HasNativeTimestamps { get; }
}

