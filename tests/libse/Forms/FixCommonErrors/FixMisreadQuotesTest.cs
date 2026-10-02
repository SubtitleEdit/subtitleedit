using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixMisreadQuotesTest
{
    private static string Fix(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        new FixMisreadQuotes().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs[0].Text;
    }

    [Theory]
    [InlineData("\"Hello'", "\"Hello\"")]
    [InlineData("\"Hello.'", "\"Hello.\"")]
    [InlineData("\"I don't know.'", "\"I don't know.\"")]
    [InlineData("He said \"yes'.", "He said \"yes\".")]
    [InlineData("\"Stop' he said.", "\"Stop\" he said.")]
    [InlineData("'Hello\"", "\"Hello\"")]
    [InlineData("'I don't know.\"", "\"I don't know.\"")]
    [InlineData("'Hello'", "\"Hello\"")]
    [InlineData("'Hello.'", "\"Hello.\"")]
    [InlineData("'Hello'.", "\"Hello\".")]
    [InlineData("'I can't do that.'", "\"I can't do that.\"")]
    [InlineData("<i>\"Hello'</i>", "<i>\"Hello\"</i>")]
    [InlineData("<i>'Hello'</i>", "<i>\"Hello\"</i>")]
    [InlineData("{\\an8}'Hello'", "{\\an8}\"Hello\"")]
    [InlineData("''Hello''", "\"Hello\"")]
    [InlineData("''Hello'", "\"Hello\"")]
    [InlineData("'Hello''", "\"Hello\"")]
    [InlineData("\"Hello''", "\"Hello\"")]
    [InlineData("''Hello\"", "\"Hello\"")]
    [InlineData("''I don't know.''", "\"I don't know.\"")]
    public void Fixes(string input, string expected)
    {
        Assert.Equal(expected, Fix(input));
    }

    [Fact]
    public void QuoteSpanningTwoLines()
    {
        Assert.Equal("\"Hello,\r\nmy friend.\"", Fix("\"Hello,\r\nmy friend.'"));
        Assert.Equal("\"Hello,\nmy friend.\"", Fix("'Hello,\nmy friend.'"));
    }

    [Fact]
    public void DialogIsFixedLineByLine()
    {
        Assert.Equal("- \"Hello\"\r\n- \"Bye.\"", Fix("- \"Hello'\r\n- 'Bye.'"));
        Assert.Equal("- \"Hello.\"\n- Bye.", Fix("- 'Hello.\"\n- Bye."));
    }

    [Theory]
    [InlineData("\"Hello.\"")]
    [InlineData("I don't know.")]
    [InlineData("It's the boys' toys.")]
    [InlineData("'Cause I said so.")]
    [InlineData("Rock 'n' roll.")]
    [InlineData("Back in the '60s.")]
    [InlineData("\"I'm goin'")]
    [InlineData("'Cause I said so\"")]
    [InlineData("\"It's the boys' toys")]
    [InlineData("\"He said 'hi'")]
    [InlineData("She said 'hi' to me.")]
    [InlineData("I'm 'fine'")]
    [InlineData("\"Hello' and 'bye'")]
    [InlineData("'Hello' and \"bye\"")]
    [InlineData("„Hallo'")]
    [InlineData("'Cause I'm leavin'")]
    [InlineData("'em all'")]
    [InlineData("")]
    public void Unchanged(string input)
    {
        Assert.Equal(input, Fix(input));
    }

    [Theory]
    [InlineData("nl", "Ik kom 's avonds terug,\" zei hij.")]
    [InlineData("it", "\"Aspetta un po',")]
    [InlineData("en", "Ik kom 's avonds terug,\" zei hij.", "Ik kom \"s avonds terug,\" zei hij.")]
    public void EnglishOnly(string language, string input, string? expected = null)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        new FixMisreadQuotes().Fix(subtitle, new EmptyFixCallback { Language = language });

        Assert.Equal(expected ?? input, subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void RunsBeforeAddMissingQuotes()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("\"Hello'", 0, 2000));
        new FixMisreadQuotes().Fix(subtitle, new EmptyFixCallback());
        new AddMissingQuotes().Fix(subtitle, new EmptyFixCallback());

        Assert.Equal("\"Hello\"", subtitle.Paragraphs[0].Text);
    }
}
