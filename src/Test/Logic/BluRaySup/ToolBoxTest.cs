using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.BluRaySup;

namespace Test.Logic.BluRaySup
{
    [TestClass]
    public class ToolBoxTest
    {
        [TestMethod]
        public void TestZeroPtsToTimeString()
        {
            Assert.AreEqual("00:00:00.000", ToolBox.PtsToTimeString(0));
        }
    }
}
