using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// The source separation model CrispASR's "--separate" task uses to split the speech from
/// music and sound effects before a transcription ("Isolate speech").
/// </summary>
public static class SpeechIsolationModel
{
    public const string FileName = "mel-band-roformer-vocals-f16.gguf";
    public const string Url = "https://huggingface.co/cstr/mel-band-roformer-vocals-GGUF/resolve/main/" + FileName;
    public const string Size = "457 MB";
    public const string DisplayName = "Mel-Band RoFormer (vocals)";

    /// <summary>The stem "--separate" writes for the speech: "&lt;input&gt;_vocals.wav".</summary>
    public const string SpeechStem = "vocals";

    /// <summary>The stem with everything but the speech - music and sound effects: "&lt;input&gt;_other.wav".</summary>
    public const string BackgroundStem = "other";

    /// <summary>CrispASR arguments that write only <paramref name="stem"/> of <paramref name="inputWaveFileName"/> into <paramref name="outputFolder"/>.</summary>
    public static string BuildSeparateArguments(string modelFileName, string inputWaveFileName, string outputFolder, string stem = SpeechStem)
    {
        return $"-m \"{modelFileName}\" -f \"{inputWaveFileName}\" --separate --stems {stem} --sep-output-dir \"{outputFolder}\"";
    }

    /// <summary>Where "--separate" puts a stem: it names the stem after its input file.</summary>
    public static string GetStemFileName(string inputWaveFileName, string outputFolder, string stem)
    {
        return Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputWaveFileName) + "_" + stem + ".wav");
    }

    public static string GetSpeechStemFileName(string inputWaveFileName, string outputFolder)
    {
        return GetStemFileName(inputWaveFileName, outputFolder, SpeechStem);
    }

    /// <summary>
    /// ffmpeg arguments that turn the stem back into what the engines get from a normal audio
    /// extraction - the separator always answers in 44.1 kHz stereo, whatever it was fed.
    /// </summary>
    public static string BuildDownmixArguments(string stemFileName, string outputWaveFileName)
    {
        return $"-nostdin -y -i \"{stemFileName}\" -vn -ar 16000 -ac 1 \"{outputWaveFileName}\"";
    }

    public static WhisperModel ToWhisperModel() => new()
    {
        Name = FileName,
        Size = Size,
        Urls = new[] { Url },
    };
}
