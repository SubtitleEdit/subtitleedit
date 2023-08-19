using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Common
{
    [TestClass]
    // ReSharper disable once IdentifierTypo
    public class FormattableTextReaderText
    {
        [TestMethod]
        public void ReadTextTest()
        {
            var text = "<i>foobar</i>";
            var reader = new FormattableTextReader(text);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual('f', reader.GetCurrent());
        }

        [TestMethod]
        public void ReadTagOnlyTest()
        {
            var text = "<i></i>";
            var reader = new FormattableTextReader(text);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual('\0', reader.GetCurrent());
            Assert.IsFalse(reader.Read());
        }
    }
}