using Nikse.SubtitleEdit.Features.Video.SyncToAudio;

namespace UITests.Features.Video.SyncToAudio;

public class AudioSubtitleSyncTests
{
    /// <summary>
    /// A made-up film: 300 lines of 1-4 s with 0.4-8 s between them, about 20 minutes.
    /// </summary>
    private static List<(double StartSeconds, double EndSeconds)> Truth(int seed = 1, int count = 300)
    {
        var random = new Random(seed);
        var lines = new List<(double, double)>();
        var t = 10.0;
        for (var i = 0; i < count; i++)
        {
            var duration = 1 + random.NextDouble() * 3;
            lines.Add((t, t + duration));
            t += duration + 0.4 + random.NextDouble() * 7.6;
        }

        return lines;
    }

    /// <summary>
    /// What a loudness detector would make of that film: speech starting and ending a little off
    /// the cue, one line in ten with no speech at all (a sign, a song), and some loud effects
    /// where nothing is said.
    /// </summary>
    private static double[] Speech(List<(double StartSeconds, double EndSeconds)> truth, int seed = 2)
    {
        var random = new Random(seed);
        var noisy = new List<(double, double)>();
        foreach (var (start, end) in truth)
        {
            if (random.Next(10) == 0)
            {
                continue;
            }

            noisy.Add((start + (random.NextDouble() - 0.5) * 0.3, end + (random.NextDouble() - 0.5) * 0.3));
        }

        var filmEnd = truth[^1].EndSeconds + 20;
        for (var i = 0; i < truth.Count / 5; i++)
        {
            var at = random.NextDouble() * filmEnd;
            noisy.Add((at, at + 0.3 + random.NextDouble() * 2));
        }

        return AudioSubtitleSync.SignalFromLines(noisy, 1, (int)(filmEnd * AudioSubtitleSync.Rate));
    }

    private static List<(double StartSeconds, double EndSeconds)> Transform(List<(double StartSeconds, double EndSeconds)> lines, double scale, double shift)
        => lines.Select(l => (l.StartSeconds * scale + shift, l.EndSeconds * scale + shift)).ToList();

    [Fact]
    public void Finds_a_positive_offset()
    {
        var truth = Truth();
        var late = Transform(truth, 1, 3.21); // the subtitle shows 3.21 s after the speech

        var result = AudioSubtitleSync.Find(Speech(truth), late, new AudioSubtitleSync.Options());

        Assert.NotNull(result);
        Assert.Equal(-3.21, result.OffsetSeconds, 0.03);
        Assert.False(result.IsFrameRateChanged);
        Assert.True(result.Confidence > 0.3, $"confidence {result.Confidence}");
    }

    [Fact]
    public void Finds_a_negative_offset_within_a_small_window()
    {
        var truth = Truth(seed: 3);
        var early = Transform(truth, 1, -7.5);

        var result = AudioSubtitleSync.Find(Speech(truth), early, new AudioSubtitleSync.Options(MaxOffsetSeconds: 10));

        Assert.NotNull(result);
        Assert.Equal(7.5, result.OffsetSeconds, 0.03);
        Assert.False(result.IsFrameRateChanged);
    }

    [Fact]
    public void Finds_a_frame_rate_mismatch()
    {
        // Timed for 25 fps, shown on 23.976 fps video: every time code is too small by 23.976/25,
        // plus a 2 s offset on top.
        var truth = Truth(seed: 4);
        var k = 23.976 / 25;
        var wrong = Transform(truth, k, 2);

        var result = AudioSubtitleSync.Find(Speech(truth), wrong, new AudioSubtitleSync.Options(VideoFrameRate: 23.976));

        Assert.NotNull(result);
        Assert.True(result.IsFrameRateChanged);
        Assert.Equal(25, result.FromFrameRate);
        Assert.Equal(23.976, result.ToFrameRate);
        Assert.Equal(1 / k, result.Ratio, 0.0005);
        Assert.Equal(-2 / k, result.OffsetSeconds, 0.05);
    }

    [Fact]
    public void Does_not_invent_a_frame_rate_change()
    {
        var truth = Truth(seed: 5);
        var late = Transform(truth, 1, 1.0);

        var result = AudioSubtitleSync.Find(Speech(truth), late, new AudioSubtitleSync.Options(TryFrameRates: true));

        Assert.NotNull(result);
        Assert.False(result.IsFrameRateChanged);
        Assert.Equal(-1.0, result.OffsetSeconds, 0.03);
    }

