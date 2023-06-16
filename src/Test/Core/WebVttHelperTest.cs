using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Core
{
    [TestClass]
    public class WebVttHelperTest
    {
        [TestMethod]
        public void RemoveColorTag1()
        {
            var styles = new List<WebVttStyle>
            {
                new WebVttStyle()
                {
                    Name = "Red",
                    Color = Color.Red,
                },
            };

            var text = "<c.Red>Red</c>";
            var result = WebVttHelper.RemoveColorTag(text, Color.Red, styles);

            Assert.AreEqual("Red", result);
        }

        [TestMethod]
        public void RemoveColorTag2()
        {
            var styles = new List<WebVttStyle>
            {
                new WebVttStyle
                {
                    Name = "Red",
                    Color = Color.Red,
                },
                new WebVttStyle
                {
                    Name = "Italic",
                    Italic = true,
                },
            };

            var text = "<c.Red.Italic>Red</c>";
            var result = WebVttHelper.RemoveColorTag(text, Color.Red, styles);

            Assert.AreEqual("<c.Italic>Red</c>", result);
        }
    }
}
