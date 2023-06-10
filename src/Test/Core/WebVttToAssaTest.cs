using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Core
{
    [TestClass]
    public class WebVttToAssaTest
    {
        [TestMethod]
        public void TestStyles1()
        {
            var subtitle = new Subtitle();
            subtitle.Header = "WEBVTT\r\n\r\nSTYLE\r\n::cue(.background-color_transparent) {\r\n  background-color: rgba(255,255,255,0.0);\r\n}\r\n::cue(.color_EBEBEB) {\r\n  color: rgba(235,235,235,1.000000);\r\n}\r\n::cue(.font-family_Arial) {\r\n  font-family: Arial;\r\n}\r\n::cue(.font-style_normal) {\r\n  font-style: normal;\r\n}\r\n::cue(.font-weight_normal) {\r\n  font-weight: normal;\r\n}\r\n::cue(.text-shadow_#101010-3px) {\r\n  text-shadow: #101010 3px;\r\n}\r\n::cue(.font-style_italic) {\r\n  font-style: italic;\r\n}";
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(converted.Header);

            Assert.AreEqual(".background-color_transparent", styles[0].Name);
            Assert.AreEqual(".color_EBEBEB", styles[1].Name);
            Assert.AreEqual(".font-family_Arial", styles[2].Name);
            Assert.AreEqual(".font-style_normal", styles[3].Name);
            Assert.AreEqual(".font-weight_normal", styles[4].Name);
            Assert.AreEqual(".text-shadow_#101010-3px", styles[5].Name);
            Assert.AreEqual(".font-style_italic", styles[6].Name);

            Assert.AreEqual(235, styles[1].Primary.R);
            Assert.AreEqual("Arial", styles[2].FontName);
            Assert.AreEqual(false, styles[3].Italic);
            Assert.AreEqual(false, styles[4].Bold);
            Assert.AreEqual(3, styles[5].ShadowWidth);
            Assert.AreEqual(true, styles[6].Italic);
        }

        [TestMethod]
        public void TestStyles2()
        {
            var subtitle = new Subtitle();
            subtitle.Header = "STYLE\r\n::cue(.styledotEAC118) { color:#EAC118 }\r\n::cue(.styledotaqua) { color:aqua }\r\n::cue(.styledotaquadotitalic) { color:aqua;font-style:italic }\r\n::cue(.styledotitalic) { font-style:italic }\r\n::cue(.styledotEAC118dotitalic) { color:#EAC118;font-style:italic }";
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(converted.Header);

            Assert.AreEqual(".styledotEAC118", styles[0].Name);
            Assert.AreEqual(".styledotaqua", styles[1].Name);
            Assert.AreEqual(".styledotaquadotitalic", styles[2].Name);
            Assert.AreEqual(".styledotitalic", styles[3].Name);
            Assert.AreEqual(".styledotEAC118dotitalic", styles[4].Name);

            Assert.AreEqual(234, styles[0].Primary.R);
            Assert.AreEqual(193, styles[0].Primary.G);
            Assert.AreEqual(24, styles[0].Primary.B);
            Assert.AreEqual(0, styles[1].Primary.R);
            Assert.AreEqual(255, styles[1].Primary.G);
            Assert.AreEqual(255, styles[1].Primary.B);
            Assert.AreEqual(true, styles[2].Italic);
            Assert.AreEqual(true, styles[3].Italic);
            Assert.AreEqual(true, styles[4].Italic);
            Assert.AreEqual(234, styles[4].Primary.R);
            Assert.AreEqual(193, styles[4].Primary.G);
            Assert.AreEqual(24, styles[4].Primary.B);
        }
    }
}
