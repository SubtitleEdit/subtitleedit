using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Forms;

namespace Test.Logic.Forms
{
    [TestClass]
    public class MoveWordUpDownTest
    {

        [TestMethod]
        public void MoveWordDownSimple()
        {
            var x = new MoveWordUpDown("Hallo my", "dear friend!");
            x.MoveWordDown();
            Assert.AreEqual("Hallo", x.S1);
            Assert.AreEqual("my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
        }

        [TestMethod]
        public void MoveWordUpSimple()
        {
            var x = new MoveWordUpDown("Hallo my", "dear friend!");
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear", x.S1);
            Assert.AreEqual("friend!", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }

        [TestMethod]
        public void MoveWordDownSimpleAssTag()
        {
            var x = new MoveWordUpDown("{\\an8}Hallo my", "dear friend!");
            x.MoveWordDown();
            Assert.AreEqual("{\\an8}Hallo", x.S1);
            Assert.AreEqual("my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
        }

        [TestMethod]
        public void MoveWordUpSimpleAssTag()
        {
            var x = new MoveWordUpDown("{\\an8}Hallo my", "dear friend!");
            x.MoveWordUp();
            Assert.AreEqual("{\\an8}Hallo my dear", x.S1);
            Assert.AreEqual("friend!", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("{\\an8}Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
            x.MoveWordUp();
            Assert.AreEqual("{\\an8}Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }

        [TestMethod]
        public void MoveWordDownSimpleHtmlTag()
        {
            var x = new MoveWordUpDown("<i>Hallo my</i>", "<i>dear friend!</i>");
            x.MoveWordDown();
            Assert.AreEqual("<i>Hallo</i>", x.S1);
            Assert.AreEqual("<i>my dear friend!</i>", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("<i>Hallo my dear friend!</i>", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("<i>Hallo my dear friend!</i>", x.S2);
        }

        [TestMethod]
        public void MoveWordUpSimpleHtmlTag()
        {
            var x = new MoveWordUpDown("<i>Hallo my</i>", "<i>dear friend!</i>");
            x.MoveWordUp();
            Assert.AreEqual("<i>Hallo my dear</i>", x.S1);
            Assert.AreEqual("<i>friend!</i>", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("<i>Hallo my dear friend!</i>", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
            x.MoveWordUp();
            Assert.AreEqual("<i>Hallo my dear friend!</i>", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }

        [TestMethod]
        public void MoveWordDownHtmlFontTag()
        {
            var x = new MoveWordUpDown("<font color=\"red\">Hallo my</font>", "dear friend!");
            x.MoveWordDown();
            Assert.AreEqual("<font color=\"red\">Hallo</font>", x.S1);
            Assert.AreEqual("my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
        }

        [TestMethod]
        public void MoveWordUpHtmlFontTag()
        {
            var x = new MoveWordUpDown("Hallo my", "<font color=\"red\">dear friend!</font>");
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear", x.S1);
            Assert.AreEqual("<font color=\"red\">friend!</font>", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }

        [TestMethod]
        public void MoveWordDownAssFontTag()
        {
            var x = new MoveWordUpDown("{\\fnArabic Typesetting\\fs34}AAA BBB CCC", "DDD");
            x.MoveWordDown();
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}AAA BBB", x.S1);
            Assert.AreEqual("CCC DDD", x.S2);
            x.MoveWordDown();
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}AAA", x.S1);
            Assert.AreEqual("BBB CCC DDD", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("AAA BBB CCC DDD", x.S2);
        }

        [TestMethod]
        public void MoveWordDownAssFontTag2()
        {
            var x = new MoveWordUpDown("AAA BBB CCC", "{\\fnArabic Typesetting\\fs34}DDD");
            x.MoveWordDown();
            Assert.AreEqual("AAA BBB", x.S1);
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}CCC DDD", x.S2);
            x.MoveWordDown();
            Assert.AreEqual("AAA", x.S1);
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}BBB CCC DDD", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}AAA BBB CCC DDD", x.S2);
        }

        [TestMethod]
        public void MoveWordUpAssFontTag()
        {
            var x = new MoveWordUpDown("AAA BBB", "{\\fnArabic Typesetting\\fs34}CCC DDD");
            x.MoveWordUp();
            Assert.AreEqual("AAA BBB CCC", x.S1);
            Assert.AreEqual("{\\fnArabic Typesetting\\fs34}DDD", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("AAA BBB CCC DDD", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
            x.MoveWordUp();
            Assert.AreEqual("AAA BBB CCC DDD", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }


        [TestMethod]
        public void MoveWordDownSimpleHtmlTagInline()
        {
            var x = new MoveWordUpDown("Hallo <i>my</i> dear", "friend!");
            x.MoveWordDown();
            Assert.AreEqual("Hallo <i>my</i>", x.S1);
            Assert.AreEqual("dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual("Hallo", x.S1);
            Assert.AreEqual("my dear friend!", x.S2);
            x.MoveWordDown();
            Assert.AreEqual(string.Empty, x.S1);
            Assert.AreEqual("Hallo my dear friend!", x.S2);
        }

        [TestMethod]
        public void MoveWordUpSimpleHtmlTagInline()
        {
            var x = new MoveWordUpDown("Hallo", "my <i>dear</i> friend!");
            x.MoveWordUp();
            Assert.AreEqual("Hallo my", x.S1);
            Assert.AreEqual("<i>dear</i> friend!", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear", x.S1);
            Assert.AreEqual("friend!", x.S2);
            x.MoveWordUp();
            Assert.AreEqual("Hallo my dear friend!", x.S1);
            Assert.AreEqual(string.Empty, x.S2);
        }

        [TestMethod]
        public void MoveWordUpAutoBr()
        {
            var x = new MoveWordUpDown("0123456789A 0123456789A 0123456789A", "0123456789A 0123456789A 0123456789A");
            x.MoveWordUp();
            Assert.AreEqual("0123456789A 0123456789A" + Environment.NewLine + "0123456789A 0123456789A", x.S1);
            Assert.AreEqual("0123456789A 0123456789A", x.S2);
        }

        [TestMethod]
        public void MoveWordDownAutoBr()
        {
            var x = new MoveWordUpDown("0123456789A 0123456789A 0123456789A", "0123456789A 0123456789A 0123456789A");
            x.MoveWordDown();
            Assert.AreEqual("0123456789A 0123456789A", x.S1);
            Assert.AreEqual("0123456789A 0123456789A" + Environment.NewLine + "0123456789A 0123456789A", x.S2);
        }
    }
}
