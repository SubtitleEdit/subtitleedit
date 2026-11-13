using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

namespace UITests.Logic.NetflixQualityCheck;

/// <summary>
/// The shot-change rules only make sense for a shot change inside the subtitle. A short line that
/// starts after a cut (or ends before one) got a "fix" with its end before its start, and Fix
/// Netflix errors applies the whole fixed paragraph.
/// </summary>
public class NetflixCheckShotChangeTests : IDisposable
{
    private readonly string _folder;
    private readonly string _video;
    private readonly string _oldDirectory;

    public NetflixCheckShotChangeTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "NetflixShotChange_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _video = Path.Combine(_folder, "video.mp4");
        File.WriteAllBytes(_video, Enumerable.Range(0, 200_000).Select(i => (byte)(i * 7)).ToArray());
        _oldDirectory = NetflixCheckShotChange.ShotChangeDirectory;
        NetflixCheckShotChange.ShotChangeDirectory = Path.Combine(_folder, "shotchanges");
    }

    public void Dispose()
    {
        NetflixCheckShotChange.ShotChangeDirectory = _oldDirectory;
        Directory.Delete(_folder, recursive: true);
    }

    private NetflixQualityController RunCheck(double shotChangeSeconds, int startMs, int endMs)
    {
        ShotChangeHelper.SaveShotChanges(_video, NetflixCheckShotChange.ShotChangeDirectory, [shotChangeSeconds]);
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hi.", startMs, endMs));
        var controller = new NetflixQualityController { VideoFileName = _video, FrameRate = 25 };

        new NetflixCheckShotChange("test").Check(subtitle, controller);

        return controller;
    }

    [Theory]
    [InlineData(10.0, 10_100, 10_400)] // starts just after the cut, ends 10 frames after it
    [InlineData(10.0, 10_000, 10_400)] // starts on the cut
    [InlineData(10.0, 9_800, 9_920)] // starts 5 frames before the cut, over before it
    public void FixedTimes_NeverEndBeforeTheyStart(double shotChange, int startMs, int endMs)
    {
        var controller = RunCheck(shotChange, startMs, endMs);

        foreach (var record in controller.Records.Where(r => r.FixedParagraph != null))
        {
            Assert.True(record.FixedParagraph!.EndTime.TotalMilliseconds > record.FixedParagraph.StartTime.TotalMilliseconds,
                $"{record.Comment}: {record.FixedParagraph.StartTime.TotalMilliseconds} --> {record.FixedParagraph.EndTime.TotalMilliseconds}");
        }
    }

    [Fact]
    public void OutCueJustAfterACutInsideTheLine_IsStillMovedBeforeTheCut()
    {
        var controller = RunCheck(10.0, 8_000, 10_200);

        var record = Assert.Single(controller.Records, r => r.FixedParagraph != null && r.FixedParagraph.EndTime.TotalMilliseconds < 10_000);
        Assert.Equal(9_920, record.FixedParagraph!.EndTime.TotalMilliseconds, 3);
    }
}
