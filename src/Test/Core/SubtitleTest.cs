using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;

namespace Test.Core
{
    [TestClass]
    public class SubtitleTest
    {

        [TestMethod]
        public void TestRemoveParagraphsByIds1()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 1000, 2000);
            var p3 = new Paragraph("3", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Paragraphs.Add(p3);

            int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.Id });
            Assert.AreEqual(removedCount, 1);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(sub.Paragraphs[0], p1);
            Assert.AreEqual(sub.Paragraphs[1], p3);
        }

        [TestMethod]
        public void TestRemoveParagraphsByIds2()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 1000, 2000);
            var p3 = new Paragraph("3", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Paragraphs.Add(p3);

            int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.Id, p3.Id });
            Assert.AreEqual(removedCount, 2);
            Assert.AreEqual(sub.Paragraphs.Count, 1);
            Assert.AreEqual(sub.Paragraphs[0], p1);
        }

        [TestMethod]
        public void TestRemoveParagraphsByIdices1()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 1000, 2000);
            var p3 = new Paragraph("3", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Paragraphs.Add(p3);

            int removedCount = sub.RemoveParagraphsByIndices(new List<int> { 1 });
            Assert.AreEqual(removedCount, 1);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(sub.Paragraphs[0], p1);
            Assert.AreEqual(sub.Paragraphs[1], p3);
        }

        [TestMethod]
        public void TestRemoveParagraphsByIdices2()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 1000, 2000);
            var p3 = new Paragraph("3", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Paragraphs.Add(p3);

            int removedCount = sub.RemoveParagraphsByIndices(new List<int> { 1, 2 });
            Assert.AreEqual(removedCount, 2);
            Assert.AreEqual(sub.Paragraphs.Count, 1);
            Assert.AreEqual(sub.Paragraphs[0], p1);
        }

        [TestMethod]
        public void TestRemoveEmptyLines()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph(" ", 1000, 2000);
            var p3 = new Paragraph("3", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Paragraphs.Add(p3);

            int removedCount = sub.RemoveEmptyLines();
            Assert.AreEqual(removedCount, 1);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(sub.Paragraphs[0], p1);
            Assert.AreEqual(sub.Paragraphs[1], p3);
        }

        [TestMethod]
        public void TestChangeFrameRate1()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);

            sub.ChangeFrameRate(25.0, 25.0);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(sub.Paragraphs[0].StartTime.TotalMilliseconds, 0);
            Assert.AreEqual(sub.Paragraphs[0].EndTime.TotalMilliseconds, 1000);
            Assert.AreEqual(sub.Paragraphs[1].StartTime.TotalMilliseconds, 2000);
            Assert.AreEqual(sub.Paragraphs[1].EndTime.TotalMilliseconds, 3000);
        }

        [TestMethod]
        public void TestChangeFrameRate2()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("1", 0, 1000);
            var p2 = new Paragraph("2", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);

            sub.ChangeFrameRate(25.0, 30.0);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(sub.Paragraphs[0].StartTime.TotalMilliseconds, 0);
            Assert.IsTrue(Math.Abs(sub.Paragraphs[0].EndTime.TotalMilliseconds - 833.33333333333) < 0.01);
            Assert.IsTrue(Math.Abs(sub.Paragraphs[1].StartTime.TotalMilliseconds - 1666.6666666666667) < 0.01);
            Assert.IsTrue(Math.Abs(sub.Paragraphs[1].EndTime.TotalMilliseconds - 2500) < 0.01);
        }

        [TestMethod]
        public void RenumberNormal()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("0", 0, 1000);
            var p2 = new Paragraph("0", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Renumber();
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(1, sub.Paragraphs[0].Number);
            Assert.AreEqual(2, sub.Paragraphs[1].Number);
        }

        [TestMethod]
        public void RenumberStartWith2()
        {
            var sub = new Subtitle();
            var p1 = new Paragraph("0", 0, 1000);
            var p2 = new Paragraph("0", 2000, 3000);
            sub.Paragraphs.Add(p1);
            sub.Paragraphs.Add(p2);
            sub.Renumber(2);
            Assert.AreEqual(sub.Paragraphs.Count, 2);
            Assert.AreEqual(2, sub.Paragraphs[0].Number);
            Assert.AreEqual(3, sub.Paragraphs[1].Number);
        }


    }
}
