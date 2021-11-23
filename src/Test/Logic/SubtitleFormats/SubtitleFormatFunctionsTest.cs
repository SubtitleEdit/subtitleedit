using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Logic.SubtitleFormats
{
    public class SubtitleFormatFunctionsTest
    {
        [TestMethod]
        public void MillisecondsToFrames1()
        {
            var x = SubtitleFormat.MillisecondsToFrames(500, 25);
            Assert.AreEqual(x, 13);
        }

        [TestMethod]
        public void FramesToMilliseconds1()
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var x = SubtitleFormat.FramesToMilliseconds(1);
            Assert.AreEqual(x, 40);
        }
    }
}
