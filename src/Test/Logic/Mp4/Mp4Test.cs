using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System;
using System.IO;

namespace Test.Logic.Mp4
{
    [TestClass]
    [DeploymentItem("Files")]
    public class Mp4Test
    {
        [TestMethod]
        public void Mp4TestFile1()
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MP4.mp4");
            var parser = new MP4Parser(fileName);

            var tracks = parser.GetSubtitleTracks();
            var paragraphs = tracks[0].Mdia.Minf.Stbl.GetParagraphs();

            //1
            //00:00:11,080-- > 00:00:13,399
            //(Alan) 'Chapter one. Sorrow.

            //808
            //00:43:14,200-- > 00:43:16,080
            //(♪ Theme music)

            Assert.IsTrue(tracks.Count == 1);
            Assert.IsTrue(paragraphs.Count == 808);

            Assert.IsTrue(Math.Abs(paragraphs[0].StartTime.TotalMilliseconds - 11080) < 0.5);
            Assert.IsTrue(Math.Abs(paragraphs[0].EndTime.TotalMilliseconds - 13399) < 0.5);
            Assert.IsTrue(paragraphs[0].Text == "(Alan) 'Chapter one. Sorrow.");

            Assert.IsTrue(Math.Abs(paragraphs[807].StartTime.TotalMilliseconds - 2594200) < 0.5);
            Assert.IsTrue(Math.Abs(paragraphs[807].EndTime.TotalMilliseconds - 2596080) < 0.5);
            Assert.IsTrue(paragraphs[807].Text == "(♪ Theme music)");
        }

        [TestMethod]
        public void Mp4TestFile2()
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MP4_SRT.mp4");
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

        private static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        [TestMethod]
        public void Mp4MvhdBox64Bit()
        {
            var buffer = StringToByteArray("010000000000000000000000000000000000000000989680000000068fcd4c00");
            using (var ms = new MemoryStream(buffer))
            {
                var mvhd = new Mvhd(ms);
                Assert.IsTrue(10000000 == mvhd.TimeScale);
            }
        }
    }
}
