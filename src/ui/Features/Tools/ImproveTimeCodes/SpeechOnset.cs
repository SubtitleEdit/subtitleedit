using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Finds where speech really starts near a start a forced aligner gave, from the audio level.
///
/// A CTC aligner puts a line's start on its own frame grid (20-80 ms) and where it first
/// recognised a letter - wav2vec2 often a tenth to a third of a second after the voice has
/// started. The level of the audio is far more precise about that, so a start in the sound is
/// moved back to where the sound rose out of the quiet before it. A start is never moved later:
/// a subtitle a little early is harmless, and the quiet before the voice may still hold a "t"
/// or a breathy "h". Nothing is changed when there is no quiet to find (speech running on from
/// the line before, music under it) or the speech hardly stands out.
/// </summary>
public static class SpeechOnset
{
    /// <summary>Length of one level frame.</summary>
    public const double FrameSeconds = 0.01;

    /// <summary>The furthest a start is moved back to the rise of the sound.</summary>
    public const double MaxEarlierSeconds = 0.4;

    /// <summary>Quiet this long (20 ms) before a sound makes it the start of the speech.</summary>
    private const int QuietFrames = 2;

    /// <summary>The longest soft consonant taken in front of the voice.</summary>
    private const int MaxLeadFrames = 12;

    /// <summary>
    /// The level of a stretch of audio, in dB per <see cref="FrameSeconds"/>: of the plain signal,
    /// and of the signal with its high frequencies lifted, so a soft "s", "f" or "h" that starts a
    /// word counts as sound too.
    /// </summary>
    public sealed class Levels
    {
        public Levels(float[] plain, float[] bright)
        {
            Plain = plain;
            Bright = bright;
        }

        public float[] Plain { get; }
        public float[] Bright { get; }
        public int Count => Plain.Length;
    }

