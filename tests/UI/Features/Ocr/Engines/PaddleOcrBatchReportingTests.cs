using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// The seam between the shared result poller and the OCR window: the poller hands results
/// back by position in the batch, and this is what turns a position back into the line it
/// belongs to. Getting that wrong is how a subtitle ends up with the right words on the
/// wrong cues, so it is pinned down here rather than left to the end-to-end run.
/// </summary>
public class PaddleOcrBatchReportingTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "paddle_report_" + Guid.NewGuid().ToString("N"));

    public PaddleOcrBatchReportingTests() => Directory.CreateDirectory(_dir);

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

    private sealed class SyncProgress<T> : IProgress<T>
    {
        private readonly Action<T> _onReport;
        public SyncProgress(Action<T> onReport) => _onReport = onReport;
        public void Report(T value) => _onReport(value);
    }

    private static string Json(string text) =>
        $$"""{ "rec_texts": ["{{text}}"], "rec_scores": [0.99], "rec_polys": [[[10, 10], [200, 10], [200, 40], [10, 40]]] }""";

    [Fact]
    public void ReportedResults_CarryTheLineIndexOfTheirInput_NotTheBatchPosition()
    {
        // A partial re-OCR: the batch holds lines 5 and 6 of the subtitle, but its files are
        // still numbered from zero. Results have to come back as 5 and 6.
        var inputs = new List<PaddleOcrBatchInput>
        {
            new() { Index = 5, FileName = Path.Combine(_dir, "0000.png") },
            new() { Index = 6, FileName = Path.Combine(_dir, "0001.png") },
        };

        var reported = new List<PaddleOcrBatchProgress>();
        var ocr = new PaddleOcr();
        ocr.InitializeForTest(inputs, new SyncProgress<PaddleOcrBatchProgress>(reported.Add));

        var poller = ocr.CreatePoller(_dir);
        File.WriteAllText(Path.Combine(_dir, "0001_res.json"), Json("Beta"));
        File.WriteAllText(Path.Combine(_dir, "0000_res.json"), Json("Alpha"));
        poller.ReportNew(ocr.ReportResult);

        Assert.Equal(new[] { 5, 6 }, reported.Select(r => r.Index));
        Assert.Equal(new[] { "Alpha", "Beta" }, reported.Select(r => r.Text));
    }

    [Fact]
    public void BatchGivenOutOfOrder_IsStillPolledAndReportedByLineOrder()
    {
        // The batch list is not guaranteed sorted; the poller works off line order, so the
        // stems it watches have to be built in that order too.
        var inputs = new List<PaddleOcrBatchInput>
        {
            new() { Index = 2, FileName = Path.Combine(_dir, "0002.png") },
            new() { Index = 0, FileName = Path.Combine(_dir, "0000.png") },
            new() { Index = 1, FileName = Path.Combine(_dir, "0001.png") },
        };

        var reported = new List<PaddleOcrBatchProgress>();
        var ocr = new PaddleOcr();
        ocr.InitializeForTest(inputs, new SyncProgress<PaddleOcrBatchProgress>(reported.Add));

        var poller = ocr.CreatePoller(_dir);
        for (var i = 0; i < 3; i++)
        {
            File.WriteAllText(Path.Combine(_dir, i.ToString("0000") + "_res.json"), Json("Line " + i));
        }

        poller.ReportNew(ocr.ReportResult);

        Assert.Equal(new[] { 0, 1, 2 }, reported.Select(r => r.Index));
        Assert.Equal(new[] { "Line 0", "Line 1", "Line 2" }, reported.Select(r => r.Text));
    }

    [Fact]
    public void ResultWithNoText_IsReportedAsABlankLine()
    {
        var inputs = new List<PaddleOcrBatchInput>
        {
            new() { Index = 0, FileName = Path.Combine(_dir, "0000.png") },
        };

        var reported = new List<PaddleOcrBatchProgress>();
        var ocr = new PaddleOcr();
        ocr.InitializeForTest(inputs, new SyncProgress<PaddleOcrBatchProgress>(reported.Add));

        var poller = ocr.CreatePoller(_dir);
        File.WriteAllText(Path.Combine(_dir, "0000_res.json"), """{ "rec_texts": [] }""");
        poller.ReportNew(ocr.ReportResult);

        var result = Assert.Single(reported);
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(0, result.Confidence);
    }
}
