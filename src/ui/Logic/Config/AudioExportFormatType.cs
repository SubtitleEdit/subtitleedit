namespace Nikse.SubtitleEdit.Logic.Config;

// Format used when writing a standalone audio file (e.g. the TTS export step).
// Default is Wav. Non-Wav values trigger a post-pipeline ffmpeg transcode — the
// codec is inferred from the output file extension.
public enum AudioExportFormatType
{
    Wav,
    Mp3,
    Ogg,
    Flac,
    M4a,
}