    [Fact]
    public void Frame_rates_can_be_left_out()
    {
        var truth = Truth(seed: 6);
        var wrong = Transform(truth, 25 / 23.976, 0);

        var result = AudioSubtitleSync.Find(Speech(truth), wrong, new AudioSubtitleSync.Options(TryFrameRates: false));

        Assert.NotNull(result);
        Assert.False(result.IsFrameRateChanged);
    }

    [Fact]
    public void An_unrelated_subtitle_gets_low_confidence()
    {
        var truth = Truth(seed: 7);
        var other = Truth(seed: 8);

        var result = AudioSubtitleSync.Find(Speech(truth), other, new AudioSubtitleSync.Options());

        Assert.NotNull(result);
        Assert.True(result.Confidence < AudioSubtitleSync.LowConfidence, $"confidence {result.Confidence}");
    }

    [Fact]
    public void Syncs_against_another_subtitle()
    {
        var truth = Truth(seed: 9);
        var reference = AudioSubtitleSync.SignalFromLines(truth);
        var early = Transform(truth, 1, -12.34);

        var result = AudioSubtitleSync.Find(reference, early, new AudioSubtitleSync.Options());

        Assert.NotNull(result);
        Assert.Equal(12.34, result.OffsetSeconds, 0.011);
        Assert.True(result.Confidence > 0.9, $"confidence {result.Confidence}");
    }

    [Fact]
    public void Agreement_is_one_for_identical_signals_and_zero_for_chance()
    {
        var a = new double[] { 1, 1, -1, -1, 1, -1, 1, 1 };
        Assert.Equal(1.0, AudioSubtitleSync.Agreement(a, a, 0), 1e-9);

        // Half the frames agree, no better than a coin toss at these on-rates.
        var b = new double[] { 1, -1, 1, -1, 1, -1, 1, -1 };
        var c = new double[] { 1, 1, -1, -1, 1, 1, -1, -1 };
        Assert.Equal(0.0, AudioSubtitleSync.Agreement(b, c, 0), 1e-9);
    }

    [Fact]
    public void Speech_from_loudness_follows_the_loud_parts()
    {
        // 126 peaks per second like the waveform: loud while "speaking", quiet otherwise.
        const int rate = 126;
        var truth = Truth(seed: 10, count: 40);
        var seconds = (int)truth[^1].EndSeconds + 5;
        var random = new Random(11);
        var loudness = new int[seconds * rate];
        for (var i = 0; i < loudness.Length; i++)
        {
            var t = i / (double)rate;
            var speaking = truth.Any(l => t >= l.StartSeconds && t < l.EndSeconds);
            loudness[i] = speaking ? 8000 + random.Next(12000) : random.Next(400);
        }

        var speech = AudioSubtitleSync.SpeechFromLoudness(loudness, rate);

        Assert.NotNull(speech);
        var expected = AudioSubtitleSync.SignalFromLines(truth, 1, speech.Length);
        var agree = speech.Zip(expected, (s, e) => s == e ? 1 : 0).Sum();
        Assert.True(agree > speech.Length * 0.97, $"agreement {agree / (double)speech.Length}");

        // ... and the whole chain: those peaks, that subtitle 4 s late.
        var result = AudioSubtitleSync.Find(speech, Transform(truth, 1, 4), new AudioSubtitleSync.Options());
        Assert.NotNull(result);
        Assert.Equal(-4, result.OffsetSeconds, 0.03);
    }

    [Fact]
    public void Flat_audio_has_no_speech()
    {
        var loudness = Enumerable.Repeat(100, 126 * 60).ToList();
        Assert.Null(AudioSubtitleSync.SpeechFromLoudness(loudness, 126));
    }

    [Fact]
    public void Nothing_to_match_gives_null()
    {
        Assert.Null(AudioSubtitleSync.Find(Array.Empty<double>(), Truth(), new AudioSubtitleSync.Options()));
        Assert.Null(AudioSubtitleSync.Find(new double[100], new List<(double, double)>(), new AudioSubtitleSync.Options()));
    }
}
