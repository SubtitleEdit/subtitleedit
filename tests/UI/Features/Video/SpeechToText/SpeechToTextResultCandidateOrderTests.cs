using System.Collections.Concurrent;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The engines are told to write into SE's per-run temp folder, so that folder must be probed
/// before the folder of the user's own file: a stale "podcast.srt" already sitting next to a
/// user-owned 16 kHz "podcast.wav" used to be picked up as the transcription result - and then
/// deleted as one of SE's temp files.
/// </summary>
public class SpeechToTextResultCandidateOrderTests
{
    [Fact]
    public void PerRunFolder_IsProbedBeforeTheUsersOwnFolder()
    {
        var userWav = Path.Combine("/music", "podcast.wav");
        var tempFolder = Path.Combine(Path.GetTempPath(), "se-stt-run");

        var candidates = SpeechToTextViewModel.GetResultFileCandidates(".srt", userWav, userWav, string.Empty, new ConcurrentQueue<string>(), tempFolder);

        var fromTemp = candidates.IndexOf(Path.Combine(tempFolder, "podcast.srt"));
        var fromUserFolder = candidates.IndexOf(Path.Combine("/music", "podcast.srt"));

        Assert.True(fromTemp >= 0, "per-run folder candidate missing");
        Assert.True(fromUserFolder >= 0, "user folder candidate missing");
        Assert.True(fromTemp < fromUserFolder, $"per-run folder ({fromTemp}) must come before the user's folder ({fromUserFolder})");
    }

    [Fact]
    public void NoPerRunFolder_KeepsWaveFileCandidateFirst()
    {
        var wav = Path.Combine("/tmp", "extracted.wav");

        var candidates = SpeechToTextViewModel.GetResultFileCandidates(".srt", wav, Path.Combine("/videos", "movie.mkv"), string.Empty, new ConcurrentQueue<string>());

        Assert.Equal(wav + ".srt", candidates[0]);
    }
}
