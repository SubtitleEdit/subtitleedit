using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using System;

namespace Test.Core
{
    [TestClass]
    public class StringExtensionsTest
    {

        [TestMethod]
        public void LineStartsWithHtmlTagEmpty()
        {
            string test = string.Empty;
            Assert.IsFalse(test.LineStartsWithHtmlTag(true));
        }

        [TestMethod]
        public void LineStartsWithHtmlTagNull()
        {
            string test = null;
            Assert.IsFalse(test.LineStartsWithHtmlTag(true));
        }

        [TestMethod]
        public void LineStartsWithHtmlTagItalic()
        {
            const string test = "<i>";
            Assert.IsTrue(test.LineStartsWithHtmlTag(true));
        }

        [TestMethod]
        public void EndsWithEmpty()
        {
            string test = string.Empty;
            Assert.IsFalse(test.EndsWith('?'));
        }

        [TestMethod]
        public void EndsWithHtmlTagEmpty()
        {
            string test = string.Empty;
            Assert.IsFalse(test.LineEndsWithHtmlTag(true));
        }

        [TestMethod]
        public void EndsWithHtmlTagItalic()
        {
            const string test = "<i>Hej</i>";
            Assert.IsTrue(test.LineEndsWithHtmlTag(true));
        }

        [TestMethod]
        public void LineBreakStartsWithHtmlTagEmpty()
        {
            string test = string.Empty;
            Assert.IsFalse(test.LineBreakStartsWithHtmlTag(true));
        }

        [TestMethod]
        public void LineBreakStartsWithHtmlTagItalic()
        {
            string test = "<i>Hej</i>" + Environment.NewLine + "<i>Hej</i>";
            Assert.IsTrue(test.LineBreakStartsWithHtmlTag(true));
        }

        [TestMethod]
        public void LineBreakStartsWithHtmlTagFont()
        {
            string test = "Hej!" + Environment.NewLine + "<font color=FFFFFF>Hej!</font>";
            Assert.IsTrue(test.LineBreakStartsWithHtmlTag(true, true));
        }

        //QUESTION: fix three lines?
        //[TestMethod]
        //public void LineBreakStartsWithHtmlTagFontThreeLines()
        //{
        //    string test = "Hej!" + Environment.NewLine + "Hej!" + Environment.NewLine + "<font color=FFFFFF>Hej!</font>";
        //    Assert.IsTrue(test.LineBreakStartsWithHtmlTag(true, true));
        //}

        [TestMethod]
        public void LineBreakStartsWithHtmlTagFontFalse()
        {
            const string test = "<font color=FFFFFF>Hej!</font>";
            Assert.IsFalse(test.LineBreakStartsWithHtmlTag(true, true));
        }

        [TestMethod]
        public void SplitToLines1()
        {
            string input = "Line1" + Environment.NewLine + "Line2";
            Assert.AreEqual(2, input.SplitToLines().Count);
        }

        [TestMethod]
        public void SplitToLines2()
        {
            string input = "Line1\r\r\nLine2\r\nLine3\rLine4\u2028Line5\nLine6";
            var res = input.SplitToLines();
            Assert.AreEqual(6, res.Count);
            Assert.AreEqual("Line1", res[0]);
            Assert.AreEqual("Line2", res[1]);
            Assert.AreEqual("Line3", res[2]);
            Assert.AreEqual("Line4", res[3]);
            Assert.AreEqual("Line5", res[4]);
            Assert.AreEqual("Line6", res[5]);
        }

        [TestMethod]
        public void SplitToLinesEmptyLines1()
        {
            string input = "\n\nLine3\r\n\r\nLine5\r";
            var res = input.SplitToLines();
            Assert.AreEqual(6, res.Count);
            Assert.AreEqual(string.Empty, res[0]);
            Assert.AreEqual(string.Empty, res[1]);
            Assert.AreEqual("Line3", res[2]);
            Assert.AreEqual(string.Empty, res[3]);
            Assert.AreEqual("Line5", res[4]);
            Assert.AreEqual(string.Empty, res[5]);
        }

        [TestMethod]
        public void SplitToLinesEmptyLines2()
        {
            string input = "\r\n\r\nLine3\u2028\rLine5\r\r\n";
            var res = input.SplitToLines();
            Assert.AreEqual(6, res.Count);
            Assert.AreEqual(string.Empty, res[0]);
            Assert.AreEqual(string.Empty, res[1]);
            Assert.AreEqual("Line3", res[2]);
            Assert.AreEqual(string.Empty, res[3]);
            Assert.AreEqual("Line5", res[4]);
            Assert.AreEqual(string.Empty, res[5]);
        }

        [TestMethod]
        public void FixExtraSpaces()
        {
            string input = "Hallo  world!";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("Hallo world!", res);
        }

        [TestMethod]
        public void FixExtraSpaces2()
        {
            string input = "Hallo   world!";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("Hallo world!", res);
        }

        [TestMethod]
        public void FixExtraSpaces3()
        {
            string input = "Hallo world!  ";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("Hallo world! ", res);
        }

        [TestMethod]
        public void FixExtraSpaces4()
        {
            string input = "Hallo " + Environment.NewLine + " world!";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("Hallo" + Environment.NewLine + "world!", res);
        }


        [TestMethod]
        public void FixExtraSpaces5()
        {
            string input = "a  " + Environment.NewLine + "b";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("a" + Environment.NewLine + "b", res);
        }

        [TestMethod]
        public void FixExtraSpaces6()
        {
            string input = "a" + Environment.NewLine + "   b";
            var res = input.FixExtraSpaces();
            Assert.AreEqual("a" + Environment.NewLine + "b", res);
        }

        [TestMethod]
        public void RemoveChar1()
        {
            string input = "Hallo world!";
            var res = input.RemoveChar(' ');
            Assert.AreEqual("Halloworld!", res);
        }

        [TestMethod]
        public void RemoveChar2()
        {
            string input = " Hallo  world! ";
            var res = input.RemoveChar(' ');
            Assert.AreEqual("Halloworld!", res);
        }

        [TestMethod]
        public void CountLetters1()
        {
            string input = " Hallo  world! ";
            var res = input.CountCharacters(false);
            Assert.AreEqual(" Hallo  world! ".Length, res);
        }

        [TestMethod]
        public void CountLetters2()
        {
            string input = " Hallo " + Environment.NewLine + " world! ";
            var res = input.CountCharacters(true);
            Assert.AreEqual("Halloworld!".Length, res);
        }

        [TestMethod]
        public void CountLetters3()
        {
            string input = " Hallo" + Environment.NewLine + "world!";
            var res = input.CountCharacters(false);
            Assert.AreEqual(" Halloworld!".Length, res);
        }

        [TestMethod]
        public void CountLetters4Ssa()
        {
            string input = "{\\an1}Hallo";
            var res = input.CountCharacters(true);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters4Html()
        {
            string input = "<i>Hallo</i>";
            var res = input.CountCharacters(true);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters5HtmlFont()
        {
            string input = "<font color=\"red\"><i>Hal lo<i></font>";
            var res = input.CountCharacters(true);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters6HtmlFontMultiLine()
        {
            string input = "<font color=\"red\"><i>Hal lo<i></font>" + Environment.NewLine + "<i>Bye!</i>";
            var res = input.CountCharacters(true);
            Assert.AreEqual("HalloBye!".Length, res);
        }

    }
}
