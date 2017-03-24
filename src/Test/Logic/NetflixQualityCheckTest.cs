using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;

namespace Test.Logic
{
    [TestClass]
    public class NetflixQualityCheckTest
    {
        [TestMethod]
        public void TestNetflixCheckGlyph()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit௓ amet, consectetur adi௟piscing elit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckGlyph();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(2, reportBuilder.Records.Count);
            Assert.AreEqual("or sit௓ amet", reportBuilder.Records[0].Context);
            Assert.AreEqual("ur adi௟pisci", reportBuilder.Records[1].Context);
        }

        [TestMethod]
        public void TestNetflixCheckWhiteSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem  ipsum dolor   sit amet, consectetur\r\n\r\nadipiscing\n\r\nelit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckWhiteSpace();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(4, reportBuilder.Records.Count);
            Assert.AreEqual("Lorem  ipsu", reportBuilder.Records[0].Context);
            Assert.AreEqual(" dolor   sit", reportBuilder.Records[1].Context);
            Assert.AreEqual("ctetur\r\n\r\nad", reportBuilder.Records[2].Context);
            Assert.AreEqual("iscing\n\r\neli", reportBuilder.Records[3].Context);
        }

        [TestMethod]
        public void TestNetflixCheckDialogeHyphenNoSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("- Lorem ipsum dolor sit" + Environment.NewLine + "- nelit focasia venlit dokalalam dilars.", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem - ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckDialogeHyphenNoSpace();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].FixedParagraph.Text, "-Lorem ipsum dolor sit" + Environment.NewLine + "-nelit focasia venlit dokalalam dilars.");
        }

        [TestMethod]
        public void TestNetflixCheckMaxCps()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet consectetur adipiscing nelit focasia venlit dokalalam dilars.", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckMaxCps();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
        }

        
        [TestMethod]
        public void TestNetflixCheckMaxLineLength()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet lasdf lajsdf ljdsf asdf asf.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckMaxLineLength();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckMaxDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckMaxDuration();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckMinDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum.", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 834);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckMinDuration();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
        }


        [TestMethod]
        public void TestNetflixCheckNumberOfLines()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2." + Environment.NewLine + "Line 3", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckNumberOfLines();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckNumbersOneToTenSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("This is 1 man", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckNumbersOneToTenSpellOut();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(2, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(reportBuilder.Records[0].FixedParagraph.Text, "This is one man");
        }

        [TestMethod]
        public void TestNetflixCheckStartNumberSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("12 is nice!", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckStartNumberSpellOut();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(reportBuilder.Records[0].FixedParagraph.Text, "Twelve is nice!");
        }

        [TestMethod]
        public void TestNetflixCheckTextForHiUseBrackets()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckTextForHiUseBrackets();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(reportBuilder.Records[0].FixedParagraph.Text, "[Enginie starting]");
        }

        [TestMethod]
        public void TestNetflixCheckTwoFramesGap()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 1010, 2832);
            sub.Paragraphs.Add(p2);

            var reportBuilder = new NetflixQualityController();
            var checker = new NetflixCheckTwoFramesGap();

            checker.Check(sub, reportBuilder);

            Assert.AreEqual(1, reportBuilder.Records.Count);
            Assert.AreEqual(reportBuilder.Records[0].OriginalParagraph, p1);
            Assert.IsTrue(reportBuilder.Records[0].FixedParagraph.EndTime.TotalMilliseconds < 1000);
        }

    }
}
