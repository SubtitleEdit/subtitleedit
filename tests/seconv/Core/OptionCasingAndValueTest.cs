using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Covers the two strict-parsing companions in <see cref="SeConv.Program"/>:
/// <c>CanonicalizeOptionCasing</c>, which keeps the SE 4.x casings working now that
/// Spectre matches option names case-sensitively, and <c>FindMissingOptionValue</c>,
/// which catches a value option with no value before Spectre's strict-mode retry can
/// bind its synthetic "__default_command" token as the value.
/// </summary>
public class OptionCasingAndValueTest
{
    [Theory]
    [InlineData("--fixcommonerrors", "--fix-common-errors")]
    [InlineData("--FixCommonErrors", "--fix-common-errors")]
    [InlineData("--FIX-COMMON-ERRORS", "--fix-common-errors")]
    [InlineData("--REDOCASING", "--redo-casing")]
    [InlineData("--removetextforhi", "--remove-text-for-hi")]
    [InlineData("--OUTPUTFOLDER", "--output-folder")]
    public void CanonicalizeOptionCasing_RewritesKnownCasingVariants(string given, string expected)
    {
        var result = SeConv.Program.CanonicalizeOptionCasing(["a.srt", given]);

        Assert.Equal(["a.srt", expected], result);
    }

    [Fact]
    public void CanonicalizeOptionCasing_PreservesEmbeddedValues()
    {
        var result = SeConv.Program.CanonicalizeOptionCasing(
            ["--OUTPUTFOLDER:my:dir", "--DeleteFirst=2", "--encoding:utf-8"]);

        Assert.Equal(["--output-folder:my:dir", "--delete-first=2", "--encoding:utf-8"], result);
    }

    [Fact]
    public void CanonicalizeOptionCasing_LeavesUnknownAndNonOptionTokensAlone()
    {
        var args = new[] { "a.srt", "subrip", "--bogus-flag", "-00:00:01:000", "--", "some value" };

        var result = SeConv.Program.CanonicalizeOptionCasing(args);

        Assert.Equal(args, result);
    }

    [Fact]
    public void FindMissingOptionValue_ReportsValueOptionAtEndOfArgs()
    {
        // Without the pre-check this converted successfully into a folder literally
        // named "__default_command" and exited 0.
        var missing = SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--output-folder"]);

        Assert.Equal("--output-folder", missing);
    }

    [Fact]
    public void FindMissingOptionValue_ReportsValueOptionFollowedByAnotherOption()
    {
        var missing = SeConv.Program.FindMissingOptionValue(["a.srt", "--output-folder", "--json", "--format", "subrip"]);

        Assert.Equal("--output-folder", missing);
    }

    [Fact]
    public void FindMissingOptionValue_AcceptsSuppliedAndEmbeddedValues()
    {
        Assert.Null(SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--output-folder", "out"]));
        Assert.Null(SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--output-folder:out"]));
    }

    [Fact]
    public void FindMissingOptionValue_AcceptsNegativeNumberValues()
    {
        Assert.Null(SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--offset", "-00:00:01:000"]));
    }

    [Fact]
    public void FindMissingOptionValue_ExemptsOptionalValueAndFlagOptions()
    {
        // --apply-min-gap is a Spectre FlagValue: bare use is legal and takes the gap
        // from the settings. Plain flags carry no value at all.
        Assert.Null(SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--apply-min-gap"]));
        Assert.Null(SeConv.Program.FindMissingOptionValue(["a.srt", "--format", "subrip", "--overwrite"]));
    }
}
