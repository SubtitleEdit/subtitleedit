using Nikse.SubtitleEdit.Logic.Media;
using System.Globalization;

namespace UITests.Logic.Media;

public class FfmpegGeneratorCloneReferenceTailTests
{
    [Fact]
    public void Parameters_TrimFadeThenPad_InThatOrder()
    {
        var args = FfmpegGenerator.PrepareCloneReferenceTailParameters("ref.wav", "out.wav", 0.01, 0.05, 0.4, 24000);

        // Trim and fade run on the reversed signal so they work on the tail; the pad comes after
        // the second areverse so it is neither trimmed nor faded.
        Assert.Contains(
            "-af \"areverse,silenceremove=start_periods=1:start_silence=0:start_threshold=0.01,afade=t=in:d=0.05,areverse,apad=pad_dur=0.4\"",
            args);
        Assert.Contains("-ar 24000 -ac 1 -c:a pcm_s16le \"out.wav\"", args);
        Assert.StartsWith("-nostdin -y -i \"ref.wav\"", args);
    }

    [Fact]
    public void Parameters_ThresholdIsClampedAndInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");
            var args = FfmpegGenerator.PrepareCloneReferenceTailParameters("r.wav", "o.wav", 0.00568853, 0.05, 0.4, 24000);
            Assert.Contains("start_threshold=0.00568853,", args);
            Assert.Contains("d=0.05,", args);
            Assert.Contains("pad_dur=0.4\"", args);

            var clamped = FfmpegGenerator.PrepareCloneReferenceTailParameters("r.wav", "o.wav", 5.0, -1, -1, 24000);
            Assert.Contains("start_threshold=1,", clamped);
            Assert.Contains("afade=t=in:d=0,", clamped);
            Assert.Contains("apad=pad_dur=0\"", clamped);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
