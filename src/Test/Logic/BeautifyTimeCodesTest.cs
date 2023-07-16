using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Test.Logic
{
    [TestClass]
    public class BeautifyTimeCodesTest
    {
        [TestMethod]
        public void TestIsInCueOnShotChange1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 1;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 1040, 3000); // 1 frame after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 1080, 3000); // 2 frames after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(false, result);

            paragraph = new Paragraph("Test.", 960, 3000); // 1 frame before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(false, result);

            paragraph = new Paragraph("Test.", 920, 3000); // 2 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestIsInCueOnShotChange2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 0;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 1001, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 1010, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 1025, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(false, result);

            paragraph = new Paragraph("Test.", 999, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 990, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 975, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestIsOutCueOnShotChange1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9960); // 1 frame before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9920); // 2 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9880); // 3 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(false, result);

            paragraph = new Paragraph("Test.", 8000, 10040); // 1 frame after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestIsOutCueOnShotChange2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 10001);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 10010);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 10030);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(false, result);

            paragraph = new Paragraph("Test.", 8000, 9999);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9950);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9915);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(true, result);

            paragraph = new Paragraph("Test.", 8000, 9890);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.AreEqual(false, result);
        }
    }
}
