using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Core
{
    [TestClass]
    public class RichTextToPlainTextTest
    {
        [TestMethod]
        public void TestConvertToRtfSlash()
        {
            var result = RichTextToPlainText.ConvertToRtf(@"Brian\Benny!");
            Assert.IsTrue(result.Contains(@"Brian\\Benny!"));
            result = RichTextToPlainText.ConvertToText(result);
            Assert.IsTrue(result.Trim() == @"Brian\Benny!");
        }

        [TestMethod]
        public void TestConvertToRtfCurlyBracketStart()
        {
            var result = RichTextToPlainText.ConvertToRtf(@"Brian{Benny!");
            Assert.IsTrue(result.Contains(@"Brian\{Benny!"));
            result = RichTextToPlainText.ConvertToText(result);
            Assert.IsTrue(result.Trim() == @"Brian{Benny!");
        }

        [TestMethod]
        public void TestConvertToRtfCurlyBracketEnd()
        {
            var result = RichTextToPlainText.ConvertToRtf(@"Brian}Benny!");
            Assert.IsTrue(result.Contains(@"Brian\}Benny!"));
            result = RichTextToPlainText.ConvertToText(result);
            Assert.IsTrue(result.Trim() == @"Brian}Benny!");
        }

    }
}
