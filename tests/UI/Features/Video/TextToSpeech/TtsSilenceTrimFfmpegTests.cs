using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic.Media;
using System.Diagnostics;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Runs the real ffmpeg trim the TTS pipeline uses on a synthetic "word": a loud vowel followed
/// by a soft fricative, at a quiet overall level like voice-clone output. With the old fixed
/// -40 dBFS threshold the fricative is trimmed off as silence (the cut last word of #14480);
/// with the threshold derived from the measured peak it survives. Skipped when no ffmpeg can be
/// started (Windows CI without a configured ffmpeg).
/// </summary>
public class TtsSilenceTrimFfmpegTests
{
    // 0.3 s silence, 0.6 s sine at -20 dBFS (peak), 0.25 s band-limited noise at about -46 dBFS
    // (a word-final "s" on a quiet clip), 0.5 s silence.
    private const double VowelSeconds = 0.6;
    private const double FricativeSeconds = 0.25;

    [Fact]
    public async Task QuietClip_PeakRelativeTrimKeepsTheFinalConsonant()
    {
        if (!FfmpegRuns())
        {
            return;
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-tts-trim-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var clip = Path.Combine(folder, "quiet-word.wav");
            await MakeQuietWordAsync(clip);

            var peak = await TtsSilenceThreshold.MeasurePeakDbfsAsync(clip, CancellationToken.None);
            Assert.NotNull(peak);
            Assert.InRange(peak!.Value, -21.5, -18.5);

            var legacyTrim = Path.Combine(folder, "legacy.wav");
            using (var legacy = FfmpegGenerator.TrimSilenceStartAndEnd(clip, legacyTrim, 0.01))
            {
                await legacy.StartAndWaitAsync(CancellationToken.None);
            }

            var relativeTrim = Path.Combine(folder, "relative.wav");
            using (var relative = FfmpegGenerator.TrimSilenceStartAndEnd(clip, relativeTrim, TtsSilenceThreshold.Amplitude(peak)))
            {
                await relative.StartAndWaitAsync(CancellationToken.None);
            }

            var legacySeconds = WavSeconds(legacyTrim);
            var relativeSeconds = WavSeconds(relativeTrim);

            // Old behaviour, documented so a regression is recognisable: the fricative is gone,
            // only the vowel plus the 100 ms of kept silence on each side remains.
            Assert.InRange(legacySeconds, VowelSeconds + 0.1, VowelSeconds + 0.3);

            // New behaviour: vowel + fricative + up to 100 ms kept silence on each side.
            var expected = VowelSeconds + FricativeSeconds;
            Assert.InRange(relativeSeconds, expected + 0.1, expected + 0.3);
            Assert.True(relativeSeconds - legacySeconds > FricativeSeconds * 0.8,
                $"peak-relative trim kept {relativeSeconds:0.000}s, legacy kept {legacySeconds:0.000}s - the fricative was not preserved");
        }
        finally
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // best effort
            }
        }
    }

    [Fact]
    public async Task MeasurePeak_ReturnsNullForAMissingFile()
    {
        if (!FfmpegRuns())
        {
            return;
        }

        var missing = Path.Combine(Path.GetTempPath(), "se-tts-trim-" + Guid.NewGuid().ToString("N") + ".wav");
        Assert.Null(await TtsSilenceThreshold.MeasurePeakDbfsAsync(missing, CancellationToken.None));
    }

    private static async Task MakeQuietWordAsync(string outputFileName)
    {
        // aevalsrc with an explicit 0.1 amplitude = exactly -20 dBFS peak (ffmpeg's sine source
        // has its own fixed -18 dBFS amplitude, so a volume multiplier on it would land elsewhere);
        // the noise is generated at about -46 dBFS peak and band-passed so it resembles a
        // fricative rather than full-band hiss - under the old -40 dBFS threshold, above the
        // peak-relative -60 dBFS one.
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var filter =
            "aevalsrc=0:d=0.3:s=24000[s1];" +
            $"aevalsrc='0.1*sin(2*PI*220*t)':d={VowelSeconds.ToString(inv)}:s=24000[v];" +
            $"anoisesrc=color=white:sample_rate=24000:duration={FricativeSeconds.ToString(inv)}:amplitude=0.005,highpass=f=3000[f];" +
            "aevalsrc=0:d=0.5:s=24000[s2];" +
            "[s1][v][f][s2]concat=n=4:v=0:a=1,aformat=sample_fmts=s16:channel_layouts=mono[out]";
        var arguments = $"-nostdin -y -filter_complex \"{filter}\" -map [out] \"{outputFileName}\"";
        using var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });
        await process.StartAndWaitAsync(CancellationToken.None);
        Assert.True(File.Exists(outputFileName) && new FileInfo(outputFileName).Length > 44, "ffmpeg did not produce the test clip");
    }

    private static bool FfmpegRuns()
    {
        try
        {
            using var process = FfmpegGenerator.GetProcess("-version", (_, _) => { });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process.WaitForExit(10_000) && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Duration from the RIFF header - independent of ffmpeg's own probing.</summary>
    private static double WavSeconds(string fileName)
    {
        Assert.True(File.Exists(fileName), $"trim produced no file: {fileName}");
        var bytes = File.ReadAllBytes(fileName);
        Assert.True(bytes.Length > 44, $"trim produced an empty wav: {fileName}");

        var byteRate = 0;
        var pos = 12;
        while (pos + 8 <= bytes.Length)
        {
            var id = System.Text.Encoding.ASCII.GetString(bytes, pos, 4);
            var size = BitConverter.ToInt32(bytes, pos + 4);
            if (id == "fmt ")
            {
                byteRate = BitConverter.ToInt32(bytes, pos + 16);
            }
            else if (id == "data")
            {
                Assert.True(byteRate > 0, "fmt chunk missing");
                var dataBytes = size <= 0 || size > bytes.Length - pos - 8 ? bytes.Length - pos - 8 : size;
                return dataBytes / (double)byteRate;
            }

            pos += 8 + size + (size & 1);
        }

        throw new InvalidDataException("no data chunk in " + fileName);
    }
}
