using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Logic
{
    [TestClass]
    public class HtmlUtilTest
    {
        [TestMethod]
        public void GetColorFromString1()
        {
            var c = HtmlUtil.GetColorFromString("#010203ff");

            Assert.AreEqual(byte.MaxValue, c.A);
            Assert.AreEqual(1, c.R);
            Assert.AreEqual(2, c.G);
            Assert.AreEqual(3, c.B);
        }

        [TestMethod]
        public void GetColorFromString2()
        {
            var c = HtmlUtil.GetColorFromString("rgb(1,2,3)");

            Assert.AreEqual(byte.MaxValue, c.A);
            Assert.AreEqual(1, c.R);
            Assert.AreEqual(2, c.G);
            Assert.AreEqual(3, c.B);
        }

        [TestMethod]
        public void GetColorFromString3()
        {
            var c = HtmlUtil.GetColorFromString("rgba(1,2,3, 1)");

            Assert.AreEqual(byte.MaxValue, c.A);
            Assert.AreEqual(1, c.R);
            Assert.AreEqual(2, c.G);
            Assert.AreEqual(3, c.B);
        }

        [TestMethod]
        public void RemoveFontName1()
        {
            var c = HtmlUtil.RemoveFontName(@"{\fnVerdena}Hallo!");

            Assert.AreEqual("Hallo!", c);
        }

        [TestMethod]
        public void RemoveFontName2()
        {
            var c = HtmlUtil.RemoveFontName(@"{\an2\fnVerdena}Hallo!");
            Assert.AreEqual(@"{\an2}Hallo!", c);
        }

        [TestMethod]
        public void RemoveFontName3()
        {
            var c = HtmlUtil.RemoveFontName(@"{\an2\fnVerdena\i1}Hallo!");
            Assert.AreEqual(@"{\an2\i1}Hallo!", c);
        }

        [TestMethod]
        public void RemoveFontName4()
        {
            var c = HtmlUtil.RemoveFontName("<font face=\"Verdena\">Hallo!</font>");

            Assert.AreEqual("Hallo!", c);
        }

        [TestMethod]
        public void IsTextFormattableFalse()
        {
            Assert.IsFalse(HtmlUtil.IsTextFormattable("<i></i>"));
            Assert.IsFalse(HtmlUtil.IsTextFormattable("<i>!?."));
            Assert.IsFalse(HtmlUtil.IsTextFormattable("!?.</i>"));
        }

        [TestMethod]
        public void IsTextFormattableTrue()
        {
            Assert.IsTrue(HtmlUtil.IsTextFormattable("<i>a</i>"));
            Assert.IsTrue(HtmlUtil.IsTextFormattable("<u>1</u>"));
            Assert.IsTrue(HtmlUtil.IsTextFormattable("<i>A</i>"));
            Assert.IsTrue(HtmlUtil.IsTextFormattable("</i")); // invalid closing tag
        }
    }
}