using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class SubtitleFormatFunctionsTest
    {
        [TestMethod]
        public void MillisecondsToFrames1()
        {
            var frames = SubtitleFormat.MillisecondsToFrames(500, 25);
            Assert.AreEqual(13, frames);
        }

        [TestMethod]
        public void MillisecondsToFrames2()
        {
            var frames = SubtitleFormat.MillisecondsToFrames(499, 25);
            Assert.AreEqual(12, frames);
        }

        [TestMethod]
        public void FramesToMilliseconds1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var ms = SubtitleFormat.FramesToMilliseconds(1);
            Assert.AreEqual(ms, 40);
        }
    }
}
