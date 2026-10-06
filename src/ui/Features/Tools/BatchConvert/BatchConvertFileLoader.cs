using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Parses the files for "Add files"/"Add folder" with a few workers and hands the results back in
/// file order, batched - one progress callback per <c>flushInterval</c> instead of one per file.
/// Several workers hide per-file latency (network shares, USB/HDD, cold cache); batching keeps the
/// UI thread from being flooded with one post per file when thousands of files are added.
/// </summary>
internal static class BatchConvertFileLoader
{
    public const int DefaultMaxDegreeOfParallelism = 4;
    public static readonly TimeSpan DefaultFlushInterval = TimeSpan.FromMilliseconds(100);

    // Marks a flushed slot - non-null, so it is not mistaken for "not parsed yet".
    private static class Empty<T>
    {
        public static readonly IReadOnlyList<T> Instance = Array.Empty<T>();
    }

    /// <param name="NewItems">Items parsed since the last callback, in file order.</param>
    /// <param name="CompletedCount">Files parsed so far (including any not yet in file order).</param>
    /// <param name="CurrentFileName">The file a worker started on most recently.</param>
    public readonly record struct Progress<T>(List<T> NewItems, int CompletedCount, string CurrentFileName);

    /// <summary>
    /// Parses <paramref name="fileNames"/> with <paramref name="parse"/> on up to
    /// <paramref name="maxDegreeOfParallelism"/> threads and reports via <paramref name="onProgress"/>,
    /// which is called on a worker thread (never concurrently, in order) - post from it to reach
    /// the UI thread. A last callback always follows the final file. On cancellation the files
    /// parsed in order up to the first unfinished one are still reported.
    /// </summary>
    public static void Load<T>(
        IReadOnlyList<string> fileNames,
        Func<string, IReadOnlyList<T>> parse,
        int maxDegreeOfParallelism,
        TimeSpan flushInterval,
        Action<Progress<T>> onProgress,
        CancellationToken token)
    {
        var results = new IReadOnlyList<T>?[fileNames.Count];
        var flushLock = new Lock();
        var sinceFlush = Stopwatch.StartNew();
        var nextToFlush = 0;
        var completed = 0;
        var currentFileName = string.Empty;

        void Flush(bool force)
        {
            lock (flushLock)
            {
                if (!force && sinceFlush.Elapsed < flushInterval)
                {
                    return;
                }

                sinceFlush.Restart();
                var newItems = new List<T>();
                while (nextToFlush < results.Length && Volatile.Read(ref results[nextToFlush]) is { } items)
                {
                    newItems.AddRange(items);
                    results[nextToFlush] = Empty<T>.Instance; // drop the reference - the caller owns the items now
                    nextToFlush++;
                }

                onProgress(new Progress<T>(newItems, Volatile.Read(ref completed), Volatile.Read(ref currentFileName)));
            }
        }

        try
        {
            Parallel.For(0, fileNames.Count, new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism),
                CancellationToken = token,
            }, i =>
            {
                Volatile.Write(ref currentFileName, fileNames[i]);
                Flush(force: false); // lets the status show a slow file while it is being read

                Volatile.Write(ref results[i], parse(fileNames[i]));
                Interlocked.Increment(ref completed);
                Flush(force: false);
            });
        }
        catch (OperationCanceledException)
        {
            // cancelled - report what was parsed in order so far
        }

        Flush(force: true);
    }
}
