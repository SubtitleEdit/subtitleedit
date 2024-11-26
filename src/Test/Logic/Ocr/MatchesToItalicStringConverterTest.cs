using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;

namespace Test.Logic.Ocr
{
    [TestClass]
    public class MatchesToItalicStringConverterTest
    {

        [TestMethod]
        public void NormalItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("<i>Hallo!</i>", result);
        }

        [TestMethod]
        public void NormalNonItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", false, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Hallo!", result);
        }

        [TestMethod]
        public void TestQuotesInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(":", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("He said: <i>''Go now!''</i>", result);
        }

        [TestMethod]
        public void TestColonInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("[", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("O", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("v", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("c", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("m", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("]", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("H", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("y", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("u", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("?", true, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("<i>[Over coms] How are you?</i>", result);
        }

        [TestMethod]
        public void TestItalicAndColon()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("L", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(":", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("T", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("h", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("y", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("h", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(".", true, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Leonard:<i>They're here.</i>", result);
        }

        [TestMethod]
        public void TestItalicAndColon2()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("C", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("A", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("E", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("S", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("A", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("R", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(":", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("I", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("v", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("h", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(".", true, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("CAESAR: <i>I live here.</i>", result);
        }

        [TestMethod]
        public void TestNonItalicAndParentheses()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("-(", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("L", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("A", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("U", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("I", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("N", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(")", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(Environment.NewLine, false, 0, Guid.NewGuid().ToString()) { },
                new VobSubOcr.CompareMatch("-", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("(", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("B", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("Y", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("E", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("B", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("Y", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("E", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(")", false, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("-(LAUGHING)" + Environment.NewLine + "-(BYE BYE)", result);
        }

        [TestMethod]
        public void TestItalicAndBrackets()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("[", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("L", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("]", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("T", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("h", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("y", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("h", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(".", true, 0, Guid.NewGuid().ToString()),
            };

            var result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("[Leonard]<i>They're here.</i>", result);
        }

        [TestMethod]
        public void TestWordInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("He said <i>now!</i>", result);
        }

        public void TestWordInItalicSecondLine()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("J", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(":", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("\r", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("\n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("b", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("y", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Joe:\r\n<i>Godbye!</i>", result);
        }

        public void TestWordInItalicSecondLineAndBrackets()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("[J", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("]:", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("\r", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("\n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("b", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("y", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("[Joe]:\r\n<i>Godbye!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("A", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("g", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("-", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Hi Angi-<i>san!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalicWithDash()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("I", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("m", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("t", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("-", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Iwamoto-<i>sensai!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalicWithStartDash()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("-", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("E", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("x", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("p", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("r", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(",", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("- Explore, <i>now!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalicQuote()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("Hi", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("t", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("ars", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Hi all'<i>stars!</i>", result);
        }
    }
}
