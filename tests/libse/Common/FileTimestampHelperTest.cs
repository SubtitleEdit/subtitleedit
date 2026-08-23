using Nikse.SubtitleEdit.Core.Common;
using Xunit;

namespace LibSETests.Common;

public class FileTimestampHelperTest
{
    [Fact]
    public void CopyTimestamps_File_CopiesLastWriteTime()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FileTimestampHelperTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var source = Path.Combine(dir, "source.srt");
            var target = Path.Combine(dir, "target.vtt");
            File.WriteAllText(source, "a");
            File.WriteAllText(target, "b");
            var when = new DateTime(2010, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(source, when);

            Assert.True(FileTimestampHelper.CopyTimestamps(source, target));
            Assert.Equal(when, File.GetLastWriteTimeUtc(target));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Capture_ThenOverwrite_ThenCopy_RestoresOriginalTime()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FileTimestampHelperTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "inplace.srt");
            File.WriteAllText(file, "a");
            var when = new DateTime(2011, 3, 4, 5, 6, 7, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(file, when);

            var captured = FileTimestampHelper.Capture(file);
            Assert.NotNull(captured);
            File.WriteAllText(file, "b"); // the conversion overwrites the source itself
            Assert.NotEqual(when, File.GetLastWriteTimeUtc(file));

            Assert.True(FileTimestampHelper.CopyTimestamps(captured.Value, file));
            Assert.Equal(when, File.GetLastWriteTimeUtc(file));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Capture_MissingFile_ReturnsNull()
    {
        Assert.Null(FileTimestampHelper.Capture(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        Assert.Null(FileTimestampHelper.Capture(string.Empty));
    }

    [Fact]
    public void CopyTimestamps_MissingSourceOrTarget_ReturnsFalse()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Assert.False(FileTimestampHelper.CopyTimestamps(missing, missing + ".x"));
        Assert.False(FileTimestampHelper.CopyTimestamps(string.Empty, missing));
    }

    [Fact]
    public void CopyTimestampsToDirectoryContents_StampsFilesAndFolder()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FileTimestampHelperTest_" + Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(dir, "images");
        Directory.CreateDirectory(outDir);
        try
        {
            var source = Path.Combine(dir, "source.sup");
            File.WriteAllText(source, "a");
            var when = new DateTime(2012, 6, 7, 8, 9, 10, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(source, when);
            var f1 = Path.Combine(outDir, "0001.png");
            var f2 = Path.Combine(outDir, "index.xml");
            File.WriteAllText(f1, "x");
            File.WriteAllText(f2, "y");

            FileTimestampHelper.CopyTimestampsToDirectoryContents(source, outDir);

            Assert.Equal(when, File.GetLastWriteTimeUtc(f1));
            Assert.Equal(when, File.GetLastWriteTimeUtc(f2));
            Assert.Equal(when, Directory.GetLastWriteTimeUtc(outDir));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
