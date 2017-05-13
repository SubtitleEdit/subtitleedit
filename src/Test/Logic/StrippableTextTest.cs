using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Logic
{
    [TestClass]
    public class StrippableTextTest
    {

        [TestMethod]
        public void StrippableTextItalic()
        {
            var st = new StrippableText("<i>Hi!</i>");
            Assert.AreEqual(st.Pre, "<i>");
            Assert.AreEqual(st.Post, "!</i>");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextAss()
        {
            var st = new StrippableText("{\\an9}Hi!");
            Assert.AreEqual(st.Pre, "{\\an9}");
            Assert.AreEqual(st.Post, "!");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextFont()
        {
            var st = new StrippableText("<font color=\"red\">Hi!</font>");
            Assert.AreEqual(st.Pre, "<font color=\"red\">");
            Assert.AreEqual(st.Post, "!</font>");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextItalic2()
        {
            var st = new StrippableText("<i>O</i>");
            Assert.AreEqual(st.Pre, "<i>");
            Assert.AreEqual(st.Post, "</i>");
            Assert.AreEqual(st.StrippedText, "O");
        }

        [TestMethod]
        public void StrippableTextItalic3()
        {
            var st = new StrippableText("<i>Hi!");
            Assert.AreEqual(st.Pre, "<i>");
            Assert.AreEqual(st.Post, "!");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextFontDontTouch()
        {
            var st = new StrippableText("{MAN} Hi, how are you today!");
            Assert.AreEqual(st.Pre, "");
            Assert.AreEqual(st.Post, "!");
            Assert.AreEqual(st.StrippedText, "{MAN} Hi, how are you today");
        }

        [TestMethod]
        public void StrippableOnlyPre()
        {
            var st = new StrippableText("(");
            Assert.AreEqual(st.Pre, "(");
            Assert.AreEqual(st.Post, "");
            Assert.AreEqual(st.StrippedText, "");
        }

        [TestMethod]
        public void StrippableOnlyPre2()
        {
            var st = new StrippableText("<");
            Assert.AreEqual(st.Pre, "");
            Assert.AreEqual(st.Post, "");
            Assert.AreEqual(st.StrippedText, "<");
        }

        [TestMethod]
        public void StrippableOnlyPre3()
        {
            var st = new StrippableText("<i>");
            Assert.AreEqual(st.Pre, "<i>");
            Assert.AreEqual(st.Post, "");
            Assert.AreEqual(st.StrippedText, "");
        }

        [TestMethod]
        public void StrippableOnlyText()
        {
            var st = new StrippableText("H");
            Assert.AreEqual(st.Pre, "");
            Assert.AreEqual(st.Post, "");
            Assert.AreEqual(st.StrippedText, "H");
        }

        [TestMethod]
        public void StrippableTextItalicAndFont()
        {
            var st = new StrippableText("<i><font color=\"red\">Hi!</font></i>");
            Assert.AreEqual(st.Pre, "<i><font color=\"red\">");
            Assert.AreEqual(st.Post, "!</font></i>");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextItalicAndMore()
        {
            var st = new StrippableText("<i>...<b>Hi!</b></i>");
            Assert.AreEqual(st.Pre, "<i>...<b>");
            Assert.AreEqual(st.Post, "!</b></i>");
            Assert.AreEqual(st.StrippedText, "Hi");
        }

        [TestMethod]
        public void StrippableTextChangeCasing()
        {
            var st = new StrippableText("this is for www.nikse.dk. thank you.");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual(st.MergedString, "This is for www.nikse.dk. Thank you.");
        }

        [TestMethod]
        public void StrippableTextChangeCasing2()
        {
            var st = new StrippableText("this is for www.nikse.dk! thank you.");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual(st.MergedString, "This is for www.nikse.dk! Thank you.");
        }

        [TestMethod]
        public void StrippableTextChangeCasing3()
        {
            var st = new StrippableText("www.nikse.dk");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual(st.MergedString, "www.nikse.dk");
        }

    }
}
