using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using System;

namespace Test.Core
{
    [TestClass]
    public class UnitTest1
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

    }
}
