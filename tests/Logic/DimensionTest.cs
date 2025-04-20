using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    public class DimensionTest
    {
        [Fact]
        public void InvalidTest()
        {
            var dimension = new Dimension();
            Assert.Equal(0, dimension.Width);
            Assert.Equal(0, dimension.Height);
            Assert.True(!dimension.IsValid());
        }

        [Fact]
        public void ValidTest()
        {
            var dimension = new Dimension(10, 10);
            Assert.True(dimension.IsValid());
        }

        [Fact]
        public void EqualityTest()
        {
            var dimensionOne = new Dimension();
            var dimensionTwo = new Dimension();
            Assert.Equal(dimensionOne, dimensionTwo);
            Assert.True(dimensionOne == dimensionTwo);
        }
    }
}