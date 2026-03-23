using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class NetflixTimedTextTest
{
    [Fact]
    public void Italic()
    {
        // Arrange
        var input = "This is an <i>italic</i> word!";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void ItalicAndFont()
    {
        // Arrange
        var input = "This is <i><font color=\"red\">red</font></i>" + Environment.NewLine +
                       "<i><font color=\"blue\">and blue</font></i>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void BoldAndFont()
    {
        // Arrange
        var input = "This is <b><font color=\"red\">red</font></b>" + Environment.NewLine +
                    "<b><font color=\"blue\">and blue</font></b>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void FontAndItalic()
    {
        // Arrange
        var input = "This is <font color=\"red\"><i>red</i></font>" + Environment.NewLine +
                    "<font color=\"blue\"><i>and blue</i></font>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void FontAndBold()
    {
        // Arrange
        var input = "This is <font color=\"red\"><b>red</b></font>" + Environment.NewLine +
                    "<font color=\"blue\"><b>and blue</b></font>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void FontAndItalicSeparate()
    {
        // Arrange
        var input = "This is <font color=\"red\">red</font> and <i>italic</i>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }

    [Fact]
    public void ItalicAndBold()
    {
        // Arrange
        var input = "This is <i><b>ib</b></i>" + Environment.NewLine +
                    "<i><b>and ib</b></i>";
        var format = new NetflixTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        // Act
        var raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        // Assert
        Assert.Single(sub.Paragraphs);
        Assert.Equal(input, sub.Paragraphs[0].Text);
    }
}
