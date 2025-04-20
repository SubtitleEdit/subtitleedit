using Nikse.SubtitleEdit.Core.BluRaySup;

namespace Tests.Logic.BluRaySup
{
    
    public class ToolBoxTest
    {
        [Fact]
        public void TestZeroPtsToTimeString()
        {
            Assert.Equal("00:00:00.000", ToolBox.PtsToTimeString(0));
        }
    }
}
