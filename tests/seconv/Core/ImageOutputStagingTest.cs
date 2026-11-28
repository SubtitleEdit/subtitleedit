using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Blu-ray sup / DVD sup / VobSub output goes through a temp file renamed into place when
/// complete, so a failed (or Ctrl+C'd) export leaves the previous output as it was.
/// </summary>
public class ImageOutputStagingTest : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "seconv-staging-" + Guid.NewGuid().ToString("N"));

    public ImageOutputStagingTest()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private static Subtitle TwoLines()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello, World!", 1000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Second line.", 5000, 8000));
        return subtitle;
    }

    private static ConversionOptions Options() => new()
    {
        Patterns = [],
        Format = "bluraysup",
        Resolution = (1920, 1080),
        ImageStyle = new ImageExportStyle(),
    };

    [Theory]
    [InlineData("bluraysup", "movie.sup")]
    [InlineData("dvdsup", "movie.sup")]
    [InlineData("vobsub", "movie.sub")]
    public void Write_FailsHalfWay_KeepsOldOutputAndLeavesNoTempFiles(string format, string fileName)
    {
        var path = Path.Combine(_folder, fileName);
        File.WriteAllText(path, "old output");
        var idxPath = Path.ChangeExtension(path, ".idx");
        if (format == "vobsub")
        {
            File.WriteAllText(idxPath, "old idx");
        }

        var inner = ImageOutputWriter.TryCreateHandler(LibSEIntegration.NormalizeFormatName(format))!;
        var handler = new FailingHandler(inner, failAtParagraph: 1);

        Assert.Throws<InvalidOperationException>(() => ImageOutputWriter.Write(TwoLines(), path, handler, Options()));

        Assert.True(handler.Disposed, "the handler's file must be closed on failure");
        Assert.Equal("old output", File.ReadAllText(path));
        var expected = format == "vobsub" ? new[] { idxPath, path } : new[] { path };
        Assert.Equal(expected.OrderBy(p => p), Directory.GetFiles(_folder).OrderBy(p => p));
        if (format == "vobsub")
        {
            Assert.Equal("old idx", File.ReadAllText(idxPath));
        }
    }

    [Fact]
    public void Write_VobSub_ReplacesSubAndIdxAndLeavesNoTempFiles()
    {
        var path = Path.Combine(_folder, "movie.sub");
        File.WriteAllText(path, "old output");
        File.WriteAllText(Path.ChangeExtension(path, ".idx"), "old idx");

        var handler = ImageOutputWriter.TryCreateHandler("VobSub")!;
        ImageOutputWriter.Write(TwoLines(), path, handler, Options());

        Assert.True(new FileInfo(path).Length > 1000);
        Assert.Contains("timestamp:", File.ReadAllText(Path.ChangeExtension(path, ".idx")));
        Assert.Equal(2, Directory.GetFiles(_folder).Length);
    }

    [Fact]
    public void Write_BluRaySup_ReplacesOldOutput()
    {
        var path = Path.Combine(_folder, "movie.sup");
        File.WriteAllText(path, "old output");

        var handler = ImageOutputWriter.TryCreateHandler("Blu-ray sup")!;
        ImageOutputWriter.Write(TwoLines(), path, handler, Options());

        var bytes = File.ReadAllBytes(path);
        Assert.Equal((byte)'P', bytes[0]);
        Assert.Equal((byte)'G', bytes[1]);
        Assert.Single(Directory.GetFiles(_folder));
    }

    /// <summary>Delegates to a real handler and throws when the given paragraph is written.</summary>
    private sealed class FailingHandler(IExportHandler inner, int failAtParagraph) : IExportHandler, IDisposable
    {
        private int _written;

        public bool Disposed { get; private set; }
        public ExportImageType ExportImageType => inner.ExportImageType;
        public string Extension => inner.Extension;
        public string Title => inner.Title;
        public bool UseFileName => inner.UseFileName;

        public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter) => inner.WriteHeader(fileOrFolderName, imageParameter);

        public void CreateParagraph(ImageParameter param) => inner.CreateParagraph(param);

        public void WriteParagraph(ImageParameter param)
        {
            if (_written++ == failAtParagraph)
            {
                throw new InvalidOperationException("disk full");
            }

            inner.WriteParagraph(param);
        }

        public void WriteFooter() => inner.WriteFooter();

        public void Dispose()
        {
            Disposed = true;
            ((IDisposable)inner).Dispose();
        }
    }
}
