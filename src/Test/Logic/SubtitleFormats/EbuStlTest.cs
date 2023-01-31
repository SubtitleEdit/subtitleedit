using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System.IO;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class EbuStlTest
    {
        [TestMethod]
        public void EbuFont()
        {
            var target = new Ebu();
            var subtitle = new Subtitle();
            var subText = "<font color=\"Red\">Red</font> <font color=\"Blue\">Blue</font> <font color=\"Green\">Green</font>";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            Ebu.EbuUiHelper = new UiEbuSaveHelper();
            target.Save("test.pac", ms, subtitle, true, new Ebu.EbuGeneralSubtitleInformation()
            {
                DisplayStandardCode = "1",
            });
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "<font color=\"Red\">Red</font> <font color=\"Blue\">Blue</font> <font color=\"Green\">Green</font>");
        }
    }
}