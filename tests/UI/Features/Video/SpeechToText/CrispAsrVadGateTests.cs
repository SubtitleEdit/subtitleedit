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

    /// <summary>Without Silero, Index-Echo falls back to fixed 60 s windows that cut sentences.</summary>
    [Fact]
    public void IndexEchoGetsVadByDefault()
    {
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrIndexEcho(), string.Empty, vadSuppressed: false));
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
    /// <summary>A VAD picked in the "VAD" combo box is used by every Crisp ASR backend (#15563).</summary>
    [Theory]
    [InlineData(CrispAsrVadModel.Silero)]
    [InlineData(CrispAsrVadModel.FireRed)]
    [InlineData(CrispAsrVadModel.WebRtc)]
    public void AChosenVadTurnsVadOnForAnyBackend(string vadChoice)
    {
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), string.Empty, vadSuppressed: false, vadChoice));
    }

    [Theory]
    [InlineData(CrispAsrVadModel.Automatic)]
    [InlineData(null)]
    [InlineData("ten-vad")]
    public void AutoOrAnUnknownChoiceKeepsTheOldBehaviour(string? vadChoice)
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), string.Empty, vadSuppressed: false, vadChoice));
        Assert.True(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrCohere(), string.Empty, vadSuppressed: false, vadChoice));
    }

    /// <summary>The user's own parameters and the empty-result retry still win over the combo box.</summary>
    [Fact]
    public void AChosenVadStillHonoursTheOptOuts()
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), "--chunk-seconds 30", vadSuppressed: false, CrispAsrVadModel.FireRed));
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), "-vm silero --vad", vadSuppressed: false, CrispAsrVadModel.FireRed));
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new CrispAsrParakeet(), string.Empty, vadSuppressed: true, CrispAsrVadModel.FireRed));
    }

    [Fact]
    public void NonCrispAsrEnginesAreLeftAlone()
    {
        Assert.False(SpeechToTextViewModel.ShouldForceCrispAsrVad(new WhisperEngineCpp(), string.Empty, vadSuppressed: false, CrispAsrVadModel.FireRed));
    }
}
