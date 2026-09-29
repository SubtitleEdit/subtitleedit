using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// ARIB STD-B36 caption files (.1hd, .2hd, .1sd, .2sd). The fixture is synthetic: the
/// "DCAPTION" header block, two program management blocks, then one page per 256-byte block.
/// </summary>
public class AribB36Test : IDisposable
{
    private readonly string _tempDirectory;

    public AribB36Test()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "LibSETests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    /// <summary>Builds an ARIB B36 file with one page per (start, end, alphanumeric text) cue.</summary>
    internal static byte[] MakeAribB36(params (string Start, string End, string Text)[] cues)
    {
        const int blockSize = 256;
        var buffer = new byte[Math.Max(3072, blockSize * (3 + cues.Length) + blockSize)];
        Encoding.ASCII.GetBytes("DCAPTION").CopyTo(buffer, 0);

        for (var i = 0; i < cues.Length; i++)
        {
            var block = blockSize * (3 + i);
            buffer[block + 2] = 0;
            buffer[block + 3] = 250; // block length
            buffer[block + 4] = 0x2a; // page management information

            var index = block + 5;
            const int pageManagementLength = 120;
            buffer[index++] = 0;
            buffer[index++] = pageManagementLength;
            Encoding.ASCII.GetBytes("T").CopyTo(buffer, index + 9); // timing unit: milliseconds
            Encoding.ASCII.GetBytes(cues[i].Start).CopyTo(buffer, index + 10); // HHMMSSmmm
            Encoding.ASCII.GetBytes(cues[i].End).CopyTo(buffer, index + 19);
            index += pageManagementLength;

            buffer[index++] = 0x3a; // caption text page management data
            const int captionTextPageManagementLength = 20;
            buffer[index++] = 0;
            buffer[index++] = captionTextPageManagementLength;
            Encoding.ASCII.GetBytes("eng").CopyTo(buffer, index + 14);
            index += captionTextPageManagementLength;

            buffer[index++] = 0x4a; // caption text data
            var textData = new List<byte> { 0x9b }; // CSI
            textData.AddRange(Encoding.ASCII.GetBytes("170;30 a"));
            textData.Add(0x0e); // LS1: alphanumeric set into GL
            textData.Add(0x89); // MSZ: middle size, so the letters decode half width
            textData.AddRange(Encoding.ASCII.GetBytes(cues[i].Text));
            var unitLength = 5 + textData.Count;
            buffer[index++] = 0;
            buffer[index++] = (byte)(15 + unitLength);
            buffer[index + 14] = (byte)unitLength; // data unit loop length
            var unit = index + 15;
            buffer[unit++] = 0x1f; // unit separator
            buffer[unit++] = 0x20; // statement body (text)
            buffer[unit++] = 0;
            buffer[unit++] = 0;
            buffer[unit++] = (byte)textData.Count;
            textData.ToArray().CopyTo(buffer, unit);
        }

        return buffer;
    }

    private string WriteFile(string name, byte[] bytes)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllBytes(fileName, bytes);
        return fileName;
    }

    [Fact]
    public void LoadsTextAndTiming()
    {
        var fileName = WriteFile("test.1hd", MakeAribB36(("000001000", "000003500", "Hello"), ("000004000", "000006000", "World")));
        var format = new AribB36();
        Assert.True(format.IsMine(null, fileName));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, null, fileName);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("World", subtitle.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("test.1HD")]
    [InlineData("test.2hd")]
    [InlineData("test.1sd")]
    [InlineData("test.2SD")]
    public void IsMine_AcceptsEveryAribExtensionInAnyCase(string name)
    {
        var fileName = WriteFile(name, MakeAribB36(("000001000", "000003000", "Hello")));

        Assert.True(new AribB36().IsMine(null, fileName));
    }

    [Fact]
    public void IsMine_RejectsOtherExtensions()
    {
        var fileName = WriteFile("test.bin", MakeAribB36(("000001000", "000003000", "Hello")));

        Assert.False(new AribB36().IsMine(null, fileName));
    }
}
