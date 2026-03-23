using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.SubtitleFormats.Assa
{
    
    public class AssaResamplerTest
    {
        [Fact]
        public void TestResampleOverrideFontTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10}Hallo!");
            Assert.Equal("{\\fs20}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideFontTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10}Hallo! {\\fs20}Hallo!");
            Assert.Equal("{\\fs20}Hallo! {\\fs40}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideFontTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fs10\\fs20\\fs30}Hallo!");
            Assert.Equal("{\\fs20\\fs40\\fs60}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideFontTags4()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\blur1.111}Hallo!");
            Assert.Equal("{\\blur2.2}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideFontTags5()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(1280, 1920, 720, 1080, "{\\blur0.667}Hallo!");
            Assert.Equal("{\\blur1}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideFontTags6DoNotChange()
        {
            var result = AssaResampler.ResampleOverrideTagsFont(100, 200, 100, 200, "{\\fscx110\\fscy138}Hallo!");
            Assert.Equal("{\\fscx110\\fscy138}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverridePositionTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10,20)}Hallo!");
            Assert.Equal("{\\pos(20,40)}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverridePositionTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10,20)\\pos(20,30)}Hallo!");
            Assert.Equal("{\\pos(20,40)\\pos(40,60)}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverridePositionTags2Negative()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(-10,20)\\pos(20,-30)}Hallo!");
            Assert.Equal("{\\pos(-20,40)\\pos(40,-60)}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverridePositionTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsPosition(100, 200, 100, 200, "{\\pos(10.111,20.222)\\pos(20.333,30.444)}Hallo!");
            Assert.Equal("{\\pos(20.2,40.4)\\pos(40.7,60.9)}Hallo!", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTagsClip()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\iclip(m 0 0 l 100 0 100 100 0 100)}");
            Assert.Equal("{\\iclip(m 0 0 l 200 0 200 200 0 200)}", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags1()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}");
            Assert.Equal("{\\p1}m 0 0 l 200 0 200 200 0 200{\\p0}", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags1a()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p2}m 0 0 l 100 0 100 100 0 100{\\p0}");
            Assert.Equal("{\\p2}m 0 0 l 200 0 200 200 0 200{\\p0}", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags2()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1}m 0 0 l 100.1 0.2 100 100 0 100{\\p0}");
            Assert.Equal("{\\p1}m 0 0 l 200.2 0.4 200 200 0 200{\\p0}", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags3()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1\\an7}m 0 0 l 100.1 0.2 100 100 0 100{\\p0}");
            Assert.Equal("{\\p1\\an7}m 0 0 l 200.2 0.4 200 200 0 200{\\p0}", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags4()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\p1\\an7}m 0 0 l 100.1 0.2 100 100 0 100");
            Assert.Equal("{\\p1\\an7}m 0 0 l 200.2 0.4 200 200 0 200", result);
        }

        [Fact]
        public void TestResampleOverrideDrawingTags5()
        {
            var result = AssaResampler.ResampleOverrideTagsDrawing(100, 200, 100, 200, "{\\an7\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}" + Environment.NewLine + "{\\iclip(m 0 0 l 100 0 100 100 0 100)}");
            Assert.Equal("{\\an7\\p1}m 0 0 l 200 0 200 200 0 200{\\p0}" + Environment.NewLine + "{\\iclip(m 0 0 l 200 0 200 200 0 200)}", result);
        }
    }
}
