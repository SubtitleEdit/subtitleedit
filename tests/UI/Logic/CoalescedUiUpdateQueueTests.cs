using System.Collections.Generic;
using System.Threading;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// The queue that keeps a fast OCR/translate run from starving input (#13885).
/// </summary>
public class CoalescedUiUpdateQueueTests
{
    private static CoalescedUiUpdateQueue MakeQueue(List<int> selects, List<string> progress, int flushIntervalMs = 100)
    {
        return new CoalescedUiUpdateQueue(
            index => selects.Add(index),
            (_, text) => progress.Add(text),
            flushIntervalMs);
    }

    [AvaloniaFact]
    public void FirstUpdateAfterAQuietPeriodIsAppliedWithoutWaitingForTheInterval()
    {
        var selects = new List<int>();
        var applied = new List<string>();
        var queue = MakeQueue(selects, new List<string>());

        queue.EnqueueUpdate(() => applied.Add("first"));
        queue.EnqueueSelect(7);

        // No timer wait - just the pending dispatcher jobs. A trailing-only throttle would
        // still be waiting out the interval here, which is what made the grid feel laggy.
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "first" }, applied);
        Assert.Equal(new[] { 7 }, selects);
    }

    [AvaloniaFact]
    public void UpdatesArrivingWhileStreamingAreBatchedIntoOneFlush()
    {
        var selects = new List<int>();
        var applied = new List<string>();
        var queue = MakeQueue(selects, new List<string>());

        queue.EnqueueUpdate(() => applied.Add("line1"));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new[] { "line1" }, applied);

        // Everything that follows immediately lands in one batch, held by the throttle.
        queue.EnqueueUpdate(() => applied.Add("line2"));
        queue.EnqueueUpdate(() => applied.Add("line3"));
        queue.EnqueueSelect(2);
        queue.EnqueueSelect(3);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "line1" }, applied);
        Assert.Empty(selects);

        queue.Flush();
        Assert.Equal(new[] { "line1", "line2", "line3" }, applied);
    }

    [AvaloniaFact]
    public void QueuedUpdatesAreAppliedInOrderAndSelectionIsLatestWins()
    {
        var selects = new List<int>();
        var applied = new List<string>();
        var queue = MakeQueue(selects, new List<string>());

        queue.EnqueueUpdate(() => applied.Add("a"));
        queue.EnqueueSelect(1);
        queue.EnqueueUpdate(() => applied.Add("b"));
        queue.EnqueueSelect(2);
        queue.EnqueueUpdate(() => applied.Add("c"));
        queue.EnqueueSelect(3);

        queue.Flush();

        // Every data update survives, in order; only the newest selection is applied.
        Assert.Equal(new[] { "a", "b", "c" }, applied);
        Assert.Equal(new[] { 3 }, selects);
    }

    [AvaloniaFact]
    public void ProgressIsLatestWinsToo()
    {
        var progress = new List<string>();
        var queue = MakeQueue(new List<int>(), progress);

        queue.EnqueueProgress(10, "10 %");
        queue.EnqueueProgress(20, "20 %");
        queue.EnqueueProgress(30, "30 %");

        queue.Flush();

        Assert.Equal(new[] { "30 %" }, progress);
    }

    [AvaloniaFact]
    public void FlushingAnEmptyQueueAppliesNothing()
    {
        var selects = new List<int>();
        var progress = new List<string>();
        var queue = MakeQueue(selects, progress);

        queue.Flush();
        queue.Flush();

        Assert.Empty(selects);
        Assert.Empty(progress);
    }

    [AvaloniaFact]
    public void AnExplicitFlushLeavesNothingBehindForTheTimerToReapply()
    {
        var selects = new List<int>();
        var applied = new List<string>();
        var queue = MakeQueue(selects, new List<string>());

        queue.EnqueueUpdate(() => applied.Add("only once"));
        queue.EnqueueSelect(4);

        // A modal (unknown-word prompt) or OK flushes directly; the scheduled flush that is
        // still pending must not replay the same work.
        queue.Flush();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "only once" }, applied);
        Assert.Equal(new[] { 4 }, selects);
    }

    [AvaloniaFact]
    public void EnqueuingFromAWorkerThreadIsAppliedOnTheUiThread()
    {
        var selects = new List<int>();
        var applied = new List<int>();
        var appliedThreadIds = new List<int>();
        var queue = MakeQueue(selects, new List<string>());
        var uiThreadId = Thread.CurrentThread.ManagedThreadId;

        var worker = new Thread(() =>
        {
            for (var i = 0; i < 50; i++)
            {
                var line = i;
                queue.EnqueueUpdate(() =>
                {
                    applied.Add(line);
                    appliedThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                });
                queue.EnqueueSelect(line);
            }
        });

        worker.Start();
        worker.Join();

        queue.Flush();

        Assert.Equal(50, applied.Count);
        Assert.Equal(0, applied[0]);
        Assert.Equal(49, applied[49]);
        Assert.Equal(new[] { 49 }, selects);
        Assert.All(appliedThreadIds, id => Assert.Equal(uiThreadId, id));
    }
}
