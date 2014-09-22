using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;

namespace Test.Logic
{
    [TestClass]
    public class HtmlUtilsTest
    {
        [TestMethod]
        public void TestRemoveOpenCloseTagCyrillicI()
        {
            const string source = "<\u0456>SubtitleEdit</\u0456>";
            Assert.AreEqual("SubtitleEdit", HtmlUtils.RemoveOpenCloseTags(source, HtmlUtils.TAG_CYRILLIC_I));
        }

        [TestMethod]
        public void TestRemoveOpenCloseTagFont()
        {
            const string source = "<font color=\"#000\">SubtitleEdit</font>";
            Assert.AreEqual("SubtitleEdit", HtmlUtils.RemoveOpenCloseTags(source, HtmlUtils.TAG_FONT));
        }
    }
}
