namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;

/// <summary>
/// A downloadable set of reference recordings for voice cloning: a zip of <c>.wav</c> files, each
/// with a <c>.txt</c> sidecar holding the spoken transcript, plus an attribution/license file.
/// Installing a pack runs every WAV through the target engine's import, so one pack serves every
/// cloning engine regardless of the sample rate it wants.
/// </summary>
public class VoicePack
{
    public string Id { get; }
    public string Name { get; }
    public string LanguageName { get; }
    public string LanguageCode { get; }
    public string Description { get; }
    public string Url { get; }
    public int VoiceCount { get; }
    public long SizeBytes { get; }
    public string License { get; }

    /// <summary>Lower-case SHA-256 of the zip; empty skips verification.</summary>
    public string Sha256 { get; }

    public VoicePack(string id, string name, string languageName, string languageCode, string description, string url, int voiceCount, long sizeBytes, string license, string sha256)
    {
        Id = id;
        Name = name;
        LanguageName = languageName;
        LanguageCode = languageCode;
        Description = description;
        Url = url;
        VoiceCount = voiceCount;
        SizeBytes = sizeBytes;
        License = license;
        Sha256 = sha256;
    }

    public override string ToString() => Name;
}
