using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.BluRaySup;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Test.Logic.BluRaySup
{
    [TestClass]
    [DeploymentItem("Files")]
    public class BluRaySupParserTest
    {
        [TestMethod]
        public void BluRaySupWriteAndReadTwoBitmaps()
        {
            var fileName = Guid.NewGuid() + ".sup";
            using (var binarySubtitleFile = new FileStream(fileName, FileMode.Create))
            {
                var brSub = new BluRaySupPicture
                {
                    StartTime = 0,
                    EndTime = 1000,
                    Width = 1080,
                    Height = 720
                };
                var buffer = BluRaySupPicture.CreateSupFrame(brSub, new Bitmap(100, 20), 10, 25, 10, ContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
                brSub.StartTime = 2000;
                brSub.EndTime = 3000;
                buffer = BluRaySupPicture.CreateSupFrame(brSub, new Bitmap(100, 20), 10, 25, 10, ContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
            }

            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);

            Assert.AreEqual(2, subtitles.Count);
        }

        [TestMethod]
        public void BluRaySupReadMultiImagePic()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_BDSUP_multi_image.sup");
            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            Assert.AreEqual(2, subtitles.Count);
            Assert.AreEqual(2, subtitles[0].PcsObjects.Count);
            using (var bmp = subtitles[0].GetBitmap())
            {
                Assert.AreEqual(784, bmp.Width);
                Assert.AreEqual(911, bmp.Height);
            }
        }

    }
}
