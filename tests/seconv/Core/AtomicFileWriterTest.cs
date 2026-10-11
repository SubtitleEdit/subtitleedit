using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class AtomicFileWriterTest : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "seconv-atomic-" + Guid.NewGuid().ToString("N"));

    public AtomicFileWriterTest()
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

    [Fact]
    public void Write_InterruptedMidWrite_KeepsOldContentAndLeavesNoTempFile()
    {
        // Issue #15829: --overwrite emptied the input when the process died mid-write.
        var path = Path.Combine(_folder, "subs.srt");
        File.WriteAllText(path, "old content");

        Assert.Throws<InvalidOperationException>(() => AtomicFileWriter.Write(path, s =>
        {
            s.Write("new"u8);
            throw new InvalidOperationException("killed");
        }));

        Assert.Equal("old content", File.ReadAllText(path));
        Assert.Equal(new[] { path }, Directory.GetFiles(_folder));
    }

    [Fact]
    public void WriteAllText_ReplacesContentAndWritesPreamble()
    {
        var path = Path.Combine(_folder, "subs.srt");
        File.WriteAllText(path, "a much longer old content than the new one");

        AtomicFileWriter.WriteAllText(path, "new", new UTF8Encoding(true));

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF, (byte)'n', (byte)'e', (byte)'w' }, File.ReadAllBytes(path));
        Assert.Equal(new[] { path }, Directory.GetFiles(_folder));
    }

    [Fact]
    public void WriteAllText_NoPreambleEncoding_MatchesFileWriteAllText()
    {
        var path = Path.Combine(_folder, "subs.srt");
        AtomicFileWriter.WriteAllText(path, "new", new UTF8Encoding(false));
        Assert.Equal("new"u8.ToArray(), File.ReadAllBytes(path));
    }

    [Fact]
    public void SaveSubtitle_OverwritesInputInPlace()
    {
        var path = Path.Combine(_folder, "subs.srt");
        File.WriteAllText(path, "1\r\n00:00:05,000 --> 00:00:07,000\r\nHello\r\n\r\n");
        var subtitle = LibSEIntegration.LoadSubtitle(path);
        subtitle.Paragraphs[0].StartTime.TotalMilliseconds -= 2000;
        subtitle.Paragraphs[0].EndTime.TotalMilliseconds -= 2000;

        LibSEIntegration.SaveSubtitle(subtitle, path, "SubRip");

        Assert.Contains("00:00:03,000 --> 00:00:05,000", File.ReadAllText(path));
        Assert.Equal(new[] { path }, Directory.GetFiles(_folder));
    }

    [Fact]
    public void SaveSubtitle_BinaryFormat_WritesViaTempFile()
    {
        var path = Path.Combine(_folder, "subs.stl");
        File.WriteAllText(path, "old");
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));

        LibSEIntegration.SaveSubtitle(subtitle, path, "EBU STL");

        var bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length > 1024);
        Assert.Equal(new[] { path }, Directory.GetFiles(_folder));
    }

    [Fact]
    public void Write_ThroughSymbolicLink_KeepsLink()
    {
        var real = Path.Combine(_folder, "real.srt");
        var link = Path.Combine(_folder, "link.srt");
        File.WriteAllText(real, "old");
        try
        {
            File.CreateSymbolicLink(link, real);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return; // Windows without developer mode cannot create symbolic links.
        }

        AtomicFileWriter.WriteAllText(link, "new", new UTF8Encoding(false));

        Assert.NotNull(new FileInfo(link).LinkTarget);
        Assert.Equal("new", File.ReadAllText(real));
    }
}
