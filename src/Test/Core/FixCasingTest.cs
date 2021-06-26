using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Test.Core
{
    [TestClass]
    public class HtmFixCasingTestlUtilTest
    {
        [TestMethod]
        public void FixStutter1()
        {
            var fc = new FixCasing("en");
            var s = new Subtitle(new List<Paragraph> { new Paragraph("W-what time is it? W-what time is it?", 0, 2000) });
            fc.FixMakeLowercase = false;
            fc.FixMakeUppercase = false;
            fc.FixNormal = true;
            fc.Fix(s);
            Assert.AreEqual("W-What time is it? W-What time is it?", s.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixStutter2()
        {
            var fc = new FixCasing("en");
            var s = new Subtitle(new List<Paragraph> { new Paragraph("W-w-what time is it? W-w-what time is it?", 0, 2000) });
            fc.FixMakeLowercase = false;
            fc.FixMakeUppercase = false;
            fc.FixNormal = true;
            fc.Fix(s);
            Assert.AreEqual("W-W-What time is it? W-W-What time is it?", s.Paragraphs[0].Text);
        }
    }
}
