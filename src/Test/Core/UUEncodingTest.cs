using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Core
{
    [TestClass]
    public class UUEncodingTest
    {
        [TestMethod]
        public void ForwardAndBackAgain()
        {
            var byteArray = new byte[byte.MaxValue];
            for (int i = byte.MinValue; i < byte.MaxValue; i++)
            {
                byteArray[i] = (byte)i;
            }

            var text = UUEncoding.UUEncode(byteArray);
            var newBytes = UUEncoding.UUDecode(text);

            Assert.AreEqual(byteArray.Length, newBytes.Length);
            for (int i = byte.MinValue; i < byte.MaxValue; i++)
            {
                Assert.AreEqual(byteArray[i], newBytes[i]);
            }
        }
    }
}
