using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Test.Logic.VideoFormats
{
    [TestClass]
    [DeploymentItem("Files")]
    public class MatroskaTest
    {

        [TestMethod]
        public void MatroskaTestValid()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MKV_SRT.mkv");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                Assert.IsTrue(parser.IsValid);
            }
        }

        [TestMethod]
        public void MatroskaTestInvalid()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_graphics.ts");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                Assert.IsFalse(parser.IsValid);
            }
        }

        [TestMethod]
        public void MatroskaTestIsSrt()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MKV_SRT.mkv");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                bool isValid;
                var tracks = parser.GetMatroskaSubtitleTracks(fileName, out isValid);
                Assert.IsTrue(tracks[0].CodecId == "S_TEXT/UTF8");
            }
        }

        [TestMethod]
        public void MatroskaTestSrtContent()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MKV_SRT.mkv");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                bool isValid;
                var tracks = parser.GetMatroskaSubtitleTracks(fileName, out isValid);
                var subtitles = parser.GetMatroskaSubtitle(fileName, Convert.ToInt32(tracks[0].TrackNumber), out isValid, null);
                Assert.IsTrue(subtitles.Count == 2);
                Assert.IsTrue(subtitles[0].Text == "Line 1");
                Assert.IsTrue(subtitles[1].Text == "Line 2");
            }
        }

        [TestMethod]
        public void MatroskaTestVobSubPgs()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MKV_VobSub_PGS.mkv");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                bool isValid;
                var tracks = parser.GetMatroskaSubtitleTracks(fileName, out isValid);
                Assert.IsTrue(tracks[0].CodecId == "S_VOBSUB");
                Assert.IsTrue(tracks[1].CodecId == "S_HDMV/PGS");
            }
        }

        [TestMethod]
        public void MatroskaTestVobSubPgsContent()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_MKV_VobSub_PGS.mkv");
            using (var parser = new Nikse.SubtitleEdit.Logic.VideoFormats.Matroska(fileName))
            {
                bool isValid;
                var tracks = parser.GetMatroskaSubtitleTracks(fileName, out isValid);
                var subtitles = parser.GetMatroskaSubtitle(fileName, Convert.ToInt32(tracks[0].TrackNumber), out isValid, null);
                Assert.IsTrue(subtitles.Count == 2);
                //TODO: check bitmaps

                //subtitles = parser.GetMatroskaSubtitle(fileName, Convert.ToInt32(tracks[1].TrackNumber), out isValid, null);
                //Assert.IsTrue(subtitles.Count == 2);
                //check bitmaps
            }
        }

    }
}
