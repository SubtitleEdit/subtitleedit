using SeConv.Commands;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class SubtitleConverterCancellationTest : IDisposable
{
    private readonly string _tempRoot;

    public SubtitleConverterCancellationTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SubtitleConverterCancellationTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConvertCommand_PreCancelledToken_PropagatesRatherThanReturningAnError()
    {
        var input = Path.Combine(_tempRoot, "cli-input.srt");
        await File.WriteAllTextAsync(input, """
            1
            00:00:01,000 --> 00:00:02,000
            Hello.

            """, TestContext.Current.CancellationToken);
        var outputFolder = Path.Combine(_tempRoot, "cli-output");
        Directory.CreateDirectory(outputFolder);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new ConvertCommand();
        var settings = new ConvertCommand.Settings
        {
            Pattern = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Quiet = true,
            Json = true,
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            command.ExecuteAsync(null!, settings, cts.Token));
        Assert.Empty(Directory.GetFiles(outputFolder));
    }

    [Fact]
    public async Task ConvertCommand_CancelledWhileWaitingForGlobalGate_DoesNotRun()
    {
        var input = Path.Combine(_tempRoot, "queued-cli.srt");
        await File.WriteAllTextAsync(input, """
            1
            00:00:01,000 --> 00:00:02,000
            Hello.

            """, TestContext.Current.CancellationToken);
        var outputFolder = Path.Combine(_tempRoot, "queued-cli-output");
        Directory.CreateDirectory(outputFolder);

        var gate = (SemaphoreSlim)typeof(SubtitleConverter)
            .GetField("ConversionGate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;

        await gate.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
            using var cts = new CancellationTokenSource();
            var task = new ConvertCommand().ExecuteAsync(null!, new ConvertCommand.Settings
            {
                Pattern = [input],
                Format = "SubRip",
                OutputFolder = outputFolder,
                Quiet = true,
                Json = true,
            }, cts.Token);

            Assert.False(task.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
            Assert.Empty(Directory.GetFiles(outputFolder));
        }
        finally
        {
            gate.Release();
        }
    }

    [Fact]
    public async Task ConvertAsync_CancelledWhileWaitingForGlobalGate_WritesNothing()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        await File.WriteAllTextAsync(input, """
            1
            00:00:01,000 --> 00:00:02,000
            Hello.

            """, TestContext.Current.CancellationToken);

        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);
        var gate = (SemaphoreSlim)typeof(SubtitleConverter)
            .GetField("ConversionGate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;

        await gate.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
            using var cts = new CancellationTokenSource();
            var conversion = new SubtitleConverter().ConvertAsync(new ConversionOptions
            {
                Patterns = [input],
                Format = "SubRip",
                OutputFolder = outputFolder,
                Overwrite = true,
            }, cts.Token);

            Assert.False(conversion.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => conversion);
            Assert.Empty(Directory.GetFiles(outputFolder));
        }
        finally
        {
            gate.Release();
        }
    }

    [Fact]
    public async Task CancelledTransform_DoesNotOverwriteExistingSubtitle()
    {
        var output = Path.Combine(_tempRoot, "existing.srt");
        await File.WriteAllTextAsync(output, "ORIGINAL CONTENT", TestContext.Current.CancellationToken);

        var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
        subtitle.Paragraphs.Add(new Nikse.SubtitleEdit.Core.Common.Paragraph("Replacement", 0, 1000));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Exercise the transform/save boundary itself: an already-started conversion
        // must not replace an existing output after cancellation is observed.
        var method = typeof(SubtitleConverter).GetMethod(
            "ApplyTransformsAndSaveAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = (Task)method.Invoke(new SubtitleConverter(),
            [subtitle, new Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip(),
                output, new ConversionOptions { Patterns = [], Format = "SubRip", Overwrite = true }, cts.Token])!;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal("ORIGINAL CONTENT", await File.ReadAllTextAsync(output, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void BurnInRunner_PreCancelledToken_DoesNotOverwriteExistingVideo()
    {
        var assa = Path.Combine(_tempRoot, "captions.ass");
        var sourceVideo = Path.Combine(_tempRoot, "source.mp4");
        File.WriteAllText(assa, "subtitle");
        File.WriteAllText(sourceVideo, "video");
        var output = BurnInRunner.GetOutputFileName(assa, sourceVideo);
        File.WriteAllText(output, "EXISTING VIDEO");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            BurnInRunner.Run(assa, sourceVideo, null, overwrite: true,
                cancellationToken: cts.Token));

        Assert.Equal("EXISTING VIDEO", File.ReadAllText(output));
        Assert.Equal("video", File.ReadAllText(sourceVideo));
    }

    [Fact]
    public void ImageOutputWriter_PreCancelledToken_DoesNotCreateOutput()
    {
        var output = Path.Combine(_tempRoot, "cancelled.sup");
        var handler = ImageOutputWriter.TryCreateHandler("Blu-ray sup");
        Assert.NotNull(handler);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            ImageOutputWriter.WritePreservedBitmaps(
                [], output, handler,
                new ConversionOptions { Patterns = [], Format = "Blu-ray sup" },
                cts.Token));
        Assert.False(File.Exists(output));
    }

    [Fact]
    public void VobSubExtractor_PreCancelledToken_DoesNotCreateOutputPair()
    {
        var output = Path.Combine(_tempRoot, "cancelled.sub");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            VobSubExtractor.Extract([], output, cancellationToken: cts.Token));
        Assert.False(File.Exists(output));
        Assert.False(File.Exists(Path.ChangeExtension(output, ".idx")));
    }

    [Fact]
    public async Task ConvertAsync_PreCancelledToken_ThrowsBeforeWritingOutput()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        await File.WriteAllTextAsync(input, """
            1
            00:00:01,000 --> 00:00:02,000
            Hello.

            """, TestContext.Current.CancellationToken);

        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var converter = new SubtitleConverter();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            converter.ConvertAsync(new ConversionOptions
            {
                Patterns = [input],
                Format = "SubRip",
                OutputFolder = outputFolder,
                Overwrite = true,
            }, cts.Token));

        Assert.Empty(Directory.GetFiles(outputFolder));
    }
}
