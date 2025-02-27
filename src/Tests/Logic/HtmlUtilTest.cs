using System.Drawing;
using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
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
        public void GetColorFromStringInvalid()
        {
            Assert.AreEqual(Color.White, HtmlUtil.GetColorFromString("invalidColorString"));
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
        public void RemoveFontNameNoFont()
        {
            var c = HtmlUtil.RemoveFontName("Hallo!");
            Assert.AreEqual("Hallo!", c);
        }

        [TestMethod]
        public void RemoveFontNameEmpty()
        {
            var c = HtmlUtil.RemoveFontName(string.Empty);
            Assert.AreEqual(string.Empty, c);
        }

        [TestMethod]
        public void RemoveFontNameNull()
        {
            var c = HtmlUtil.RemoveFontName(null);
            Assert.IsNull(c);
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

        [TestMethod]
        public void IsTextFormattableEmpty()
        {
            Assert.IsFalse(HtmlUtil.IsTextFormattable(string.Empty));
        }

        [TestMethod]
        public void IsTextFormattableNull()
        {
            Assert.IsFalse(HtmlUtil.IsTextFormattable(null));
        }
    }
}