using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace Test.Logic.TransportStream
{
    [TestClass]
    [DeploymentItem("Files")]
    public class TransportStreamTest
    {
        [TestMethod]
        public void TransportStreamTestImage()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_graphics.ts");
            var parser = new TransportStreamParser();
            parser.Parse(fileName, null);
            var subtitles = parser.GetDvbSubtitles(41);

            Assert.IsTrue(subtitles.Count == 10);
            using (var bmp = subtitles[0].Pes.GetImageFull())
            {
                Assert.IsTrue(bmp.Width == 719);
                Assert.IsTrue(bmp.Height == 575);
            }
        }

        [TestMethod]
        public void TransportStreamTestTeletext()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_teletext.ts");
            var parser = new TransportStreamParser();
            parser.Parse(fileName, null);
            Assert.AreEqual(1, parser.TeletextSubtitlesLookup.Count);
            Assert.AreEqual(5104, parser.TeletextSubtitlesLookup.First().Key); // package Id
            var packagePages = parser.TeletextSubtitlesLookup[parser.TeletextSubtitlesLookup.First().Key];
            Assert.AreEqual(2, packagePages.Count);
            Assert.AreEqual(1, packagePages[150].Count); // first page number
            Assert.AreEqual(1, packagePages[799].Count); // second page number

            Assert.AreEqual("Für diese Klassenstufe ist er nicht" + Environment.NewLine +
                            "geeignet.  <font color=\"#00ffff\">  Stufen Sie ihn zurück!</font>", packagePages[150][0].Text);

            Assert.AreEqual("Han er ikke egnet" + Environment.NewLine +
                            "til dette klassetrin.", packagePages[799][0].Text);
        }
    }
}
