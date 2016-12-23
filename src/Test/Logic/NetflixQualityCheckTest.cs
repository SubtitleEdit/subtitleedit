using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Logic
{
    [TestClass]
    public class NetflixQualityCheckTest
    {
        [TestMethod]
        public void TestNetflixGlyphChecker()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit௓ amet, consectetur adi௟piscing elit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            NetflixQualityReportBuilder reportBuilder = new NetflixQualityReportBuilder();
            NetflixGlyphChecker glyphChecker = new NetflixGlyphChecker();

            glyphChecker.Check(sub, reportBuilder);

            Assert.AreEqual(2, reportBuilder.Records.Count);
            Assert.AreEqual("or sit௓ amet", reportBuilder.Records[0].Context);
            Assert.AreEqual("ur adi௟pisci", reportBuilder.Records[1].Context);
        }

        [TestMethod]
        public void TestNetflixWhiteSpaceChecker()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem  ipsum dolor   sit amet, consectetur\r\n\r\nadipiscing\n\r\nelit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            NetflixQualityReportBuilder reportBuilder = new NetflixQualityReportBuilder();
            NetflixWhiteSpaceChecker glyphChecker = new NetflixWhiteSpaceChecker();

            glyphChecker.Check(sub, reportBuilder);

            Assert.AreEqual(4, reportBuilder.Records.Count);
            Assert.AreEqual("Lorem  ipsu", reportBuilder.Records[0].Context);
            Assert.AreEqual(" dolor   sit", reportBuilder.Records[1].Context);
            Assert.AreEqual("ctetur\r\n\r\nad", reportBuilder.Records[2].Context);
            Assert.AreEqual("iscing\n\r\neli", reportBuilder.Records[3].Context);
        }
    }
}
