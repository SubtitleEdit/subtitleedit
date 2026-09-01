using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Text;

namespace LibSETests.Common;

// EBU STL carries the horizontal justification per text block, but the preview only ever used the
// centered alignments, so changing the justification in the EBU options dialog did nothing on
// screen.
public class SubtitlePositionEbuJustificationTest
{
    private static string MakeStlHeader()
    {
        var buffer = new byte[1024];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0x20;
        }

        Encoding.ASCII.GetBytes("850").CopyTo(buffer, 0);
        Encoding.ASCII.GetBytes("STL25.01").CopyTo(buffer, 3);
        Encoding.ASCII.GetBytes("1").CopyTo(buffer, 11); // display standard code: level 1 teletext
        Encoding.ASCII.GetBytes("00").CopyTo(buffer, 12);
        return Ebu.ReadHeader(buffer).ToString();
    }

    private static Paragraph Positioned(int justificationCode, string? teletextRow, string text = "Hello world", bool usePositions = true)
    {
        var settings = Configuration.Settings.SubtitleSettings;
        var oldJustification = settings.EbuStlJustificationCode;
        var oldMarginBottom = settings.EbuStlMarginBottom;
        var oldNewLineRows = settings.EbuStlNewLineRows;
        try
        {
            settings.EbuStlJustificationCode = justificationCode;
            settings.EbuStlMarginBottom = 2;
            settings.EbuStlNewLineRows = 2;
            var header = MakeStlHeader();
            var subtitle = new Subtitle { Header = header };
            subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000) { MarginV = teletextRow });

            SubtitlePositionToAssa.ApplyPositions(subtitle, header, usePositions);

            return subtitle.Paragraphs[0];
        }
        finally
        {
            settings.EbuStlNewLineRows = oldNewLineRows;
            settings.EbuStlMarginBottom = oldMarginBottom;
            settings.EbuStlJustificationCode = oldJustification;
        }
    }

    private static string Position(int justificationCode, string? teletextRow, string text = "Hello world")
    {
        return Positioned(justificationCode, teletextRow, text).Text;
    }

    [Theory]
    [InlineData(0, "{\\an2}")] // unchanged
    [InlineData(1, "{\\an1}")] // left
    [InlineData(2, "{\\an2}")] // centered
    [InlineData(3, "{\\an3}")] // right
    public void JustificationPicksTheAlignmentColumnOnTheBottomRow(int justificationCode, string expected)
    {
        Assert.StartsWith(expected, Position(justificationCode, "22"));
    }

    [Theory]
    [InlineData(1, "{\\an7}")] // left
    [InlineData(2, "{\\an8}")] // centered
    [InlineData(3, "{\\an9}")] // right
    public void JustificationPicksTheAlignmentColumnOnTheTopRow(int justificationCode, string expected)
    {
        Assert.StartsWith(expected, Position(justificationCode, "2"));
    }

    [Fact]
    public void AnAlignmentAlreadyInTheTextWins()
    {
        Assert.StartsWith("{\\an1}", Position(3, "22", "{\\an1}Hello world"));
    }

    // The code comes from the EBU STL settings, not from Ebu.EbuUiHelper - that carrier only exists
    // once a save or the save options dialog has created one, so a preview before either used to
    // show every line centered whatever the dialog said (user report on PR #14229).
    [Fact]
    public void TheSaveHelperDoesNotDecideTheJustification()
    {
        var oldHelper = Ebu.EbuUiHelper;
        try
        {
            Ebu.EbuUiHelper = null;

            Assert.StartsWith("{\\an1}", Position(1, "22"));
            Assert.StartsWith("{\\an3}", Position(3, "22"));
        }
        finally
        {
            Ebu.EbuUiHelper = oldHelper;
        }
    }

    // Only a subtitle read from an STL file carries teletext rows in MarginV. A subtitle that was
    // typed in or converted from another format has none, and used to be skipped outright - so the
    // justification and the vertical margins of the EBU options dialog did nothing on screen for
    // the far more common case (user report on PR #14228).
    [Theory]
    [InlineData(0, "{\\an2}")] // unchanged
    [InlineData(1, "{\\an1}")] // left
    [InlineData(2, "{\\an2}")] // centered
    [InlineData(3, "{\\an3}")] // right
    public void JustificationAlsoAppliesWithoutATeletextRow(int justificationCode, string expected)
    {
        Assert.StartsWith(expected, Position(justificationCode, null));
    }

    [Fact]
    public void WithoutATeletextRowTheLineSitsAtTheBottomMargin()
    {
        // 2 rows up from the bottom of the 23 teletext rows, in the 288 high libass script.
        Assert.Equal("25", Positioned(2, null).MarginV);
    }

    [Fact]
    public void WithoutATeletextRowTheSecondLineStillFitsAboveTheBottomMargin()
    {
        // Two lines at 2 rows each start higher up, but the last one keeps the same bottom margin.
        Assert.Equal("25", Positioned(2, null, "Hello" + Environment.NewLine + "world").MarginV);
    }

    [Fact]
    public void ATeletextRowFromTheFileStillWins()
    {
        Assert.Equal("38", Positioned(2, "20").MarginV); // 3 rows up from the bottom of 23, in 288
    }

    [Fact]
    public void NothingIsPositionedWhenPositionsAreTurnedOff()
    {
        var p = Positioned(1, null, usePositions: false);

        Assert.Equal("Hello world", p.Text);
        Assert.Null(p.MarginV);
    }
}
