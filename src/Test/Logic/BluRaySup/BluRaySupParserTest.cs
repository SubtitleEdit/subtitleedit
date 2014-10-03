using System;
using System.Drawing;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic.BluRaySup;
using System.IO;

namespace Test.Logic.BluRaySup
{
    [TestClass]
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
                var buffer = BluRaySupPicture.CreateSupFrame(brSub, new Bitmap(100, 20), 25, 10, ContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
                brSub.StartTime = 2000;
                brSub.EndTime = 3000;
                buffer = BluRaySupPicture.CreateSupFrame(brSub, new Bitmap(100, 20), 25, 10, ContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
            }

            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);

            Assert.AreEqual(2, subtitles.Count);
        }

    }
}
