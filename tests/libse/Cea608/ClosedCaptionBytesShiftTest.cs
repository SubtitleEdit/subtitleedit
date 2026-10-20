using Nikse.SubtitleEdit.Core.Cea608;

namespace LibSETests.Cea608;

/// <summary>
/// Caption data moved to the start of the video: a broadcast .scc/.mcc carries the time of day,
/// usually from 01:00:00:00, and the remux subtracts that start-of-program offset (#16220).
/// </summary>
public class ClosedCaptionBytesShiftTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    [Fact]
    public void ShiftMovesSlotsAndDropsPairsBeforeZero()
    {
        var captions = new ClosedCaptionBytes
        {
            Field1 = SccBytePairs.FromSccLines(new[] { "Scenarist_SCC V1.0", "", "00:00:01:00\t9420 94ae 942f" }),
            Field2 = SccBytePairs.FromSccLines(new[] { "Scenarist_SCC V1.0", "", "00:00:00:10\t9420 9420" }),
        };
        captions.Dtvcc.Add(new ClosedCaptionBytes.DtvccTriplet(400, 3, 0x01, 0x02));
        captions.Dtvcc.Add(new ClosedCaptionBytes.DtvccTriplet(1200, 2, 0x03, 0x04));

        captions.Shift(-500);

        Assert.Equal(new long[] { 15, 16, 17 }, captions.Field1.Select(p => p.Slot)); // slot 30 - 15
        Assert.Equal(0x94, captions.Field1[0].Data1);
        Assert.Empty(captions.Field2); // slots 10 and 11 - 15 < 0
        var triplet = Assert.Single(captions.Dtvcc);
        Assert.Equal(700, triplet.Milliseconds);
        Assert.Equal(0x03, triplet.Data1);
    }

    [Fact]
    public void ShiftByZeroChangesNothing()
    {
        var captions = new ClosedCaptionBytes { Field1 = SccBytePairs.FromSccLines(new[] { "Scenarist_SCC V1.0", "", "00:00:01:00\t9420" }) };
        captions.Shift(0);
        Assert.Equal(30, Assert.Single(captions.Field1).Slot);
    }

    [Fact]
    public void FirstTimeCodeOfSccIsItsFirstLine()
    {
        var scc = Path.Combine(Path.GetTempPath(), "se-first-tc-" + Guid.NewGuid().ToString("N") + ".scc");
        File.WriteAllText(scc, "Scenarist_SCC V1.0\n\n01:00:00;00\t9420 9420\n\n01:00:02;00\t942c 942c\n");
        try
        {
            Assert.Equal(TimeSpan.FromHours(1), ClosedCaptionBytes.GetFirstTimeCode(scc));
        }
        finally
        {
            File.Delete(scc);
        }
    }

    [Fact]
    public void FirstTimeCodeOfMccUsesItsTimeCodeRate()
    {
        var mcc = Path.Combine(Path.GetTempPath(), "se-first-tc-" + Guid.NewGuid().ToString("N") + ".mcc");
        File.WriteAllText(mcc, "File Format=MacCaption_MCC V1.0\n\nTime Code Rate=25\n\n00:59:58:05\t6101499669494F43000072F4FC8080FD8080FA0000\n");
        try
        {
            var first = ClosedCaptionBytes.GetFirstTimeCode(mcc);
            Assert.NotNull(first);
            Assert.Equal(59 * 60_000 + 58_000 + 200, (int)Math.Round(first.Value.TotalMilliseconds));
        }
        finally
        {
            File.Delete(mcc);
        }
    }

    [Fact]
    public void FirstTimeCodeIsNullForOtherFormats()
    {
        var srt = Path.Combine(Path.GetTempPath(), "se-first-tc-" + Guid.NewGuid().ToString("N") + ".srt");
        File.WriteAllText(srt, "1\n01:00:00,000 --> 01:00:02,000\nHello\n");
        try
        {
            Assert.Null(ClosedCaptionBytes.GetFirstTimeCode(srt));
        }
        finally
        {
            File.Delete(srt);
        }
    }

    [Fact]
    public void FixtureMccStartsAtZero()
    {
        Assert.Equal(TimeSpan.Zero, ClosedCaptionBytes.GetFirstTimeCode(FilePath("sample_mcc_cea608_only.mcc")));
    }
}
