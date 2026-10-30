using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace LibUiLogicTests.Export;

public class PlainTextFormatterTests
{
    private static List<Paragraph> MakeParagraphs() =>
    [
        new("<i>Line one</i>" + Environment.NewLine + "line two.", 1000, 3000),
        new("Second.", 3500, 6000),
    ];

    [Fact]
    public void Generate_MergeAllLines_RemovesStylingAndJoinsOnOneLine()
    {
        var text = PlainTextFormatter.Generate(MakeParagraphs(), new PlainTextOptions { TextProcessing = PlainTextProcessing.Merge });

        Assert.Equal("Line one line two. Second.", text);
    }

    [Fact]
    public void Generate_LineNumbersAndTimeCodes_OnOneLineEach()
    {
        var options = new PlainTextOptions
        {
            ShowLineNumbers = true,
            AddNewLineAfterLineNumber = false,
            ShowTimeCodes = true,
            AddNewLineAfterTimeCode = true,
            TimeCodeFormat = PlainTextFormatter.TimeCodeFormatSeconds,
            TimeCodeSeparator = " - ",
            AddLineAfterText = false,
            AddLineBetweenSubtitles = true,
            RemoveStyling = false,
        };

        var text = PlainTextFormatter.Generate(MakeParagraphs(), options);

        var nl = Environment.NewLine;
        Assert.Equal("1 1.000 - 3.000" + nl + "<i>Line one</i>" + nl + "line two." + nl + nl +
                     "2 3.500 - 6.000" + nl + "Second.", text);
    }

    [Fact]
    public void Generate_Unbreak_NoBlankLines_OneSubtitlePerLine()
    {
        var options = new PlainTextOptions
        {
            TextProcessing = PlainTextProcessing.Unbreak,
            AddLineAfterText = false,
            AddLineBetweenSubtitles = false,
        };

        var text = PlainTextFormatter.Generate(MakeParagraphs(), options);

        Assert.Equal("Line one line two." + Environment.NewLine + "Second.", text);
    }
}
