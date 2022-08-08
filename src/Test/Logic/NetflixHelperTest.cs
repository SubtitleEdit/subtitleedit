using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;

namespace Test.Logic
{
    [TestClass]
    public class NetflixHelperTest
    {
        [TestMethod]
        public void ConvertNumberToString1()
        {
            var result = NetflixHelper.ConvertNumberToString("5", false, "da");
            Assert.AreEqual("fem", result);
        }

        [TestMethod]
        public void ConvertNumberToString2()
        {
            var result = NetflixHelper.ConvertNumberToString("5", false, "en");
            Assert.AreEqual("five", result);
        }

        [TestMethod]
        public void ConvertNumberToString3()
        {
            var result = NetflixHelper.ConvertNumberToString("5", true, "en");
            Assert.AreEqual("Five", result);
        }

        [TestMethod]
        public void ConvertNumberToString4()
        {
            var result = NetflixHelper.ConvertNumberToString("50000", true, "en");
            Assert.AreEqual("50000", result);
        }
    }
}
