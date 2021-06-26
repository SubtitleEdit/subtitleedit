using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using System.Collections.Generic;

namespace Test.Logic
{
    [TestClass]
    public class BridgeGapsTest
    {
        [TestMethod]
        public void InvalidMinMaxTest()
        {
            Assert.AreEqual(0, DurationsBridgeGaps.BridgeGaps(GetSubtitle(), 1000, true, 10, null, null, false));
        }

        [TestMethod]
        public void AdjustGapsTest()
        {
            var stubDic = new Dictionary<string, string>();
            var stubList = new List<int>();

            int result = DurationsBridgeGaps.BridgeGaps(GetSubtitle(), 24, true, 100, stubList, stubDic, false);

            Assert.AreNotEqual(0, result);
            // expedtec to contains both p and p + 1 index of adjusted paragraph
            Assert.AreEqual(6, stubList.Count);
            // expected to contains only index of adjusted paragraph
            Assert.AreEqual(3, stubDic.Count);
        }

        public static Subtitle GetSubtitle()
        {
            var paragraphs = new List<Paragraph>
            {
                new Paragraph("", TimeCode.ParseToMilliseconds("00:00:49,520"), TimeCode.ParseToMilliseconds("00:00:52,390")),
                new Paragraph("", TimeCode.ParseToMilliseconds("00:00:52,470"), TimeCode.ParseToMilliseconds("00:00:55,100")),
                new Paragraph("", TimeCode.ParseToMilliseconds("00:00:55,180"), TimeCode.ParseToMilliseconds("00:00:57,060")),
                new Paragraph("", TimeCode.ParseToMilliseconds("00:00:57,140"), TimeCode.ParseToMilliseconds("00:01:01,100")),
            };

            return new Subtitle(paragraphs);
        }
    }
}
