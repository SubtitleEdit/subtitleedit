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
