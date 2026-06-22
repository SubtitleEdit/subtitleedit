using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// Guards that every <c>UiUtil.ShowHelp("name")</c> in the UI maps to a real help page under
/// <c>docs/</c> (GitHub Pages serves <c>docs/name.md</c> as <c>name.html</c>). Catches typos /
/// renamed docs that would make F1 open a 404.
/// </summary>
public class HelpLinksResolveTests
{
    private static readonly Regex ShowHelpRegex =
        new("ShowHelp\\(\"(?<name>[^\"]+)\"", RegexOptions.Compiled);

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    [Fact]
    public void EveryShowHelpTargetHasADocsPage()
    {
        var root = FindRepoRoot();
        var uiDir = Path.Combine(root, "src", "ui");
        var docsDir = Path.Combine(root, "docs");

        var missing = new List<string>();
        var found = 0;

        foreach (var file in Directory.EnumerateFiles(uiDir, "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match m in ShowHelpRegex.Matches(File.ReadAllText(file)))
            {
                var name = m.Groups["name"].Value; // e.g. "features/spell-check" or "index"
                var mdPath = Path.Combine(docsDir, name.Replace('/', Path.DirectorySeparatorChar) + ".md");
                if (File.Exists(mdPath))
                {
                    found++;
                }
                else
                {
                    missing.Add($"{name} (in {Path.GetFileName(file)}) -> {mdPath}");
                }
            }
        }

        Assert.True(found > 0, "No ShowHelp(...) calls found — test wiring is broken.");
        Assert.True(missing.Count == 0, "ShowHelp targets without a docs page:\n" + string.Join("\n", missing));
    }
}
