using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Core
{
    [TestClass]
    public class SubtitleFormatTest
    {
        [TestMethod]
        public void MillisecondsToFrames()
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            var fr = SubtitleFormat.MillisecondsToFrames(100);
            Assert.AreEqual(2, fr);
            fr = SubtitleFormat.MillisecondsToFrames(999);
            Assert.AreEqual(24, fr);

            Configuration.Settings.General.CurrentFrameRate = 30;
            fr = SubtitleFormat.MillisecondsToFrames(100);
            Assert.AreEqual(3, fr);
            fr = SubtitleFormat.MillisecondsToFrames(999);
            Assert.AreEqual(30, fr);

            fr = SubtitleFormat.MillisecondsToFrames(2000);
            Assert.AreEqual(60, fr);
        }

        [TestMethod]
        public void MillisecondsToFramesMaxFrameRate()
        {
            Configuration.Settings.General.CurrentFrameRate = 30;
            var fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(100);
            Assert.AreEqual(3, fr);
            fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(1000);
            Assert.AreEqual(29, fr);

            fr = SubtitleFormat.MillisecondsToFramesMaxFrameRate(2000);
            Assert.AreEqual(29, fr);
        }

        [TestMethod]
        public void FramesToMilliseconds()
        {
            Configuration.Settings.General.CurrentFrameRate = 30;
            var fr = SubtitleFormat.FramesToMilliseconds(1);
            Assert.AreEqual(33, fr);
            fr = SubtitleFormat.FramesToMilliseconds(30);
            Assert.AreEqual(1000, fr);

            fr = SubtitleFormat.FramesToMillisecondsMax999(30);
            Assert.AreEqual(999, fr);
        }
    }
}