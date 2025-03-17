using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Tests.Logic
{
    [TestClass]
    public class StrippableTextTest
    {

        [TestMethod]
        public void StrippableTextItalic()
        {
            var st = new StrippableText("<i>Hi!</i>");
            Assert.AreEqual("<i>", st.Pre);
            Assert.AreEqual("!</i>", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextAss()
        {
            var st = new StrippableText("{\\an9}Hi!");
            Assert.AreEqual("{\\an9}", st.Pre);
            Assert.AreEqual("!", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextFont()
        {
            var st = new StrippableText("<font color=\"red\">Hi!</font>");
            Assert.AreEqual("<font color=\"red\">", st.Pre);
            Assert.AreEqual("!</font>", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextItalic2()
        {
            var st = new StrippableText("<i>O</i>");
            Assert.AreEqual("<i>", st.Pre);
            Assert.AreEqual("</i>", st.Post);
            Assert.AreEqual("O", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextItalic3()
        {
            var st = new StrippableText("<i>Hi!");
            Assert.AreEqual("<i>", st.Pre);
            Assert.AreEqual("!", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextFontDontTouch()
        {
            var st = new StrippableText("{MAN} Hi, how are you today!");
            Assert.AreEqual("", st.Pre);
            Assert.AreEqual("!", st.Post);
            Assert.AreEqual("{MAN} Hi, how are you today", st.StrippedText);
        }

        [TestMethod]
        public void StrippableOnlyPre()
        {
            var st = new StrippableText("(");
            Assert.AreEqual("(", st.Pre);
            Assert.AreEqual("", st.Post);
            Assert.AreEqual("", st.StrippedText);
        }

        [TestMethod]
        public void StrippableOnlyPre2()
        {
            var st = new StrippableText("<");
            Assert.AreEqual("", st.Pre);
            Assert.AreEqual("", st.Post);
            Assert.AreEqual("<", st.StrippedText);
        }

        [TestMethod]
        public void StrippableOnlyPre3()
        {
            var st = new StrippableText("<i>");
            Assert.AreEqual("<i>", st.Pre);
            Assert.AreEqual("", st.Post);
            Assert.AreEqual("", st.StrippedText);
        }

        [TestMethod]
        public void StrippableOnlyText()
        {
            var st = new StrippableText("H");
            Assert.AreEqual("", st.Pre);
            Assert.AreEqual("", st.Post);
            Assert.AreEqual("H", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextItalicAndFont()
        {
            var st = new StrippableText("<i><font color=\"red\">Hi!</font></i>");
            Assert.AreEqual("<i><font color=\"red\">", st.Pre);
            Assert.AreEqual("!</font></i>", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }

        [TestMethod]
        public void StrippableTextItalicAndMore()
        {
            var st = new StrippableText("<i>...<b>Hi!</b></i>");
            Assert.AreEqual("<i>...<b>", st.Pre);
            Assert.AreEqual("!</b></i>", st.Post);
            Assert.AreEqual("Hi", st.StrippedText);
        }
    }
}
