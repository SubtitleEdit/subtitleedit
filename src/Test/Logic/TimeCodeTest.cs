using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Logic
{
    [TestClass]
    public class TimeCodeTest
    {
        [TestMethod]
        public void TimeCodeDaysTest()
        {
            var tc = new TimeCode(24 * 3, 0, 0, 0)
            {
                Hours = 0,
                Minutes = 0,
                Seconds = 0,
                Milliseconds = 0,
            };
            Assert.IsTrue(tc.TotalMilliseconds > 0);
        }

        [TestMethod]
        public void TimeCodeMilliseconds()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Milliseconds = 9 };

            Assert.AreEqual(tc.TotalMilliseconds, new TimeSpan(0, 1, 2, 3, 9).TotalMilliseconds);
        }

        [TestMethod]
        public void TimeCodeSeconds1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Seconds = 9 };

            Assert.AreEqual(tc.TotalMilliseconds, new TimeSpan(0, 1, 2, 9, 4).TotalMilliseconds);
        }

        [TestMethod]
        public void TimeCodeMinutes1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Minutes = 9 };

            Assert.AreEqual(tc.TotalMilliseconds, new TimeSpan(0, 1, 9, 3, 4).TotalMilliseconds);
        }

        [TestMethod]
        public void TimeCodeHours1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Hours = 9 };

            Assert.AreEqual(tc.TotalMilliseconds, new TimeSpan(0, 9, 2, 3, 4).TotalMilliseconds);
        }

        [TestMethod]
        public void TimeCodeParseToMilliseconds1()
        {
            var ms = TimeCode.ParseToMilliseconds("01:02:03:999");

            Assert.AreEqual(ms, new TimeSpan(0, 1, 2, 3, 999).TotalMilliseconds);
        }

        [TestMethod]
        public void TimeCodeGetTotalMilliseconds()
        {
            var tc = new TimeCode(1, 2, 3, 4);

            Assert.AreEqual(tc.TotalMilliseconds, 3723004);
            Assert.IsTrue(Math.Abs(tc.TotalMilliseconds - (tc.TotalSeconds * 1000.0)) < 0.001);
        }

        [TestMethod]
        public void ToShortStringHhmmssff1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(1, 2, 3, 0).ToShortStringHHMMSSFF();
            Assert.AreEqual("01:02:03:00", res);
        }

        [TestMethod]
        public void ToShortStringHhmmssff2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 2, 3, 0).ToShortStringHHMMSSFF();
            Assert.AreEqual(res, "02:03:00", res);
        }

        [TestMethod]
        public void ToShortStringHhmmssff3()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 0, 3, 0).ToShortStringHHMMSSFF();
            Assert.AreEqual("03:00", res);
        }

        [TestMethod]
        public void ToShortStringHhmmssff4()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 0, 0, 0).ToShortStringHHMMSSFF();
            Assert.AreEqual("00:00", res);
        }

    }
}
