using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

/// <summary>
/// <c>SubtitleFormat.WarmUpAsync</c> builds the format cache on a worker thread to keep ~330 type
/// loads off the start-up critical path, so <see cref="SubtitleFormat.AllSubtitleFormats"/> is now
/// genuinely reached from more than one thread.
///
/// The getter assigns the backing field before ordering it, on purpose, so
/// <c>GetOrderedFormatsList</c> can re-enter through <c>GetSubtitleFormatByFriendlyName</c>. That
/// makes the intermediate, unordered list observable to a second thread unless the whole build is
/// held under one lock. These tests pin both halves of that: re-entrancy still works, and no caller
/// can see a partially built list.
/// </summary>
[Collection("NonParallelTests")]
public class SubtitleFormatConcurrencyTest
{
    [Fact]
    public void AllSubtitleFormats_UnderConcurrentFirstAccess_AlwaysReturnsSameCompleteList()
    {
        const int threads = 32;
        var results = new List<SubtitleFormat>[threads];
        var start = new ManualResetEventSlim(false);

        var workers = Enumerable.Range(0, threads).Select(i => new Thread(() =>
        {
            // Release all threads at once so they collide on the first build rather than
            // trickling in after the cache is already warm.
            start.Wait();
            results[i] = SubtitleFormat.AllSubtitleFormats.ToList();
        })).ToList();

        foreach (var worker in workers)
        {
            worker.Start();
        }

        start.Set();
        foreach (var worker in workers)
        {
            Assert.True(worker.Join(TimeSpan.FromSeconds(30)), "Timed out - likely a deadlock in the format cache lock.");
        }

        var expected = results[0];
        Assert.NotEmpty(expected);

        foreach (var result in results)
        {
            Assert.NotNull(result);

            // Same count and same instances in the same order: a thread that observed the
            // intermediate unordered list would differ here.
            Assert.Equal(expected.Count, result.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Same(expected[i], result[i]);
            }
        }
    }

    [Fact]
    public async Task WarmUpAsync_ProducesTheSameListAsDirectAccess()
    {
        await SubtitleFormat.WarmUpAsync();

        var formats = SubtitleFormat.AllSubtitleFormats.ToList();

        Assert.NotEmpty(formats);
        Assert.Contains(formats, f => f.Name == SubRip.NameOfFormat);

        // Every format constructed cleanly - a constructor that threw on the worker thread
        // (e.g. CHK resolving code page 850 without the provider registered) would surface here.
        Assert.DoesNotContain(formats, f => f == null);
    }

    [Fact]
    public void AllSubtitleFormats_IsCachedAcrossCalls()
    {
        var first = SubtitleFormat.AllSubtitleFormats;
        var second = SubtitleFormat.AllSubtitleFormats;

        Assert.Same(first, second);
    }
}
