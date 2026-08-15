using SeConv.Helpers;
using Xunit;

namespace SeConvTests.Core;

public class HelpDisplayTest
{
    // Wide enough that no assertion below straddles a wrap.
    private static readonly string Help = HelpDisplay.Render(200);

    [Fact]
    public void Help_NamesTheActualBinary()
    {
        // The usage line said "SubtitleEdit", which is the desktop app, not this executable.
        Assert.Contains("seconv [pattern]", Help);
        Assert.DoesNotContain("SubtitleEdit [pattern]", Help);
    }

    [Fact]
    public void Help_DoesNotMangleArrowsInDescriptions()
    {
        // Descriptions used to run through the '<value>' placeholder escaping, which rewrote
        // every '>' to ']': "-> text with time codes only" rendered as "-] text ...".
        Assert.Contains("-> text with time codes only", Help);
        Assert.DoesNotContain("-] text with time codes only", Help);
    }

    [Fact]
    public void Help_StillRendersPlaceholdersAsBrackets()
    {
        Assert.Contains("--adjust-duration:[ms]", Help);
        Assert.Contains("--apply-min-gap[:[ms]]", Help);
    }

    [Fact]
    public void Help_DocumentsTheMachineReadableEntryPoints()
    {
        Assert.Contains("--help-json", Help);
        Assert.Contains("--option:value", Help);
    }

    [Fact]
    public void Help_ListsEveryOptionTheParserBinds()
    {
        // The help text is hand-written while the parser binds attributes, so the two can
        // drift. Anything bound but undocumented is invisible to a reader of --help.
        // Aliases are deliberately not listed; only canonical names are checked.
        var undocumented = CliSchema.Options
            .Select(o => o.Name)
            .Where(name => !Help.Contains(name, StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            undocumented.Length == 0,
            $"Options bound by the parser but missing from --help: {string.Join(", ", undocumented)}");
    }
}
