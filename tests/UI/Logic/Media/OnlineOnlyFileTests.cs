using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// A real online-only placeholder cannot be made on a build machine, so these check the flag tests
/// and that the macOS stat call reads the flags from the right place in the struct (#14912).
/// </summary>
public class OnlineOnlyFileTests
{
    [Fact]
    public void IsOnlineOnly_LocalFile_IsFalse()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mp4");
        File.WriteAllBytes(fileName, new byte[1024]);
        try
        {
            Assert.False(OnlineOnlyFile.IsOnlineOnly(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void IsOnlineOnly_MissingFile_IsFalse()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mp4");

        Assert.False(OnlineOnlyFile.IsOnlineOnly(fileName));
    }

    [Fact]
    public void TryGetMacStatFlags_HiddenFile_ReadsHiddenFlag()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Skip("stat flags are only read on macOS.");
        }

        // .NET sets UF_HIDDEN (0x8000) for FileAttributes.Hidden on macOS, so seeing it back proves
        // st_flags is read at the right offset - a wrong layout reads zeros or unrelated bytes.
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mp4");
        File.WriteAllBytes(fileName, new byte[1024]);
        try
        {
            File.SetAttributes(fileName, FileAttributes.Hidden);

            Assert.True(OnlineOnlyFile.TryGetMacStatFlags(fileName, out var flags));
            Assert.Equal(0x8000u, flags & 0x8000u);
            Assert.False(OnlineOnlyFile.HasDatalessFlag(flags));
        }
        finally
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(0x40000000u, true)]
    [InlineData(0x40008000u, true)]
    [InlineData(0x00008000u, false)]
    [InlineData(0u, false)]
    public void HasDatalessFlag(uint flags, bool expected)
    {
        Assert.Equal(expected, OnlineOnlyFile.HasDatalessFlag(flags));
    }

    [Theory]
    [InlineData(FileAttributes.Normal, false)]
    [InlineData(FileAttributes.Archive | FileAttributes.ReadOnly, false)]
    [InlineData(FileAttributes.Archive | FileAttributes.Offline, true)]
    [InlineData(FileAttributes.Archive | (FileAttributes)0x00040000, true)]
    [InlineData(FileAttributes.Archive | FileAttributes.SparseFile | (FileAttributes)0x00400000, true)]
    public void IsOnlineOnlyAttributes(FileAttributes attributes, bool expected)
    {
        Assert.Equal(expected, OnlineOnlyFile.IsOnlineOnlyAttributes(attributes));
    }
}
