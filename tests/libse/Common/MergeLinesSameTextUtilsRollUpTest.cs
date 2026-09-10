using Xunit;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common
{
    public class MergeLinesSameTextUtilsRollUpTest
    {
        private static Subtitle MakeRollUp()
        {
            var nl = System.Environment.NewLine;
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("FOUR SCORE", 1000, 1900));
            s.Paragraphs.Add(new Paragraph("FOUR SCORE" + nl + "AND SEVEN YEARS AGO,", 2000, 2900));
            s.Paragraphs.Add(new Paragraph("FOUR SCORE" + nl + "AND SEVEN YEARS AGO," + nl + "OUR FATHERS", 3000, 3900));
            s.Paragraphs.Add(new Paragraph("AND SEVEN YEARS AGO," + nl + "OUR FATHERS" + nl + "BROUGHT FORTH", 4000, 4900));
            s.Paragraphs.Add(new Paragraph("OUR FATHERS" + nl + "BROUGHT FORTH" + nl + "UPON THIS CONTINENT", 5000, 5900));
            s.Paragraphs.Add(new Paragraph("BROUGHT FORTH" + nl + "UPON THIS CONTINENT" + nl + "A NEW NATION:", 6000, 6900));
            return s;
        }

        [Fact]
        public void RollUpChainIsRechunkedIntoTwoLineParagraphs()
        {
            var nl = System.Environment.NewLine;
            var result = MergeLinesSameTextUtils.MergeRollUpCaptions(MakeRollUp(), 250);

            Assert.Equal(3, result.Paragraphs.Count);
            Assert.Equal("FOUR SCORE" + nl + "AND SEVEN YEARS AGO,", result.Paragraphs[0].Text);
            Assert.Equal(1000, result.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(3000, result.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal("OUR FATHERS" + nl + "BROUGHT FORTH", result.Paragraphs[1].Text);
            Assert.Equal(3000, result.Paragraphs[1].StartTime.TotalMilliseconds);
            Assert.Equal(5000, result.Paragraphs[1].EndTime.TotalMilliseconds);
            Assert.Equal("UPON THIS CONTINENT" + nl + "A NEW NATION:", result.Paragraphs[2].Text);
            Assert.Equal(5000, result.Paragraphs[2].StartTime.TotalMilliseconds);
            Assert.Equal(6900, result.Paragraphs[2].EndTime.TotalMilliseconds);
        }

        [Fact]
        public void TopLineDropsWhileNewSecondLineArrives()
        {
            var nl = System.Environment.NewLine;
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("A" + nl + "B", 1000, 1900));
            s.Paragraphs.Add(new Paragraph("B" + nl + "C", 2000, 2900));
            s.Paragraphs.Add(new Paragraph("C" + nl + "D", 3000, 3900));
            s.Paragraphs.Add(new Paragraph("Unrelated", 4000, 4900));

            var result = MergeLinesSameTextUtils.MergeRollUpCaptions(s, 250);

            Assert.Equal(3, result.Paragraphs.Count);
            Assert.Equal("A" + nl + "B", result.Paragraphs[0].Text);
            Assert.Equal(2000, result.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal("C" + nl + "D", result.Paragraphs[1].Text);
            Assert.Equal(2000, result.Paragraphs[1].StartTime.TotalMilliseconds);
            Assert.Equal(3900, result.Paragraphs[1].EndTime.TotalMilliseconds);
            Assert.Equal("Unrelated", result.Paragraphs[2].Text);
        }

        [Fact]
        public void PureIncrementingSequenceIsLeftToIncrementMerge()
        {
            var nl = System.Environment.NewLine;
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("A", 1000, 1900));
            s.Paragraphs.Add(new Paragraph("A" + nl + "B", 2000, 2900));

            var result = MergeLinesSameTextUtils.MergeRollUpCaptions(s, 250);
            Assert.Equal(2, result.Paragraphs.Count);

            var merged = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(s, true, 250, true);
            Assert.Equal(1, merged.Paragraphs.Count);
            Assert.Equal("A" + nl + "B", merged.Paragraphs[0].Text);
        }

        [Fact]
        public void GapLargerThanMaxBreaksChain()
        {
            var nl = System.Environment.NewLine;
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("A" + nl + "B", 1000, 1900));
            s.Paragraphs.Add(new Paragraph("B" + nl + "C", 5000, 5900));

            var result = MergeLinesSameTextUtils.MergeRollUpCaptions(s, 250);
            Assert.Equal(2, result.Paragraphs.Count);
        }
    }
}
