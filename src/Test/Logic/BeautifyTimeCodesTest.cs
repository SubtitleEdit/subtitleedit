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

        [TestMethod]
        public void TestGetPreviousShotChange()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1.0, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1.0, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1.0, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1.0, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(20.0, result);
        }

        [TestMethod]
        public void TestGetPreviousShotChangeInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1000, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1000, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1000, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1000, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(20000, result);
        }

        [TestMethod]
        public void TestGetPreviousShotChangePlusGapInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1080, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1080, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1080, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(1080, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.AreEqual(20080, result);
        }

        [TestMethod]
        public void TestGetNextShotChange()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10.0, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(1.0, result);
        }

        [TestMethod]
        public void TestGetNextShotChangeInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10000, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10000, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10000, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(10000, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(1000, result);
        }

        [TestMethod]
        public void TestGetNextShotChangeMinusGapInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(9920, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(9920, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(9920, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(9920, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(null, result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.AreEqual(920, result);
        }
    }
}
