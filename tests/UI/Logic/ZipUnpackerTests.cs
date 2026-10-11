using Nikse.SubtitleEdit.Logic.Compression;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace UITests.Logic;

public class ZipUnpackerTests
{
    // Callers hash the archive stream after unpacking (.installed.sha256 sidecar), so both the
    // .zip and the .tar.gz path must leave it open.
    [Fact]
    public void TarGzUnpackLeavesStreamOpen()
    {
        var archive = new MemoryStream();
        using (var gzip = new GZipStream(archive, CompressionMode.Compress, leaveOpen: true))
        using (var tar = new TarWriter(gzip))
        {
            var entry = new PaxTarEntry(TarEntryType.RegularFile, "engine/llama-server")
            {
                DataStream = new MemoryStream(Encoding.UTF8.GetBytes("hello")),
            };
            tar.WriteEntry(entry);
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-tgz-" + Guid.NewGuid().ToString("N"));
        try
        {
            archive.Position = 0;
            var files = new List<string>();
            new ZipUnpacker().UnpackZipStream(archive, folder, string.Empty, true, new List<string>(), files);

            Assert.Single(files);
            Assert.Equal("hello", File.ReadAllText(Path.Combine(folder, "llama-server")));
            Assert.True(archive.CanRead);
            archive.Position = 0;
            Assert.True(archive.Length > 0);
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
