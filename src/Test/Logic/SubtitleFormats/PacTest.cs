using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Text;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class PacTest
    {
        [TestMethod]
        public void PacItalic1()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "Now <i>go</i> on!";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "Now <i>go</i> on!");
        }

        [TestMethod]
        public void PacItalic2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "<i>Now go on!</i>";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "<i>Now go on!</i>");
        }

        [TestMethod]
        public void PacItalic3()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "<i>Now</i>. Go on!";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "<i>Now</i>. Go on!");
        }

        [TestMethod]
        public void PacItalic4()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "V <i>Now</i> Go on!";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "V <i>Now</i> Go on!");
        }

        [TestMethod]
        public void PacItalic5()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "V <i>Now</i> G";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "V <i>Now</i> G");
        }

        [TestMethod]
        public void PacItalic6()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var target = new Pac();
            var subtitle = new Subtitle();
            string subText = "V <i>Now</i>.";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var ms = new MemoryStream();
            target.Save("test.pac", ms, subtitle);
            var reload = new Subtitle();
            target.LoadSubtitle(reload, ms.ToArray());
            Assert.IsTrue(reload.Paragraphs[0].Text == "V <i>Now</i>.");
        }
    }
}