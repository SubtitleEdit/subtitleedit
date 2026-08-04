using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;

namespace UITests.Controls;

/// <summary>
/// The line-indexed document behind the virtualizing editor: offsets have to survive every edit,
/// because the caret, the selection and undo are all expressed in them.
/// </summary>
public class SyntaxTextDocumentTests
{
    private static SyntaxTextDocument Document(string text) => new() { Text = text };

    [Fact]
    public void EmptyDocumentHasOneEmptyLine()
    {
        var document = new SyntaxTextDocument();

        Assert.Equal(1, document.LineCount);
        Assert.Equal(0, document.TextLength);
        Assert.Equal(string.Empty, document.Text);
    }

    [Theory]
    [InlineData("a\r\nb", "\r\n")]
    [InlineData("a\nb", "\n")]
    [InlineData("a\rb", "\r")]
    public void LineBreakStyleFollowsTheLoadedText(string text, string expected)
    {
        Assert.Equal(expected, Document(text).NewLine);
    }

    [Fact]
    public void OffsetsCountTheLineBreak()
    {
        var document = Document("ab\r\ncd");

        Assert.Equal(2, document.LineCount);
        Assert.Equal(6, document.TextLength);
        Assert.Equal(0, document.GetLineStartOffset(0));
        Assert.Equal(2, document.GetLineEndOffset(0));
        Assert.Equal(4, document.GetLineStartOffset(1));
        Assert.Equal(new SyntaxTextPosition(1, 1), document.GetPosition(5));
        Assert.Equal(5, document.GetOffset(1, 1));
    }

    [Fact]
    public void PositionsInsideALineBreakBelongToTheLineBefore()
    {
        var document = Document("ab\r\ncd");

        // Offset 3 sits between \r and \n - it can only be reported as the end of line 0.
        Assert.Equal(new SyntaxTextPosition(0, 2), document.GetPosition(3));
    }

    [Fact]
    public void GetTextSpansLines()
    {
        var document = Document("one\r\ntwo\r\nthree");

        Assert.Equal("one", document.GetText(0, 3));
        Assert.Equal("one\r\ntwo", document.GetText(0, 8));
        Assert.Equal("two\r\nthr", document.GetText(5, 8));
        Assert.Equal(document.Text, document.GetText(0, document.TextLength));
    }

    [Fact]
    public void InsertWithoutLineBreakKeepsTheLineCount()
    {
        var document = Document("one\r\ntwo");
        document.Insert(3, "!");

        Assert.Equal(2, document.LineCount);
        Assert.Equal("one!\r\ntwo", document.Text);
        Assert.Equal(6, document.GetLineStartOffset(1));
    }

    [Fact]
    public void InsertWithLineBreaksSplitsTheLine()
    {
        var document = Document("onetwo");
        document.Insert(3, "\r\nX\r\n");

        // The seed text has no line break, so the document falls back to
        // Environment.NewLine and normalizes the inserted breaks to it.
        var nl = Environment.NewLine;
        Assert.Equal(3, document.LineCount);
        Assert.Equal($"one{nl}X{nl}two", document.Text);
        Assert.Equal("X", document.GetLine(1));
    }

    [Fact]
    public void RemoveAcrossLinesJoinsThem()
    {
        var document = Document("one\r\ntwo\r\nthree");
        document.Remove(2, 8); // "e", the break, "two" and the second break

        Assert.Equal(1, document.LineCount);
        Assert.Equal("onthree", document.Text);
    }

    [Fact]
    public void RemoveEverythingLeavesOneEmptyLine()
    {
        var document = Document("one\r\ntwo\r\nthree");
        document.Remove(0, document.TextLength);

        Assert.Equal(1, document.LineCount);
        Assert.Equal(string.Empty, document.Text);
        Assert.Equal(0, document.TextLength);
    }

    [Fact]
    public void VersionMovesOnEveryChange()
    {
        var document = Document("a");
        var version = document.Version;

        document.Insert(1, "b");
        Assert.NotEqual(version, document.Version);

        version = document.Version;
        document.Remove(0, 1);
        Assert.NotEqual(version, document.Version);
    }

    [Fact]
    public void OffsetsSurviveManyEdits()
    {
        var document = Document("line0\r\nline1\r\nline2\r\nline3");

        document.Insert(document.GetLineStartOffset(2), "x");
        document.Remove(document.GetLineStartOffset(1), 2);
        document.Insert(document.TextLength, "\r\nline4");

        var text = document.Text;
        for (var line = 0; line < document.LineCount; line++)
        {
            var start = document.GetLineStartOffset(line);
            Assert.Equal(document.GetLine(line), text.Substring(start, document.GetLineLength(line)));
        }

        Assert.Equal(text.Length, document.TextLength);
    }

    [Fact]
    public void CharAtReportsALineBreakAsNewline()
    {
        var document = Document("ab\r\ncd");

        Assert.Equal('b', document.GetCharAt(1));
        Assert.Equal('\n', document.GetCharAt(2));
        Assert.Equal('c', document.GetCharAt(4));
    }

    [Fact]
    public void BigDocumentKeepsOffsetsConsistent()
    {
        var lines = new string[50_000];
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = $"line {i}";
        }

        var document = Document(string.Join("\r\n", lines));

        Assert.Equal(50_000, document.LineCount);
        Assert.Equal(0, document.GetLineStartOffset(0));
        Assert.Equal(document.GetLineStartOffset(49_999), document.GetOffset(49_999, 0));
        Assert.Equal(49_999, document.GetPosition(document.TextLength).Line);
        Assert.Equal("line 25000", document.GetText(document.GetLineStartOffset(25_000), 10));
    }
}
