using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

public enum VoiceKind
{
    /// <summary>A reference recording the user imported or cloned - the manageable kind.</summary>
    Clone,

    /// <summary>A speaker baked into a local model.</summary>
    Preset,

    /// <summary>A trained voice model (Piper), downloaded per voice.</summary>
    Model,

    /// <summary>A voice served by an online engine.</summary>
    Online,
}

public partial class VoiceManagerRow : ObservableObject
{
    public Voice Voice { get; }
    public string Name => Voice.Name;
    public string DisplayName => Voice.DisplayName;
    public VoiceKind Kind { get; }
    public string KindText { get; }
    public string KindIcon { get; }

    /// <summary>The reference WAV for a <see cref="VoiceKind.Clone"/>; null for every other kind.</summary>
    public string? FilePath { get; }
    public bool IsFileVoice => FilePath != null;

    public double DurationSeconds { get; }
    public string Duration { get; }
    public string Format { get; }

    [ObservableProperty] private bool _hasTranscript;
    [ObservableProperty] private string _transcriptText = string.Empty;

    public VoiceManagerRow(Voice voice, VoiceKind kind, string kindText, string kindIcon, string? filePath, double durationSeconds, string format, bool hasTranscript)
    {
        Voice = voice;
        Kind = kind;
        KindText = kindText;
        KindIcon = kindIcon;
        FilePath = filePath;
        DurationSeconds = durationSeconds;
        Duration = durationSeconds > 0 ? $"{durationSeconds:0.0} s" : string.Empty;
        Format = format;
        HasTranscript = hasTranscript;
        TranscriptText = hasTranscript ? "✓" : string.Empty;
    }

    partial void OnHasTranscriptChanged(bool value)
    {
        TranscriptText = value ? "✓" : string.Empty;
    }
}
