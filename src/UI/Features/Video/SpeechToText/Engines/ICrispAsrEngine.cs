namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public interface ICrispAsrEngine : ISpeechToTextEngine
{
    /// <summary>The value passed to --backend on the command line.</summary>
    string BackendName { get; }

    /// <summary>The default language code used when no language is selected.</summary>
    string DefaultLanguage { get; }

    /// <summary>Whether the -l (language) flag should be included in the command line.</summary>
    bool IncludeLanguage { get; }
}

