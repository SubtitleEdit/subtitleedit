using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

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
        public void TestResampleOverrideFontTags4()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\blur1.111}Hallo!");
            Assert.AreEqual("{\\blur2.222}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideFontTags5()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(1280, 1920, 720, 1080, "{\\blur0.667}Hallo!");
            Assert.AreEqual("{\\blur1.001}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideFontTags6DoNotChange()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fscx110\\fscy138}Hallo!");
            Assert.AreEqual("{\\fscx110\\fscy138}Hallo!", result);
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
        public void TestResampleOverridePositionTags2Negative()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(-10,20)\\pos(20,-30)}Hallo!");
            Assert.AreEqual("{\\pos(-20,40)\\pos(40,-60)}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverridePositionTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10.111,20.222)\\pos(20.333,30.444)}Hallo!");
            Assert.AreEqual("{\\pos(20.222,40.444)\\pos(40.666,60.888)}Hallo!", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTagsClip()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\iclip(m 0 0 l 100 0 100 100 0 100)}");
            Assert.AreEqual("{\\iclip(m 0 0 l 200 0 200 200 0 200)}", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}");
            Assert.AreEqual("{\\p1}m 0 0 l 200 0 200 200 0 200{\\p0}", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1}m 0 0 l 100.111 0.222 100 100 0 100{\\p0}");
            Assert.AreEqual("{\\p1}m 0 0 l 200.222 0.444 200 200 0 200{\\p0}", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1\\an7}m 0 0 l 100.111 0.222 100 100 0 100{\\p0}");
            Assert.AreEqual("{\\p1\\an7}m 0 0 l 200.222 0.444 200 200 0 200{\\p0}", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags4()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1\\an7}m 0 0 l 100.111 0.222 100 100 0 100");
            Assert.AreEqual("{\\p1\\an7}m 0 0 l 200.222 0.444 200 200 0 200", result);
        }

        [TestMethod]
        public void TestResampleOverrideDrawingTags5()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\an7\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}" + Environment.NewLine + "{\\iclip(m 0 0 l 100 0 100 100 0 100)}");
            Assert.AreEqual("{\\an7\\p1}m 0 0 l 200 0 200 200 0 200{\\p0}" + Environment.NewLine + "{\\iclip(m 0 0 l 200 0 200 200 0 200)}", result);
        }
    }
}
