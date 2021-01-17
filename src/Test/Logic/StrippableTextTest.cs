using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Logic
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

        [TestMethod]
        public void StrippableTextChangeCasing()
        {
            var st = new StrippableText("this is for www.nikse.dk. thank you.");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("This is for www.nikse.dk. Thank you.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing2()
        {
            var st = new StrippableText("this is for www.nikse.dk! thank you.");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("This is for www.nikse.dk! Thank you.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing3()
        {
            var st = new StrippableText("www.nikse.dk");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("www.nikse.dk", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing4()
        {
            var st = new StrippableText("- hi joe!" + Environment.NewLine + "- hi jane.");
            st.FixCasing(new System.Collections.Generic.List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("- Hi joe!" + Environment.NewLine + "- Hi jane.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing6()
        {
            var st = new StrippableText("- hi joe!" + Environment.NewLine + "- hi jane.");
            st.FixCasing(new System.Collections.Generic.List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("- Hi Joe!" + Environment.NewLine + "- Hi Jane.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing7()
        {
            var st = new StrippableText("[ newsreel narrator ] ominous clouds of war.");
            st.FixCasing(new System.Collections.Generic.List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("[ Newsreel narrator ] Ominous clouds of war.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing8()
        {
            var st = new StrippableText("andy: dad!");
            st.FixCasing(new System.Collections.Generic.List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("Andy: Dad!", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing9()
        {
            var st = new StrippableText("- quit! wait outside!" + Environment.NewLine + "- girl: miss, i've got a headache.");
            st.FixCasing(new System.Collections.Generic.List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("- Quit! Wait outside!" + Environment.NewLine + "- Girl: Miss, i've got a headache.", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeCasing10()
        {
            var st = new StrippableText("Uh, “thor and doctor jones”");
            st.FixCasing(new System.Collections.Generic.List<string> { "Thor", "Jones" }, true, true, true, "Bye.");
            Assert.AreEqual("Uh, “Thor and doctor Jones”", st.MergedString);
        }

        [TestMethod]
        public void StrippableTextChangeEllipsis()
        {
            var st = new StrippableText("…but never could.");
            st.FixCasing(new System.Collections.Generic.List<string>(), true, true, true, "Bye.");
            Assert.AreEqual("…but never could.", st.MergedString);
        }

    }
}
