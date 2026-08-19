using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OperationOrderParserTest
{
    [Fact]
    public void RepeatedFixCommonErrors_RunsOncePerOccurrence()
    {
        // Regression for discussion #11744: repeated --fix-common-errors must run multiple passes.
        var args = new[] { "in.srt", "subrip", "--overwrite", "--fix-common-errors", "--fix-common-errors" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "FixCommonErrors", "FixCommonErrors" }, ops.ToArray());
    }

    [Fact]
    public void PreservesCommandLineOrder()
    {
        var args = new[] { "in.srt", "subrip", "--remove-text-for-hi", "--fix-common-errors", "--balance-lines" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "RemoveTextForHI", "FixCommonErrors", "BalanceLines" }, ops.ToArray());
    }

    [Fact]
    public void InterleavedRepeats_KeepOrderAndCount()
    {
        var args = new[] { "--fix-common-errors", "--balance-lines", "--fix-common-errors" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "FixCommonErrors", "BalanceLines", "FixCommonErrors" }, ops.ToArray());
    }

    [Fact]
    public void PascalCaseAndHyphenAliasesBothMatch()
    {
        var args = new[] { "--FixCommonErrors", "--remove-formatting" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "FixCommonErrors", "RemoveFormatting" }, ops.ToArray());
    }

    [Fact]
    public void RulesOptionAloneImpliesOneFcePass()
    {
        var args = new[] { "in.srt", "subrip", "--fix-common-errors-rules:FixCommas" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "FixCommonErrors" }, ops.ToArray());
    }

    [Fact]
    public void RulesOptionDoesNotAddExtraPassWhenBareFlagPresent()
    {
        var args = new[] { "--fix-common-errors", "--fix-common-errors-rules:FixCommas" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true);

        Assert.Equal(new[] { "FixCommonErrors" }, ops.ToArray());
    }

    [Fact]
    public void NonOperationFlagsAndPositionalsAreIgnored()
    {
        var args = new[] { "my-balance-lines-file.srt", "subrip", "--encoding", "utf-8", "--overwrite" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: false);

        Assert.Empty(ops);
    }

    [Fact]
    public void RemoveFormattingRulesAloneImpliesOnePass_AtFlagPosition()
    {
        var args = new[] { "in.srt", "subrip", "--balance-lines", "--remove-formatting-rules:RemoveItalic", "--redo-casing" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: false, removeFormattingRequested: true);

        Assert.Equal(new[] { "BalanceLines", "RemoveFormatting", "RedoCasing" }, ops.ToArray());
    }

    [Fact]
    public void RemoveFormattingRulesDoesNotAddExtraPassWhenBareFlagPresent()
    {
        var args = new[] { "--remove-formatting", "--remove-formatting-rules:RemoveItalic" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: false, removeFormattingRequested: true);

        Assert.Equal(new[] { "RemoveFormatting" }, ops.ToArray());
    }

    [Fact]
    public void BothRulesOptionsAlone_ImplyPassesInCommandLineOrder()
    {
        var args = new[] { "in.srt", "subrip", "--remove-formatting-rules:RemoveItalic", "--fix-common-errors-rules:FixCommas" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true, removeFormattingRequested: true);

        Assert.Equal(new[] { "RemoveFormatting", "FixCommonErrors" }, ops.ToArray());
    }

    [Fact]
    public void BothRulesOptionsAroundBareOperation_KeepTheirPositions()
    {
        var args = new[] { "--fix-common-errors-rules:FixCommas", "--balance-lines", "--remove-formatting-rules:RemoveItalic" };

        var ops = OperationOrderParser.BuildOperations(args, fceRequested: true, removeFormattingRequested: true);

        Assert.Equal(new[] { "FixCommonErrors", "BalanceLines", "RemoveFormatting" }, ops.ToArray());
    }
}
