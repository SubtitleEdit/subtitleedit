using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    [Collection("NonParallelTests")]
    public class TimeCodeTest
    {
        [Fact]
        public void TimeCodeDaysTest()
        {
            var tc = new TimeCode(24 * 3, 0, 0, 0)
            {
                Hours = 0,
                Minutes = 0,
                Seconds = 0,
                Milliseconds = 0,
            };
            Assert.True(tc.TotalMilliseconds > 0);
        }

        [Fact]
        public void TimeCodeMilliseconds()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Milliseconds = 9 };

            Assert.Equal(tc.TotalMilliseconds, new TimeSpan(0, 1, 2, 3, 9).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeSeconds1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Seconds = 9 };

            Assert.Equal(tc.TotalMilliseconds, new TimeSpan(0, 1, 2, 9, 4).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeMinutes1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Minutes = 9 };

            Assert.Equal(tc.TotalMilliseconds, new TimeSpan(0, 1, 9, 3, 4).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeHours1()
        {
            var tc = new TimeCode(1, 2, 3, 4) { Hours = 9 };

            Assert.Equal(tc.TotalMilliseconds, new TimeSpan(0, 9, 2, 3, 4).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeParseToMilliseconds1()
        {
            var ms = TimeCode.ParseToMilliseconds("01:02:03:999");

            Assert.Equal(ms, new TimeSpan(0, 1, 2, 3, 999).TotalMilliseconds);
        }


        [Fact]
        public void TimeCodeParseToMilliseconds2()
        {
            var ms = TimeCode.ParseToMilliseconds("01:02:03:9");

            Assert.Equal(ms, new TimeSpan(0, 1, 2, 3, 900).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeParseToMilliseconds3()
        {
            var ms = TimeCode.ParseToMilliseconds("01:02:03:99");

            Assert.Equal(ms, new TimeSpan(0, 1, 2, 3, 990).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeParseToMilliseconds4()
        {
            var ms = TimeCode.ParseToMilliseconds("02:03:99");

            Assert.Equal(ms, new TimeSpan(0, 0, 2, 3, 990).TotalMilliseconds);
        }

        [Fact]
        public void TimeCodeGetTotalMilliseconds()
        {
            var tc = new TimeCode(1, 2, 3, 4);

            Assert.Equal(3723004, tc.TotalMilliseconds);
            Assert.True(Math.Abs(tc.TotalMilliseconds - tc.TotalSeconds * 1000.0) < 0.001);
        }

        [Fact]
        public void ToShortStringHhmmssff1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(1, 2, 3, 0).ToShortStringHHMMSSFF();
            Assert.Equal("01:02:03:00", res);
        }

        [Fact]
        public void ToShortStringHhmmssff2()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 2, 3, 0).ToShortStringHHMMSSFF();
            Assert.True(res == "02:03:00", res);
        }

        [Fact]
        public void ToShortStringHhmmssff3()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 0, 3, 0).ToShortStringHHMMSSFF();
            Assert.Equal("03:00", res);
        }

        [Fact]
        public void ToShortStringHhmmssff4()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(0, 0, 0, 0).ToShortStringHHMMSSFF();
            Assert.Equal("00:00", res);
        }

        [Fact]
        public void ToHHMMSSFFTestSecond()
        {
            Configuration.Settings.General.CurrentFrameRate = 00;
            var res = new TimeCode(23, 23, 21, 0).ToHHMMSSFF();

            // note: one second is added when converting to ToHHMMSSFF
            Assert.Equal("23:23:22:00", res);
        }

        [Fact]
        public void ToHHMMSSFFTestFrame()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var res = new TimeCode(23, 23, 21, 0).ToHHMMSSFF();

            // note: one second is added when converting to ToHHMMSSFF
            Assert.Equal("23:23:21:00", res);
        }

        [Fact]
        public void ToHHMMSSFFTestNegative()
        {
            Configuration.Settings.General.CurrentFrameRate = 00;
            var res = new TimeCode(1, -1, 0, 0).ToHHMMSSFF();

            // note: one second is added when converting to ToHHMMSSFF
            Assert.Equal("00:59:01:00", res);
        }

        [Fact]
        
        public void ToHHMMSSPeriodFFTest()
        {
            Configuration.Settings.General.CurrentFrameRate = 00;
            var res = new TimeCode(23, 23, 21, 0).ToHHMMSSPeriodFF();

            // note: one second is added when converting to ToHHMMSSFF
            Assert.Equal("23:23:22.00", res);
        }
    }
}
