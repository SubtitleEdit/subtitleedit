using Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;
using Xunit;
using Phase = Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn.BurnedInSubtitleRemover.ProPainterPhase;

namespace UITests.Features.Video.RemoveBurnedIn;

public class ProPainterProgressTests
{
    [Theory]
    [InlineData("RAFT 3/40", Phase.EstimatingMotion, 3, 40)]
    [InlineData("RAFT 40/40", Phase.CompletingMotion, 0, 0)]
    [InlineData("Completed optical flow", Phase.Inpainting, 0, 0)]
    [InlineData("Inpainted window at frame 6/41", Phase.Inpainting, 6, 41)]
    public void ReadsRendererPhase(string line, Phase phase, int frame, int total)
    {
        Assert.Equal(new BurnedInSubtitleRemover.ProPainterStatus(phase, frame, total),
            BurnedInSubtitleRemover.ParseProPainterStatus(line));
    }

    [Theory]
    [InlineData("ProPainter ggml backend: MTL0")]
    [InlineData("RAFT missing")]
    [InlineData("RAFT 7/6")]
    [InlineData("Inpainted window at frame 1/0")]
    public void IgnoresUnrelatedAndInvalidMessages(string line)
    {
        Assert.Null(BurnedInSubtitleRemover.ParseProPainterStatus(line));
    }
}
