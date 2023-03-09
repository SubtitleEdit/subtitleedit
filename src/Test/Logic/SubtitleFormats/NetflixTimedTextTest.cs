using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class NetflixTimedTextTest
    {
        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }

        [TestMethod]
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
            Assert.AreEqual(1, sub.Paragraphs.Count);
            Assert.AreEqual(input, sub.Paragraphs[0].Text);
        }
    }
}
