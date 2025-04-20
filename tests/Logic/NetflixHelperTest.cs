using Nikse.SubtitleEdit.Core.NetflixQualityCheck;

namespace Tests.Logic
{
    
    public class NetflixHelperTest
    {
        [Fact]
        public void ConvertNumberToString1()
        {
            var result = NetflixHelper.ConvertNumberToString("5", false, "da");
            Assert.Equal("fem", result);
        }

        [Fact]
        public void ConvertNumberToString2()
        {
            var result = NetflixHelper.ConvertNumberToString("5", false, "en");
            Assert.Equal("five", result);
        }

        [Fact]
        public void ConvertNumberToString3()
        {
            var result = NetflixHelper.ConvertNumberToString("5", true, "en");
            Assert.Equal("Five", result);
        }

        [Fact]
        public void ConvertNumberToString4()
        {
            var result = NetflixHelper.ConvertNumberToString("50000", true, "en");
            Assert.Equal("50000", result);
        }
    }
}
