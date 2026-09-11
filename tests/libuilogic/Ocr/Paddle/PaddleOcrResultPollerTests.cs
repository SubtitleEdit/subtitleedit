using Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

namespace LibUiLogicTests.Ocr.Paddle;

public class PaddleOcrResultPollerTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "paddle_poll_" + Guid.NewGuid().ToString("N"));

    public PaddleOcrResultPollerTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dir, true);
        }
        catch
        {
            // best effort
        }
    }

    private static string Json(string text) =>
        $$"""{ "rec_texts": ["{{text}}"], "rec_scores": [0.99], "rec_polys": [[[10, 10], [200, 10], [200, 40], [10, 40]]] }""";

    private void WriteResult(int index, string text) =>
        File.WriteAllText(Path.Combine(_dir, index.ToString("0000") + "_res.json"), Json(text));

    private static List<string> Stems(int count) =>
        Enumerable.Range(0, count).Select(PaddleOcrImagePrep.InputStem).ToList();

    private PaddleOcrResultPoller Poller(int count) => new(_dir, Stems(count));

    [Fact]
    public void ReportNew_ReportsEachResultOnce_AndSkipsFilesStillBeingWritten()
    {
        var poller = Poller(2);
        var reported = new List<(int Position, string Text)>();
        void Collect(int position, List<PaddleOcrTextRegion> regions) =>
            reported.Add((position, regions.Count > 0 ? regions[0].Text : string.Empty));

        WriteResult(0, "Alpha");
        File.WriteAllText(Path.Combine(_dir, "0001_res.json"), """{ "rec_texts": ["Beta" """);

        Assert.Equal(1, poller.ReportNew(Collect));
        Assert.Equal([(0, "Alpha")], reported);
        Assert.False(poller.IsComplete);

        WriteResult(1, "Beta");
        Assert.Equal(1, poller.ReportNew(Collect));
        Assert.Equal([(0, "Alpha"), (1, "Beta")], reported);
        Assert.True(poller.IsComplete);

        // Nothing new on a further poll - each result handed over exactly once.
        Assert.Equal(0, poller.ReportNew(Collect));
        Assert.Equal(2, reported.Count);
    }

    [Fact]
    public void ReportNew_OutOfOrderCompletion_HandsResultsOverInLineOrder()
    {
        // PaddleOCR's worker pool does not finish images in order; three land before the first
        // poll, so a single poll has to deliver them sorted by line, not by arrival.
        var poller = Poller(4);
        var positions = new List<int>();

        foreach (var i in new[] { 2, 0, 3 })
        {
            WriteResult(i, "Line " + i);
        }

        poller.ReportNew((position, _) => positions.Add(position));
        Assert.Equal([0, 2, 3], positions);

        WriteResult(1, "Line 1");
        poller.ReportNew((position, _) => positions.Add(position));
        Assert.Equal([0, 2, 3, 1], positions);
    }

    [Fact]
    public void ReportNew_IgnoresTheAnnotatedImagesSavedAlongsideTheJson()
    {
        // --save_path runs save_all, so every image also gets a "<stem>_ocr_res_img.png" next
        // to its json. The poll must not mistake one for a result.
        var poller = Poller(1);
        var reported = 0;

        File.WriteAllText(Path.Combine(_dir, "0000_ocr_res_img.png"), "not json");
        Assert.Equal(0, poller.ReportNew((_, _) => reported++));
        Assert.Equal(0, reported);

        WriteResult(0, "Alpha");
        Assert.Equal(1, poller.ReportNew((_, _) => reported++));
        Assert.Equal(1, reported);
    }

    [Fact]
    public void ReportNew_UnreadableResult_ReportsTheErrorButStillDeliversTheLine()
    {
        // One corrupt file must not cost the line its slot, or every later result would be
        // attributed to the wrong subtitle.
        var poller = Poller(2);
        File.WriteAllText(Path.Combine(_dir, "0000_res.json"), "{ not json at all }");
        WriteResult(1, "Beta");

        var reported = new List<(int Position, int Regions)>();
        var errors = new List<string>();
        poller.ReportNew((position, regions) => reported.Add((position, regions.Count)), (stem, _) => errors.Add(stem));

        Assert.Equal([(0, 0), (1, 1)], reported);
        Assert.Equal(["0000"], errors);
        Assert.True(poller.IsComplete);
    }

    [Fact]
    public void ReportNew_MissingFolder_IsJustNothingYet()
    {
        var poller = new PaddleOcrResultPoller(Path.Combine(_dir, "not-created-yet"), Stems(1));
        Assert.Equal(0, poller.ReportNew((_, _) => Assert.Fail("nothing should be reported")));
    }

    [Fact]
    public async Task PollUntilDoneAsync_ReturnsOnceEveryResultIsIn()
    {
        var poller = Poller(2);
        WriteResult(0, "Alpha");
        WriteResult(1, "Beta");

        var reported = new List<int>();
        await poller.PollUntilDoneAsync(() => true, (position, _) => reported.Add(position), null, CancellationToken.None);

        Assert.Equal([0, 1], reported);
        Assert.True(poller.IsComplete);
    }

    [Fact]
    public async Task PollUntilDoneAsync_Cancelled_ReturnsWithoutWaitingOutTheGrace()
    {
        var poller = Poller(2);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await poller.PollUntilDoneAsync(() => false, (_, _) => Assert.Fail("nothing to report"), null, cts.Token);

        Assert.Equal(0, poller.ReportedCount);
    }
}
