using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Assa
{
    [TestClass]
    public class AssaResamplerTest
    {
        [TestMethod]
        public void TestResampleOverrideFontTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10}Hallo!");
            Assert.AreEqual("{\\fs20}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideFontTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10}Hallo! {\\fs20}Hallo!");
            Assert.AreEqual("{\\fs20}Hallo! {\\fs40}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideFontTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10\\fs20\\fs30}Hallo!");
            Assert.AreEqual("{\\fs20\\fs40\\fs60}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverridePositionTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10,20)}Hallo!");
            Assert.AreEqual("{\\pos(20,40)}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverridePositionTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10,20)\\pos(20,30)}Hallo!");
            Assert.AreEqual("{\\pos(20,40)\\pos(40,60)}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}");
            Assert.AreEqual("{\\p1}m 0 0 l 200 0 200 200 0 200{\\p0}", result);
        }
    }
}
