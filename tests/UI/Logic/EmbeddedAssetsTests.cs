using Avalonia.Headless.XUnit;
using Avalonia.Platform;

namespace UITests.Logic;

/// <summary>
/// Guards the <c>Assets</c> globs in <c>UI.csproj</c>: every file under <c>src/ui/Assets</c>
/// must actually be embedded as an Avalonia resource. Traditional Chinese was missing from the
/// language picker because <c>ChineseTraditional.json</c> sat in the folder but was never listed
/// in the csproj (issue #12722), and the failure was silent at run-time. A file that no glob
/// covers - a new extension, or a new sub-folder - fails here instead of shipping unnoticed.
/// </summary>
public class EmbeddedAssetsTests
{
    /// <summary><c>Assets\Themes\**</c> is deliberately excluded from the build.</summary>
    private const string ExcludedFolder = "Themes";

    [AvaloniaFact]
    public void EveryAssetOnDisk_IsEmbedded()
    {
        var missing = GetAssetsOnDisk()
            .Except(GetEmbeddedAssets(), StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            missing.Count == 0,
            "These files exist under src/ui/Assets but are not embedded as Avalonia resources. " +
            "Add a matching <AvaloniaResource Include=\"...\" /> glob in src/ui/UI.csproj:" +
            Environment.NewLine + string.Join(Environment.NewLine, missing));
    }

    [AvaloniaFact]
    public void EveryEmbeddedAsset_ExistsOnDisk()
    {
        var stale = GetEmbeddedAssets()
            .Except(GetAssetsOnDisk(), StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            stale.Count == 0,
            "These resources are embedded but have no file under src/ui/Assets:" +
            Environment.NewLine + string.Join(Environment.NewLine, stale));
    }

    /// <summary>
    /// Asset paths relative to the Assets folder, using '/' separators. Dot-files are skipped:
    /// macOS drops a .DS_Store into these folders and it is neither tracked nor shipped.
    /// </summary>
    private static IEnumerable<string> GetAssetsOnDisk()
    {
        var root = GetAssetsFolder();
        var excluded = Path.Combine(root, ExcludedFolder) + Path.DirectorySeparatorChar;

        return Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(p => !p.StartsWith(excluded, StringComparison.Ordinal))
            .Where(p => !Path.GetFileName(p).StartsWith('.'))
            .Select(p => Path.GetRelativePath(root, p).Replace(Path.DirectorySeparatorChar, '/'));
    }

    /// <summary>Embedded asset paths relative to the Assets folder, using '/' separators.</summary>
    private static IEnumerable<string> GetEmbeddedAssets()
    {
        const string prefix = "/Assets/";

        return AssetLoader
            .GetAssets(new Uri("avares://SubtitleEdit/Assets"), null)
            .Select(uri => Uri.UnescapeDataString(uri.AbsolutePath))
            .Where(p => p.StartsWith(prefix, StringComparison.Ordinal))
            .Select(p => p[prefix.Length..])
            .Where(p => !p.StartsWith(ExcludedFolder + "/", StringComparison.Ordinal));
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
