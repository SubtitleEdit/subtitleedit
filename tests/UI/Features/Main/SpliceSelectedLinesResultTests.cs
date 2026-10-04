using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Main;

public class SpliceSelectedLinesResultTests
{
    private static SubtitleLineViewModel Line(string text) => new() { Text = text };

    private static SubtitleLineViewModel Copy(SubtitleLineViewModel line, string text)
    {
        var copy = new SubtitleLineViewModel(line) { Text = text };
        return copy;
    }

    [Fact]
    public void ScatteredSelection_StaysInPlace()
    {
        var rows = new List<SubtitleLineViewModel> { Line("a"), Line("b"), Line("c"), Line("d") };
        var selected = new List<SubtitleLineViewModel> { rows[0], rows[2] };
        var result = new[] { Copy(rows[0], "A"), Copy(rows[2], "C") };

        var spliced = MainViewModel.SpliceSelectedLinesResult(rows, selected, result);

        Assert.Equal(new[] { "A", "b", "C", "d" }, spliced.Select(p => p.Text));
        Assert.Same(rows[1], spliced[1]);
    }

    [Fact]
    public void AddedLine_FollowsItsSourceLine()
    {
        var rows = new List<SubtitleLineViewModel> { Line("a"), Line("b"), Line("c") };
        var selected = new List<SubtitleLineViewModel> { rows[0], rows[2] };
        var added = new SubtitleLineViewModel(rows[0], generateNewId: true) { Text = "A2" };
        var result = new[] { Copy(rows[0], "A1"), added, Copy(rows[2], "C") };

        var spliced = MainViewModel.SpliceSelectedLinesResult(rows, selected, result);

        Assert.Equal(new[] { "A1", "A2", "b", "C" }, spliced.Select(p => p.Text));
    }

    [Fact]
    public void LineMissingFromResult_IsDropped()
    {
        var rows = new List<SubtitleLineViewModel> { Line("a"), Line("b") };
        var selected = new List<SubtitleLineViewModel> { rows[0], rows[1] };
        var result = new[] { Copy(rows[1], "B") };

        var spliced = MainViewModel.SpliceSelectedLinesResult(rows, selected, result);

        Assert.Equal(new[] { "B" }, spliced.Select(p => p.Text));
    }
}
