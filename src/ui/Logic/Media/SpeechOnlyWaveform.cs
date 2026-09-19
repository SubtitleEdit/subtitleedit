using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// File names and ffmpeg arguments for the "Show speech only" waveform: the normal waveform's
/// peaks, but made from the audio with music and sound effects removed.
/// </summary>
public static class SpeechOnlyWaveform
{
    /// <summary>
    /// The speech-only peaks live next to the normal peak file, so both stay cached and the
    /// option can be switched back and forth without generating anything twice.
    /// </summary>
    /// <remarks>
    /// A prefix, not a suffix: without a track number the normal peak file is looked up as
    /// "&lt;hash&gt;-*.wav", and a "&lt;hash&gt;-1.speech.wav" would match that - and sort first.
    /// </remarks>
    public static string GetPeakFileName(string peakWaveFileName)
    {
        return Path.Combine(
            Path.GetDirectoryName(peakWaveFileName) ?? string.Empty,
            "speech-" + Path.GetFileName(peakWaveFileName));
    }

    /// <summary>
    /// ffmpeg arguments for the audio the source separation is fed. 16 kHz mono separates as well
    /// as the full-quality track and keeps the temp file of a feature film around 200 MB.
    /// </summary>
    public static string BuildExtractArguments(string videoFileName, int audioTrackNumber, string outputWaveFileName)
    {
        var map = audioTrackNumber >= 0 ? $"-map 0:{audioTrackNumber}? " : string.Empty;
        return $"-nostdin -y -i \"{videoFileName}\" -vn {map}-ar 16000 -ac 1 \"{outputWaveFileName}\"";
    }
}
