using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats.Assa
{
    
    public class AssaTimeCodes
    {
        [Fact]
        public void TestAssaWritingWithCorrectRounding()
        {
            var text = @"
            1
            00:03:44,037-- > 00:03:45,997
            Text 1";

            var s = new Subtitle();
            new SubRip().LoadSubtitle(s, text.SplitToLines(), null);
            var res = new AdvancedSubStationAlpha().ToText(s, string.Empty);
            Assert.Contains("0,0:03:44.04,0:03:46.00", res);
        }
    }
}
