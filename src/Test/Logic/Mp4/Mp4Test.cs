using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.IO;

namespace Test.Logic.Mp4
{
    [TestClass]
    [DeploymentItem("Files")]
    public class Mp4Test
    {
        [TestMethod]
        public void Mp4Test1()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MP4_SRT.mp4");
            var parser = new MP4Parser(fileName);

            var tracks = parser.GetSubtitleTracks();
            var paragraphs = tracks[0].Mdia.Minf.Stbl.GetParagraphs();

            //1
            //00:00:01,000-- > 00:00:03,000
            //Line 1

            //2
            //00:00:03,024-- > 00:00:05,024
            //Line 2

            Assert.IsTrue(tracks.Count == 1);
            Assert.IsTrue(paragraphs.Count == 2);

            Assert.IsTrue(Math.Abs(paragraphs[0].StartTime.TotalMilliseconds - 1000) < 0.01);
            Assert.IsTrue(Math.Abs(paragraphs[0].EndTime.TotalMilliseconds - 3000) < 0.01);
            Assert.IsTrue(paragraphs[0].Text == "Line 1");

            Assert.IsTrue(Math.Abs(paragraphs[1].StartTime.TotalMilliseconds - 3024) < 0.01);
            Assert.IsTrue(Math.Abs(paragraphs[1].EndTime.TotalMilliseconds - 5024) < 0.01);
            Assert.IsTrue(paragraphs[1].Text == "Line 2");
        }
    }

}
