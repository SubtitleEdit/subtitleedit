using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Core
{
    [TestClass]
    public class HtmlUtilTest
    {
        [TestMethod]
        public void TestRemoveOpenCloseTagCyrillicI()
        {
            const string source = "<\u0456>SubtitleEdit</\u0456>";
            Assert.AreEqual("SubtitleEdit", HtmlUtil.RemoveOpenCloseTags(source, HtmlUtil.TagCyrillicI));
        }

        [TestMethod]
        public void TestRemoveOpenCloseTagFont()
        {
            const string source = "<font color=\"#000\">SubtitleEdit</font>";
            Assert.AreEqual("SubtitleEdit", HtmlUtil.RemoveOpenCloseTags(source, HtmlUtil.TagFont));
        }

        [TestMethod]
        public void RemoveHtmlTags1()
        {
            const string source = "<font color=\"#000\"><i>SubtitleEdit</i></font>";
            Assert.AreEqual("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
        }

        [TestMethod]
        public void RemoveHtmlTags2()
        {
            const string source = "<font size=\"12\" color=\"#000\">Hi <font color=\"#000\"><i>SubtitleEdit</i></font></font>";
            Assert.AreEqual("Hi SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
        }

        [TestMethod]
        public void RemoveHtmlTags3()
        {
            const string source = "{\\an9}<i>How are you? Are you <font='arial'>happy</font> and 1 < 2</i>";
            Assert.AreEqual("{\\an9}How are you? Are you happy and 1 < 2", HtmlUtil.RemoveHtmlTags(source));
        }

        [TestMethod]
        public void RemoveHtmlTagsKeepAss()
        {
            const string source = "{\\an2}<i>SubtitleEdit</i>";
            Assert.AreEqual("{\\an2}SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
        }

        [TestMethod]
        public void RemoveHtmlTagsRemoveAss()
        {
            const string source = "{\\an2}<i>SubtitleEdit</i>";
            Assert.AreEqual("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source, true));
        }

        [TestMethod]
        public void RemoveHtmlTagsBadItalic()
        {
            const string source = "<i>SubtitleEdit<i/>";
            Assert.AreEqual("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
        }

        [TestMethod]
        public void FixInvalidItalicTags1()
        {
            const string s = "<i>foobar<i>?";
            Assert.AreEqual("<i>foobar</i>?", HtmlUtil.FixInvalidItalicTags(s));
        }

        [TestMethod]
        public void FixInvalidItalicTags2()
        {
            string s = "<i>foobar?" + Environment.NewLine + "<i>foobar?";
            Assert.AreEqual("<i>foobar?</i>" + Environment.NewLine + "<i>foobar?</i>", HtmlUtil.FixInvalidItalicTags(s));
        }

    }
}
