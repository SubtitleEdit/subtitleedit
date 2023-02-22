using System;
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
            target.Save("test.stl", ms, subtitle, true, new Ebu.EbuGeneralSubtitleInformation()
            {
                DisplayStandardCode = "1",
            });
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "<font color=\"Red\">Red</font> <font color=\"Blue\">Blue</font> <font color=\"Green\">Green</font>");
        }

        [TestMethod]
        public void EbuFont2()
        {
            var target = new Ebu();
            var subtitle = new Subtitle();
            var subText = "Gimme <font color=\"Red\">the</font> gun, <font color=\"Red\">Henry.</font>";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            Ebu.EbuUiHelper = new UiEbuSaveHelper();
            target.Save("test.stl", ms, subtitle, true, new Ebu.EbuGeneralSubtitleInformation()
            {
                DisplayStandardCode = "1",
            });
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == subText);
        }

        [TestMethod]
        public void EbuFont3()
        {
            var target = new Ebu();
            var subtitle = new Subtitle();
            var subText = "<font color=\"Magenta\">This is a magenta line.</font>" + Environment.NewLine + "This is white.";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            Ebu.EbuUiHelper = new UiEbuSaveHelper();
            target.Save("test.stl", ms, subtitle, true, new Ebu.EbuGeneralSubtitleInformation()
            {
                DisplayStandardCode = "1",
            });
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == subText);
        }
    }
}