using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    public class ParagraphTest
    {
        [Fact]
        public void TestMethodNumberOfLinesTwoLines()
        {
            var p = new Paragraph { Text = "Hallo!" + Environment.NewLine + "How are you?" };
            Assert.Equal(2, p.NumberOfLines);
        }

        [Fact]
        public void TestMethodNumberOfLinesThreeLines()
        {
            var p = new Paragraph { Text = "Hallo!" + Environment.NewLine + Environment.NewLine + "How are you?" };
            Assert.Equal(3, p.NumberOfLines);
        }

        [Fact]
        public void TestMethodNumberOfLinesOneLine()
        {
            var p = new Paragraph { Text = "Hallo!" };
            Assert.Equal(1, p.NumberOfLines);
        }

        [Fact]
        public void TestMethodNumberOfLinesZero()
        {
            var p = new Paragraph { Text = string.Empty };
            Assert.Equal(0, p.NumberOfLines);
        }

        [Fact]
        public void TestAdjust1()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 1, 1), EndTime = new TimeCode(0, 1, 3, 1) };
            p.Adjust(1, 10);
            Assert.Equal(new TimeCode(0, 1, 11, 1).TotalMilliseconds, p.StartTime.TotalMilliseconds);
            Assert.Equal(new TimeCode(0, 1, 13, 1).TotalMilliseconds, p.EndTime.TotalMilliseconds);
        }

        [Fact]
        public void TestAdjust2()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 1, 1), EndTime = new TimeCode(0, 1, 4, 1) };
            p.Adjust(2, 10);
            Assert.Equal(new TimeCode(0, 2, 12, 2).TotalMilliseconds, p.StartTime.TotalMilliseconds);
            Assert.Equal(new TimeCode(0, 2, 18, 2).TotalMilliseconds, p.EndTime.TotalMilliseconds);
        }

        [Fact]
        public void TestAdjust3()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode() };
            p.Adjust(1, 1);
            Assert.Equal(1, p.StartTime.TotalSeconds);
        }

        [Fact]
        public void TestDuration()
        {
            var p = new Paragraph { Text = string.Empty, StartTime = new TimeCode(0, 1, 0, 0), EndTime = new TimeCode(0, 1, 1, 1) };
            Assert.Equal(1001, p.DurationTotalMilliseconds);
        }

    }
}
