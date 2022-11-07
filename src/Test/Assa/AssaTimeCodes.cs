using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Assa
{
    [TestClass]
    public class AssaTimeCodes
    {
        [TestMethod]
        public void TestAssaWritingWithCorrectRounding()
        {
            var text = @"
            1
            00:03:44,037-- > 00:03:45,997
            Text 1";

            var s = new Subtitle();
            new SubRip().LoadSubtitle(s, text.SplitToLines(), null);
            var res = new AdvancedSubStationAlpha().ToText(s,string.Empty);
            Assert.IsTrue(res.Contains("0,0:03:44.04,0:03:46.00"));
        }
    }
}
