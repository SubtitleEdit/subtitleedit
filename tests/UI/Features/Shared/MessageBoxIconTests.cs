using Nikse.SubtitleEdit.Features.Shared;
using System.IO.Compression;

namespace UITests.Features.Shared;

/// <summary>
/// <see cref="MessageBox"/> resolves its icon by name - <c>{MessageBoxIcon}.png</c> inside the
/// current theme's image folder, unpacked from Themes.zip at start-up. Nothing at compile time
/// ties the enum to those file names, and the load sits behind a catch, so a rename would just
/// silently drop the icon. That is how the icons came to be loaded from a path that never
/// resolved at all. These tests pin the contract to the shipped zip.
/// </summary>
public class MessageBoxIconTests
{
    /// <summary>Image folders shipped in Themes.zip; MessageBox may resolve to any of them.</summary>
    private static readonly string[] ThemeFolders = ["Classic", "Dark", "Light"];

    public static TheoryData<string> ThemeFolderNames()
    {
        var data = new TheoryData<string>();
        foreach (var folder in ThemeFolders)
        {
            data.Add(folder);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ThemeFolderNames))]
    public void EveryMessageBoxIcon_HasAnImageInTheme(string themeFolder)
    {
        var entries = GetThemeZipEntries();

        var missing = Enum.GetValues<MessageBoxIcon>()
            .Where(i => i != MessageBoxIcon.None)
            .Select(i => $"{themeFolder}/{i}.png")
            .Where(p => !entries.Contains(p))
            .ToList();

        Assert.True(
            missing.Count == 0,
            "MessageBox resolves icons by enum name, but Themes.zip has no image for:" +
            Environment.NewLine + string.Join(Environment.NewLine, missing));
    }

    /// <summary>
    /// The theme images are not embedded assets - Assets\Themes\** is excluded from the build and
    /// only Themes.zip ships - so an avares:// URI or a relative path cannot reach them.
    /// </summary>
    [Fact]
    public void ThemeImages_ShipOnlyInsideThemesZip()
    {
        var assets = GetAssetsFolder();

        Assert.False(
            Directory.Exists(Path.Combine(assets, "Themes")),
            "src/ui/Assets/Themes exists but is excluded from the build - theme images must come " +
            "from Themes.zip, unpacked into Se.ThemesFolder at start-up.");
        Assert.True(File.Exists(Path.Combine(assets, "Themes.zip")));
    }

    private static HashSet<string> GetThemeZipEntries()
    {
        using var zip = ZipFile.OpenRead(Path.Combine(GetAssetsFolder(), "Themes.zip"));
        return zip.Entries.Select(e => e.FullName).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Walks up from the test output directory to the repository root and returns the
    /// <c>src/ui/Assets</c> folder. Throws when it cannot be found.
    /// </summary>
    private static string GetAssetsFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ui", "Assets");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate 'src/ui/Assets' walking up from '{AppContext.BaseDirectory}'.");
    }
}
