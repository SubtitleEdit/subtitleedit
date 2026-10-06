using Nikse.SubtitleEdit.Features.Tools.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConvertFileLoaderTests
{
    private static List<string> MakeFileNames(int count) =>
        Enumerable.Range(0, count).Select(i => $"file{i:0000}.srt").ToList();

    // Parses slower for some files than others, so workers finish out of order.
    private static IReadOnlyList<string> JitteryParse(string fileName)
    {
        var number = int.Parse(fileName.AsSpan(4, 4));
        Thread.Sleep(number % 3);
        return number % 5 == 0
            ? new[] { fileName + "#1", fileName + "#2" } // a container with two tracks
            : new[] { fileName };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    public void Load_ReportsEveryItemInFileOrder(int workers)
    {
        var fileNames = MakeFileNames(200);
        var reported = new List<string>();
        var lastCompleted = 0;

        BatchConvertFileLoader.Load(fileNames, JitteryParse, workers, TimeSpan.FromMilliseconds(5), p =>
        {
            reported.AddRange(p.NewItems);
            lastCompleted = p.CompletedCount;
        }, TestContext.Current.CancellationToken);

        var expected = fileNames.SelectMany(JitteryParseNoSleep).ToList();
        Assert.Equal(expected, reported);
        Assert.Equal(fileNames.Count, lastCompleted);
    }

    private static IEnumerable<string> JitteryParseNoSleep(string fileName)
    {
        var number = int.Parse(fileName.AsSpan(4, 4));
        return number % 5 == 0 ? new[] { fileName + "#1", fileName + "#2" } : new[] { fileName };
    }

    [Fact]
    public void Load_BatchesProgressCallbacks()
    {
        // 500 instant files with a long flush interval: a handful of callbacks, not one per file
        var fileNames = MakeFileNames(500);
        var callbacks = 0;
        var items = 0;

        BatchConvertFileLoader.Load(fileNames, f => new[] { f }, 4, TimeSpan.FromSeconds(10), p =>
        {
            callbacks++;
            items += p.NewItems.Count;
        }, TestContext.Current.CancellationToken);

        Assert.Equal(500, items);
        Assert.True(callbacks <= 2, $"{callbacks} callbacks");
    }

    [Fact]
    public void Load_ProgressCallbacksNeverOverlap()
    {
        var fileNames = MakeFileNames(300);
        var inCallback = 0;
        var overlapped = false;

        BatchConvertFileLoader.Load(fileNames, JitteryParse, 8, TimeSpan.Zero, _ =>
        {
            if (Interlocked.Increment(ref inCallback) > 1)
            {
                overlapped = true;
            }

            Thread.SpinWait(100);
            Interlocked.Decrement(ref inCallback);
        }, TestContext.Current.CancellationToken);

        Assert.False(overlapped);
    }

    [Fact]
    public void Load_Cancelled_ReportsOnlyAnInOrderPrefix()
    {
        var fileNames = MakeFileNames(1000);
        using var cts = new CancellationTokenSource();
        var reported = new List<string>();
        var parsed = 0;

        BatchConvertFileLoader.Load(fileNames, f =>
        {
            if (Interlocked.Increment(ref parsed) == 50)
            {
                cts.Cancel();
            }

            return JitteryParse(f);
        }, 4, TimeSpan.Zero, p => reported.AddRange(p.NewItems), cts.Token);

        Assert.True(reported.Count < fileNames.Count);
        var expectedPrefix = fileNames.SelectMany(JitteryParseNoSleep).Take(reported.Count);
        Assert.Equal(expectedPrefix, reported);
    }

    [Fact]
    public void Load_NoFiles_StillReportsOnce()
    {
        var callbacks = 0;
        BatchConvertFileLoader.Load(new List<string>(), f => new[] { f }, 4, TimeSpan.Zero, _ => callbacks++, TestContext.Current.CancellationToken);
        Assert.Equal(1, callbacks);
    }
}

public class BatchConvertAddFileParallelTests
{
    // AddFile runs on several loader threads at once; format detection goes through the shared
    // SubtitleFormat instances, so parallel must detect exactly what sequential does.
    [Fact]
    public void AddFile_InParallel_DetectsSameFormatsAsSequential()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // as the app does at startup (binary formats need code page 850)
        var dir = Directory.CreateTempSubdirectory("se-batch-add-parallel");
        try
        {
            var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
            for (var i = 0; i < 40; i++)
            {
                subtitle.Paragraphs.Add(new Nikse.SubtitleEdit.Core.Common.Paragraph($"Line {i}\nsecond line", i * 3000, i * 3000 + 2000));
            }

            var formats = Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat.AllSubtitleFormats
                .Where(f => f.IsTextBased && !f.Name.StartsWith("Unknown", StringComparison.Ordinal))
                .Take(60)
                .ToList();
            var fileNames = new List<string>();
            for (var copy = 0; copy < 4; copy++)
            {
                for (var i = 0; i < formats.Count; i++)
                {
                    string text;
                    try
                    {
                        text = formats[i].ToText(subtitle, "test");
                    }
                    catch
                    {
                        continue;
                    }

                    var fileName = Path.Combine(dir.FullName, $"{copy}_{i}{formats[i].Extension}");
                    File.WriteAllText(fileName, text);
                    fileNames.Add(fileName);
                }
            }

            var sequential = fileNames.Select(f => string.Join("|", BatchConvertViewModel.AddFile(f).Select(p => p.Format))).ToList();

            var parallel = new List<Nikse.SubtitleEdit.UiLogic.BatchConvert.BatchConvertItem>();
            BatchConvertFileLoader.Load(fileNames, BatchConvertViewModel.AddFile, 8, TimeSpan.Zero,
                p => parallel.AddRange(p.NewItems), TestContext.Current.CancellationToken);
            var parallelByFile = fileNames.Select(f => string.Join("|", parallel.Where(p => p.FileName == f).Select(p => p.Format))).ToList();

            Assert.Equal(sequential, parallelByFile);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
