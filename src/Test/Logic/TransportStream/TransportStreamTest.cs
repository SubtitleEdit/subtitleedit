using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Test.Logic.TransportStream
{
    [TestClass]
    [DeploymentItem("Files")]
    public class TransportStreamTest
    {
        [TestMethod]
        public void TransportStreamTest1()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_graphics.ts");
            var parser = new Nikse.SubtitleEdit.Logic.TransportStream.TransportStreamParser();
            parser.ParseTSFile(fileName);
            var subtitles = parser.GetDvbSubtitles(41);

            Assert.IsTrue(subtitles.Count == 10);
            Assert.IsTrue(subtitles[0].Pes.GetImageFull().Width == 719);
            Assert.IsTrue(subtitles[0].Pes.GetImageFull().Height == 575);
        }
    }
}
