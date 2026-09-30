using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace LibSETests.Core.Cea608;

public class CcDataC608ParserRollUpTest
{
    private static List<Paragraph> Decode(params (int a, int b)[] pairs)
    {
        var paragraphs = new List<Paragraph>();
        var parser = new CcDataC608Parser();
        parser.DisplayScreen += data => Cea608CueBuilder.Add(paragraphs, SerializedScreenText.GetText(data.Screen), data.Start, data.End);
        var t = 0;
        foreach (var (a, b) in pairs)
        {
            parser.AddData(t, new[] { a, b });
            t += 100;
        }

        return paragraphs;
    }

    /// <summary>
    /// A recording cut in the middle of a roll-up line starts with characters before any command.
    /// They were dropped (C-SPAN sample: the opening "DO"); once the first command is a roll-up they
    /// are shown, and the preamble that follows moves the roll-up window - rows and all - to its
    /// base row, so the carriage return scrolls them like any other row.
    /// </summary>
    [Fact]
    public void CharactersBeforeFirstRollUpCommandAreKept()
    {
        // control codes are sent twice, as in the broadcast (the parser skips the repeat)
        var paragraphs = Decode(
            (0x44, 0x4F), // "DO"
            (0x14, 0x25), (0x14, 0x25), // RU2
            (0x14, 0x2D), (0x14, 0x2D), // CR
            (0x80, 0x80), (0x80, 0x80), (0x80, 0x80), (0x80, 0x80), (0x80, 0x80), // padding, 0.5 s
            (0x13, 0x50), (0x13, 0x50), // PAC row 12
            (0x48, 0x49), // "HI"
            (0x14, 0x25), (0x14, 0x25), // RU2
            (0x14, 0x2D), (0x14, 0x2D), // CR
            (0x13, 0x50), (0x13, 0x50), // PAC row 12
            (0x59, 0x4F), // "YO"
            (0x14, 0x2C), (0x14, 0x2C)); // EDM

        Assert.Equal(new[] { "DO", "DO\nHI", "HI\nYO" }, paragraphs.ConvertAll(p => p.Text.Replace("\r\n", "\n")));
        Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds);
    }

    /// <summary>
    /// Pop-on text is loaded off screen, so characters before a first RCL were never displayed.
    /// </summary>
    [Fact]
    public void CharactersBeforeFirstPopOnCommandAreDropped()
    {
        var paragraphs = Decode(
            (0x44, 0x4F), // "DO"
            (0x14, 0x20), (0x14, 0x20), // RCL
            (0x14, 0x70), (0x14, 0x70), // PAC row 15
            (0x48, 0x49), // "HI"
            (0x14, 0x2F), (0x14, 0x2F), // EOC
            (0x14, 0x2C), (0x14, 0x2C)); // EDM

        Assert.Equal(new[] { "HI" }, paragraphs.ConvertAll(p => p.Text));
    }
}
