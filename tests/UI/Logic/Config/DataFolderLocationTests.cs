using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

/// <summary>
/// Pins where user data lives. The portable check used to rely on
/// <c>ExePath.StartsWith(programFilesX86)</c> with an empty path off Windows, which is always
/// true - so every non-Windows install landed in the per-user folder by accident. That outcome
/// is the intended one and is now explicit; these tests exist because "correcting" the check
/// into real portable detection would move the data folder out from under existing macOS and
/// Linux users, losing their settings, dictionaries and themes.
/// </summary>
public class DataFolderLocationTests
{
    [Fact]
    public void NonWindows_IsNeverPortable_AndUsesThePerUserDataFolder()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

        Assert.False(Se.IsPortable);
        Assert.Equal(expected, Se.DataFolder);
    }

    /// <summary>
    /// Off Windows <c>GetFolderPath(ApplicationData)</c> returns an empty string when the folder
    /// does not exist yet, so <c>Path.Combine(..., "Subtitle Edit")</c> used to yield the bare
    /// relative "Subtitle Edit" - settings, dictionaries and themes then landed in whatever
    /// directory the app happened to be started from. Anything absolute beats that.
    /// </summary>
    [Fact]
    public void MissingApplicationDataFolder_FallsBackToTheExeFolder_NeverARelativePath()
    {
        var resolved = Se.ResolveDataFolder(isPortable: false, Se.ExePath, appDataFolder: string.Empty);

        Assert.Equal(Se.ExePath, resolved);
        Assert.True(Path.IsPathRooted(resolved), $"\"{resolved}\" is not an absolute path.");
    }

    /// <summary>
    /// Every runtime folder hangs off DataFolder, so pinning that pins the rest. Guards against
    /// a stray absolute or working-directory-relative path creeping back in.
    /// </summary>
    [Fact]
    public void RuntimeFolders_AreAllUnderTheDataFolder()
    {
        string[] folders = [Se.ThemesFolder, Se.TranslationFolder, Se.OcrFolder, Se.SevenZipFolder];

        foreach (var folder in folders)
        {
            Assert.True(
                Path.IsPathRooted(folder),
                $"\"{folder}\" is not an absolute path - it would resolve against the working directory.");
            Assert.StartsWith(Se.DataFolder, folder, StringComparison.Ordinal);
        }
    }
}
