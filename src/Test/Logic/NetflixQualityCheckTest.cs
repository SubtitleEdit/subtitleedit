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

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckGlyph();

            checker.Check(sub, controller);

            Assert.AreEqual(2, controller.Records.Count);
            Assert.AreEqual("or sit௓ amet", controller.Records[0].Context);
            Assert.AreEqual("ur adi௟pisci", controller.Records[1].Context);
        }

        [TestMethod]
        public void TestNetflixCheckWhiteSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem  ipsum dolor   sit amet, consectetur\r\n\r\nadipiscing\n\r\nelit.", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckWhiteSpace();

            checker.Check(sub, controller);

            Assert.AreEqual(4, controller.Records.Count);
            Assert.AreEqual("Lorem  ipsu", controller.Records[0].Context);
            Assert.AreEqual(" dolor   sit", controller.Records[1].Context);
            Assert.AreEqual("ctetur\r\n\r\nad", controller.Records[2].Context);
            Assert.AreEqual("iscing\n\r\neli", controller.Records[3].Context);
        }

        [TestMethod]
        public void TestNetflixCheckDialogeHyphenNoSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("- Lorem ipsum dolor sit" + Environment.NewLine + "- nelit focasia venlit dokalalam dilars.", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem - ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckDialogeHyphenSpace();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "-Lorem ipsum dolor sit" + Environment.NewLine + "-nelit focasia venlit dokalalam dilars.");
        }

        [TestMethod]
        public void TestNetflixCheckDialogeHyphenNoSpaceItalic()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>- Lorem ipsum dolor sit</i>" + Environment.NewLine + "<i>- nelit focasia venlit dokalalam dilars.</i>", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem - ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckDialogeHyphenSpace();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "<i>-Lorem ipsum dolor sit</i>" + Environment.NewLine + "<i>-nelit focasia venlit dokalalam dilars.</i>");
        }

        [TestMethod]
        public void TestNetflixCheckDialogeHyphenAddSpace()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("-Lorem ipsum dolor sit." + Environment.NewLine + "-Nelit focasia venlit dokalalam dilars.", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem-ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController() { Language = "fr" };
            var checker = new NetflixCheckDialogeHyphenSpace();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "- Lorem ipsum dolor sit." + Environment.NewLine + "- Nelit focasia venlit dokalalam dilars.");
        }

        [TestMethod]
        public void TestNetflixCheckDialogeHyphenAddSpaceItalic()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>-Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>-Nelit focasia venlit dokalalam dilars.</i>", 0, 4000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem-ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController() { Language = "fr" };
            var checker = new NetflixCheckDialogeHyphenSpace();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "<i>- Lorem ipsum dolor sit.</i>" + Environment.NewLine + "<i>- Nelit focasia venlit dokalalam dilars.</i>");
        }

        [TestMethod]
        public void TestNetflixCheckMaxCps()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet consectetur adipiscing nelit focasia venlit dokalalam dilars.", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 1000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxCps();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
        }


        [TestMethod]
        public void TestNetflixCheckMaxLineLength()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet lasdf lajsdf ljdsf asdf asf.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxLineLength();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckMaxDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum dolor sit amet.", 0, 8000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 7000);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMaxDuration();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckMinDuration()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum.", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum.", 0, 834);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckMinDuration();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
        }


        [TestMethod]
        public void TestNetflixCheckNumberOfLines()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2." + Environment.NewLine + "Line 3", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumberOfLines();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
        }

        [TestMethod]
        public void TestNetflixCheckNumbersOneToTenSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("This is 1 man", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckNumbersOneToTenSpellOut();

            checker.Check(sub, controller);

            Assert.AreEqual(2, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "This is one man");
        }

        [TestMethod]
        public void TestNetflixCheckStartNumberSpellOut()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("12 is nice!", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckStartNumberSpellOut();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "Twelve is nice!");
        }

        [TestMethod]
        public void TestNetflixCheckTextForHiUseBrackets()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 832);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 0, 832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckTextForHiUseBrackets();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "[Enginie starting]");
        }

        [TestMethod]
        public void TestNetflixCheckTwoFramesGap()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("(Enginie starting)", 0, 1000);
            sub.Paragraphs.Add(p1);
            var p2 = new Paragraph("Lorem ipsum." + Environment.NewLine + "Line 2.", 1010, 2832);
            sub.Paragraphs.Add(p2);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckTwoFramesGap();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.IsTrue(controller.Records[0].FixedParagraph.EndTime.TotalMilliseconds < 1000);
        }

        [TestMethod]
        public void TestNetflixCheckItalicsRemove()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController { Language = "zh" };
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.IsFalse(controller.Records[0].FixedParagraph.Text.Contains("<i>"));
        }

        [TestMethod]
        public void TestNetflixCheckItalicsKeep()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("<i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController { Language = "en" };
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.AreEqual(0, controller.Records.Count);
        }

        [TestMethod]
        public void TestNetflixCheckItalicsFix()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("</i>Enginie starting</i>", 0, 1000);
            sub.Paragraphs.Add(p1);

            var controller = new NetflixQualityController();
            var checker = new NetflixCheckItalics();

            checker.Check(sub, controller);

            Assert.AreEqual(1, controller.Records.Count);
            Assert.AreEqual(controller.Records[0].OriginalParagraph, p1);
            Assert.AreEqual(controller.Records[0].FixedParagraph.Text, "<i>Enginie starting</i>");
        }

    }
}
