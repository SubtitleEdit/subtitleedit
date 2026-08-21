using System.Text.Json;
using SeConv.Helpers;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Covers the machine-readable side of the CLI: the schema behind <c>seconv --help-json</c>,
/// and the unknown-option suggestion that replaced silently ignoring the flag.
/// </summary>
public class CliSchemaTest
{
    [Fact]
    public void Options_AreReflectedOffTheSettingsClass()
    {
        // The schema exists so it cannot drift from what the parser binds. If someone adds a
        // [CommandOption] it must appear here with no second edit.
        var options = CliSchema.Options;

        Assert.NotEmpty(options);
        Assert.Contains(options, o => o.Name == "--format" && o.Aliases.Contains("-f"));
        Assert.Contains(options, o => o.Name == "--fps" && o.Type == "number");
        Assert.Contains(options, o => o.Name == "--renumber" && o.Type == "integer");
        Assert.Contains(options, o => o.Name == "--overwrite" && o.Type == "flag");
    }

    [Fact]
    public void Options_SeparateOperationsFromPlainOptions()
    {
        var options = CliSchema.Options;

        Assert.Equal("operation", options.Single(o => o.Name == "--remove-formatting").Group);
        Assert.Equal("operation", options.Single(o => o.Name == "--split-long-lines").Group);
        Assert.Equal("operation", options.Single(o => o.Name == "--bridge-gaps").Group);
        Assert.Equal("option", options.Single(o => o.Name == "--output-folder").Group);
        Assert.Equal("option", options.Single(o => o.Name == "--encoding").Group);
    }

    [Fact]
    public void Options_CarryDiscoveryCommandsAndChoices()
    {
        var options = CliSchema.Options;

        Assert.Equal("seconv formats --json", options.Single(o => o.Name == "--format").Discover);
        Assert.Contains("tesseract", options.Single(o => o.Name == "--ocr-engine").Choices!);
    }

    [Fact]
    public void CuratedTables_OnlyNameRealOptions()
    {
        // Choices and discovery hints are hand-maintained; a rename elsewhere would otherwise
        // leave them silently attached to nothing.
        var known = CliSchema.Options.Select(o => o.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var annotated = CliSchema.Options
            .Where(o => o.Choices != null || o.Discover != null)
            .Select(o => o.Name);

        Assert.All(annotated, name => Assert.Contains(name, known));
        Assert.Contains(CliSchema.Options, o => o.Discover != null);
    }

    [Theory]
    [InlineData("--remove-formating", "--remove-formatting")]
    [InlineData("--output-folder", "--output-folder")]
    [InlineData("--overwite", "--overwrite")]
    [InlineData("--fix-common-error", "--fix-common-errors")]
    public void SuggestClosest_FindsTheIntendedOption(string typo, string expected)
    {
        Assert.Equal(expected, CliSchema.SuggestClosest(typo));
    }

    [Fact]
    public void SuggestClosest_ReturnsNullForSomethingUnrelated()
    {
        Assert.Null(CliSchema.SuggestClosest("--zzzzzzzzzzzzzzzz"));
    }

    [Fact]
    public void ToJson_IsValidAndDescribesTheContract()
    {
        using var document = JsonDocument.Parse(CliSchema.ToJson());
        var root = document.RootElement;

        Assert.Equal("seconv", root.GetProperty("name").GetString());
        Assert.NotEmpty(root.GetProperty("options").EnumerateArray());
        Assert.NotEmpty(root.GetProperty("subcommands").EnumerateArray());

        // Exit codes are part of the contract a caller scripts against.
        var codes = root.GetProperty("exitCodes").EnumerateArray().Select(e => e.GetProperty("code").GetInt32());
        Assert.Equal([0, 1], codes.ToArray());

        // All three value syntaxes are accepted; the schema has to say so, because guessing
        // wrong used to be indistinguishable from success.
        var syntax = root.GetProperty("valueSyntax").GetProperty("accepted")
            .EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("--option:value", syntax);
        Assert.Contains("--option=value", syntax);
        Assert.Contains("--option value", syntax);
    }

    [Fact]
    public void FindUnknownOption_IgnoresRealOptionsAndTheirValues()
    {
        Assert.Null(SeConv.Program.FindUnknownOption(
            ["a.srt", "subrip", "--fps", "25", "--output-folder:out", "--overwrite", "--remove-formatting"]));
    }

    [Theory]
    [InlineData(new[] { "a.srt", "subrip", "--bogus" }, "--bogus")]
    [InlineData(new[] { "a.srt", "subrip", "--bogus:1" }, "--bogus")]
    [InlineData(new[] { "a.srt", "subrip", "--bogus=1" }, "--bogus")]
    [InlineData(new[] { "a.srt", "subrip", "--overwrite", "-Z" }, "-Z")]
    public void FindUnknownOption_ReportsTheTokenAsTyped(string[] args, string expected)
    {
        Assert.Equal(expected, SeConv.Program.FindUnknownOption(args));
    }

    [Fact]
    public void FindUnknownOption_AcceptsLegacySmashedAliases()
    {
        // Older scripts pass --outputfolder / --FixCommonErrors; strict parsing must not turn
        // those into errors.
        Assert.Null(SeConv.Program.FindUnknownOption(["a.srt", "subrip", "--outputfolder:o", "--FixCommonErrors"]));
    }
}
