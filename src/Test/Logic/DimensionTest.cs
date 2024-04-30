using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Logic
{
    [TestClass]
    public class DimensionTest
    {
        [TestMethod]
        public void InvalidTest()
        {
            var dimension = new Dimension();
            Assert.AreEqual(0, dimension.Width);
            Assert.AreEqual(0, dimension.Height);
            Assert.IsTrue(!dimension.IsValid());
        }
        
        [TestMethod]
        public void ValidTest()
        {
            var dimension = new Dimension(10, 10);
            Assert.IsTrue(dimension.IsValid());
        }
        
        [TestMethod]
        public void EqualityTest()
        {
            var dimensionOne = new Dimension();
            var dimensionTwo = new Dimension();
            Assert.AreEqual(dimensionOne, dimensionTwo);
            Assert.IsTrue(dimensionOne == dimensionTwo);
        }
    }
}