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

            Assert.AreEqual(".background-color_transparent", styles[1].Name);
            Assert.AreEqual(".color_EBEBEB", styles[2].Name);
            Assert.AreEqual(".font-family_Arial", styles[3].Name);
            Assert.AreEqual(".font-style_normal", styles[4].Name);
            Assert.AreEqual(".font-weight_normal", styles[5].Name);
            Assert.AreEqual(".text-shadow_#101010-3px", styles[6].Name);
            Assert.AreEqual(".font-style_italic", styles[7].Name);

            Assert.AreEqual(235, styles[2].Primary.R);
            Assert.AreEqual("Arial", styles[3].FontName);
            Assert.AreEqual(false, styles[4].Italic);
            Assert.AreEqual(false, styles[5].Bold);
            Assert.AreEqual(3, styles[6].ShadowWidth);
            Assert.AreEqual(true, styles[7].Italic);
        }

        [TestMethod]
        public void TestStyles2()
        {
            var subtitle = new Subtitle();
            subtitle.Header = "STYLE\r\n::cue(.styledotEAC118) { color:#EAC118 }\r\n::cue(.styledotaqua) { color:aqua }\r\n::cue(.styledotaquadotitalic) { color:aqua;font-style:italic }\r\n::cue(.styledotitalic) { font-style:italic }\r\n::cue(.styledotEAC118dotitalic) { color:#EAC118;font-style:italic }";
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(converted.Header);

            Assert.AreEqual(".styledotEAC118", styles[1].Name);
            Assert.AreEqual(".styledotaqua", styles[2].Name);
            Assert.AreEqual(".styledotaquadotitalic", styles[3].Name);
            Assert.AreEqual(".styledotitalic", styles[4].Name);
            Assert.AreEqual(".styledotEAC118dotitalic", styles[5].Name);

            Assert.AreEqual(234, styles[1].Primary.R);
            Assert.AreEqual(193, styles[1].Primary.G);
            Assert.AreEqual(24, styles[1].Primary.B);
            Assert.AreEqual(0, styles[2].Primary.R);
            Assert.AreEqual(255, styles[2].Primary.G);
            Assert.AreEqual(255, styles[2].Primary.B);
            Assert.AreEqual(true, styles[3].Italic);
            Assert.AreEqual(true, styles[4].Italic);
            Assert.AreEqual(true, styles[5].Italic);
            Assert.AreEqual(234, styles[5].Primary.R);
            Assert.AreEqual(193, styles[5].Primary.G);
            Assert.AreEqual(24, styles[5].Primary.B);
        }

        [TestMethod]
        public void TestLineStyles1()
        {
            var subtitle = new Subtitle();
            subtitle.Header = "STYLE\r\n::cue(.styledotEAC118) { color:#EAC118 }";
            subtitle.Paragraphs.Add(new Paragraph("<c.styledotEAC118>Hi</c>", 0,0));
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);

            Assert.AreEqual("Hi", converted.Paragraphs[0].Text);
            Assert.AreEqual(".styledotEAC118", converted.Paragraphs[0].Extra);
        }

        [TestMethod]
        public void TestLineStyles2()
        {
            var subtitle = new Subtitle();
            subtitle.Header = "STYLE\r\n::cue(.styleItalic) { font-style:italic }\r\n::cue(.styleColor123456) {  color:#123456 }";
            subtitle.Paragraphs.Add(new Paragraph("<c.styleItalic.styleColor123456>Hi</c>", 0, 0));
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);

            Assert.AreEqual("{\\c&H563412\\i1}Hi", converted.Paragraphs[0].Text);
        }

        [TestMethod]
        public void TestItalicInline()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo <i>italic</i> world.", 0, 0));
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var text = converted.ToText(new AdvancedSubStationAlpha());
            Assert.AreEqual("Hallo {\\i1}italic{\\i0} world.", converted.Paragraphs[0].Text);
            Assert.IsTrue(text.Contains("Hallo {\\i1}italic{\\i0} world."));
        }

        [TestMethod]
        public void TestBoldInline()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo <b>bold</b> world.", 0, 0));
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var text = converted.ToText(new AdvancedSubStationAlpha());
            Assert.IsTrue(text.Contains("Hallo {\\b1}bold{\\b0} world."));
        }

        [TestMethod]
        public void TestUnderlineInline()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hallo <u>underline</u> world.", 0, 0));
            var converted = WebVttToAssa.Convert(subtitle, new SsaStyle(), 1920, 1080);
            var text = converted.ToText(new AdvancedSubStationAlpha());
            Assert.IsTrue(text.Contains("Hallo {\\u1}underline{\\u0} world."));
        }
    }
}
