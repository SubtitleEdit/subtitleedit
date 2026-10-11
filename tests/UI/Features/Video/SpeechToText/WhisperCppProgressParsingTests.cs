using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// whisper.cpp "--print-progress" lines (#15836): current whisper-cli prints them from
/// whisper_print_progress_callback, older builds from whisper_full.
/// </summary>
public class WhisperCppProgressParsingTests
{
    [Theory]
    [InlineData("whisper_print_progress_callback: progress =  78%", 78)]
    [InlineData("whisper_print_progress_callback: progress =   5%", 5)]
    [InlineData("whisper_print_progress_callback: progress = 100%", 100)]
    [InlineData("whisper_full: progress = 25%", 25)]
    [InlineData("whisper_full_with_state: progress =  40%", 40)]
    public void ProgressLine_YieldsPercent(string line, double expected)
    {
        Assert.True(SpeechToTextViewModel.TryParseWhisperCppProgress(line, out var pct));
        Assert.Equal(expected, pct, 3);
    }

    [Theory]
    [InlineData("[00:00:05.220 --> 00:00:08.060]  These are the latest headlines.")]
    [InlineData("crispasr: progress =  14% (1/7 slices)")]
    [InlineData("whisper_vad: Reduced audio from 788480 to 594880 samples (24.6% reduction)")]
    [InlineData("whisper_print_timings:    total time = 12345.67 ms")]
    [InlineData("")]
    public void OtherLines_AreIgnored(string line)
    {
        Assert.False(SpeechToTextViewModel.TryParseWhisperCppProgress(line, out _));
    }
}
