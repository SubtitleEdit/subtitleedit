using Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

namespace UITests.Features.Video.TextToSpeech.SpeakFromLine;

public class PlayWithSpeechReaderTests
{
    private static readonly SpeechLine[] Lines =
    [
        new(1.0, 3.0, "one"),
        new(4.0, 6.0, "two"),
        new(5.5, 8.0, "three (overlaps two)"),
    ];

    [Fact]
    public void FindNextLine_BeforeFirst_IsFirst()
    {
        Assert.Equal(0, PlayWithSpeechReader.FindNextLine(Lines, 0));
    }

    [Fact]
    public void FindNextLine_JustPastStart_StillSpeaksThatLine()
    {
        Assert.Equal(1, PlayWithSpeechReader.FindNextLine(Lines, 4.2));
    }

    [Fact]
    public void FindNextLine_InsideLine_SkipsToNext()
    {
        Assert.Equal(1, PlayWithSpeechReader.FindNextLine(Lines, 2.0));
    }

    [Fact]
    public void FindNextLine_AfterLast_IsCount()
    {
        Assert.Equal(3, PlayWithSpeechReader.FindNextLine(Lines, 7.0));
    }

    [Fact]
    public void GetSlot_UsesLineDuration()
    {
        Assert.Equal(2.0, PlayWithSpeechReader.GetSlot(Lines, 0), 3);
    }

    [Fact]
    public void GetSlot_OverlappingNextLine_EndsAtNextStart()
    {
        Assert.Equal(1.5, PlayWithSpeechReader.GetSlot(Lines, 1), 3);
    }

    [Fact]
    public void GetSpeechRate_ClipFits_NormalSpeed()
    {
        Assert.Equal(1.0, PlayWithSpeechReader.GetSpeechRate(1.5, 2.0, 1.5));
    }

    [Fact]
    public void GetSpeechRate_ClipTooLong_SpedUpToFit()
    {
        Assert.Equal(1.25, PlayWithSpeechReader.GetSpeechRate(2.5, 2.0, 1.5), 3);
    }

    [Fact]
    public void GetSpeechRate_ClipFarTooLong_CappedAtMax()
    {
        Assert.Equal(1.5, PlayWithSpeechReader.GetSpeechRate(6.0, 2.0, 1.5), 3);
    }

    [Fact]
    public void IsSeek_NormalPlayback_IsNotSeek()
    {
        Assert.False(PlayWithSpeechReader.IsSeek(10.0, 10.04, 0.04, true, 1.0));
        Assert.False(PlayWithSpeechReader.IsSeek(10.0, 10.5, 0.25, true, 2.0)); // a slow tick at 2x
    }

    [Fact]
    public void IsSeek_JumpForwardOrBack_IsSeek()
    {
        Assert.True(PlayWithSpeechReader.IsSeek(10.0, 30.0, 0.04, true, 1.0));
        Assert.True(PlayWithSpeechReader.IsSeek(10.0, 5.0, 0.04, true, 1.0));
    }

    [Fact]
    public void IsSeek_PausedAndMoved_IsSeek()
    {
        Assert.True(PlayWithSpeechReader.IsSeek(10.0, 11.0, 0.04, false, 1.0));
        Assert.False(PlayWithSpeechReader.IsSeek(10.0, 10.0, 0.04, false, 1.0));
    }
}
