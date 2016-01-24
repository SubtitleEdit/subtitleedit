using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.TransportStream;
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
    }
}
