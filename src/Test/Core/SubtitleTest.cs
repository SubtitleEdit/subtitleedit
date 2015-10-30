﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
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

            int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.ID });
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

            int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.ID, p3.ID });
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

    }
}
