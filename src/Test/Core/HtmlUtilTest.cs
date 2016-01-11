﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

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

    }
}
