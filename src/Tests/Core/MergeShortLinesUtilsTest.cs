using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Core
{
    [TestClass]
    public class MergeShortLinesUtilsTest
    {
        [TestMethod]
        public void ThreeShortLines()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
            subtitle.Paragraphs.Add(new Paragraph("are", 200, 400));
            subtitle.Paragraphs.Add(new Paragraph("you?", 400, 600));
            var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 80, true);

            Assert.AreEqual(1, mergedSubtitle.Paragraphs.Count);
            Assert.AreEqual("How are you?", mergedSubtitle.Paragraphs[0].Text);
        }

        [TestMethod]
        public void ThreeShortLinesNoMergeDueToLength()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
            subtitle.Paragraphs.Add(new Paragraph("are", 200, 400));
            subtitle.Paragraphs.Add(new Paragraph("you?", 400, 600));
            var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 2, true);

            Assert.AreEqual(3, mergedSubtitle.Paragraphs.Count);
        }

        [TestMethod]
        public void ThreeShortLinesNoMergeDueToGap()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("How", 0, 200));
            subtitle.Paragraphs.Add(new Paragraph("are", 2000, 2400));
            subtitle.Paragraphs.Add(new Paragraph("you?", 4400, 4600));
            var mergedSubtitle = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, 500, 80, true);

            Assert.AreEqual(3, mergedSubtitle.Paragraphs.Count);
        }
    }
}