    /// <summary>Reads a 16-bit PCM WAV file; null when it is not one.</summary>
    public static Levels? FromWaveFile(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(fileName);
            if (bytes.Length < 44 || bytes[0] != 'R' || bytes[1] != 'I' || bytes[2] != 'F' || bytes[3] != 'F')
            {
                return null;
            }

            var channels = 0;
            var sampleRate = 0;
            var bits = 0;
            var position = 12;
            while (position + 8 <= bytes.Length)
            {
                var id = System.Text.Encoding.ASCII.GetString(bytes, position, 4);
                var size = BitConverter.ToInt32(bytes, position + 4);
                var body = position + 8;
                if (id == "fmt " && size >= 16 && body + 16 <= bytes.Length)
                {
                    channels = BitConverter.ToInt16(bytes, body + 2);
                    sampleRate = BitConverter.ToInt32(bytes, body + 4);
                    bits = BitConverter.ToInt16(bytes, body + 14);
                }
                else if (id == "data")
                {
                    if (channels < 1 || sampleRate < 1000 || bits != 16)
                    {
                        return null;
                    }

                    // ffmpeg writes a data size of 0 or -1 when it streamed the file.
                    var length = size <= 0 || body + size > bytes.Length ? bytes.Length - body : size;
                    var count = length / 2 / channels;
                    var samples = new float[count];
                    for (var i = 0; i < count; i++)
                    {
                        samples[i] = BitConverter.ToInt16(bytes, body + (i * channels * 2)) / 32768f;
                    }

                    return FromSamples(samples, sampleRate);
                }

                position = body + size + (size & 1);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Levels FromSamples(float[] samples, int sampleRate)
    {
        var frameLength = Math.Max(1, (int)Math.Round(sampleRate * FrameSeconds));
        var count = samples.Length / frameLength;
        var plain = new float[count];
        var bright = new float[count];
        var previous = 0f;
        for (var f = 0; f < count; f++)
        {
            double sum = 0;
            double sumBright = 0;
            var offset = f * frameLength;
            for (var i = 0; i < frameLength; i++)
            {
                var x = samples[offset + i];
                var y = x - (0.97f * previous); // pre-emphasis: lifts the high frequencies
                previous = x;
                sum += x * x;
                sumBright += y * y;
            }

            plain[f] = ToDb(sum / frameLength);
            bright[f] = ToDb(sumBright / frameLength);
        }

        return new Levels(plain, bright);
    }

    private static float ToDb(double meanSquare) => (float)(10.0 * Math.Log10(meanSquare + 1e-10));

    /// <summary>
    /// Where the speech that the aligner placed at <paramref name="startSeconds"/> starts, or null
    /// when the level cannot tell. Times are relative to the start of <paramref name="levels"/>.
    /// </summary>
    /// <param name="earliestSeconds">
    /// Never go back past this - the end of the speech before, as far as it is known.
    /// </param>
    public static double? Find(Levels levels, double startSeconds, double earliestSeconds)
    {
        // The voice decides where the speech starts; the lifted highs only add a soft consonant
        // in front of it - on their own they also hear breaths, clicks and hiss.
        var plain = Track.Make(levels.Plain, startSeconds);
        if (plain == null)
        {
            return null;
        }

        var bright = Track.Make(levels.Bright, startSeconds);
        var start = (int)Math.Round(startSeconds / FrameSeconds);
        if (start < 0 || start >= levels.Count)
        {
            return null;
        }

        var earliest = Math.Max(0, Math.Max(start - (int)Math.Round(MaxEarlierSeconds / FrameSeconds), (int)Math.Ceiling(earliestSeconds / FrameSeconds)));
        // An aligner start in a short break inside the word (the closure before a "p" or "t")
        // belongs to the sound just before it.
        for (var k = 0; k < QuietFrames && start > 0 && !plain.IsSound(start); k++)
        {
            if (plain.IsSound(start - 1))
            {
                start--;
            }
            else
            {
                break;
            }
        }

        // Only ever earlier. In the quiet before the voice there may still be a plosive or a
        // breathy start the level does not count, so a start there is left as the aligner put it.
        if (!plain.IsSound(start))
        {
            return null;
        }

        // Back to where the sound rose out of quiet, never through a short dip.
        var onset = start;
        while (true)
        {
            if (onset - QuietFrames < earliest)
            {
                return null; // no quiet before it - running speech, or music
            }

            var quiet = true;
            for (var k = 1; k <= QuietFrames; k++)
            {
                quiet &= !plain.IsSound(onset - k);
            }

            if (quiet)
            {
                break;
            }

            onset--;
        }

        // A soft "s", "f" or "h" in front of the voice. Highs that go on for longer than any
        // consonant are noise, and add nothing.
        if (bright != null)
        {
            var lead = 0;
            while (onset - lead - 1 >= earliest && bright.IsSound(onset - lead - 1) && lead <= MaxLeadFrames)
            {
                lead++;
            }

            if (lead <= MaxLeadFrames)
            {
                onset -= lead;
            }
        }

        // The level crosses the line a little after the sound has begun.
        return Math.Max(0, (onset - 1) * FrameSeconds);
    }

    /// <summary>One level track, with the quiet and loud levels round a start measured.</summary>
    private sealed class Track
    {
        private readonly float[] _levels;
        private readonly float _threshold;

        private Track(float[] levels, float threshold)
        {
            _levels = levels;
            _threshold = threshold;
        }

        public bool IsSound(int frame) => frame >= 0 && frame < _levels.Length && _levels[frame] >= _threshold;

        /// <summary>Null when speech does not stand out clearly from the quiet round it.</summary>
        public static Track? Make(float[] levels, double startSeconds)
        {
            var start = (int)Math.Round(startSeconds / FrameSeconds);
            if (levels.Length == 0 || start < 0 || start >= levels.Length)
            {
                return null;
            }

            // The quiet: the low end of the level over a few seconds round the start.
            var from = Math.Max(0, start - 200);
            var to = Math.Min(levels.Length, start + 200);
            var floor = Percentile(levels, from, to, 0.1);

            // The speech: the loud end of the first second from the start (or just before it).
            var peak = Percentile(levels, Math.Max(0, start - 10), Math.Min(levels.Length, start + 100), 0.9);
            var contrast = peak - floor;
            if (contrast < 15)
            {
                return null;
            }

            return new Track(levels, floor + Math.Max(9f, contrast * 0.4f));
        }

        private static float Percentile(float[] levels, int from, int to, double share)
        {
            var sorted = levels.Skip(from).Take(to - from).OrderBy(v => v).ToArray();
            return sorted.Length == 0 ? 0 : sorted[Math.Min(sorted.Length - 1, (int)(sorted.Length * share))];
        }
    }
}
