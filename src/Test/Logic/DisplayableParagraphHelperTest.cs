using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Test.Logic
{
    [TestClass]
    public class DisplayableParagraphHelperTest
    {

        /// <summary>
        /// Tests that the longest paragraph is selected when neither has any overlap.
        /// </summary>
        [TestMethod]
        public void GetLongestParagraphTest()
        {
            var paragraphs = new List<Paragraph>()
            {
                new Paragraph("Longer", TimeCode.ParseToMilliseconds("00:00:10,500"), TimeCode.ParseToMilliseconds("00:00:15,000")),
                new Paragraph("Shorter", TimeCode.ParseToMilliseconds("00:00:20,000"), TimeCode.ParseToMilliseconds("00:00:21,000"))
            };
            DisplayableParagraphHelper helper = new DisplayableParagraphHelper(TimeCode.ParseToMilliseconds("00:00:00,000"), TimeCode.ParseToMilliseconds("00:00:30,000"), 1000);
            AddAllParagraphs(helper, paragraphs);

            List<Paragraph> selectedParagraphs = helper.GetParagraphs(1);
            Assert.AreEqual(1, selectedParagraphs.Count);

            Assert.AreEqual("Longer", selectedParagraphs[0].Text);
        }

        /// <summary>
        /// Tests that the paragraph without overlap is chosen when the alternative is completely overlapped by a longer paragraph.
        /// </summary>
        [TestMethod]
        public void GetLeastOverlappingParagraphTest()
        {
            var paragraphs = new List<Paragraph>()
            {
                new Paragraph("Outer", TimeCode.ParseToMilliseconds("00:00:5,000"), TimeCode.ParseToMilliseconds("00:00:15,000")),
                new Paragraph("Inner", TimeCode.ParseToMilliseconds("00:00:10,000"), TimeCode.ParseToMilliseconds("00:00:11,000")),
                new Paragraph("Second", TimeCode.ParseToMilliseconds("00:00:20,000"), TimeCode.ParseToMilliseconds("00:00:21,000")),
            };
            DisplayableParagraphHelper helper = new DisplayableParagraphHelper(TimeCode.ParseToMilliseconds("00:00:00,000"), TimeCode.ParseToMilliseconds("00:00:30,000"), 1000);
            AddAllParagraphs(helper, paragraphs);

            List<Paragraph> selectedParagraphs = helper.GetParagraphs(2);
            Assert.AreEqual(2, selectedParagraphs.Count);

            Assert.AreEqual("Outer", selectedParagraphs[0].Text);
            Assert.AreEqual("Second", selectedParagraphs[1].Text);
        }

        /// <summary>
        /// Tests that a paragraph that partially overlaps another paragraph is chosen when the alternative completely overlaps another paragraph.
        /// </summary>
        [TestMethod]
        public void GetPartiallyOverlappingTest()
        {
            var paragraphs = new List<Paragraph>()
            {
                new Paragraph("Outer", TimeCode.ParseToMilliseconds("00:00:5,000"), TimeCode.ParseToMilliseconds("00:00:15,000")),
                new Paragraph("Inner", TimeCode.ParseToMilliseconds("00:00:07,000"), TimeCode.ParseToMilliseconds("00:00:10,000")),
                new Paragraph("Partial", TimeCode.ParseToMilliseconds("00:00:14,000"), TimeCode.ParseToMilliseconds("00:00:16,000")),
            };
            DisplayableParagraphHelper helper = new DisplayableParagraphHelper(TimeCode.ParseToMilliseconds("00:00:00,000"), TimeCode.ParseToMilliseconds("00:00:30,000"), 1000);
            AddAllParagraphs(helper, paragraphs);

            List<Paragraph> selectedParagraphs = helper.GetParagraphs(2);
            Assert.AreEqual(2, selectedParagraphs.Count);

            Assert.AreEqual("Outer", selectedParagraphs[0].Text);
            Assert.AreEqual("Partial", selectedParagraphs[1].Text);
        }

        /// <summary>
        /// Tests that consecutive paragraphs can be chosen (starting and ending at the same time).
        /// </summary>
        [TestMethod]
        public void GetConsecutiveParagraphsTest()
        {
            List<Paragraph> paragraphs = CreateConsecutiveParagraphs(1);
            DisplayableParagraphHelper helper = new DisplayableParagraphHelper(TimeCode.ParseToMilliseconds("00:00:00,000"), TimeCode.ParseToMilliseconds("00:00:30,000"), 1000);
            AddAllParagraphs(helper, paragraphs);

            List<Paragraph> selectedParagraphs = helper.GetParagraphs(3);

            Assert.AreEqual(3, selectedParagraphs.Count);
            Assert.AreEqual("P1 L1", selectedParagraphs[0].Text);
            Assert.AreEqual("P2 L1", selectedParagraphs[1].Text);
            Assert.AreEqual("P3 L1", selectedParagraphs[2].Text);
        }

        /// <summary>
        /// Tests that only a single layer of paragraphs will be chosen when all paragraphs overlap in a layer 3 deep.
        /// </summary>
        [TestMethod]
        public void GetSingleOverlapLayerTest()
        {
            var paragraphs = new List<Paragraph>();
            paragraphs.AddRange(CreateConsecutiveParagraphs(1));
            paragraphs.AddRange(CreateConsecutiveParagraphs(2));
            paragraphs.AddRange(CreateConsecutiveParagraphs(3));
            DisplayableParagraphHelper helper = new DisplayableParagraphHelper(TimeCode.ParseToMilliseconds("00:00:00,000"), TimeCode.ParseToMilliseconds("00:00:30,000"), 1000);
            AddAllParagraphs(helper, paragraphs);

            List<Paragraph> selectedParagraphs = helper.GetParagraphs(4);

            Assert.AreEqual(4, selectedParagraphs.Count);
            Assert.IsTrue(selectedParagraphs[0].Text.StartsWith("P1"));
            Assert.IsTrue(selectedParagraphs[1].Text.StartsWith("P2"));
            Assert.IsTrue(selectedParagraphs[2].Text.StartsWith("P3"));
            Assert.IsTrue(selectedParagraphs[3].Text.StartsWith("P4"));
        }

        private List<Paragraph> CreateConsecutiveParagraphs(int layerNumber)
        {
            var paragraphs = new List<Paragraph>()
            {
                new Paragraph($"P1 L{layerNumber}", TimeCode.ParseToMilliseconds("00:00:2,500"), TimeCode.ParseToMilliseconds("00:00:3,000")),
                new Paragraph($"P2 L{layerNumber}", TimeCode.ParseToMilliseconds("00:00:3,000"), TimeCode.ParseToMilliseconds("00:00:3,500")),
                new Paragraph($"P3 L{layerNumber}", TimeCode.ParseToMilliseconds("00:00:3,500"), TimeCode.ParseToMilliseconds("00:00:4,000")),
                new Paragraph($"P4 L{layerNumber}", TimeCode.ParseToMilliseconds("00:00:4,000"), TimeCode.ParseToMilliseconds("00:00:4,500")),
            };
            return paragraphs;
        }

        private void AddAllParagraphs(DisplayableParagraphHelper helper, List<Paragraph> paragraphs)
        {
            foreach (var paragraph in paragraphs)
            {
                helper.Add(paragraph);
            }
        }

    }
}
