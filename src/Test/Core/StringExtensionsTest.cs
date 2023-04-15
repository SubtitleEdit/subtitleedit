using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

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
        public void RemoveChar3()
        {
            string input = " Hallo  world! ";
            var res = input.RemoveChar(' ', '!');
            Assert.AreEqual("Halloworld", res);
        }

        [TestMethod]
        public void RemoveChar4()
        {
            string input = " Hallo  world! ";
            var res = input.RemoveChar(' ', '!', 'H');
            Assert.AreEqual("alloworld", res);
        }

        [TestMethod]
        public void CountLetters1()
        {
            var input = " Hallo  world! ";
            var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
            Assert.AreEqual(" Hallo  world! ".Length, res);
        }

        [TestMethod]
        public void CountLetters2()
        {
            var input = " Hallo " + Environment.NewLine + " world! ";
            var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
            Assert.AreEqual("Halloworld!".Length, res);
        }

        [TestMethod]
        public void CountLetters3()
        {
            var input = " Hallo" + Environment.NewLine + "world!";
            var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
            Assert.AreEqual(" Halloworld!".Length, res);
        }

        [TestMethod]
        public void CountLetters4Ssa()
        {
            var input = "{\\an1}Hallo";
            var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters4Html()
        {
            var input = "<i>Hallo</i>";
            var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters5HtmlFont()
        {
            var input = "<font color=\"red\"><i>Hal lo<i></font>";
            var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
            Assert.AreEqual("Hallo".Length, res);
        }

        [TestMethod]
        public void CountLetters6HtmlFontMultiLine()
        {
            var input = "<font color=\"red\"><i>Hal lo<i></font>" + Environment.NewLine + "<i>Bye!</i>";
            var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
            Assert.AreEqual("HalloBye!".Length, res);
        }

        [TestMethod]
        public void ToggleCasing1()
        {
            var input = "how are you";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("How Are You", res);
        }

        [TestMethod]
        public void ToggleCasing1WithItalic()
        {
            var input = "how <i>are</i> you";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("How <i>Are</i> You", res);
        }

        [TestMethod]
        public void ToggleCasing1WithItalicStart()
        {
            var input = "<i>how</i> are you";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("<i>How</i> Are You", res);
        }

        [TestMethod]
        public void ToggleCasing1WithItalicEnd()
        {
            var input = "how are <i>you</i>";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("How Are <i>You</i>", res);
        }

        [TestMethod]
        public void ToggleCasing1WithItalicEndAndBold()
        {
            var input = "how are <i><b>you</b></i>";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("How Are <i><b>You</b></i>", res);
        }

        [TestMethod]
        public void ToggleCasing2()
        {
            var input = "How Are You";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("HOW ARE YOU", res);
        }

        [TestMethod]
        public void ToggleCasing3()
        {
            var input = "HOW ARE YOU";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("how are you", res);
        }

        [TestMethod]
        public void ToggleCasingWithFont()
        {
            var input = "<font color=\"Red\">HOW ARE YOU</font>";
            var res = input.ToggleCasing(new SubRip());
            Assert.AreEqual("<font color=\"Red\">how are you</font>", res);
        }

        [TestMethod]
        public void ToggleCasingAssa()
        {
            var input = "{\\i1}This is an example…{\\i0}";
            var res = input.ToggleCasing(new AdvancedSubStationAlpha());
            Assert.AreEqual("{\\i1}THIS IS AN EXAMPLE…{\\i0}", res);
        }

        [TestMethod]
        public void ToggleCasingAssaSoftLineBreak()
        {
            var input = "HOW ARE\\nYOU?";
            var res = input.ToggleCasing(new AdvancedSubStationAlpha());
            Assert.AreEqual("how are\\nyou?", res);
        }
    }
}
