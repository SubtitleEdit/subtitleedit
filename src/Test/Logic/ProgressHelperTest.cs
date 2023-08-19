using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Test.Logic
{
    [TestClass]
    public class ProgressHelperTest
    {
        [TestMethod]
        public void TestOneMinute()
        {
            var result = ProgressHelper.ToProgressTime(new TimeCode(0, 1, 0, 0).TotalMilliseconds);
            Assert.AreEqual("Time remaining: One minute", result);
        }

        [TestMethod]
        public void TestOneMinuteAndTwoSeconds()
        {
            var result = ProgressHelper.ToProgressTime(new TimeCode(0, 1, 2, 0).TotalMilliseconds);
            Assert.AreEqual("Time remaining: One minute and 2 seconds", result);
        }

        [TestMethod]
        public void TestTwoMinuteAndTwoSeconds()
        {
            var result = ProgressHelper.ToProgressTime(new TimeCode(0, 2, 2, 0).TotalMilliseconds);
            Assert.AreEqual("Time remaining: 2 minutes and 2 seconds", result);
        }

        [TestMethod]
        public void TestOneSecond()
        {
            var result = ProgressHelper.ToProgressTime(new TimeCode(0, 0, 1, 0).TotalMilliseconds);
            Assert.AreEqual("Time remaining: A few seconds", result);
        }
    }
}
