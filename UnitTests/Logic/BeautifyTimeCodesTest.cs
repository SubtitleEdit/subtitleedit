using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    
    public class BeautifyTimeCodesTest
    {
        [Fact]
        public void TestIsInCueOnShotChange1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 1;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 1040, 3000); // 1 frame after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 1080, 3000); // 2 frames after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.False(result);

            paragraph = new Paragraph("Test.", 960, 3000); // 1 frame before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.False(result);

            paragraph = new Paragraph("Test.", 920, 3000); // 2 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.False(result);
        }

        [Fact]
        public void TestIsInCueOnShotChange2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 0;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 1001, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 1010, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 1025, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.False(result);

            paragraph = new Paragraph("Test.", 999, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 990, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 975, 3000);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.StartTime, true);
            Assert.False(result);
        }

        [Fact]
        public void TestIsOutCueOnShotChange1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9960); // 1 frame before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9920); // 2 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9880); // 3 frames before
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.False(result);

            paragraph = new Paragraph("Test.", 8000, 10040); // 1 frame after
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.False(result);
        }

        [Fact]
        public void TestIsOutCueOnShotChange2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000);
            var result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 10001);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 10010);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 10030);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.False(result);

            paragraph = new Paragraph("Test.", 8000, 9999);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9950);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9915);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.True(result);

            paragraph = new Paragraph("Test.", 8000, 9890);
            result = ShotChangeHelper.IsCueOnShotChange(shotChangesSeconds, paragraph.EndTime, false);
            Assert.False(result);
        }

        [Fact]
        public void TestGetPreviousShotChange()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1.0, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1.0, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1.0, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1.0, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChange(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(20.0, result);
        }

        [Fact]
        public void TestGetPreviousShotChangeInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1000, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1000, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1000, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1000, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChangeInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(20000, result);
        }

        [Fact]
        public void TestGetPreviousShotChangePlusGapInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 1000, 3000); // On shot
            var result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1080, result);

            paragraph = new Paragraph("Test.", 1500, 3000); // Away from shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1080, result);

            paragraph = new Paragraph("Test.", 990, 3000); // On shot after rounding
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1080, result);

            paragraph = new Paragraph("Test.", 1010, 3000); // Tiny bit from shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(1080, result);

            paragraph = new Paragraph("Test.", 500, 3000); // No more shots
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 20500, 30000); // Other shot
            result = ShotChangeHelper.GetPreviousShotChangePlusGapInMs(shotChangesSeconds, paragraph.StartTime);
            Assert.Equal(20080, result);
        }

        [Fact]
        public void TestGetNextShotChange()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10.0, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10.0, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChange(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void TestGetNextShotChangeInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10000, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10000, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10000, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(10000, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChangeInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(1000, result);
        }

        [Fact]
        public void TestGetNextShotChangeMinusGapInMs()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = 2;

            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            var paragraph = new Paragraph("Test.", 8000, 10000); // On shot
            var result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(9920, result);

            paragraph = new Paragraph("Test.", 8000, 9500); // Away from shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(9920, result);

            paragraph = new Paragraph("Test.", 8000, 10010); // On shot after rounding
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(9920, result);

            paragraph = new Paragraph("Test.", 8000, 9990); // Tiny bit from shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(9920, result);

            paragraph = new Paragraph("Test.", 30000, 32000); // No more shots
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Null(result);

            paragraph = new Paragraph("Test.", 0, 800); // Other shot
            result = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChangesSeconds, paragraph.EndTime);
            Assert.Equal(920, result);
        }

        [Fact]
        public void TestClosestTo()
        {
            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            double Aggregate(double value)
            {
                return shotChangesSeconds.Aggregate((x, y) => Math.Abs(x - value) < Math.Abs(y - value) ? x : y);
            }

            Assert.Equal(Aggregate(-2), shotChangesSeconds.ClosestTo(-2));
            Assert.Equal(Aggregate(-1), shotChangesSeconds.ClosestTo(-1));
            Assert.Equal(Aggregate(0), shotChangesSeconds.ClosestTo(0));
            Assert.Equal(Aggregate(1), shotChangesSeconds.ClosestTo(1));
            Assert.Equal(Aggregate(2), shotChangesSeconds.ClosestTo(2));
            Assert.Equal(Aggregate(3), shotChangesSeconds.ClosestTo(3));
            Assert.Equal(Aggregate(4), shotChangesSeconds.ClosestTo(4));
            Assert.Equal(Aggregate(5), shotChangesSeconds.ClosestTo(5));
            Assert.Equal(Aggregate(6), shotChangesSeconds.ClosestTo(6));
            Assert.Equal(Aggregate(7), shotChangesSeconds.ClosestTo(7));
            Assert.Equal(Aggregate(8), shotChangesSeconds.ClosestTo(8));
            Assert.Equal(Aggregate(9), shotChangesSeconds.ClosestTo(9));
            Assert.Equal(Aggregate(10), shotChangesSeconds.ClosestTo(10));
            Assert.Equal(Aggregate(11), shotChangesSeconds.ClosestTo(11));
            Assert.Equal(Aggregate(12), shotChangesSeconds.ClosestTo(12));
            Assert.Equal(Aggregate(13), shotChangesSeconds.ClosestTo(13));
            Assert.Equal(Aggregate(14), shotChangesSeconds.ClosestTo(14));
            Assert.Equal(Aggregate(15), shotChangesSeconds.ClosestTo(15));
            Assert.Equal(Aggregate(16), shotChangesSeconds.ClosestTo(16));
            Assert.Equal(Aggregate(17), shotChangesSeconds.ClosestTo(17));
            Assert.Equal(Aggregate(18), shotChangesSeconds.ClosestTo(18));
            Assert.Equal(Aggregate(19), shotChangesSeconds.ClosestTo(19));
            Assert.Equal(Aggregate(20), shotChangesSeconds.ClosestTo(20));
            Assert.Equal(Aggregate(21), shotChangesSeconds.ClosestTo(21));
            Assert.Equal(Aggregate(22), shotChangesSeconds.ClosestTo(22));
        }

        [Fact]
        public void TestFirstOnOrAfter()
        {
            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            Assert.Equal(1, shotChangesSeconds.FirstOnOrAfter(-10, -1));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrAfter(-1, -1));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrAfter(0, -1));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrAfter(0.999, -1));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrAfter(1, -1));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrAfter(1.001, -1));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrAfter(2, -1));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrAfter(8, -1));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrAfter(9, -1));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrAfter(10, -1));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrAfter(11, -1));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrAfter(19, -1));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrAfter(20, -1));
            Assert.Equal(-1, shotChangesSeconds.FirstOnOrAfter(21, -1));
            Assert.Equal(-1, shotChangesSeconds.FirstOnOrAfter(22, -1));
            Assert.Equal(-1, shotChangesSeconds.FirstOnOrAfter(200, -1));
        }

        [Fact]
        public void TestFirstOnOrBefore()
        {
            var shotChangesSeconds = new List<double>() { 1, 10, 20 };

            Assert.Equal(20, shotChangesSeconds.FirstOnOrBefore(250, 9999));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrBefore(25, 9999));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrBefore(21, 9999));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrBefore(20.001, 9999));
            Assert.Equal(20, shotChangesSeconds.FirstOnOrBefore(20, 9999));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrBefore(19.999, 9999));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrBefore(12, 9999));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrBefore(11, 9999));
            Assert.Equal(10, shotChangesSeconds.FirstOnOrBefore(10, 9999));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrBefore(9, 9999));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrBefore(2, 9999));
            Assert.Equal(1, shotChangesSeconds.FirstOnOrBefore(1, 9999));
            Assert.Equal(9999, shotChangesSeconds.FirstOnOrBefore(0.999, 9999));
            Assert.Equal(9999, shotChangesSeconds.FirstOnOrBefore(0, 9999));
            Assert.Equal(9999, shotChangesSeconds.FirstOnOrBefore(-1, 9999));
            Assert.Equal(9999, shotChangesSeconds.FirstOnOrBefore(-10, 9999));
        }

        [Fact]
        public void TestFirstWithin()
        {
            var shotChangesFrames = new List<int>() { 50, 1000, 3000, 5000, 10000, 11000, 14000, 15000, 16000, 20000, 22000 };

            int? First(int start, int end)
            {
                try
                {
                    return shotChangesFrames.First(x => x >= start && x <= end);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            Assert.Equal(First(-100, 100000), shotChangesFrames.FirstWithin(-100, 100000));
            Assert.Equal(First(0, 100), shotChangesFrames.FirstWithin(0, 100));
            Assert.Equal(First(0, 1000), shotChangesFrames.FirstWithin(0, 1000));
            Assert.Equal(First(1000, 1000), shotChangesFrames.FirstWithin(1000, 1000));
            Assert.Equal(First(1000, 1100), shotChangesFrames.FirstWithin(1000, 1100));
            Assert.Equal(First(1100, 900), shotChangesFrames.FirstWithin(1100, 900));
            Assert.Equal(First(900, 15000), shotChangesFrames.FirstWithin(900, 15000));
            Assert.Equal(First(900, 30000), shotChangesFrames.FirstWithin(900, 30000));
            Assert.Equal(First(19999, 19999), shotChangesFrames.FirstWithin(19999, 19999));
            Assert.Equal(First(19999, 20000), shotChangesFrames.FirstWithin(19999, 20000));
            Assert.Equal(First(20000, 20000), shotChangesFrames.FirstWithin(20000, 20000));
            Assert.Equal(First(20000, 20001), shotChangesFrames.FirstWithin(20000, 20001));
            Assert.Equal(First(19999, 20001), shotChangesFrames.FirstWithin(19999, 20001));
            Assert.Equal(First(20001, 20001), shotChangesFrames.FirstWithin(20001, 20001));
            Assert.Equal(First(30000, 30000), shotChangesFrames.FirstWithin(30000, 30000));
            Assert.Equal(First(30000, 40000), shotChangesFrames.FirstWithin(30000, 40000));
            Assert.Equal(First(25000, 5000), shotChangesFrames.FirstWithin(25000, 5000));
            Assert.Equal(First(50, 60), shotChangesFrames.FirstWithin(50, 60));
            Assert.Equal(First(50, 1100), shotChangesFrames.FirstWithin(50, 1100));
        }

        [Fact]
        public void TestLastWithin()
        {
            var shotChangesFrames = new List<int>() { 50, 1000, 3000, 5000, 10000, 11000, 14000, 15000, 16000, 20000, 22000 };

            int? Last(int start, int end)
            {
                try
                {
                    return shotChangesFrames.Last(x => x >= start && x <= end);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            Assert.Equal(Last(-100, 100000), shotChangesFrames.LastWithin(-100, 100000));
            Assert.Equal(Last(0, 100), shotChangesFrames.LastWithin(0, 100));
            Assert.Equal(Last(0, 1000), shotChangesFrames.LastWithin(0, 1000));
            Assert.Equal(Last(1000, 1000), shotChangesFrames.LastWithin(1000, 1000));
            Assert.Equal(Last(1000, 1100), shotChangesFrames.LastWithin(1000, 1100));
            Assert.Equal(Last(1100, 900), shotChangesFrames.LastWithin(1100, 900));
            Assert.Equal(Last(900, 15000), shotChangesFrames.LastWithin(900, 15000));
            Assert.Equal(Last(900, 30000), shotChangesFrames.LastWithin(900, 30000));
            Assert.Equal(Last(19999, 19999), shotChangesFrames.LastWithin(19999, 19999));
            Assert.Equal(Last(19999, 20000), shotChangesFrames.LastWithin(19999, 20000));
            Assert.Equal(Last(20000, 20000), shotChangesFrames.LastWithin(20000, 20000));
            Assert.Equal(Last(20000, 20001), shotChangesFrames.LastWithin(20000, 20001));
            Assert.Equal(Last(19999, 20001), shotChangesFrames.LastWithin(19999, 20001));
            Assert.Equal(Last(20001, 20001), shotChangesFrames.LastWithin(20001, 20001));
            Assert.Equal(Last(30000, 30000), shotChangesFrames.LastWithin(30000, 30000));
            Assert.Equal(Last(30000, 40000), shotChangesFrames.LastWithin(30000, 40000));
            Assert.Equal(Last(25000, 5000), shotChangesFrames.LastWithin(25000, 5000));
            Assert.Equal(Last(50, 60), shotChangesFrames.LastWithin(50, 60));
            Assert.Equal(Last(50, 1100), shotChangesFrames.LastWithin(50, 1100));
        }
    }
}
