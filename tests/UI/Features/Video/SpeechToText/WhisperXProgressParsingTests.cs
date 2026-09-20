using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// WhisperX prints no progress percentage by default; its per-chunk
/// "Transcript: [start --> end]  text" lines are the live signal the progress bar runs on.
/// </summary>
public class WhisperXProgressParsingTests
{
    [Theory]
    [InlineData("Transcript: [1101.31 --> 1130.774]  I really spoke with some of the best.", 1130.774)]
    [InlineData("Transcript: [1202.155 --> 1202.628]  Now.", 1202.628)]
    [InlineData("Transcript: [0 --> 12]  Thanks for watching!", 12)]
    [InlineData("Transcript: [1272.271 --> 1278.143]", 1278.143)]
    public void TranscriptLine_YieldsChunkEndSeconds(string line, double expectedEnd)
    {
        Assert.True(SpeechToTextViewModel.TryParseWhisperXTranscriptEndSeconds(line, out var end));
        Assert.Equal(expectedEnd, end, 3);
    }

    [Theory]
    [InlineData("[00:18:21.310 --> 00:18:50.774]  I really spoke with some of the best.")]
    [InlineData("Progress: 12.34%...")]
    [InlineData("Transcript: [1,101.31 --> 1,130.774]  comma separated")]
    [InlineData("Detected language: en (1.00) in first 30s of audio...")]
    [InlineData("")]
    public void OtherLines_AreIgnored(string line)
    {
        Assert.False(SpeechToTextViewModel.TryParseWhisperXTranscriptEndSeconds(line, out _));
    }
}
