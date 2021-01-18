using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Logic
{
    [TestClass]
    public class ParagraphTest
    {
        [TestMethod]
        public void TestMethodNumberOfLinesTwoLines()
        {
            var p = new Paragraph { Text = "Hallo!" + Environment.NewLine + "How are you?" };
            Assert.AreEqual(2, p.NumberOfLines);
        }

        [TestMethod]
        public void TestMethodNumberOfLinesThreeLines()
        {
            var p = new Paragraph { Text = "Hallo!" + Environment.NewLine + Environment.NewLine + "How are you?" };
            Assert.AreEqual(3, p.NumberOfLines);
        }

        [TestMethod]
        public void TestMethodNumberOfLinesOneLine()
        {
            var p = new Paragraph { Text = "Hallo!" };
            Assert.AreEqual(1, p.NumberOfLines);
        }

        [TestMethod]
        public void TestMethodNumberOfLinesZero()
        {
            var p = new Paragraph { Text = string.Empty };
            Assert.AreEqual(0, p.NumberOfLines);
        }

        [TestMethod]
        public void TestAdjust1()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 1, 1), EndTime = new TimeCode(0, 1, 3, 1) };
            p.Adjust(1, 10);
            Assert.AreEqual(new TimeCode(0, 1, 11, 1).TotalMilliseconds, p.StartTime.TotalMilliseconds);
            Assert.AreEqual(new TimeCode(0, 1, 13, 1).TotalMilliseconds, p.EndTime.TotalMilliseconds);
        }

        [TestMethod]
        public void TestAdjust2()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 1, 1), EndTime = new TimeCode(0, 1, 4, 1) };
            p.Adjust(2, 10);
            Assert.AreEqual(new TimeCode(0, 2, 12, 2).TotalMilliseconds, p.StartTime.TotalMilliseconds);
            Assert.AreEqual(new TimeCode(0, 2, 18, 2).TotalMilliseconds, p.EndTime.TotalMilliseconds);
        }

        [TestMethod]
        public void TestAdjust3()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode() };
            p.Adjust(1, 1);
            Assert.AreEqual(1, p.StartTime.TotalSeconds);
        }

        [TestMethod]
        public void TestDuration()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 0, 0), EndTime = new TimeCode(0, 1, 1, 1) };
            Assert.AreEqual(p.Duration.TotalMilliseconds, 1001);
        }

    }
}
