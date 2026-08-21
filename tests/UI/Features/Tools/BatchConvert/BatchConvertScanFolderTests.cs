using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConvertScanFolderTests
{
    // "Add folder" collects the files the file picker would also offer, and nothing else.

    private static DirectoryInfo MakeTree()
    {
        var dir = Directory.CreateTempSubdirectory("se-scan-folder-test");
        File.WriteAllText(Path.Combine(dir.FullName, "one.srt"), string.Empty);
        File.WriteAllText(Path.Combine(dir.FullName, "two.ASS"), string.Empty); // casing must not matter
        File.WriteAllText(Path.Combine(dir.FullName, "readme.pdf"), string.Empty);
        File.WriteAllText(Path.Combine(dir.FullName, "notes"), string.Empty); // no extension

        var subFolder = Directory.CreateDirectory(Path.Combine(dir.FullName, "season 2"));
        File.WriteAllText(Path.Combine(subFolder.FullName, "three.vtt"), string.Empty);
        File.WriteAllText(Path.Combine(subFolder.FullName, "cover.jpg"), string.Empty);

        return dir;
    }

    private static List<string> Scan(string folder, bool recursive, CancellationToken token = default)
    {
        var extensions = new HashSet<string>(FileHelper.GetOpenSubtitleExtensions(true), StringComparer.OrdinalIgnoreCase);
        var fileNames = new List<string>();
        BatchConvertViewModel.ScanFolder(folder, recursive, extensions, fileNames, token, _ => { });
        return fileNames;
    }

    [Fact]
    public void ScanFolder_NotRecursive_TakesSubtitleFilesInTheFolderOnly()
    {
        var dir = MakeTree();
        try
        {
            var found = Scan(dir.FullName, recursive: false, TestContext.Current.CancellationToken).Select(Path.GetFileName).ToList();

            Assert.Equal(2, found.Count);
            Assert.Contains("one.srt", found);
            Assert.Contains("two.ASS", found);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_Recursive_AlsoTakesSubfolders()
    {
        var dir = MakeTree();
        try
        {
            var found = Scan(dir.FullName, recursive: true, TestContext.Current.CancellationToken).Select(Path.GetFileName).ToList();

            Assert.Equal(3, found.Count);
            Assert.Contains("three.vtt", found);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_Cancelled_StopsWithoutCollecting()
    {
        var dir = MakeTree();
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Empty(Scan(dir.FullName, recursive: true, cts.Token));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_SymlinkCycle_TerminatesAndCollectsEachFileOnce()
    {
        var dir = MakeTree();
        try
        {
            // A subfolder linking back to the root - without cycle detection the walk
            // loops (and duplicates every file) until the user cancels.
            try
            {
                Directory.CreateSymbolicLink(Path.Combine(dir.FullName, "season 2", "loop"), dir.FullName);
            }
            catch (Exception)
            {
                return; // symlinks unavailable (e.g. Windows without developer mode) - nothing to test
            }

            var found = Scan(dir.FullName, recursive: true, TestContext.Current.CancellationToken).Select(Path.GetFileName).ToList();

            Assert.Equal(3, found.Count);
            Assert.Contains("three.vtt", found);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_MissingFolder_ReturnsNothingInsteadOfThrowing()
    {
        var missing = Path.Combine(Path.GetTempPath(), "se-scan-folder-does-not-exist-" + Guid.NewGuid().ToString("N"));

        Assert.Empty(Scan(missing, recursive: true, TestContext.Current.CancellationToken));
    }
}
