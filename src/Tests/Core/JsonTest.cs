using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Tests.Core
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        public void TestUnicodeFirst()
        {
            var result = Json.DecodeJsonText("\u05d1 ");
            Assert.AreEqual("ב ", result);
        }

        [TestMethod]
        public void TestUnicodeLast()
        {
            var result = Json.DecodeJsonText(" \u05d1");
            Assert.AreEqual(" ב", result);
        }
    }
}
