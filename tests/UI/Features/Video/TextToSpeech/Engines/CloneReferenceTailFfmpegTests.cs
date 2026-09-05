using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Runs the real ffmpeg preparation on a synthetic reference: 2 s of tone followed by 0.3 s of
/// quiet noise (room tone under the peak-relative threshold) and an abrupt end. The prepared
/// copy must lose the noise tail, gain the silence pad, and be reused on the next call. Skipped
/// when no ffmpeg can be started (Windows CI without a configured ffmpeg).
/// </summary>
public class CloneReferenceTailFfmpegTests
{
    private const double ToneSeconds = 2.0;
    private const double NoiseSeconds = 0.3;

    [Fact]
    public async Task PrepareAsync_TrimsTheNoiseTail_PadsWithSilence_AndCaches()
    {
        if (!FfmpegRuns())
        {
            return;
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-clone-tail-ffmpeg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var reference = Path.Combine(folder, "voice.wav");
            await MakeReferenceAsync(reference);

            var prepared = await CloneReferenceTail.PrepareAsync(reference, "Test engine", CancellationToken.None);

            Assert.Equal(CloneReferenceTail.GetPreparedFileName(reference), prepared);
            Assert.True(File.Exists(prepared), "no prepared copy was written");
            Assert.True(File.Exists(prepared + ".stamp"), "no stamp was written");

            var samples = ReadPcm16(prepared);
            var seconds = samples.Length / (double)CloneReferenceTail.SampleRate;

            // Tone kept, noise tail gone, pad added: 2.0 s + 0.4 s, give or take the trim edge.
            Assert.InRange(seconds, ToneSeconds + CloneReferenceTail.SilencePadSeconds - 0.05, ToneSeconds + CloneReferenceTail.SilencePadSeconds + 0.05);

            var padSamples = (int)(CloneReferenceTail.SilencePadSeconds * CloneReferenceTail.SampleRate);
            Assert.All(samples[^padSamples..], s => Assert.Equal(0, s));

            // The 50 ms before the pad are faded, so the sample just before it is far below the
            // tone's -20 dBFS peak (3277 in PCM16).
            Assert.InRange(Math.Abs((int)samples[^(padSamples + 1)]), 0, 200);

            var written = File.GetLastWriteTimeUtc(prepared);
            var again = await CloneReferenceTail.PrepareAsync(reference, "Test engine", CancellationToken.None);
            Assert.Equal(prepared, again);
            Assert.Equal(written, File.GetLastWriteTimeUtc(prepared));
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

    private static async Task MakeReferenceAsync(string outputFileName)
    {
        // Tone at exactly -20 dBFS peak, then white noise around -66 dBFS peak: 46 dB under
        // the peak, so it is under the -60 dBFS peak-relative trim threshold, and the clip
        // ends on it without any silence - the shape of a reference cut out of film dialogue.
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var filter =
            $"aevalsrc='0.1*sin(2*PI*220*t)':d={ToneSeconds.ToString(inv)}:s=24000[v];" +
            $"anoisesrc=color=white:sample_rate=24000:duration={NoiseSeconds.ToString(inv)}:amplitude=0.0005[n];" +
            "[v][n]concat=n=2:v=0:a=1,aformat=sample_fmts=s16:channel_layouts=mono[out]";
        var arguments = $"-nostdin -y -filter_complex \"{filter}\" -map [out] \"{outputFileName}\"";
        using var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });
        await process.StartAndWaitAsync(CancellationToken.None);
        Assert.True(File.Exists(outputFileName) && new FileInfo(outputFileName).Length > 44, "ffmpeg did not produce the test reference");
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

    /// <summary>PCM16 mono samples from the RIFF data chunk.</summary>
    private static short[] ReadPcm16(string fileName)
    {
        var bytes = File.ReadAllBytes(fileName);
        var pos = 12;
        while (pos + 8 <= bytes.Length)
        {
            var id = System.Text.Encoding.ASCII.GetString(bytes, pos, 4);
            var size = BitConverter.ToInt32(bytes, pos + 4);
            if (id == "data")
            {
                var dataBytes = size <= 0 || size > bytes.Length - pos - 8 ? bytes.Length - pos - 8 : size;
                var samples = new short[dataBytes / 2];
                Buffer.BlockCopy(bytes, pos + 8, samples, 0, samples.Length * 2);
                return samples;
            }

            pos += 8 + size + (size & 1);
        }

        throw new InvalidDataException("no data chunk in " + fileName);
    }
}
