using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Xunit;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// SE adds its own Silero VAD to the Cohere and Mega Crisp ASR backends, which otherwise emit a
/// zero-byte SRT on long audio. Two things have to stay true about that: the user can still turn
/// it off with crispasr's own --chunk-seconds (#13849), and a job that came back empty can be
/// re-run with VAD off (#13911) - on a per-line clip Silero can reject the whole clip as
/// non-speech, which left clips silently unconverted.
/// </summary>
public class CrispAsrVadGateTests
{
    [Fact]
    public void CohereGetsVadByDefault()
    {
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrCohere(), string.Empty, vadSuppressed: false));
    }

    [Fact]
    public void MegaGetsVadByDefault()
    {
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrMega(), null, vadSuppressed: false));
    }

    [Fact]
    public void OtherBackendsAreLeftAlone()
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), string.Empty, vadSuppressed: false));
    }

    /// <summary>The #13849 opt-out: these are the user saying "I am handling chunking".</summary>
    [Theory]
    [InlineData("--chunk-seconds 30")]
    [InlineData("-ck 30")]
    [InlineData("--vad")]
    [InlineData("--vad-model foo.bin")]
    [InlineData("-vm foo.bin")]
    [InlineData("--max-len 50 --chunk-seconds 30")]
    public void UserVadOrChunkParametersSuppressOurs(string crispArgs)
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrCohere(), crispArgs, vadSuppressed: false));
    }

    /// <summary>
    /// Parameters that merely contain the letters must not count - only whole flags do.
    /// </summary>
    [Theory]
    [InlineData("--max-len 50 --split-on-punct")]
    [InlineData("--invalid-vad-ish")]
    public void UnrelatedParametersDoNotSuppressOurs(string crispArgs)
    {
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrCohere(), crispArgs, vadSuppressed: false));
    }

    /// <summary>The retry (#13911): the second attempt at an empty job leaves VAD off.</summary>
    [Fact]
    public void TheEmptyResultRetryLeavesVadOff()
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrCohere(), string.Empty, vadSuppressed: true));
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrMega(), "--max-len 50", vadSuppressed: true));
    }
}
